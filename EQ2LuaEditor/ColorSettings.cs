using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace EQ2LuaEditor
{
    public partial class ColorSettings : Form
    {
        public ColorSettings()
        {
            InitializeComponent();
        }

        private string[] colors = new string[7];
        private string[] backColors = new string[7];

        private void Settings_Load(object sender, EventArgs e)
        {
            ArrayList colorList = new ArrayList();
            Type colorType = typeof(System.Drawing.Color);
            PropertyInfo[] propInfoList = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (PropertyInfo c in propInfoList)
            {
                cmbColor.Items.Add(c.Name);
                cmbBackColor.Items.Add(c.Name);
            }

            txtName.Text = EQ2LuaEditor.Settings.AuthorName;

            colors[0] = EQ2LuaEditor.Settings.CommentColor;
            colors[1] = EQ2LuaEditor.Settings.NumberColor;
            colors[2] = EQ2LuaEditor.Settings.StringColor;
            colors[3] = EQ2LuaEditor.Settings.TextColor;
            colors[4] = EQ2LuaEditor.Settings.Keyword0Color;
            colors[5] = EQ2LuaEditor.Settings.Keyword1Color;
            colors[6] = EQ2LuaEditor.Settings.Keyword2Color;

            backColors[0] = EQ2LuaEditor.Settings.CommentBackColor;
            backColors[1] = EQ2LuaEditor.Settings.NumberBackColor;;
            backColors[2] = EQ2LuaEditor.Settings.StringBackColor;
            backColors[3] = EQ2LuaEditor.Settings.TextBackColor;
            backColors[4] = EQ2LuaEditor.Settings.Keyword0BackColor;
            backColors[5] = EQ2LuaEditor.Settings.Keyword1BackColor;
            backColors[6] = EQ2LuaEditor.Settings.Keyword2BackColor;
        }

        private void cmbColor_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = new Font("Arial", 9, FontStyle.Regular);
                Color c = Color.FromName(n);
                Brush b = new SolidBrush(c);
                g.DrawString(n, f, Brushes.Black, rect.X, rect.Top);
                g.FillRectangle(b, rect.X + 110, rect.Y + 5, rect.Width, rect.Height - 5);
            }
        }

        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            Color c = Color.FromName(cmbColor.Items[cmbColor.SelectedIndex].ToString());
            lblSample.ForeColor = c;
            if (lbKeywordType.SelectedIndex > -1)
                colors[lbKeywordType.SelectedIndex] = cmbColor.Items[cmbColor.SelectedIndex].ToString();
        }

        private void cmbBackColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            Color c = Color.FromName(cmbBackColor.Items[cmbBackColor.SelectedIndex].ToString());
            lblSample.BackColor = c;
            if (lbKeywordType.SelectedIndex > -1)
                backColors[lbKeywordType.SelectedIndex] = cmbBackColor.Items[cmbBackColor.SelectedIndex].ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            EQ2LuaEditor.Settings.AuthorName = txtName.Text;

            EQ2LuaEditor.Settings.CommentColor = colors[0];
            EQ2LuaEditor.Settings.NumberColor = colors[1];
            EQ2LuaEditor.Settings.StringColor = colors[2];
            EQ2LuaEditor.Settings.TextColor = colors[3];
            EQ2LuaEditor.Settings.Keyword0Color = colors[4];
            EQ2LuaEditor.Settings.Keyword1Color = colors[5];
            EQ2LuaEditor.Settings.Keyword2Color = colors[6];

            EQ2LuaEditor.Settings.CommentBackColor = backColors[0];
            EQ2LuaEditor.Settings.NumberBackColor = backColors[1];
            EQ2LuaEditor.Settings.StringBackColor = backColors[2];
            EQ2LuaEditor.Settings.TextBackColor = backColors[3];
            EQ2LuaEditor.Settings.Keyword0BackColor = backColors[4];
            EQ2LuaEditor.Settings.Keyword1BackColor = backColors[5];
            EQ2LuaEditor.Settings.Keyword2BackColor = backColors[6];

            this.DialogResult = DialogResult.OK;
        }

        private void lbKeywordType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = lbKeywordType.SelectedIndex;
            if (index == -1)
                return;

            cmbColor.SelectedIndex = cmbColor.Items.IndexOf(colors[index]);
            cmbBackColor.SelectedIndex = cmbBackColor.Items.IndexOf(backColors[index]);
        }
    }
}
