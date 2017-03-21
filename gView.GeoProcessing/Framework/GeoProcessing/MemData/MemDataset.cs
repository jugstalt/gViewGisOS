using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;

namespace gView.Framework.GeoProcessing.MemData
{
    class MemDataset : IFeatureDataset
    {
        private MemDatabase _database = null;
        private DatasetState _state = DatasetState.unknown;
        private string _name=String.Empty, _errMsg = String.Empty;
        private ISpatialReference _sRef = null;
        private List<IDatasetElement> _elements = new List<IDatasetElement>();

        public MemDataset(MemDatabase database, string name, ISpatialReference sRef)
        {
            _database = database;
            _name = name;
            _sRef = sRef;
        }

        #region IFeatureDataset Member

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get { return new Envelope(); }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        #endregion

        #region IDataset Member

        public string ConnectionString
        {
            get
            {
                return String.Empty;
            }
            set
            {
                
            }
        }

        public string DatasetGroupName
        {
            get { return "Memory Dataset"; }
        }

        public string DatasetName
        {
            get { return _name; }
        }

        public string ProviderName
        {
            get { return "gViewGis"; }
        }

        public DatasetState State
        {
            get { return _state; }
        }

        public bool Open()
        {
            _state = DatasetState.opened;
            return true;
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public List<IDatasetElement> Elements
        {
            get { return _elements; }
        }

        public string Query_FieldPrefix
        {
            get { return String.Empty; }
        }

        public string Query_FieldPostfix
        {
            get { return String.Empty; }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get { return _database; }
        }

        public IDatasetElement this[string title]
        {
            get
            {
                foreach (IDatasetElement element in _elements)
                {
                    if (element.Title == title)
                        return element;
                }

                return null;
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

        #region IMetadata Member

        public void ReadMetadata(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        public void WriteMetadata(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        public gView.Framework.IO.IMetadataProvider MetadataProvider(Guid guid)
        {
            return null;
        }

        public List<gView.Framework.IO.IMetadataProvider> Providers
        {
            get { return new List<IMetadataProvider>(); }
        }

        #endregion

        public void AddFeatureClass(MemFeatureClass fc)
        {
            DatasetElement element = new DatasetElement(fc);
            element.Title = fc.Name;

            _elements.Add(element);
        }
    }
}
