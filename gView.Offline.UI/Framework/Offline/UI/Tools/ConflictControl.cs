using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Offline;
using gView.Framework.Data;
using gView.Framework.UI;

namespace gView.Framework.Offline.UI.Tools
{
    public partial class ConflictControl : UserControl
    {
        private Replication.Conflict _conflict;
        private IMapDocument _doc = null;

        public ConflictControl()
        {
            InitializeComponent();
        }

        public Replication.Conflict Conflict
        {
            get
            {
                return _conflict;
            }
            set
            {
                _conflict = value;
                RefreshGUI();
            }
        }
        public IMapDocument MapDocument
        {
            set { _doc = value; }
        }

        public void RefreshGUI()
        {
            this.Controls.Clear();
            if (_conflict == null || _conflict.FeatureClass == null) return;

            int y = 5, i = 0;
            foreach (Replication.Conflict.FieldConflict cField in _conflict.ConflictFields)
            {
                if (cField == null) continue;
                Label lbl = new Label();
                lbl.Text = cField.FieldName + ":";
                lbl.Location = new Point(5, y);
                lbl.TextAlign = ContentAlignment.MiddleRight;
                lbl.Width = 90;

                TextBox txtLinked = new TextBox();
                txtLinked.Location = new Point(410, y);
                txtLinked.Width = 100;

                //if (cField.FieldName == _conflict.FeatureClass.ShapeFieldName)
                //{
                //    Button btnHighlight = new Button();
                //    btnHighlight.Text = "";
                //    btnHighlight.Width = 22;
                //    btnHighlight.Location = new Point(520, y);
                //    btnHighlight.Click += new EventHandler(btnHighlight_Click);
                //    this.Controls.Add(btnHighlight);
                //}

                MultiColumnComboBox combo = new MultiColumnComboBox();
                combo.AutoComplete = true;
                combo.AutoDropdown = false;
                combo.ColumnNames = "";
                combo.ColumnWidthDefault = 100;
                combo.ColumnWidths = ColumnWidths(cField.ConflictTable);
                combo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
                combo.FormattingEnabled = true;
                combo.LinkedColumnIndex = 1;
                combo.LinkedTextBox = txtLinked;
                combo.Name = "combo" + (i++).ToString();
                combo.TabIndex = 0;
                combo.Width = 300;
                combo.Location = new Point(100, y);
                combo.SelectedIndexChanged += new EventHandler(combo_SelectedIndexChanged);
                
                this.Controls.Add(lbl);
                this.Controls.Add(txtLinked);
                this.Controls.Add(combo);

                combo.DataSource = cField.ConflictTable;
                combo.DisplayMember = "Value";
                combo.ValueMember = "Value";

                y += 25;
            }
            foreach (Replication.Conflict.FieldConflict cField in _conflict.NeutralFields)
            {
                if (cField == null) continue;
                Label lbl = new Label();
                lbl.Text = cField.FieldName + ":";
                lbl.Location = new Point(5, y);
                lbl.TextAlign = ContentAlignment.MiddleRight;
                lbl.Width = 90;

                TextBox txtBox = new TextBox();
                txtBox.Location = new Point(100, y);
                txtBox.Width = 300;
                txtBox.Enabled = false;
                txtBox.Text = (cField.Value != null) ? cField.Value.ToString() : null;

                this.Controls.Add(lbl);
                this.Controls.Add(txtBox);

                y += 25;
            }

            this.ResumeLayout(false);
        }

        void btnHighlight_Click(object sender, EventArgs e)
        {
            if (_conflict == null || _conflict.FeatureClass==null ||
                _doc == null || _doc.FocusMap == null)
                return;

            IFeature feature = _conflict.SolvedFeature;
            if (feature == null) return;

            _doc.FocusMap.HighlightGeometry(feature.Shape, 1000);
        }

        void combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            MultiColumnComboBox combo = sender as MultiColumnComboBox;
            if (combo == null) return;

            foreach (Replication.Conflict.FieldConflict cField in _conflict.ConflictFields)
            {
                if (combo.DataSource == cField.ConflictTable)
                {
                    cField.ValueIndex = combo.SelectedIndex;
                    break;
                }
            }
        }
        private string ColumnWidths(DataTable tab)
        {
            if (tab == null) return String.Empty;

            using (Graphics gr = Graphics.FromHwnd(this.Handle))
            {
                string columnWidths = String.Empty;
                for (int i = 0; i < tab.Columns.Count; i++)
                {
                    int max = 20;
                    foreach (DataRow row in tab.Rows)
                    {
                        if (row[i] == null) continue;
                        max = (int)Math.Max(max, gr.MeasureString(row[i].ToString(), this.Font).Width);
                    }
                    if (columnWidths.Length > 0) columnWidths += ";";
                    columnWidths += (max + 6).ToString();
                }

                return columnWidths;
            }
        }
    }
}
