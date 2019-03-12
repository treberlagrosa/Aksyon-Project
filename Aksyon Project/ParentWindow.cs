using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aksyon_Project
{
    public partial class ParentWindow : Form
    {
        public ParentWindow()
        {
            InitializeComponent();
        }

        public static class activeChildForm
        {
            public static Form childForm;
        }

        private void ParentWindow_Load(object sender, EventArgs e)
        {
            
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Are you sure you want to closed the program?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(res == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btn_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void ParentWindow_Shown(object sender, EventArgs e)
        {
            try
            {
                MainWindow main = new MainWindow();
                activeChildForm.childForm = null;
                activeChildForm.childForm = main;
                main.MdiParent = this;
                main.Dock = DockStyle.Fill;
                main.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
