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

            public static string data_owner;
        }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            checkConfig();
        }


        private Boolean login()
        {
            Boolean res;
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
                //MainWindow mainWin = new MainWindow();
                //mainWin.Show();
                ParentWindow parent = new ParentWindow();
                parent.Show();
                res = true;
            }
            else
            {
                MessageBox.Show("Unable to Login", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Text = "";
                res = false;
            }
            return res;
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
            if(login()) requestUser("/api/user");
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

        void requestUser(string ip)
        {
            string url = Properties.Settings.Default.ip + ip;
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
            IRestResponse response = client.Execute(request);
            var jobject = JsonConvert.DeserializeObject<RUser>(response.Content);
            apiCon.data_owner = jobject.acct_types.description;
        }

        public class User
        {
            public int id { get; set; }
            public string acct_id { get; set; }
            public string last_name { get; set; }
            public string first_name { get; set; }
            public object middle_name { get; set; }
            public object qualifier_id { get; set; }
            public int rank_id { get; set; }
            public string badge_number { get; set; }
            public int acct_level_class_id { get; set; }
            public int acct_type_id { get; set; }
            public string region { get; set; }
            public string ppo { get; set; }
            public string mps { get; set; }
            public string contact_number { get; set; }
            public string email { get; set; }
            public int user_type_id { get; set; }
            public int status { get; set; }
            public string email_verified_at { get; set; }
            public string image { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
        }

        public class AcctTypes
        {
            public int id { get; set; }
            public string description { get; set; }
            public string level { get; set; }
            public string utype { get; set; }
        }

        public class Rank
        {
            public int id { get; set; }
            public string code { get; set; }
            public string description { get; set; }
            public object created_at { get; set; }
            public object updated_at { get; set; }
        }

        public class RUser
        {
            public User user { get; set; }
            public AcctTypes acct_types { get; set; }
            public Rank rank { get; set; }
        }


    }
}
