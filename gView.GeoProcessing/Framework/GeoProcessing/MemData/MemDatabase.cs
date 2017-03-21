using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.FDB;
using gView.Framework.Data;

namespace gView.Framework.GeoProcessing.MemData
{
    public class MemDatabase : IFeatureDatabase
    {
        private List<MemDataset> _datasets = new List<MemDataset>();
        private string _errMsg = String.Empty;

        public MemDatabase()
        {
        }

        #region IFeatureDatabase Member

        public int CreateDataset(string name, gView.Framework.Geometry.ISpatialReference sRef)
        {
            if (this[name] != null)
            {
                _errMsg = "Dataset " + name + " already exists!";
                return -1;
            }
            MemDataset dataset = new MemDataset(this, name, sRef);
            _datasets.Add(dataset);

            return _datasets.IndexOf(dataset);
        }

        public int CreateFeatureClass(string dsname, string fcname, gView.Framework.Geometry.IGeometryDef geomDef, gView.Framework.Data.IFields fields)
        {
            MemDataset dataset = this[dsname] as MemDataset;
            if (dataset == null)
            {
                _errMsg = "Unknown dataset '" + dsname + "'";
                return -1;
            }
            if (dataset[fcname] != null)
            {
                _errMsg = "Feature class '" + fcname + "' already exists!";
                return -1;
            }

            MemFeatureClass featureClass = new MemFeatureClass(dataset, fcname, fields, geomDef);

            dataset.AddFeatureClass(featureClass);
            return 1;
        }

        public gView.Framework.Data.IFeatureDataset this[string name]
        {
            get
            {
                foreach (MemDataset dataset in _datasets)
                    if (dataset.DatasetName == name)
                        return dataset;

                return null;
            }
        }

        public bool DeleteDataset(string dsName)
        {
            MemDataset dataset = this[dsName] as MemDataset;
            if (dataset != null)
            {
                _datasets.Remove(dataset);
                return true;
            }

            return false;
        }

        public string[] DatasetNames
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public bool DeleteFeatureClass(string fcName)
        {
            return false;
        }

        public gView.Framework.Data.IFeatureCursor Query(gView.Framework.Data.IFeatureClass fc, gView.Framework.Data.IQueryFilter filter)
        {
            return fc.GetFeatures(filter);
        }

        public bool RenameDataset(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public bool RenameFeatureClass(string name, string newName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IDatabase Member

        public bool Create(string name)
        {
            return true;
        }

        public bool Open(string name)
        {
            return true;
        }

        public string lastErrorMsg
        {
            get { return _errMsg; }
        }

        public Exception lastException { get { return null; } }
        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            
        }

        #endregion

        #region IFeatureUpdater Member

        public bool Insert(gView.Framework.Data.IFeatureClass fClass, gView.Framework.Data.IFeature feature)
        {
            List<IFeature> features = new List<IFeature>();
            features.Add(feature);

            return Insert(fClass, features);
        }

        public bool Insert(gView.Framework.Data.IFeatureClass fClass, List<gView.Framework.Data.IFeature> features)
        {
            if (fClass is MemFeatureClass)
            {
                foreach (IFeature feature in features)
                {
                    ((MemFeatureClass)fClass).InsertFeature(feature);
                }
                return true;
            }
            return false;
        }

        public bool Update(gView.Framework.Data.IFeatureClass fClass, gView.Framework.Data.IFeature feature)
        {
            List<IFeature> features = new List<IFeature>();
            features.Add(feature);

            return Update(fClass, features);
        }

        public bool Update(gView.Framework.Data.IFeatureClass fClass, List<gView.Framework.Data.IFeature> features)
        {
            return false;
        }

        public bool Delete(gView.Framework.Data.IFeatureClass fClass, int oid)
        {
            return false;
        }

        public bool Delete(gView.Framework.Data.IFeatureClass fClass, string where)
        {
            return false;
        }

        public int SuggestedInsertFeatureCountPerTransaction
        {
            get { return 1000; }
        }

        #endregion

    }
}
