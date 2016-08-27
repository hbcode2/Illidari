using Styx;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Illidari
{
    public partial class ViewBagsForm : Form
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        public WoWItem SelectedItem { get; set; }
        public ViewBagsForm()
        {
            InitializeComponent();
        }

        private void lvBags_DoubleClick(object sender, EventArgs e)
        {
            if (lvBags.SelectedItems.Count > 0)
            {
                SelectedItem = (WoWItem)lvBags.SelectedItems[0].Tag;
                this.DialogResult = DialogResult.OK;
            }
            
            this.Close();
        }

        private void ViewBags_Load(object sender, EventArgs e)
        {
            lvBags.Items.Clear();
            foreach (var item in Me.BagItems.OrderBy(i => i.Name))
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = item.ItemInfo.Id.ToString();
                lvi.SubItems.Add(item.SafeName);
                lvi.SubItems.Add(item.StackCount.ToString());
                lvi.Tag = item;
                lvBags.Items.Add(lvi);
            }
        }
    }
}
