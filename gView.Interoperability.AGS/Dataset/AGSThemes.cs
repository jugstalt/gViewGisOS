using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Interoperability.AGS.Proxy;
using gView.Framework.system;
using gView.Framework.Geometry;
using gView.Interoperability.AGS.Helper;

namespace gView.Interoperability.AGS.Dataset
{
    internal class AGSThemeFeatureClass : IWebFeatureClass
    {
        private string _name, _id, _idFieldName = String.Empty, _shapeFieldName = String.Empty;
        private IEnvelope _envelope;
        private ISpatialReference _sRef;
        private AGSDataset _dataset;
        private geometryType _geomType;
        private gView.Framework.Data.Fields _fields = new gView.Framework.Data.Fields();

        public AGSThemeFeatureClass(AGSDataset dataset, MapLayerInfo layerInfo, geometryType geomType)
        {
            if (dataset is IFeatureDataset)
            {
                _envelope = ((IFeatureDataset)dataset).Envelope;
                _sRef = ((IFeatureDataset)dataset).SpatialReference;
            }
            _dataset = dataset;

            _name = layerInfo.Name;
            _id = layerInfo.LayerID.ToString();
            _geomType = geomType;

            #region Fields
            foreach (Proxy.Field fieldInfo in layerInfo.Fields.FieldArray)
            {
                gView.Framework.Data.Field field = new gView.Framework.Data.Field(fieldInfo.Name);
                switch (fieldInfo.Type)
                {
                    case esriFieldType.esriFieldTypeBlob:
                        field.type = FieldType.binary;
                        break;
                    case esriFieldType.esriFieldTypeDate:
                        field.type = FieldType.Date;
                        break;
                    case esriFieldType.esriFieldTypeDouble:
                        field.type = FieldType.Double;
                        break;
                    case esriFieldType.esriFieldTypeGeometry:
                        _shapeFieldName = field.name;
                        field.type = FieldType.Shape;
                        break;
                    case esriFieldType.esriFieldTypeGlobalID:
                        field.type = FieldType.unknown;
                        break;
                    case esriFieldType.esriFieldTypeGUID:
                        field.type = FieldType.guid;
                        break;
                    case esriFieldType.esriFieldTypeInteger:
                        field.type = FieldType.integer;
                        break;
                    case esriFieldType.esriFieldTypeOID:
                        _idFieldName = field.name;
                        field.type = FieldType.ID;
                        break;
                    case esriFieldType.esriFieldTypeRaster:
                        field.type = FieldType.binary;
                        break;
                    case esriFieldType.esriFieldTypeSingle:
                        field.type = FieldType.Float;
                        break;
                    case esriFieldType.esriFieldTypeSmallInteger:
                        field.type = FieldType.smallinteger;
                        break;
                    case esriFieldType.esriFieldTypeString:
                    case esriFieldType.esriFieldTypeXML:
                        field.type = FieldType.String;
                        break;
                }
                _fields.Add(field);
            }
            #endregion
        }

        #region IWebFeatureClass Member

        public string ID
        {
            get { return _id; }
        }

        #endregion

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return _shapeFieldName; }
        }

        public IEnvelope Envelope
        {
            get { return _envelope; }
        }

        public int CountFeatures
        {
            get { return 0; }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {

            if (_dataset == null || _dataset._mapServer == null || _dataset._mapDescription == null)
                return null;

            gView.Interoperability.AGS.Proxy.QueryFilter qFilter = ArcServerHelper.QueryFilter(filter, this);
            try
            {
                QueryResultOptions options = new QueryResultOptions();

                RecordSet rSet = _dataset._mapServer.QueryFeatureData(_dataset._mapDescription.Name, int.Parse(_id), qFilter);
                return new FeatureCursor(rSet);
            }
            catch (Exception ex)
            {
                //if (ex.Message == "The requested capability is not supported." && filter is ISpatialFilter && String.IsNullOrEmpty(filter.WhereClause))
                //{
                //    try
                //    {
                //        MapServerIdentifyResult[] res =
                //        _dataset._mapServer.Identify(_dataset._mapDescription,
                //            null,
                //            ((gView.Interoperability.AGS.Proxy.SpatialFilter)qFilter).FilterGeometry,
                //            0,
                //            esriIdentifyOption.esriIdentifyAllLayers,
                //            new int[] { int.Parse(_id) });
                //    }
                //    catch (Exception e)
                //    {

                //    }
                //}
                throw new gView.Framework.UI.UIException("Can't query features for '" + this.Name + "'!", ex);
            }
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            return GetFeatures(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            filter.SubFields = this.IDFieldName;

            IFeatureCursor cursor = (IFeatureCursor)this.Search(filter);
            IFeature feat;

            GlobalIDSelectionSet selSet = new GlobalIDSelectionSet();
            while ((feat = cursor.NextFeature) != null)
            {
                if (feat is IGlobalFeature)
                    selSet.AddID(((IGlobalFeature)feat).GlobalOID);
                else
                    selSet.AddID(feat.OID);
            }
            return selSet;
        }

        public IFields Fields
        {
            get { return _fields; }
        }

        public IField FindField(string name)
        {
            return _fields.FindField(name);
        }

        public string IDFieldName
        {
            get { return _idFieldName; }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IGeometryDef Member

        public bool HasZ
        {
            get { return false; }
        }

        public bool HasM
        {
            get { return false; }
        }

        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
        }

        public geometryType GeometryType
        {
            get { return _geomType; }
        }

        #endregion

        #region Helper Classes

        private class FeatureCursor : IFeatureCursor
        {
            private RecordSet _rSet;
            private int _pos = 0;
            gView.Interoperability.AGS.Proxy.Field[] _fields = null;

            public FeatureCursor(RecordSet rSet)
            {
                _rSet = rSet;
                if (_rSet != null)
                    _fields = rSet.Fields.FieldArray;
            }

            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get
                {
                    if (_rSet == null || _pos >= _rSet.Records.Length)
                        return null;

                    Feature feature = new Feature();
                    Record rec = _rSet.Records[_pos++];

                    for (int f = 0; f < _fields.Length; f++)
                    {
                        if (_fields[f].Type == esriFieldType.esriFieldTypeGeometry)
                            feature.Shape = GeometryHelper.ToGView(rec.Values[f] as Proxy.Geometry);
                        else if (_fields[f].Type == esriFieldType.esriFieldTypeOID)
                        {
                            feature.OID = (int)rec.Values[f];
                            feature.Fields.Add(new FieldValue(_fields[f].Name, rec.Values[f]));
                        }
                        else
                        {
                            feature.Fields.Add(new FieldValue(_fields[f].Name, rec.Values[f]));
                        }
                    }

                    return feature;
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {

            }

            #endregion
        }

        #endregion
    }

    internal class AGSThemeClass : IClass
    {
        private IDataset _dataset;
        private MapLayerInfo _layerInfo;

        public AGSThemeClass(IDataset dataset, MapLayerInfo layerInfo)
        {
            _dataset = dataset;
            _layerInfo = layerInfo;
        }

        #region IClass Member

        public string Name
        {
            get
            {
                if (_layerInfo != null)
                    return _layerInfo.Name;
                return String.Empty;
            }
        }

        public string Aliasname
        {
            get
            {
                if (_layerInfo != null)
                    return _layerInfo.Name;
                return String.Empty;
            }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion
    }

    internal class AGSThemeRasterClass : IWebRasterClass
    {
        private IDataset _dataset;
        private string _name, _id;

        public AGSThemeRasterClass(IDataset dataset, MapLayerInfo layerInfo)
        {
            _dataset = dataset;

            _name = layerInfo.Name;
            _id = layerInfo.LayerID.ToString();
        }

        #region IWebRasterClass Member

        public string ID
        {
            get { return _id; }
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion
    }
}
