using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace EQ2LuaEditor
{
    public partial class Errors : DockContent
    {
        public Errors()
        {
            InitializeComponent();

            /*dgvOutput.Columns[0].FillWeight = 5;
            dgvOutput.Columns[1].FillWeight = 70;
            dgvOutput.Columns[2].FillWeight = 20;
            dgvOutput.Columns[3].FillWeight = 5;*/

            lvErrors.Columns[0].Width = 50;
            lvErrors.Columns[1].Width = 700;
            lvErrors.Columns[2].Width = 200;
            lvErrors.Columns[3].Width = 50;

            
        }
    }
}
