using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.Reflection;
using gView.Framework.UI;

namespace gView.Interoperability.AGS.Dataset
{
    [gView.Framework.system.RegisterPlugIn("39005C1B-6438-4643-A235-50F3BE9F6B3C")]
    class AGSServiceDataset : IServiceableDataset
    {
        AGSDataset _dataset = null;

        #region IServiceableDataset Member

        public string Name
        {
            get { return "ArcGIS Server Service Dataset"; }
        }

        public string Provider
        {
            get { return "gView GIS"; }
        }

        public List<IDataset> Datasets
        {
            get
            {
                List<IDataset> datasets = new List<IDataset>();
                datasets.Add(_dataset);
                return datasets;
            }
        }

        public bool GenerateNew()
        {
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Interoperability.AGS.UI.dll");

                IModalDialog dlg = uiAssembly.CreateInstance("gView.Interoperability.AGS.UI.FormSelectService") as IModalDialog;
                if (dlg is IConnectionString)
                {
                    if (dlg.OpenModal())
                    {
                        string connectionString = ((IConnectionString)dlg).ConnectionString;

                        _dataset = new AGSDataset();
                        _dataset.ConnectionString = connectionString;
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _dataset = null;
            string connectionString = (string)stream.Load("ConnectionString", String.Empty);

            if (connectionString != String.Empty)
            {
                _dataset = new AGSDataset();
                _dataset.ConnectionString = connectionString;
            }
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            if (_dataset == null) return;

            stream.Save("ConnectionString", _dataset.ConnectionString);
        }

        #endregion
    }
}
