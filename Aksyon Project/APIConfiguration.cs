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
            try
            {
                using(AksyonProjectEntities api = new AksyonProjectEntities())
                {
                    var res = api.APIConfigs.Count();
                    if (res > 0)
                    {
                        updateConfig();
                    }
                    else SaveConfig();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error \n" + ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        async void SaveConfig()
        {
            using (AksyonProjectEntities api = new AksyonProjectEntities())
            {
                APIConfig apiconfig = new APIConfig()
                {
                    client_id = Int32.Parse(txtClientID.Text),
                    client_secret = txtClientSecret.Text,
                    grant_type = txtGrantType.Text,
                    url = txtURL.Text
                };
                api.APIConfigs.Add(apiconfig);
                await api.SaveChangesAsync();
                MessageBox.Show("Success", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        async void updateConfig()
        {
            using (AksyonProjectEntities api = new AksyonProjectEntities())
            {
                var res = api.APIConfigs.FirstOrDefault();
                res.client_secret = txtClientSecret.Text;
                res.client_id = Int32.Parse(txtClientID.Text);
                res.grant_type = txtGrantType.Text;
                res.url = txtURL.Text;
                await api.SaveChangesAsync();
                MessageBox.Show("Success", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void APIConfiguration_Load(object sender, EventArgs e)
        {
            getConfig();
        }

        public void getConfig()
        {
            using (AksyonProjectEntities api = new AksyonProjectEntities())
            {
                var data = (from u in api.APIConfigs
                            select new
                            {
                                id = u.id,
                                Client_ID = u.client_id,
                                Client_Secret = u.client_secret,
                                Grant_Type = u.grant_type,
                                URL = u.url
                            }).FirstOrDefault();
                if (data == null) return;
                if (data.Client_ID != null) txtClientID.Text = data.Client_ID.ToString();
                if (data.Client_Secret != null) txtClientSecret.Text = data.Client_Secret.ToString();
                if (data.Grant_Type != null) txtGrantType.Text = data.Grant_Type.ToString();
                if (data.URL != null) txtURL.Text = data.URL.ToString();
            }
        }

        private void APIConfiguration_FormClosing(object sender, FormClosingEventArgs e)
        {
            txtClientID.Text = "";
            txtClientSecret.Text = "";
            txtGrantType.Text = "";
            txtURL.Text = "";
        }
    }
}
