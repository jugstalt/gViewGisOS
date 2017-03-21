using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;

namespace gView.Framework.GeoProcessing.MemData
{
    class MemFeatureCursor : IFeatureCursor
    {
        private int _pos = 0;
        private List<IFeature> _features;

        public MemFeatureCursor(List<IFeature> features, IQueryFilter filter)
        {
            _features = features;
        }
        #region IFeatureCursor Member

        public IFeature NextFeature
        {
            get
            {
                if (_features == null || _features.Count <= _pos) return null;

                return _features[_pos++];
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
        }

        #endregion
    }
}
