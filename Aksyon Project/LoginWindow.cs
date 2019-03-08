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
            var client = new RestClient(Properties.Settings.Default.ip + "/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddParameter("client_id", Properties.Settings.Default.client_id);
            request.AddParameter("client_secret", Properties.Settings.Default.client_secret);
            request.AddParameter("grant_type", Properties.Settings.Default.grant_type);
            request.AddParameter("username", txtUsername.Text.ToString());
            request.AddParameter("password", txtPassword.Text.ToString());
            IRestResponse response = client.Execute(request);
            var jobject = JsonConvert.DeserializeObject<RootObject>(response.Content);
            if (response.IsSuccessful)
            {
                apiCon.access_token = jobject.access_token;
                Console.WriteLine("Access token "+apiCon.access_token);
                apiCon.token_type = jobject.token_type;
                Console.WriteLine("Token type "+apiCon.token_type);
                apiCon.expires_in = jobject.expires_in;
                Console.WriteLine("Expires in "+apiCon.expires_in);
                apiCon.refresh_token = jobject.refresh_token;
                Console.WriteLine("Refresh token "+apiCon.refresh_token);
                
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
            if (string.IsNullOrEmpty(Properties.Settings.Default.ip) && string.IsNullOrEmpty(Properties.Settings.Default.client_id) && string.IsNullOrEmpty(Properties.Settings.Default.client_secret) && string.IsNullOrEmpty(Properties.Settings.Default.grant_type))
            {
                MessageBox.Show("Setup api configuration first.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                APIConfiguration apiCon = new APIConfiguration();
                apiCon.ShowDialog();
                return;
            }
            login();
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

        
        
        void request(string api, int index)
        {
            string ip = "http://192.168.100.217:8000/" + api;
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "applic`ation/json");
            request.AddHeader("Authorization", apiCon.token_type +" "+ apiCon.access_token);
            IRestResponse response = client.Execute(request);
            
        }

    }
}
