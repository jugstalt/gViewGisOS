using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Db;
using System.Data;
using gView.Framework.IO;
using System.Reflection;

namespace gView.DataSources.EventTable
{
    [gView.Framework.system.RegisterPlugIn("8EF4C53A-EE74-4c4a-B733-95D7CD23BE11")]
    public class Dataset : DatasetMetadata, IFeatureDataset, IPersistable, gView.Framework.UI.IConnectionStringDialog
    {
        private EventTableConnection _etcon = null;
        private DatasetState _state = DatasetState.unknown;
        private String _errMsg = String.Empty;
        private IFeatureClass _fc = null;

        #region IFeatureDataset Member

        public IEnvelope Envelope
        {
            get
            {
                if (_fc != null)
                    return _fc.Envelope;

                return null;
            }
        }

        public ISpatialReference SpatialReference
        {
            get
            {
                if (_etcon != null)
                    return _etcon.SpatialReference;

                return null;
            }
            set
            {
                if (_etcon != null)
                    _etcon.SpatialReference = value;
            }
        }

        #endregion

        #region IDataset Member

        public string ConnectionString
        {
            get
            {
                if (_etcon != null)
                    return _etcon.ToXmlString();

                return String.Empty;
            }
            set
            {
                if (value == String.Empty)
                    _etcon = null;
                else
                {
                    _etcon = new EventTableConnection();
                    _etcon.FromXmlString(value);
                    _state = DatasetState.unknown;
                }
            }
        }

        public string DatasetGroupName
        {
            get { return "Event Table"; }
        }

        public string DatasetName
        {
            get
            {
                if (_fc != null)
                    return _fc.Name;

                return "???";
            }
        }

        public string ProviderName
        {
            get { return "gView Event Table"; }
        }

        public DatasetState State
        {
            get { return _state;  }
        }

        public bool Open()
        {
            RefreshClasses();
            _state = DatasetState.opened;
            return true;
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public List<IDatasetElement> Elements
        {
            get {
                List<IDatasetElement> elements = new List<IDatasetElement>();
                if (_fc != null)
                {
                    elements.Add(LayerFactory.Create(_fc));
                }
                return elements; 
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

        public gView.Framework.FDB.IDatabase Database
        {
            get { return null; }
        }

        public IDatasetElement this[string title]
        {
            get
            {
                if (_fc != null)
                {
                    return LayerFactory.Create(_fc);
                }
                return null;
            }
        }

        public void RefreshClasses()
        {
            if (_etcon != null)
            {
                _fc = new FeatureClass(this, _etcon);
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            this.ConnectionString = (string)stream.Load("connectionstring", String.Empty);
            this.Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.SaveEncrypted("connectionstring", this.ConnectionString);
        }

        #endregion

        #region IConnectionStringDialog Member

        public string ShowConnectionStringDialog(string initConnectionString)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.DataSources.EventTable.UI.dll");

            gView.Framework.UI.IConnectionStringDialog p = uiAssembly.CreateInstance("gView.Datasources.EventTable.UI.FormEventTableConnection") as gView.Framework.UI.IConnectionStringDialog;
            if (p != null)
                return p.ShowConnectionStringDialog(initConnectionString);

            return null;
        }

        #endregion
    }
}
