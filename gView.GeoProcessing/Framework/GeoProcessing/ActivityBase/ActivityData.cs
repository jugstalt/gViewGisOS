using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;

namespace gView.Framework.GeoProcessing.ActivityBase
{
    public class ActivityData : IActivityData, IDirtyEvent
    {
        private string _displayName = "Data";
        private IDatasetElement _data = null;
        private QueryMethod _queryMethod = QueryMethod.All;
        private string _filterClause = String.Empty;

        public ActivityData(string displayName)
        {
            _displayName = displayName;
        }

        #region IActivityData Member

        public IDatasetElement Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                if (DirtyEvent != null)
                    DirtyEvent(this, new EventArgs());
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        virtual public bool ProcessAble(gView.Framework.Data.IDatasetElement data)
        {
            if (data == null || data.Class == null) return false;

            return true;
        }

        public QueryMethod QueryMethod
        {
            get
            {
                return _queryMethod;
            }
            set
            {
                _queryMethod = value;
            }
        }

        public string FilterClause
        {
            get
            {
                return _filterClause;
            }
            set
            {
                _filterClause = value;
            }
        }

        public IFeatureCursor GetFeatures(string appendToClause)
        {

            if (_data == null ||
                !(_data.Class is IFeatureClass)) return null;

            IFeatureClass fc = (IFeatureClass)_data.Class;
            switch (_queryMethod)
            {
                case QueryMethod.All:
                case QueryMethod.Filter:
                    QueryFilter filter = new QueryFilter();
                    filter.AddField("*");
                    filter.WhereClause = (_queryMethod == QueryMethod.Filter) ? _filterClause : String.Empty;
                    if (!String.IsNullOrEmpty(appendToClause))
                    {
                        if (String.IsNullOrEmpty(filter.WhereClause))
                            filter.WhereClause = appendToClause;
                        else
                            filter.WhereClause += " AND " + appendToClause;
                    }
                    return fc.GetFeatures(filter);
                case QueryMethod.Selected:
                    IFeatureSelection featSelection = _data as IFeatureSelection;
                    if (featSelection == null) 
                        throw new ArgumentException("Data is not as FeatureSelection...");

                    if (featSelection.SelectionSet is IIDSelectionSet)
                    {
                        return new SelectionCursor(_data.Class as IFeatureClass, featSelection.SelectionSet as IIDSelectionSet, appendToClause);
                    }
                    else if (featSelection.SelectionSet is IGlobalIDSelectionSet)
                    {
                        return new GlobalSelectionCursor(_data.Class as IFeatureClass, featSelection.SelectionSet as IGlobalIDSelectionSet, appendToClause);
                    }
                    break;
            }

            return null;
        }

        #endregion

        #region IDirtyEvent Member

        public event EventHandler DirtyEvent = null;

        #endregion

        private class SelectionCursor : IFeatureCursor
        {
            private IFeatureClass _fc;
            private IIDSelectionSet _selSet;
            private int _pos;
            private string _appendToClause = String.Empty;

            public SelectionCursor(IFeatureClass fc, IIDSelectionSet selSet)
            {
                _pos = 0;
                _fc = fc;
                _selSet = selSet;
            }
            public SelectionCursor(IFeatureClass fc, IIDSelectionSet selSet, string appendToClause)
                : this(fc, selSet)
            {
                _appendToClause = appendToClause;
            }

            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get {
                    if (_fc == null || _selSet == null) return null;

                    if (_pos >= _selSet.IDs.Count) return null;

                    QueryFilter filter = new QueryFilter();
                    filter.WhereClause = _fc.IDFieldName + "=" + _selSet.IDs[_pos++];
                    filter.AddField("*");

                    if (!String.IsNullOrEmpty(_appendToClause))
                        filter.WhereClause += " AND " + _appendToClause;

                    using (IFeatureCursor cursor = _fc.GetFeatures(filter))
                    {
                        IFeature feature = cursor.NextFeature;
                        if (feature == null) return NextFeature;

                        return feature;
                    }
                }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {
                
            }

            #endregion
        }

        private class GlobalSelectionCursor : IFeatureCursor
        {
            private IFeatureClass _fc;
            private IGlobalIDSelectionSet _selSet;
            private int _pos;
            private string _appendToClause = String.Empty;

            public GlobalSelectionCursor(IFeatureClass fc, IGlobalIDSelectionSet selSet)
            {
                _pos = 0;
                _fc = fc;
                _selSet = selSet;
            }
            public GlobalSelectionCursor(IFeatureClass fc, IGlobalIDSelectionSet selSet, string appendToClause)
                : this(fc, selSet)
            {
                _appendToClause = appendToClause;
            }

            #region IFeatureCursor Member

            public IFeature NextFeature
            {
                get
                {
                    if (_fc == null || _selSet == null) return null;

                    if (_pos >= _selSet.IDs.Count) return null;

                    QueryFilter filter = new QueryFilter();
                    filter.WhereClause = _fc.IDFieldName + "=" + _selSet.IDs[_pos++];
                    filter.AddField("*");

                    if (!String.IsNullOrEmpty(_appendToClause))
                        filter.WhereClause += " AND " + _appendToClause;

                    using (IFeatureCursor cursor = _fc.GetFeatures(filter))
                    {
                        IFeature feature = cursor.NextFeature;
                        if (feature == null) return NextFeature;

                        return feature;
                    }
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

    public class ActivityFeatureData : ActivityData
    {
        private geometryType _geomType = geometryType.Unknown;

        public ActivityFeatureData(string displayName) 
            : base(displayName)
        {
        }
        public ActivityFeatureData(string displayName, geometryType geomType)
            : base(displayName)
        {
            _geomType = geomType;
        }

        public override bool ProcessAble(IDatasetElement data)
        {
            if (data == null || data.Class == null) return false;

            if (data.Class is IFeatureClass)
            {
                if (_geomType == geometryType.Unknown) return true;

                return ((IFeatureClass)data.Class).GeometryType == _geomType;
            }
            return false;
        }
    }

    public class ActivityRasterData : ActivityData
    {
        public ActivityRasterData(string displayName)
            : base(displayName)
        {
        }

        public override bool ProcessAble(IDatasetElement data)
        {
            if (data == null || data.Class == null) return false;

            return data.Class is IRasterClass;
        }
    }

    public class TargetDatasetElement : IDatasetElement
    {
        private TargetClass _class = null;

        public TargetDatasetElement(string name, IDataset dataset)
        {
            if (!String.IsNullOrEmpty(name) || dataset != null)
                _class = new TargetClass(name, dataset);
        }

        #region IDatasetElement Member

        public string Title
        {
            get
            {
                if (_class != null)
                    return _class.Name;

                return String.Empty;
            }
            set
            {
                if (_class != null) _class.Name = value;
            }
        }

        public IClass Class
        {
            get { return _class; }
        }

        private int _datasetID = -1;
        public int DatasetID
        {
            get
            {
                return _datasetID;
            }
            set
            {
                _datasetID = value;
            }
        }

        public event PropertyChangedHandler PropertyChanged = null;

        public void FirePropertyChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged();
        }

        #endregion

        #region IID Member
        private int _iid = -1;
        public int ID
        {
            get
            {
                return _iid;
            }
            set
            {
                _iid = value;
            }
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
            get { return new List<gView.Framework.IO.IMetadataProvider>(); }
        }

        #endregion

        public class TargetClass : IClass
        {
            private string _name;
            private IDataset _dataset;

            public TargetClass(string name, IDataset dataset)
            {
                _name = name;
                _dataset = dataset;
            }
            #region IClass Member

            public string Name
            {
                get { return _name; }
                set { if(!String.IsNullOrEmpty(value)) _name = value; }
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

        #region IStringID Member

        public string SID
        {
            get
            {
                return this.ID.ToString();
            }
            set
            {
                
            }
        }

        public bool HasSID
        {
            get { return false; }
        }

        #endregion
    }
}
