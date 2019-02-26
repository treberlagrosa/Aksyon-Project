using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using GBMSGUI_NET;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_ErrorCodesDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_VisualInterfaceLCDDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_AcquisitionProcessDefines;
using AN2K_NET_WRAPPER;
using JPEG_NET_WRAPPER;
using WSQPACK_NET_WRAPPER;
using System.Text.RegularExpressions;
using An2k_2011_NetWrapper;
using An2k_Engine_Net_Wrapper;
#if GBDCGUI_DEMO
using GBDCGUI_Net;
#endif


namespace Aksyon_Project
{
    public partial class UserDataWindow : Form
    {
        public String ImagesPath;       // path where to store/read images
        public bool ViewMode;           // true = view mode; false = new mode
        public bool FingerprintCard;    // true = Acquire from Fingerprint Card

        //private DemoForm.UserData UserData; // class where to store user data
        //private DemoForm DemoFormRef;       // reference to main form
        private Byte[] ImageBuffer;         // buffer for images
        private GBMSGUI MyGUI = new GBMSGUI();  // GBMSGUI class   
        public UserDataWindow()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
