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

            LoadUsersList();
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            // refresh devices list
            btnReload_Click(this, null);
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
                MessageBox.Show("No attached device found!",
                    Application.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    Item.UserName = Data.Surname + " " + Data.Name;
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
    }
}
