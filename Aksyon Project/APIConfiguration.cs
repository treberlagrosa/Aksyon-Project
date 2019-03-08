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
    public partial class APIConfiguration : Form
    {
        public APIConfiguration()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        void SaveConfig()
        {
            try
            {
                Properties.Settings.Default.ip = txtURL.Text;
                Properties.Settings.Default.grant_type = txtGrantType.Text;
                Properties.Settings.Default.client_secret = txtClientSecret.Text;
                Properties.Settings.Default.client_id = txtClientID.Text;
                Properties.Settings.Default.Save();
                MessageBox.Show("Success", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void APIConfiguration_Load(object sender, EventArgs e)
        {
            getConfig();
        }

        public void getConfig()
        {
            txtURL.Text = Properties.Settings.Default.ip;
            txtClientSecret.Text = Properties.Settings.Default.client_secret;
            txtClientID.Text = Properties.Settings.Default.client_id;
            txtGrantType.Text = Properties.Settings.Default.grant_type;
        }
    }
}
