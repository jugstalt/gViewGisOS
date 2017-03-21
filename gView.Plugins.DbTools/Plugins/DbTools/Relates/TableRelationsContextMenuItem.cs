using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Data;

namespace gView.Plugins.DbTools.Relates
{
    [RegisterPlugIn("9d377f08-9503-461f-962a-e9364119b1e8")]
    public class TableRelationsContextMenuItem : IDatasetElementContextMenuItem
    {
        private IMapDocument _doc = null;

        #region IDatasetElementContextMenuItem Member

        public string Name
        {
            get { return "Relates"; }
        }

        public bool Enable(object element)
        {
            return element is IFeatureLayer;
        }

        public bool Visible(object element)
        {
            return element is IFeatureLayer;
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
            if (_doc == null || !(element is IFeatureLayer) && !(((IFeatureLayer)element).Class is IFeatureClass)) return;

            TableRelationsDialog dlg = new TableRelationsDialog(_doc, (IFeatureLayer)element);
            dlg.ShowDialog();
        }

        public object Image
        {
            get { return global::gView.Plugins.DbTools.Properties.Resources.table_relationship_32; }
        }

        public int SortOrder
        {
            get { return 46; }
        }

        #endregion
    }
}
