using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.system;
using System.Threading;

namespace gView.Framework.GeoProcessing.UI
{
    [gView.Framework.system.RegisterPlugIn("AD24DF4B-A03F-4022-A671-FAE1BEF4D302")]
    public class GeoProcessingTool : ITool, IExTool
    {
        IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return "Geo Processor"; }
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
            get { return global::gView.GeoProcessing.UI.Properties.Resources.toolbox; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
        }

        public void OnEvent(object MapEvent)
        {
            FormGeoProcessor dlg = (_doc != null && _doc.FocusMap != null) ?
                new FormGeoProcessor(_doc.FocusMap.MapElements) :
                new FormGeoProcessor();

            dlg.ShowInTaskbar = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                IActivity activity = dlg.Activity;
                if (activity != null)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(StartProgress));
                    IProgressDialog progress = ProgressDialog.CreateProgressDialogInstance();
                    progress.ShowProgressDialog(activity, activity, thread);


                    if (_datas != null &&
                        _doc != null &&
                        _doc.FocusMap != null)
                    {
                        if (MessageBox.Show("Add new feature class to map?", "Add", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            foreach (IActivityData data in _datas)
                            {
                                if (data == null || data.Data == null || data.Data.Class == null) continue;

                                ILayer layer = LayerFactory.Create(data.Data.Class);
                                _doc.FocusMap.AddLayer(layer, 0);

                            }
                        }
                        if (_doc.Application is IMapApplication)
                            ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                    }
                }
            }
        }

        #endregion

        List<IActivityData> _datas = null;
        private void StartProgress(object argument)
        {
            _datas = null;
            if (!(argument is IActivity)) return;

            try
            {
                _datas = ((IActivity)argument).Process();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message + "\n" + ex.StackTrace, "ERROR");
                return;
            }
        }
    }
}
