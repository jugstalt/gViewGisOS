using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.SpatialAlgorithms;
using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.FillElasticSearch
{
    class Program
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: gView.Cmd.FillElasticSearch [json-file]");
                return;
            }

            try
            {
                //gView.Framework.system.SystemVariables.CustomApplicationDirectory =
                //    System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                Console.WriteLine("Environment");

                Console.WriteLine("Working Directory: " + gView.Framework.system.SystemVariables.StartupDirectory);
                Console.WriteLine("64Bit=" + gView.Framework.system.Wow.Is64BitProcess);

                var importConfig = JsonConvert.DeserializeObject<ImportConfig>(File.ReadAllText(args[0]));

                var searchContext = new ElasticSearchContext(importConfig.Connection.Url, importConfig.Connection.DefaultIndex);

                if (importConfig.Connection.DeleteIndex)
                    searchContext.DeleteIndex();

                searchContext.CreateIndex();
                searchContext.Map<Item>();
                searchContext.Map<Meta>();

                ISpatialReference sRefTarget = SpatialReference.FromID("epsg:4326");

                Console.WriteLine("Target Spatial Reference: " + sRefTarget.Name + " " + String.Join(" ", sRefTarget.Parameters));

                foreach (var datasetConfig in importConfig.Datasets)
                {
                    if (datasetConfig.FeatureClasses == null)
                        continue;

                    IDataset dataset = new PlugInManager().CreateInstance(datasetConfig.DatasetGuid) as IDataset;
                    if (dataset == null)
                        throw new ArgumentException("Can't load dataset with guid " + datasetConfig.DatasetGuid.ToString());

                    dataset.ConnectionString = datasetConfig.ConnectionString;
                    dataset.Open();

                    foreach (var featureClassConfig in datasetConfig.FeatureClasses)
                    {
                        var itemProto = featureClassConfig.IndexItemProto;
                        if (itemProto == null)
                            continue;

                        string metaId = Guid.NewGuid().ToString("N").ToLower();
                        string category = featureClassConfig.Category;
                        if(!String.IsNullOrWhiteSpace(category))
                        {
                            var meta = new Meta()
                            {
                                Id = metaId,
                                Category = category,
                                Descrption = featureClassConfig.Meta?.Descrption,
                                Sample = featureClassConfig?.Meta.Sample,
                                Service = featureClassConfig?.Meta.Service,
                                Query = featureClassConfig?.Meta.Query
                            };
                            searchContext.Index<Meta>(meta);
                        }

                        bool useGeometry = featureClassConfig.UserGeometry;

                        IDatasetElement dsElement = dataset[featureClassConfig.Name];
                        if (dsElement == null)
                            throw new ArgumentException("Unknown dataset element " + featureClassConfig.Name);
                        IFeatureClass fc = dsElement.Class as IFeatureClass;
                        if (fc == null)
                            throw new ArgumentException("Dataobject is not a featureclass " + featureClassConfig.Name);

                        Console.WriteLine("Index " + fc.Name);
                        Console.WriteLine("=====================================================================");

                        QueryFilter filter = new QueryFilter();
                        filter.SubFields = "*";

                        List<Item> items = new List<Item>();
                        int count = 0;

                        ISpatialReference sRef = fc.SpatialReference ?? SpatialReference.FromID("epsg:" + featureClassConfig.SRefId);
                        Console.WriteLine("Source Spatial Reference: " + sRef.Name + " " + String.Join(" ", sRef.Parameters));

                        using (GeometricTransformer transformer = new GeometricTransformer())
                        {
                            if (useGeometry)
                                transformer.SetSpatialReferences(sRef, sRefTarget);

                            IFeatureCursor cursor = (IFeatureCursor)fc.GetFeatures(filter);
                            IFeature feature;
                            while ((feature = cursor.NextFeature) != null)
                            {
                                var indexItem = ParseFeature(metaId, category, feature, itemProto, useGeometry, transformer, featureClassConfig);
                                items.Add(indexItem);
                                count++;

                                if (items.Count >= 500)
                                {
                                    searchContext.IndexMany<Item>(items.ToArray());
                                    items.Clear();

                                    Console.Write(count + "...");
                                }
                            }

                            if (items.Count > 0)
                            {
                                searchContext.IndexMany<Item>(items.ToArray());
                                Console.WriteLine(count + "...finish");
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static private Item ParseFeature(string metaId, string category, IFeature feature, Item proto, bool useGeometry, GeometricTransformer transformer, ImportConfig.FeatureClassDefinition featureClassDef)
        {
            var replace = featureClassDef.Replacements;

            var result = new Item();

            string oid = feature.OID.ToString();
            if (feature.OID <= 0 && !String.IsNullOrWhiteSpace(featureClassDef.ObjectOidField))
            {
                var idFieldValue = feature.FindField(featureClassDef.ObjectOidField);
                if (idFieldValue != null)
                    oid = idFieldValue.Value?.ToString();
            }

            result.Id = metaId + "." + oid;
            result.SuggestedText = ParseFeatureField(feature, proto.SuggestedText);
            result.SubText = ParseFeatureField(feature, proto.SubText);
            result.ThumbnailUrl = ParseFeatureField(feature, proto.ThumbnailUrl);
            result.Category = category;

            if(replace!=null)
            {
                foreach(var r in replace)
                {
                    result.SuggestedText = result.SuggestedText?.Replace(r.From, r.To);
                    result.SubText = result.SubText?.Replace(r.From, r.To);
                }
            }

            if(useGeometry == true && feature.Shape!=null)
            {
                IGeometry shape = feature.Shape;

                if(shape is IPoint)
                {
                    IPoint point = (IPoint)transformer.Transform2D(feature.Shape);
                    result.Geo = new Nest.GeoLocation(point.Y, point.X);
                }
                else if(shape is IPolyline)
                {
                    IEnvelope env = shape.Envelope;
                    if (env != null)
                    {
                        IPoint point = Algorithm.PolylinePoint((IPolyline)shape, ((IPolyline)shape).Length / 2.0);

                        if (point != null)
                        {
                            point = (IPoint)transformer.Transform2D(point);
                            result.Geo = new Nest.GeoLocation(point.Y, point.X);
                        }

                        result.BBox = GetBBox(env, transformer);
                    }
                }
                else if(shape is IPolygon)
                {
                    IEnvelope env = shape.Envelope;
                    if (env != null)
                    {
                        var points = Algorithm.OrderPoints(Algorithm.PolygonLabelPoints((IPolygon)shape), env.Center);
                        if (points != null && points.PointCount > 0)
                        {
                            IPoint point = (IPoint)transformer.Transform2D(points[0]);
                            result.Geo = new Nest.GeoLocation(point.Y, point.X);
                        }

                        result.BBox = GetBBox(env, transformer);
                    }
                }
            }

            return result;
        }

        static private string ParseFeatureField(IFeature feature, string pattern)
        {
            if (pattern == null || !pattern.Contains("{"))
                return pattern;

            string[] parameters = GetKeyParameters(pattern);
            foreach(string parameter in parameters)
            {
                var fieldValue = feature.FindField(parameter);
                if (fieldValue == null) continue;

                string val = fieldValue.Value != null ? fieldValue.Value.ToString() : String.Empty;
                pattern = pattern.Replace("{" + parameter + "}", val);
            }

            return pattern;
        }

        static private string[] GetKeyParameters(string pattern)
        {
            int pos1 = 0, pos2;
            pos1 = pattern.IndexOf("{");
            string parameters = "";

            while (pos1 != -1)
            {
                pos2 = pattern.IndexOf("}", pos1);
                if (pos2 == -1) break;
                if (parameters != "") parameters += ";";
                parameters += pattern.Substring(pos1 + 1, pos2 - pos1 - 1);
                pos1 = pattern.IndexOf("{", pos2);
            }
            if (parameters != "")
                return parameters.Split(';');
            else
                return new string[0];
        }

        static private string GetBBox(IEnvelope env, GeometricTransformer transformer)
        {
            try
            {
                env = ((IGeometry)transformer.Transform2D(env)).Envelope;

                return Math.Round(env.minx, 7).ToString(_nhi) + "," +
                       Math.Round(env.miny, 7).ToString(_nhi) + "," +
                       Math.Round(env.maxx, 7).ToString(_nhi) + "," +
                       Math.Round(env.maxy, 7).ToString(_nhi);
            }
            catch { return String.Empty; }
        }
    }
}
