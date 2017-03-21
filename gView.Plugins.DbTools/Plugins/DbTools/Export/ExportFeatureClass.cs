using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.system;
using System.Windows.Forms;
using gView.Framework.Carto;
using gView.Framework.Globalisation;

namespace gView.Plugins.DbTools.Export
{
    [RegisterPlugIn("F11BA03D-0401-495d-88C8-2A1D39F4CA45")]
    public class ExportFeatureClass : IDatasetElementContextMenuItem
    {
        private IMapDocument _doc = null;

        #region IDatasetElementContextMenuItem Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.ExportFeatures", "Export Features..."); }
        }

        public bool Enable(object element)
        {
            if (_doc == null || _doc.Application == null || !(element is IDatasetElement)) return false;

            if (_doc.Application is IMapApplication &&
                    ((IMapApplication)_doc.Application).ReadOnly == true) return false;

            return (((IDatasetElement)element).Class is IFeatureClass);
        }

        public bool Visible(object element)
        {
            return Enable(element);
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = hook as IMapDocument;
            }
        }

        public void OnEvent(object element, object dataset)
        {
            if (!(element is IFeatureLayer) || !(((IFeatureLayer)element).Class is IFeatureClass)) return;

            ExportFeatureClassDialog dlg = new ExportFeatureClassDialog(
                ((_doc != null && _doc.FocusMap != null) ? _doc.FocusMap.Display : null),
                element as IFeatureLayer);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                IDatasetElement destElement = dlg.DestinationDatasetElement;
                if (destElement != null && destElement.Class != null)
                {
                    if (MessageBox.Show("Add new feature class to map?", "Add", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ILayer layer = LayerFactory.Create(destElement.Class);
                        _doc.FocusMap.AddLayer(layer, 0);

                        if (_doc.Application is IMapApplication)
                            ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                    }
                }
            }
        }

        public object Image
        {
            get { return global::gView.Plugins.DbTools.Properties.Resources.export; }
        }

        public int SortOrder
        {
            get { return 55; }
        }

        #endregion
    }
}