using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.GeoProcessing.MemData;

namespace gView.Framework.GeoProcessing.UI
{
    internal partial class FormGeoProcessor : IDE.Forms.WizardDialog
    {
        private List<IDatasetElement> _elements = null;

        public FormGeoProcessor()
        {
            InitializeComponent();

            PlugInManager compMan = new PlugInManager();
            foreach (XmlNode activityNode in compMan.GetPluginNodes(Plugins.Type.IActivity))
            {
                IActivity activity = compMan.CreateInstance(activityNode) as IActivity;
                if (activity == null) continue;

                CategoryNode catNode = TreeCategoryNode(activity.CategoryName);
                catNode.Nodes.Add(new ActivityNode(activity));
            }

            if (tvActivity.Nodes.Count > 0)
                tvActivity.Nodes[0].Expand();
        }

        public FormGeoProcessor(List<IDatasetElement> elements)
            : this()
        {
            _elements = elements;
        }

        #region Properties
        public IActivity Activity
        {
            get
            {
                if (tvActivity.SelectedNode is ActivityNode)
                    return ((ActivityNode)tvActivity.SelectedNode).Activity;

                return null;
            }
        }
        #endregion

        #region Events
        private void tvActivity_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvActivity.SelectedNode is ActivityNode)
                MakeGUI(((ActivityNode)tvActivity.SelectedNode).Activity);
            else
                MakeGUI(null);
        }
        #endregion

        private void MakeGUI(IActivity activity)
        {
            if (wizardControl.WizardPages.Contains(wpSource)) wizardControl.WizardPages.Remove(wpSource);
            if (wizardControl.WizardPages.Contains(wpTarget)) wizardControl.WizardPages.Remove(wpTarget);
            if (wizardControl.WizardPages.Contains(wpProperties)) wizardControl.WizardPages.Remove(wpProperties);

            if (activity == null) return;

            #region Source
            if (activity.Sources != null && activity.Sources.Count > 0)
            {
                wizardControl.WizardPages.Add(wpSource);
            }
            MakeDataGUI(wpSource, activity.Sources, true);
            #endregion

            #region Target
            if (activity.Targets != null && activity.Targets.Count > 0)
            {
                wizardControl.WizardPages.Add(wpTarget);
            }
            MakeDataGUI(wpTarget, activity.Targets, false);
            #endregion

            #region PropertyPage
            if (activity is IPropertyPage)
            {
                while (wpProperties.Controls.Count > 0)
                    wpProperties.Controls.RemoveAt(0);
                Control ctrl = ((IPropertyPage)activity).PropertyPage(null) as Control;
                if (ctrl != null)
                {
                    wpProperties.Controls.Add(ctrl);
                    ctrl.Dock = DockStyle.Fill;
                }
                wizardControl.WizardPages.Add(wpProperties);
            }

            #endregion
        }

        private void MakeDataGUI(IDE.Controls.WizardPage page, List<IActivityData> dataList, bool open)
        {
            List<DataSelectorControl> selectors = new List<DataSelectorControl>();

            foreach (Control ctrl in page.Controls)
            {
                if (ctrl is DataSelectorControl)
                    selectors.Add((DataSelectorControl)ctrl);
            }
            foreach (DataSelectorControl selector in selectors)
                page.Controls.Remove(selector);

            if (dataList == null) return;

            int y = 5;
            foreach (IActivityData data in dataList)
            {
                if (data == null) return;

                DataSelectorControl selector = new DataSelectorControl(data);
                selector.Open = open;
                selector.Location = new Point(2, y);
                selector.Width = wpSource.Width - 80;
                y += selector.Height + 5;
                selector.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                if (open && _elements != null)
                {
                    foreach (IDatasetElement element in _elements)
                    {
                        selector.AddData(element);
                    }
                }
                else if (open == false)
                {
                    MemDatabase memDb = new MemDatabase();
                    memDb.CreateDataset("MemDataset", null);
                    DataSelectorControl.DatasetItem dsItem = new DataSelectorControl.DatasetItem(memDb["MemDataset"], "NewFeatureclass");
                    selector.AddData(dsItem);
                }

                page.Controls.Add(selector);
            }


        }

        #region ItemClasses
        private class CategoryNode : TreeNode
        {
            public CategoryNode(string name)
            {
                base.Text = name;
                base.ImageIndex = 0;
            }
        }
        private class ActivityNode : TreeNode
        {
            private IActivity _activity;

            public ActivityNode(IActivity activity)
            {
                _activity = activity;

                if (_activity != null)
                    this.Text = _activity.DisplayName;
                base.ImageIndex = base.SelectedImageIndex = 1;
            }

            public IActivity Activity
            {
                get { return _activity; }
            }
        }
        #endregion

        #region Helper
        private CategoryNode TreeCategoryNode(string name)
        {
            foreach (TreeNode node in tvActivity.Nodes)
            {
                if (node is CategoryNode &&
                    node.Text == name)
                    return (CategoryNode)node;
            }

            CategoryNode catNode = new CategoryNode(name);
            tvActivity.Nodes.Add(catNode);
            return catNode;
        }
        #endregion
    }
}