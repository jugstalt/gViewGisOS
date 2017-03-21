using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.Offline;
using gView.Framework.Geometry;
using gView.Framework.Carto;
using gView.Framework.Globalisation;

namespace gView.Framework.Offline.UI.Tools
{
    public partial class FormSolveConflicts : Form
    {
        private IMapDocument _doc;
        private IFeatureClass _fc;
        private TreeNode _rootNode;

        public FormSolveConflicts(IMapDocument doc)
        {
            InitializeComponent();

            _doc = doc;

            _rootNode = new TreeNode(LocalizedResources.GetResString("String.Conflicts", "Conflicts"));
            conflictControl1.MapDocument = _doc;
        }

        #region Form Events
        private void FormSolveConflicts_Load(object sender, EventArgs e)
        {
            if (_doc == null || _doc.FocusMap == null)
            {
                this.Close();
                return;
            }

            tvConflicts.Nodes.Add(_rootNode);

            foreach (IDatasetElement element in _doc.FocusMap.MapElements)
            {
                if (element == null || !(element.Class is IFeatureClass)) continue;

                List<Guid> guids = Replication.FeatureClassConflictsParentGuids(element.Class as IFeatureClass);
                if (guids == null || guids.Count == 0) continue;

                string title = element.Title;
                if (_doc.FocusMap.TOC != null && element is ILayer)
                {
                    ITOCElement tocElement = _doc.FocusMap.TOC.GetTOCElement((ILayer)element);
                    if (tocElement != null) title = tocElement.Name;
                }
                FeatureClassNode fcNode = new FeatureClassNode(element.Class as IFeatureClass, title);
                _rootNode.Nodes.Add(fcNode);

                foreach (Guid guid in guids)
                {
                    fcNode.Nodes.Add(new GuidTreeNode(guid));
                }
                fcNode.Expand();
            }
            
            _rootNode.ExpandAll();

            if (_rootNode.Nodes.Count == 0)
            {
                MessageBox.Show("No conflicts in current map detected.");
                this.Close();
            }
        }
        #endregion

        #region ItemClasses
        private class FeatureClassNode : TreeNode
        {
            private IFeatureClass _fc;

            public FeatureClassNode(IFeatureClass fc, string name)
            {
                this.Text = name;
                _fc = fc;
            }

            public IFeatureClass FeatureClass
            {
                get { return _fc; }
            }
        }
        private class GuidTreeNode : TreeNode
        {
            private Guid _guid;
            public GuidTreeNode(Guid guid)
            {
                base.Text = guid.ToString();
                _guid = guid;
            }

            public Guid Guid
            {
                get { return _guid; }
            }

            public IFeatureClass FeatureClass
            {
                get
                {
                    if (this.Parent is FeatureClassNode)
                    {
                        return ((FeatureClassNode)this.Parent).FeatureClass;
                    }
                    return null;
                }
            }
        }
        #endregion

        #region Tree Events
        private void tvConflicts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvConflicts.SelectedNode is GuidTreeNode)
            {
                GuidTreeNode node = (GuidTreeNode)tvConflicts.SelectedNode;

                _fc = node.FeatureClass;
                if (_fc == null) return;

                Replication.Conflict conflict = Replication.FeatureClassConflict(_fc, node.Guid);
                if (conflict == null) return;

                IEnvelope env = (conflict.Feature != null && conflict.Feature.Shape != null) ? conflict.Feature.Shape.Envelope.Clone() as IEnvelope : null;

                foreach (Replication.ConflictFeature cFeature in conflict.ConflictFeatures)
                {
                    if (cFeature.Feature == null || cFeature.Feature.Shape == null) continue;

                    if (env == null)
                    {
                        env = cFeature.Feature.Shape.Envelope.Clone() as IEnvelope;
                    }
                    else
                    {
                        env.Union(cFeature.Feature.Shape.Envelope);
                    }
                }

                if (_doc != null && _doc.FocusMap != null &&
                    _doc.FocusMap.Display != null &&
                    _doc.Application is IMapApplication &&
                    env != null)
                {
                    Envelope e2 = new Envelope(env);
                    e2.Raise(150.0);
                    _doc.FocusMap.Display.ZoomTo(e2);
                    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                }

                conflictControl1.Conflict = conflict;

                btnSolve.Enabled = btnRemove.Enabled = btnHighlight.Enabled = true;
            }
            else
            {
                conflictControl1.Conflict = null;
                btnSolve.Enabled = btnRemove.Enabled = btnHighlight.Enabled = false;
            }
        }
        #endregion

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Replication.Conflict conflict = conflictControl1.Conflict;
            if (conflict == null) return;

            string errMsg;
            if (!conflict.RemoveConflict(out errMsg))
            {
                MessageBox.Show("Error: " + errMsg);
            }
            else
            {
                TreeNode selNode = tvConflicts.SelectedNode;
                tvConflicts.SelectedNode = (selNode.NextNode != null) ? selNode.NextNode : _rootNode;

                if (selNode.Parent.Nodes.Count == 1)
                {
                    _rootNode.Nodes.Remove(selNode.Parent);
                }
                else
                {
                    selNode.Parent.Nodes.Remove(selNode);
                }

                if (_doc != null && _doc.Application is IMapApplication)
                {
                    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                }
            }
        }

        private void btnSolve_Click(object sender, EventArgs e)
        {
            Replication.Conflict conflict = conflictControl1.Conflict;
            if (conflict == null) return;

            string errMsg;
            if (!conflict.SolveConflict(out errMsg))
            {
                MessageBox.Show("Error: " + errMsg);
            }
            else
            {
                TreeNode selNode = tvConflicts.SelectedNode;
                tvConflicts.SelectedNode = (selNode.NextNode != null) ? selNode.NextNode : _rootNode;

                if (selNode.Parent.Nodes.Count == 1)
                {
                    _rootNode.Nodes.Remove(selNode.Parent);
                }
                else
                {
                    selNode.Parent.Nodes.Remove(selNode);
                }
                
                if (_doc != null && _doc.Application is IMapApplication)
                {
                    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                }
            }
        }

        private void btnHighlight_Click(object sender, EventArgs e)
        {
            if (conflictControl1.Conflict == null || conflictControl1.Conflict.FeatureClass == null ||
                _doc == null || _doc.FocusMap == null)
                return;

            try
            {
                IGeometry geometry = null;
                bool found = false;
                foreach (Replication.Conflict.FieldConflict cField in conflictControl1.Conflict.ConflictFields)
                {
                    if (cField.FieldName == conflictControl1.Conflict.FeatureClass.ShapeFieldName)
                    {
                        geometry = conflictControl1.Conflict.SolvedFeature.Shape;
                        found = true;
                        break;
                    }
                }
                if (!found && conflictControl1.Conflict.Feature != null)
                {
                    geometry = conflictControl1.Conflict.Feature.Shape;
                }
                if (geometry == null) return;

                _doc.FocusMap.HighlightGeometry(geometry, 1000);
            }
            catch { }
        }
    }
}