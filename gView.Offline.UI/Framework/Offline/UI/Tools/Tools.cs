using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system;
using System.Windows.Forms;
using gView.Framework.Globalisation;
using gView.Framework.Data;
using gView.Framework.Offline;

namespace gView.Framework.Offline.UI.Tools
{
    [RegisterPlugIn("B601BEDD-013E-48ed-8F94-77CCFBC6BDAB")]
    public class FeatureClassLabel : ITool, IToolItem
    {
        #region ITool Member

        public string Name
        {
            get { return "Featureclass"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            
        }

        public void OnEvent(object MapEvent)
        {
            
        }

        #endregion

        #region IToolItem Member

        public System.Windows.Forms.ToolStripItem ToolItem
        {
            get
            {
                return new ToolStripLabel(
                  LocalizedResources.GetResString("Text.ConflictManagement", "Conflict Management") + ":"
                  );
            }
        }

        #endregion
    }

    //[RegisterPlugIn("1D414834-0672-43d3-A615-74797F109CDE")]
    public class FeatureClassCombo : ITool, IToolItem
    {
        private IMapDocument _doc = null;
        private ToolStripComboBox _combo;

        public FeatureClassCombo()
        {
            _combo = new ToolStripComboBox();
            _combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        #region ITool Member

        public string Name
        {
            get { return "Featureclass Combo"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;

                _doc.AfterSetFocusMap += new AfterSetFocusMapEvent(_doc_AfterSetFocusMap);
                _doc.LayerAdded += new gView.Framework.Carto.LayerAddedEvent(_doc_LayerAdded);
                _doc.LayerRemoved += new gView.Framework.Carto.LayerRemovedEvent(_doc_LayerRemoved);
                _doc.MapAdded += new MapAddedEvent(_doc_MapAdded);
                
                Refresh();
            }
        }

        public void OnEvent(object MapEvent)
        {
            
        }

        #endregion

        #region IToolItem Member

        public ToolStripItem ToolItem
        {
            get { return _combo; }
        }

        #endregion

        #region MapDocumentEvents
        void _doc_MapAdded(gView.Framework.Carto.IMap map)
        {
            if (!_combo.Visible) return;
        }

        void _doc_LayerRemoved(gView.Framework.Carto.IMap sender, ILayer layer)
        {
            if (!_combo.Visible) return;

            if (layer != null && layer.Class is IFeatureClass &&
                ((IFeatureClass)layer.Class).Dataset != null &&
                ((IFeatureClass)layer.Class).Dataset.Database is IFeatureDatabaseReplication)
            {
                this.Refresh();
            }
        }

        void _doc_LayerAdded(gView.Framework.Carto.IMap sender, ILayer layer)
        {
            if (!_combo.Visible) return;

            if (layer != null && layer.Class is IFeatureClass &&
                ((IFeatureClass)layer.Class).Dataset != null &&
                ((IFeatureClass)layer.Class).Dataset.Database is IFeatureDatabaseReplication)
            {
                this.Refresh();
            }
        }

        void _doc_AfterSetFocusMap(gView.Framework.Carto.IMap map)
        {
            if (!_combo.Visible) return;

            this.Refresh();
        }
        #endregion

        internal void Refresh()
        {
            if (_doc == null || _doc.FocusMap == null) return;
            if (!_combo.Visible) return;

            _combo.Items.Clear();
            foreach (IDatasetElement element in _doc.FocusMap.MapElements)
            {
                if (element == null) continue;
                if (Replication.FeatureClassHasConflicts(element.Class as IFeatureClass))
                {
                    if (_doc.FocusMap.TOC != null && element is ILayer)
                    {
                        ITOCElement tocElement = _doc.FocusMap.TOC.GetTOCElement((ILayer)element);
                        if (tocElement != null)
                            _combo.Items.Add(new ComboItem(element.Class as IFeatureClass, tocElement));
                        else
                            _combo.Items.Add(new ComboItem(element.Class as IFeatureClass));
                    }
                    else
                    {
                        _combo.Items.Add(new ComboItem(element.Class as IFeatureClass));
                    }
                }
            }

            if (_combo.Items.Count > 0)
                _combo.SelectedIndex = 0;
        }

        internal class ComboItem
        {
            private IFeatureClass _fc = null;
            private ITOCElement _tocElement = null;

            public ComboItem(IFeatureClass fc)
            {
                _fc = fc;
            }
            public ComboItem(IFeatureClass fc, ITOCElement tocElement) : this(fc)
            {
                _tocElement = tocElement;
            }

            public IFeatureClass FeatureClass
            {
                get { return _fc; }
            }

            public override string ToString()
            {
                if (_tocElement != null)
                    return _tocElement.Name;

                if (_fc != null)
                    return _fc.Name;

                return "???";
            }
        }

        internal IFeatureClass SelectedFeatureClass
        {
            get
            {
                ComboItem item = _combo.SelectedItem as ComboItem;
                if (item == null) return null;

                return item.FeatureClass;
            }
        }
    }

    //[RegisterPlugIn("8C6A25BF-D60B-4cb5-B7BD-2D95F09D4B50")]
    public class RefreshFeatureClassCombo : ITool
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return "Refresh Featureclass Combo"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Offline.UI.Properties.Resources.Refresh; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null || !(_doc.Application is IMapApplication)) return;

            FeatureClassCombo combo = ((IMapApplication)_doc.Application).Tool(new Guid("1D414834-0672-43d3-A615-74797F109CDE")) as FeatureClassCombo;

            if (combo == null) return;
            combo.Refresh();
        }

        #endregion


    }

    [RegisterPlugIn("22C9DC8E-D346-472a-87F0-11A426F3E50D")]
    public class SolveConflicts : ITool
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return "Solve Conflicts"; }
        }

        public bool Enabled
        {
            get
            {
                return true;

                //if (_doc != null && _doc.Application is IMapApplication)
                //{
                //    FeatureClassCombo combo = ((IMapApplication)_doc.Application).Tool(new Guid("1D414834-0672-43d3-A615-74797F109CDE")) as FeatureClassCombo;
                //    if (combo == null) return false;

                //    return combo.SelectedFeatureClass != null;
                //}

                //return false;
            }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Offline.UI.Properties.Resources.arrow_join; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc != null && _doc.Application is IMapApplication)
            {
                //FeatureClassCombo combo = ((IMapApplication)_doc.Application).Tool(new Guid("1D414834-0672-43d3-A615-74797F109CDE")) as FeatureClassCombo;
                //if (combo == null || combo.SelectedFeatureClass == null) return;

                FormSolveConflicts dlg = new FormSolveConflicts(_doc);
                dlg.ShowDialog();
            }
        }

        #endregion
    }

    [RegisterPlugIn("E91D1205-4C1A-464d-86F9-EC4CAA926F8A")]
    public class Toolbar : IToolbar
    {
        List<Guid> _guids = new List<Guid>();

        public Toolbar()
        {
            _guids.Add(new Guid("B601BEDD-013E-48ed-8F94-77CCFBC6BDAB"));
            //_guids.Add(new Guid("1D414834-0672-43d3-A615-74797F109CDE"));
            //_guids.Add(new Guid("8C6A25BF-D60B-4cb5-B7BD-2D95F09D4B50"));
            //_guids.Add(new Guid());
            _guids.Add(new Guid("22C9DC8E-D346-472a-87F0-11A426F3E50D"));
        }
        #region IToolbar Member

        public string Name
        {
            get { return "Replication Conflicts"; }
        }

        public List<Guid> GUIDs
        {
            get
            {
                return _guids;
            }
            set
            {
                _guids = value;
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            
        }

        #endregion
    }
}
