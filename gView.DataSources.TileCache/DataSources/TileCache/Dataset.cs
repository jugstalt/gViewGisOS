using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Tiling;

namespace gView.DataSources.TileCache
{
    [gView.Framework.system.RegisterPlugIn("fce6b9a8-c0b1-4600-bdd5-3eb10fe6b29d")]
    public class Dataset : DatasetMetadata, IDataset, IPersistable, IDataCopyright
    {
        private string _dsName = String.Empty;
        private DatasetState _state = DatasetState.unknown;
        private string _connectionString, _lastErrMsg=String.Empty, _copyright;
        private ParentRasterClass _class = null;
        private IDatasetElement _dsElement = null;

        public Dataset()
        {
            
        }

        #region IDataset Member

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;

                _dsName = ConfigTextStream.ExtractValue(value, "name");
                string extent = ConfigTextStream.ExtractValue(value, "extent");
                if (!String.IsNullOrEmpty(extent))
                {
                    string[] bbox = extent.Split(',');
                    this.Extent = new Envelope(double.Parse(bbox[0], Numbers.Nhi),
                                               double.Parse(bbox[1], Numbers.Nhi),
                                               double.Parse(bbox[2], Numbers.Nhi),
                                               double.Parse(bbox[3], Numbers.Nhi));
                }
                string origin = ConfigTextStream.ExtractValue(value, "origin");
                if (!String.IsNullOrEmpty(origin))
                    this.Origin = (GridOrientation)int.Parse(origin);
                string sref64 = ConfigTextStream.ExtractValue(value, "sref64");
                if (!String.IsNullOrEmpty(sref64))
                {
                    this.SpatialReference = new SpatialReference();
                    this.SpatialReference.FromBase64String(sref64);
                }
                string scales = ConfigTextStream.ExtractValue(value, "scales");
                List<double> listScales = new List<double>();
                foreach (string scale in scales.Split(','))
                {
                    if (String.IsNullOrEmpty(scale)) continue;
                    listScales.Add(double.Parse(scale, Numbers.Nhi));
                }
                this.Scales = listScales.ToArray();

                string tileWidth = ConfigTextStream.ExtractValue(value, "tilewidth");
                if (!String.IsNullOrEmpty(tileWidth))
                    TileWidth=int.Parse(tileWidth);
                string tileHeight = ConfigTextStream.ExtractValue(value, "tileheight");
                if (!String.IsNullOrEmpty(tileHeight))
                    TileHeight = int.Parse(tileHeight);
                TileUrl = ConfigTextStream.ExtractValue(value, "tileurl");

                _copyright = ConfigTextStream.ExtractValue(value, "copyright");
            }
        }

        public string DatasetGroupName
        {
            get { return "Tile Cache"; }
        }

        public string DatasetName
        {
            get { return _dsName; }
        }

        public string ProviderName
        {
            get { return "gView.GIS"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public bool Open()
        {
            _class = new ParentRasterClass(this);
            _dsElement = new DatasetElement(_class);
            _dsElement.Title = "Cache";

            _state = DatasetState.opened;
            return true;
        }

        public string lastErrorMsg
        {
            get { return _lastErrMsg; }
        }

        public List<IDatasetElement> Elements
        {
            get 
            {
                if (_dsElement == null)
                    return null;

                return new List<IDatasetElement>() { _dsElement }; 
            }
        }

        public string Query_FieldPrefix
        {
            get { return String.Empty; }
        }

        public string Query_FieldPostfix
        {
            get { return String.Empty; }
        }

        public Framework.FDB.IDatabase Database
        {
            get { return null; }
        }

        public IDatasetElement this[string title]
        {
            get
            {
                return _dsElement;
            }
        }

        public void RefreshClasses()
        {
            
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            
        }

        #endregion

        #region Properties

        public IEnvelope Extent { get; private set; }

        public GridOrientation Origin { get; private set; }

        public ISpatialReference SpatialReference { get; private set; }

        public double[] Scales { get; private set; }

        public int TileWidth { get; private set; }

        public int TileHeight { get; private set; }

        public string TileUrl { get; private set; }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            this.ConnectionString = (string)stream.Load("connectionstring", String.Empty);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("connectionstring", _connectionString);
        }

        #endregion

        #region IDataCopyright Member

        public bool HasDataCopyright
        {
            get { return !String.IsNullOrEmpty(_copyright); }
        }

        public string DataCopyrightText
        {
            get { return _copyright; }
        }

        #endregion
    }
}
