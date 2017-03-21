using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Framework.GeoProcessing.MemData
{
    class MemFeatureClass : IFeatureClass
    {
        private MemDataset _dataset;
        private string _name;
        private List<IFeature> _features = new List<IFeature>();
        private IFields _fields;
        private IGeometryDef _geometryDef;

        public MemFeatureClass(MemDataset dataset, string name, IFields fields, IGeometryDef geometryDef)
        {
            _dataset = dataset;
            _name = name;
            _fields = fields;
            _geometryDef = geometryDef;
        }
        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get { return "Shape"; }
        }

        public gView.Framework.Geometry.IEnvelope Envelope
        {
            get
            {
                gView.Framework.Geometry.Envelope env = null;

                foreach (IFeature feature in _features)
                {
                    if (feature == null || feature.Shape == null) continue;
                    if (env == null)
                        env = new gView.Framework.Geometry.Envelope(feature.Shape.Envelope);
                    else
                        env.Union(feature.Shape.Envelope);
                }
                return env;
            }
        }

        public int CountFeatures
        {
            get { return _features.Count; }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            return new MemFeatureCursor(_features, filter);
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            return new MemFeatureCursor(_features, filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            return null;
        }

        public IFields Fields
        {
            get { return _fields; }
        }

        public IField FindField(string name)
        {
            if (_fields!=null)
            {
                foreach (IField field in _fields)
                    if (field.name == name)
                        return field;
            }

            return null;
        }

        public string IDFieldName
        {
            get { return "Id"; }
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
            get { return _geometryDef.HasZ; }
        }

        public bool HasM
        {
            get { return _geometryDef.HasM; }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get
            {
                if (_dataset != null) return _dataset.SpatialReference;
                return null;
            }
        }

        public gView.Framework.Geometry.geometryType GeometryType
        {
            get { return _geometryDef.GeometryType; }
        }

        #endregion

        public void InsertFeature(IFeature feature)
        {
            if (feature == null) return;

            _features.Add(feature);
        }
    }
}
