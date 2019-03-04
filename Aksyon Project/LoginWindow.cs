using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using RestSharp;
using System.IO;
using Newtonsoft.Json;

namespace Aksyon_Project
{
    public partial class LoginWindow : Form
    {
        public string client_id;
        public string client_secret;
        public string grant_type;
        public string url;

        // for accessing api
        public static class apiCon
        {
            public static string token_type;
            public static int expires_in;
            public static string access_token;
            public static string refresh_token;
        }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            checkConfig();
        }


        private void login()
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW", "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"client_id\"\r\n\r\n" + client_id + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"client_secret\"\r\n\r\n" + client_secret + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"grant_type\"\r\n\r\n" + grant_type + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"username\"\r\n\r\n" + txtUsername.Text.ToString() + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"password\"\r\n\r\n" + txtPassword.Text.ToString() + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW--", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var jobject = JsonConvert.DeserializeObject<RootObject>(response.Content);
            if (response.IsSuccessful)
            {
                apiCon.access_token = jobject.access_token;
                apiCon.token_type = jobject.token_type;
                apiCon.expires_in = jobject.expires_in;
                apiCon.refresh_token = jobject.refresh_token;
                this.Hide();
                MainWindow mainWin = new MainWindow();
                mainWin.Show();
            }
            else
            {
                MessageBox.Show("Unable to Login", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Text = "";
            }
        }

        private void LoginWindow_Load(object sender, EventArgs e)
        {
            
        }

        void checkConfig()
        {
            using (AksyonProjectEntities api = new AksyonProjectEntities())
            {
                try
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
                    if (data == null)
                    {
                        MessageBox.Show("Configure API first.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        APIConfiguration config = new APIConfiguration();
                        config.Show();
                    }
                    else
                    {
                        client_id = data.Client_ID.ToString();
                        client_secret = data.Client_Secret;
                        grant_type = data.Grant_Type;
                        url = data.URL;
                        login();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }

        public class RootObject
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            APIConfiguration apiconfig = new APIConfiguration();
            apiconfig.Show();
        }

    }
}
