using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using GBMSGUI_NET;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_ErrorCodesDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_VisualInterfaceLCDDefines;
using RestSharp;
using Newtonsoft.Json;

namespace Aksyon_Project
{
    public partial class MainWindow : Form
    {

        // device characteristics
        internal bool Resolution1000Dpi;
        internal bool LCDPresent;
        // size for image buffer
        internal int MaxImageBufferSize;

        // Item for User list
        private class UserListItem
        {
            public String UserName;
            public String Path;
            public override String ToString()
            {
                return UserName;
            }
        }

        public static class selectedPersonality
        {
            public static string id;
            public static string name;
        }

        // list of available Green Bit devices
        private GBMSAPI_NET_DeviceInfoStruct[] DevList = new GBMSAPI_NET_DeviceInfoStruct[GBMSAPI_NET_DeviceInfoConstants.GBMSAPI_NET_MAX_PLUGGED_DEVICE_NUM];
        private int DeviceNumber;

        public GBMSAPI_NET_DeviceInfoStruct CurrentDevice;

        // only used here to get MSAPI error messages
        //private GBMSGUI MyGUI = new GBMSGUI();
        public GBMSGUI MyGUI = new GBMSGUI();

        // configuration
        public Configuration DemoConfig;

        // 1.13.5.0
        // declare static XmlSerializers, to avoid known memory leak
        public static XmlSerializer DemoConfigXmlSerializer = new XmlSerializer(typeof(Configuration));
        public static XmlSerializer DemoUsersXmlSerializer = new XmlSerializer(typeof(UserData));

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Text = Application.ProductName + " " + Application.ProductVersion;

            int Major, Minor, Build, Revision;
            MyGUI.GetVersion(out Major, out Minor, out Build, out Revision);
            // show GUI version
            lblGBMSGUIVersion.Text += Major.ToString() + "." + Minor.ToString() + "."
                + Build.ToString() + "." + Revision.ToString();

            // read configuration
            String CfgFileName = Path.ChangeExtension(Application.ExecutablePath, ".cfg");
            if (File.Exists(CfgFileName))
                DemoConfig = MainWindow.Configuration.Deserialize(CfgFileName);
            else
                DemoConfig = new Configuration();   // default values

            // StripeAcquisition not yet certified
            DemoConfig.AcquisitionOptions &= ~GBMSGUI.AcquisitionOption.EnableRollStripeAcquisition;
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            // refresh devices list
            btnReload_Click(this, null);
            //LoadUsersList();
            requestData("/api/data");
            loadPersonalities();
        }

        private bool FillDeviceList()
        {
            uint USBErr;
            int ret;
            int i;

            cmbDevices.Items.Clear();

            // initialize array
            for (i = 0; i < GBMSAPI_NET_DeviceInfoConstants.GBMSAPI_NET_MAX_PLUGGED_DEVICE_NUM; i++)
                DevList[i] = new GBMSAPI_NET_DeviceInfoStruct();

            ret = GBMSAPI_NET_DeviceSettingRoutines.GBMSAPI_NET_GetAttachedDeviceList(DevList, out DeviceNumber, out USBErr);
            if (ret != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                MessageBox.Show(MyGUI.GetMSAPIErrorMessage("GBMSAPI_NET_GetAttachedDeviceList", ret),
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }


            if (USBErr != 0)
            {
                MessageBox.Show("USB error " + USBErr.ToString("X") + " (GBMSAPI_NET_GetAttachedDeviceList) ",
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return false;
            }

            if (DeviceNumber == 0)
            {
                btnNew.Enabled = false;
                //MessageBox.Show("No attached device found!",
                //    Application.ProductName,
                //    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            else
                btnNew.Enabled = true;

            // fill combobox
            for (i = 0; i < DeviceNumber; i++)
                //cmbDevices.Items.Add(GetDeviceName(DevList[i].DeviceID));
                cmbDevices.Items.Add(GetDeviceName(DevList[i].DeviceID) + " (" + DevList[i].DeviceSerialNumber + ")");

#if GBDCGUI_DEMO
            //if (File.Exists("GBDCGUI.dll"))
            // add Fingerprint Card source
            cmbDevices.Items.Add("Fingerprint Card");
#endif

            // select first
            if (cmbDevices.Items.Count != 0)
                cmbDevices.SelectedIndex = 0;

            return true;
        }

        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            // if is the last, is "Fingerprint Card" 
            //if (cmbDevices.SelectedIndex != (cmbDevices.Items.Count-1))
            if (cmbDevices.Text != "Fingerprint Card")
                SelectDevice(cmbDevices.SelectedIndex);
        }

        private bool SelectDevice(int Index)
        {
            CurrentDevice = DevList[Index];

            // select device
            Cursor = Cursors.WaitCursor;
            int ret = GBMSAPI_NET_DeviceSettingRoutines.GBMSAPI_NET_SetCurrentDevice(
                CurrentDevice.DeviceID,
                CurrentDevice.DeviceSerialNumber);
            Cursor = Cursors.Default;
            if (ret != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                MessageBox.Show(MyGUI.GetMSAPIErrorMessage("GBMSAPI_NET_SetCurrentDevice", ret),
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // get maximum image size for allocating buffer (in UserDataForm)
            uint MaxSizeX, MaxSizeY;
            GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetMaxImageSize(out MaxSizeX, out MaxSizeY);
            MaxImageBufferSize = (int)(MaxSizeX * MaxSizeY);

            // get characteristics of current device
            uint Features;
            GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetDeviceFeatures(out Features);
            if (GBMSGUI.CheckMask(Features, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_1000DPI_RESOLUTION))
                Resolution1000Dpi = true;
            else
                Resolution1000Dpi = false;

            // check presence of optional devices
            uint ExternalEquipment;
            ret = GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetOptionalExternalEquipment(out ExternalEquipment);
            if (ret != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                MessageBox.Show(MyGUI.GetMSAPIErrorMessage("GBMSAPI_NET_GetOptionalExternalEquipment", ret),
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // check external equipment
            //if (GBMSGUI.CheckMask(ExternalEquipment, GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_PEDAL))
            //    PedalPresent = true;

            if (GBMSGUI.CheckMask(ExternalEquipment, GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_VUI_LCD))
                LCDPresent = true;

            return true;
        }

        public static String GetDeviceName(byte DeviceID)
        {
            String Name = "";

            switch (DeviceID)
            {
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40:
                    Name = "DactyScan40";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84:
                    Name = "DactyScan84";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS1000:
                    Name = "MultiScan1000";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS500:
                    Name = "MultiScan500";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_PS2:
                    Name = "PoliScan2";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_VS3:
                    Name = "Visascan3";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS26:
                    Name = "DactyScan26";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC500:
                    Name = "MC500";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC500:
                    Name = "MSC500";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I:
                    Name = "DactyScan40i";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C:
                    Name = "DactyScan84c";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517:
                    Name = "MC517";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC517:
                    Name = "MSC517";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32:
                    Name = "DactyScan32";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527:
                    Name = "MultiScan527";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84t:
                    Name = "DactyScan84t";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DSID20:
                    Name = "DactyID20";
                    break;
                case GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t:
                    Name = "MultiScan527t";
                    break;
                default:
                    Name = "";
                    break;
            }

            return Name;
        }

        public static byte GetDeviceIDFromName(String DeviceName)
        {
            byte DeviceID = 0;

            switch (DeviceName)
            {
                case "DactyScan40":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40;
                    break;
                case "DactyScan84":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84;
                    break;
                case "MultiScan1000":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS1000;
                    break;
                case "MultiScan500":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS500;
                    break;
                case "PoliScan2":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_PS2;
                    break;
                case "Visascan3":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_VS3;
                    break;
                case "DactyScan26":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS26;
                    break;
                case "MC500":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC500;
                    break;
                case "MSC500":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC500;
                    break;
                case "DactyScan40i":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I;
                    break;
                case "DactyScan84c":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C;
                    break;
                case "MC517":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517;
                    break;
                case "MSC517":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC517;
                    break;
                case "DactyScan32":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32;
                    break;
                case "MultiScan527":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527;
                    break;
                case "DactyScan84t":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84t;
                    break;
                case "DactyID20":
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DSID20;
                    break;
                case "MultiScan527t":   // 2.3.0.0
                    DeviceID = GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t;
                    break;
            }

            return DeviceID;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            // open UserDataForm in "new" mode
            UserDataWindow UserForm = new UserDataWindow(this);

            UserForm.ViewMode = false;

            // if is the last, is "Fingerprint Card" 
            //if (cmbDevices.SelectedIndex == (cmbDevices.Items.Count - 1))
            if (cmbDevices.Text == "Fingerprint Card")
                UserForm.FingerprintCard = true;
            else
                UserForm.FingerprintCard = false;

            if (UserForm.ShowDialog() == DialogResult.Cancel)
                return;

            // update list
            LoadUsersList();
        }

        private void LoadUsersList()
        {
            String[] DirList = Directory.GetDirectories(Path.GetDirectoryName(Application.ExecutablePath));
            String UserFile;

            // search for folders containing UserData.xml file
            lstUsers.Items.Clear();
            foreach (String DirName in DirList)
            {
                UserFile = DirName + Path.DirectorySeparatorChar + "UserData.xml";
                if (File.Exists(UserFile))
                {
                    // read file
                    UserData Data = UserData.Deserialize(UserFile);
                    UserListItem Item = new UserListItem();
                    Item.Path = DirName;
                    Item.UserName = Data.Name;
                    // add user name to the list
                    lstUsers.Items.Add(Item);
                }
            }

            btnView.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedIndex != -1)
            {
                btnView.Enabled = true;
                btnDelete.Enabled = true;
            }
            else
            {
                btnView.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            // open UserDataForm in "view" mode
            UserListItem UserItem = (UserListItem)lstUsers.Items[lstUsers.SelectedIndex];
            UserDataWindow UserForm = new UserDataWindow(this);

            UserForm.ViewMode = true;
            UserForm.ImagesPath = UserItem.Path;

            if (UserForm.ShowDialog() == DialogResult.Cancel)
                return;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            UserListItem Item = (UserListItem)lstUsers.Items[lstUsers.SelectedIndex];

            // ask confirmation
            if (MessageBox.Show("Delete \"" + Item.UserName + "\"?", Application.ProductName,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                return;

            // delete folder and content
            Directory.Delete(Item.Path, true);

            // refresh list
            LoadUsersList();
        }

        private void lstUsers_DoubleClick(object sender, EventArgs e)
        {
            if (lstUsers.SelectedIndex != -1)
                btnView_Click(this, null);
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            bool success;

            // enumeration of connected devices can be a long task; perform after form is shown
            btnNew.Enabled = false;
            Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            success = FillDeviceList();
            Cursor = Cursors.Default;
            // 2.2.0.0
            //if (success)
            //    btnNew.Enabled = true;
        }

        void loadPersonalities()
        {
            try
            {
                var client = new RestClient("http://192.168.100.217:8000/api/personalities/");
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    var jobject = JsonConvert.DeserializeObject<RootObject>(response.Content);
                    string name;
                    if (jobject.personalities.Count != 0)
                    {
                        Console.WriteLine(jobject.personalities.Count + " personalities have been loaded.");
                        for (int i = 0; i < jobject.personalities.Count; i++)
                        {
                            name = jobject.personalities[i].first_name + " " + jobject.personalities[i].last_name;

                            dgvPersonalities.Rows.Add(new object[] {
                    jobject.personalities[i].person_id,
                    name,
                    jobject.personalities[i].regional_office.description,
                    jobject.personalities[i].provincial_office.description,
                    jobject.personalities[i].police_station.description,
                    jobject.personalities[i].category_hvtslt.description,
                    jobject.personalities[i].class_suspect.description,
                    jobject.personalities[i].drugperson_status.description,
                    jobject.personalities[i].date_entry_watchlist,
                    jobject.personalities[i].listed
                });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error" + ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class Region
        {
            public int id { get; set; }
            public string abbrev { get; set; }
            public string description { get; set; }
        }

        public class Province
        {
            public int id { get; set; }
            public string description { get; set; }
            public int prov_region_id { get; set; }
        }

        public class Municipality
        {
            public int id { get; set; }
            public int region_id { get; set; }
            public int province_id { get; set; }
            public string description { get; set; }
        }

        public class Barangay
        {
            public int id { get; set; }
            public int region_id { get; set; }
            public int province_id { get; set; }
            public int city_id { get; set; }
            public string description { get; set; }
        }

        public class CategoryHvtslt
        {
            public int id { get; set; }
            public int hvt_slt_id { get; set; }
            public string description { get; set; }
        }

        public class ClassSuspect
        {
            public int id { get; set; }
            public int class_suspect_id { get; set; }
            public string description { get; set; }
        }

        public class DrugpersonStatus
        {
            public int id { get; set; }
            public string description { get; set; }
            public object created_at { get; set; }
            public object updated_at { get; set; }
        }

        public class RegionalOffice
        {
            public int id { get; set; }
            public string description { get; set; }
            public int seq { get; set; }
            public int cat { get; set; }
            public int pop { get; set; }
        }

        public class ProvincialOffice
        {
            public int id { get; set; }
            public int pros_id { get; set; }
            public int ppo_id { get; set; }
            public string description { get; set; }
        }

        public class PoliceStation
        {
            public int id { get; set; }
            public int pro_id { get; set; }
            public int ppo_id { get; set; }
            public string description { get; set; }
            public string uccn { get; set; }
            public string municipal_code { get; set; }
        }

        public class Personality
        {
            public int id { get; set; }
            public int person_id { get; set; }
            public string last_name { get; set; }
            public string first_name { get; set; }
            public string middle_name { get; set; }
            public object qualifier_id { get; set; }
            public string alias { get; set; }
            public string birthday { get; set; }
            public object place_of_birth { get; set; }
            public Region region { get; set; }
            public Province province { get; set; }
            public Municipality municipality { get; set; }
            public Barangay barangay { get; set; }
            public object street_details { get; set; }
            public string gender { get; set; }
            public string occupation { get; set; }
            public object occupation_details { get; set; }
            public string education { get; set; }
            public string related_ego { get; set; }
            public string related_ofw { get; set; }
            public object group_affiliation { get; set; }
            public object group_affiliation_pos { get; set; }
            public object citizenship { get; set; }
            public object ethnic_group { get; set; }
            public object dialect { get; set; }
            public object religion { get; set; }
            public object tech_skills { get; set; }
            public object social_media { get; set; }
            public object contact_number { get; set; }
            public string date_surrendered_arrested { get; set; }
            public string oic { get; set; }
            public object validated_by { get; set; }
            public object remarks { get; set; }
            public CategoryHvtslt category_hvtslt { get; set; }
            public ClassSuspect class_suspect { get; set; }
            public DrugpersonStatus drugperson_status { get; set; }
            public RegionalOffice regional_office { get; set; }
            public ProvincialOffice provincial_office { get; set; }
            public PoliceStation police_station { get; set; }
            public string listed { get; set; }
            public string validated { get; set; }
            public string recap { get; set; }
            public int encoded { get; set; }
            public string data_owner { get; set; }
            public string date_entry_watchlist { get; set; }
            public string subjected_tokhang { get; set; }
            public string image_path { get; set; }
            public int status { get; set; }
            public int confidentiality { get; set; }
            public int hide_person { get; set; }
            public string photo_file { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public object right_thumb { get; set; }
            public object right_point { get; set; }
            public object right_mid { get; set; }
            public object right_ring { get; set; }
            public object right_pink { get; set; }
            public object right_roll { get; set; }
            public object left_thumb { get; set; }
            public object left_point { get; set; }
            public object left_mid { get; set; }
            public object left_ring { get; set; }
            public object left_pink { get; set; }
            public object left_roll { get; set; }
        }

        public class RootObject
        {
            public List<Personality> personalities { get; set; }
        }

        private void dgvPersonalities_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedPersonality.id = dgvPersonalities.Rows[e.RowIndex].Cells["colPID"].Value.ToString();
            selectedPersonality.name = dgvPersonalities.Rows[e.RowIndex].Cells["colName"].Value.ToString();
            UserDataWindow UserForm = new UserDataWindow(this);
            Console.WriteLine("Selected Personality id " + selectedPersonality.id);
            UserForm.ViewMode = false;
            this.Hide();
            UserForm.MdiParent = ParentWindow.ActiveForm;
            ParentWindow.activeChildForm.childForm = null;
            ParentWindow.activeChildForm.childForm = UserForm;
            UserForm.Dock = DockStyle.Fill;
            UserForm.Show();
        }

        void requestData(string ip)
        {
            string url = Properties.Settings.Default.ip + ip;
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
            IRestResponse response = client.Execute(request);
            var jobject = JsonConvert.DeserializeObject<RData>(response.Content);
            int index = jobject.pros.Count;
            for (int i = 0; i < index; i++)
            {
                ComboboxItemPRORegion item = new ComboboxItemPRORegion();
                item.Text = jobject.pros[i].description;
                item.Value = jobject.pros[i].id;
                cmbPRORegion.Items.Add(item);
            }
            int qIndex = jobject.qualifiers.Count;
            for (int i = 0; i < qIndex; i++)
            {
                ComboboxItemQualifiers qItems = new ComboboxItemQualifiers();
                qItems.Text = jobject.qualifiers[i].description;
                qItems.Value = jobject.qualifiers[i].id;
                cmb_qualifier.Items.Add(qItems);
            }
        }

        void requestPPO(string ip, string pro_id)
        {
            string url = Properties.Settings.Default.ip + ip;
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
            request.AddParameter("pro_id", pro_id);
            IRestResponse response = client.Execute(request);
            var jobject = JsonConvert.DeserializeObject<RPPOS>(response.Content);
            int index = jobject.ppos.Count;
            for (int i = 0; i < index; i++)
            {
                ComboboxItemPPO item = new ComboboxItemPPO();
                item.Text = jobject.ppos[i].description;
                item.Value = jobject.ppos[i].id;
                cmbPPOCPODistrict.Items.Add(item);
            }
        }

        void requestMPS(string ip, string pro_id, string ppo_id)
        {
            string url = Properties.Settings.Default.ip + ip;
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
            request.AddParameter("pro_id", pro_id);
            request.AddParameter("ppo_id", ppo_id);
            IRestResponse response = client.Execute(request);
            var jobject = JsonConvert.DeserializeObject<RMPS>(response.Content);
            int index = jobject.mps.Count;
            for (int i = 0; i < index; i++)
            {
                ComboboxItemMPS item = new ComboboxItemMPS();
                item.Text = jobject.mps[i].description;
                item.Value = jobject.mps[i].id;
                cmbMPSPS.Items.Add(item);
            }
        }

        //for request Data
        public class Qualifier
        {
            public int id { get; set; }
            public string description { get; set; }
        }

        public class Occupation
        {
            public int id { get; set; }
            public string description { get; set; }
        }

        public class Education
        {
            public int id { get; set; }
            public string description { get; set; }
        }

        public class Language
        {
            public int id { get; set; }
            public string description { get; set; }
        }

        public class Hvtslt
        {
            public int id { get; set; }
            public int hvt_slt_id { get; set; }
            public string description { get; set; }
        }

        public class DrugPersonStatu
        {
            public int id { get; set; }
            public string description { get; set; }
            public object created_at { get; set; }
            public object updated_at { get; set; }
        }

        public class ClassifcationSuspect
        {
            public int id { get; set; }
            public int class_suspect_id { get; set; }
            public string description { get; set; }
        }

        public class Ethnic
        {
            public int id { get; set; }
            public string description { get; set; }
        }

        public class Rank
        {
            public int id { get; set; }
            public string code { get; set; }
            public string description { get; set; }
            public object created_at { get; set; }
            public object updated_at { get; set; }
        }

        public class Pro
        {
            public int id { get; set; }
            public string description { get; set; }
            public int seq { get; set; }
            public int cat { get; set; }
            public int pop { get; set; }
        }

        public class Regions
        {
            public int id { get; set; }
            public string abbrev { get; set; }
            public string description { get; set; }
        }

        public class OpnCategory
        {
            public int id { get; set; }
            public string code { get; set; }
            public string description { get; set; }
        }

        public class RData
        {
            public List<Qualifier> qualifiers { get; set; }
            public List<Occupation> occupations { get; set; }
            public List<Education> educations { get; set; }
            public List<Language> languages { get; set; }
            public List<Hvtslt> hvtslts { get; set; }
            public List<DrugPersonStatu> drug_person_status { get; set; }
            public List<ClassifcationSuspect> classifcation_suspect { get; set; }
            public List<Ethnic> ethnics { get; set; }
            public List<Rank> ranks { get; set; }
            public List<Pro> pros { get; set; }
            public List<Regions> regions { get; set; }
            public List<OpnCategory> opn_categories { get; set; }
        }

        public class Ppos
        {
            public int id { get; set; }
            public int pros_id { get; set; }
            public int ppo_id { get; set; }
            public string description { get; set; }
        }

        public class RPPOS
        {
            public List<Ppos> ppos { get; set; }
        }

        public class Mp
        {
            public int id { get; set; }
            public int pro_id { get; set; }
            public int ppo_id { get; set; }
            public string description { get; set; }
            public string uccn { get; set; }
            public string municipal_code { get; set; }
        }

        public class RMPS
        {
            public List<Mp> mps { get; set; }
        }

        public class ComboboxItemPRORegion
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        public class ComboboxItemPPO
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        public class ComboboxItemMPS
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        public class ComboboxItemQualifiers
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }


        private void cmbPRORegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbPPOCPODistrict.Text = "";
            cmbPPOCPODistrict.Items.Clear();
            requestPPO("/api/ppos", (cmbPRORegion.SelectedItem as ComboboxItemPRORegion).Value.ToString());
        }

        private void cmbPPOCPODistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbMPSPS.Text = "";
            cmbMPSPS.Items.Clear();
            requestMPS("/api/mps", (cmbPRORegion.SelectedItem as ComboboxItemPRORegion).Value.ToString(), (cmbPPOCPODistrict.SelectedItem as ComboboxItemPPO).Value.ToString());
        }

        private void cmbMPSPS_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            searchPersonalities("/api/personalities/search");
        }

        void searchPersonalities(string ip)
        {
            try
            {
                string url = Properties.Settings.Default.ip + ip;
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
                request.AddParameter("pro_id", (cmbPRORegion.SelectedItem as ComboboxItemPRORegion).Value.ToString());
                request.AddParameter("ppo_id", (cmbPPOCPODistrict.SelectedItem as ComboboxItemPPO).Value.ToString());
                request.AddParameter("mps_id", (cmbMPSPS.SelectedItem as ComboboxItemMPS).Value.ToString());
                request.AddParameter("firstname", txt_firstname.Text);
                request.AddParameter("lastname", txt_lastname.Text);
                request.AddParameter("middlename", txt_middlename.Text);
                request.AddParameter("alias", txt_alias.Text);
                request.AddParameter("qualifier_id", cmb_qualifier.SelectedText);
                request.AddParameter("birthday", dtp_birthday.Text);
                request.AddParameter("data_owner", LoginWindow.apiCon.data_owner);
                IRestResponse response = client.Execute(request);
                if ((int)response.StatusCode == 200)
                {
                    
                    var jobject = JsonConvert.DeserializeObject<RootSearchPersonalities.RootObject>(response.Content);
                    Console.WriteLine(jobject.personalities.Count + " personalities found");
                    string name;
                    if (jobject.personalities.Count == 0)
                    {
                        dgvPersonalities.Columns.Clear();
                        dgvPersonalities.Columns.Add("colNullResult", "No Records Found");
                        return;
                    }
                    dgvPersonalities.Columns.Clear();
                    loadDGVColumns();
                    dgvPersonalities.Rows.Clear();
                    if (jobject.personalities.Count != 0)
                    {
                        for (int i = 0; i < jobject.personalities.Count; i++)
                        {
                            name = jobject.personalities[i].first_name + " " + jobject.personalities[i].last_name;

                            dgvPersonalities.Rows.Add(new object[] {
                    jobject.personalities[i].person_id,
                    name,
                    jobject.personalities[i].regional_office.description,
                    jobject.personalities[i].provincial_office.description,
                    jobject.personalities[i].police_station.description,
                    jobject.personalities[i].category_hvtslt.description,
                    jobject.personalities[i].class_suspect.description,
                    jobject.personalities[i].drugperson_status.description,
                    jobject.personalities[i].date_entry_watchlist,
                    jobject.personalities[i].listed
                });
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void loadDGVColumns()
        {
            dgvPersonalities.Columns.Add("colPID", "Person ID");
            dgvPersonalities.Columns[0].Visible = false;
            dgvPersonalities.Columns.Add("colName", "Name");
            dgvPersonalities.Columns.Add("colPRO", "PRO");
            dgvPersonalities.Columns.Add("colPPO", "PPO");
            dgvPersonalities.Columns.Add("colPS", "PS");
            dgvPersonalities.Columns.Add("colHVTSLT", "HVT/SLT");
            dgvPersonalities.Columns.Add("colClassification", "Classification");
            dgvPersonalities.Columns.Add("colStatus", "Status");
            dgvPersonalities.Columns.Add("colDateEntry", "Date of Entry");
            dgvPersonalities.Columns.Add("colListedBy", "Listed by DI");
        }

        private void btn_loadPersonalities_Click(object sender, EventArgs e)
        {
            dgvPersonalities.Rows.Clear();
            loadPersonalities();
        }
    }
}
