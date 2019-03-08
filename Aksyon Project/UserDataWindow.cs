using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.Net;
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
using RestSharp;
using Newtonsoft.Json;
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

        private MainWindow.UserData UserData; // class where to store user data
        private MainWindow DemoFormRef;       // reference to main form
        private Byte[] ImageBuffer;         // buffer for images
        private GBMSGUI MyGUI = new GBMSGUI();  // GBMSGUI class
        public UserDataWindow()
        {
            InitializeComponent();
        }

        private class ScanItem : Object
        {
            public uint ScanObjID;  // ID of the object
            public String Text;     // Descriptive text (used also to build filename)
            public PictureBox ImagePictureBox;  // picturebox where to show image
            public bool Supported;              // if object supported by the current scanner
            public ScanItemData ItemData = new ScanItemData();  // other data collected after scanning

            public ScanItem(uint ScanObjID, String Text, PictureBox ImagePictureBox)
            {
                this.ScanObjID = ScanObjID;
                this.Text = Text;
                this.ImagePictureBox = ImagePictureBox;
            }

            public override String ToString()
            {
                return Text;
            }
        }

        private ScanItem CurrentScanItem;

        // data for scan items
        private class ScanItemData
        {
            public int Quality;
            public int QualityAlgorithm;
            public int UnavailabilityReason;
        }

        // delegate for OnAfterStart
        public delegate bool AfterStartEvent();

        public AfterStartEvent AfterStartDelegate;

        public UserDataWindow(MainWindow DemoFormRef)
        {
            // reference to main form
            this.DemoFormRef = DemoFormRef;

            FingerprintCard = false;

            InitializeComponent();

            // delegate for callback
            AfterStartDelegate = new AfterStartEvent(OnAfterStart);

            // assign popupmenu to items
            //pboxLeftThumb.ContextMenu = popAcquireItemMenu;
            //pboxLeftIndex.ContextMenu = popAcquireItemMenu;
            //pboxLeftMiddle.ContextMenu = popAcquireItemMenu;
            //pboxLeftRing.ContextMenu = popAcquireItemMenu;
            //pboxLeftLittle.ContextMenu = popAcquireItemMenu;
            //pboxRightThumb.ContextMenu = popAcquireItemMenu;
            //pboxRightIndex.ContextMenu = popAcquireItemMenu;
            //pboxRightMiddle.ContextMenu = popAcquireItemMenu;
            //pboxRightRing.ContextMenu = popAcquireItemMenu;
            //pboxRightLittle.ContextMenu = popAcquireItemMenu;
        }

        private void UserDataWindow_Load(object sender, EventArgs e)
        {
            byte DeviceID;

            // handle to the form instance to pass to callback
            //gcUserDataFormHandle = GCHandle.Alloc(this, GCHandleType.Normal);

            // load scanned objects
            LoadScanObjectsList();

            txtPesonalityName.Text = MainWindow.selectedPersonality.name;

            getFingers();
            return;

            if (!ViewMode)
            {
                // set form's caption
                Text = "Acquire Person Fingerprint";

                txtPesonalityName.Text = MainWindow.selectedPersonality.name;

                UserData = new MainWindow.UserData();

                // set device information
                if (DemoFormRef.Resolution1000Dpi)
                    UserData.AcquisitionDpi = 1000;
                else
                    UserData.AcquisitionDpi = 500;
                if (!FingerprintCard)
                {
                    UserData.DeviceName = MainWindow.GetDeviceName(DemoFormRef.CurrentDevice.DeviceID);
                    UserData.DeviceSerialNumber = DemoFormRef.CurrentDevice.DeviceSerialNumber;

                    // allocate buffer for max image size from scanner
                    ImageBuffer = new Byte[DemoFormRef.MaxImageBufferSize];
                }
                else
                {
                    UserData.DeviceName = "Fingerprint Card";
                    UserData.DeviceSerialNumber = "";
                }

                // create a new folder name
                ImagesPath = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + Path.GetRandomFileName();
                if (!Directory.Exists(ImagesPath))
                {
                    try
                    {
                        Directory.CreateDirectory(ImagesPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        this.Close();
                        return;
                    }
                }

                if (!FingerprintCard)
                {
                    // load configuration for types for sequence
                    GBMSGUI.CheckMask(DemoFormRef.DemoConfig.SequenceTypes, MainWindow.SequenceType.Rolled);

                    // enable only supported objects
                    uint ObjectTypes;
                    GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetScannableTypes(out ObjectTypes);
                    foreach (ScanItem Item in lstScannedObjects.Items)
                    {
                        if (GBMSGUI.CheckMask(ObjectTypes, GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(Item.ScanObjID)))
                            Item.Supported = true;
                        else
                            Item.Supported = false;
                    }

                    DeviceID = DemoFormRef.CurrentDevice.DeviceID;
                    Application.DoEvents();

                    if (DemoFormRef.LCDPresent)
                    {
                        btnAcquire.Text = btnAcquire.Text + "\n(touch button on LCD)";

                        // enable start button on LCD
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_EnableStartButtonOnLogoScreen();
                        // start timer
                        TouchScreenTimer.Enabled = true;
                    }
                }
                else
                {
                    btnAcquire.Visible = false;
                }
            }
            else
            {
                // set form's caption
                Text = "View User";
                //txtPesonalityName.ReadOnly = true;
                btnAcquire.Visible = false;

                // read user data
                UserData = MainWindow.UserData.Deserialize(ImagesPath + Path.DirectorySeparatorChar + "UserData.xml");
                txtPesonalityName.Text = UserData.Name;

                // read other data
                ReadItemsData();

                DeviceID = MainWindow.GetDeviceIDFromName(UserData.DeviceName);
                Application.DoEvents();

                // load all images
                Cursor = Cursors.WaitCursor;
                LoadSavedImages();
                Cursor = Cursors.Default;

                //V1.10
                // for now not supported
                //EnableViewEJI();
            }
        }

        private void LoadScanObjectsList()
        {
            ScanItem Item;

            lstScannedObjects.Items.Clear();


            //V1.10 - group by finger
            // Left Thumb
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB, "Left Rolled Thumb", pboxLeftThumb);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX, "Left Rolled Index", pboxLeftIndex);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE, "Left Rolled Middle", pboxLeftMiddle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING, "Left Rolled Ring", pboxLeftRing);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE, "Left Rolled Little", pboxLeftLittle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB, "Right Rolled Thumb", pboxRightThumb);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX, "Right Rolled Index", pboxRightIndex);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE, "Right Rolled Middle", pboxRightMiddle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING, "Right Rolled Ring", pboxRightRing);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE, "Right Rolled Little", pboxRightLittle);
            lstScannedObjects.Items.Add(Item);
        }

        private void Acquire()
        {
            int Index;
            int ret;
            int ImageSizeX, ImageSizeY;
            int Resolution;
            Bitmap bmp;
            uint AcqOptions = DemoFormRef.DemoConfig.AcquisitionOptions;
            uint SessionOpt = DemoFormRef.DemoConfig.SessionOptions; ;
            String PersonID;
            int AcquisitionMode;
            int QualityThreshold1, QualityThreshold2;

            byte DeviceID = DemoFormRef.CurrentDevice.DeviceID;

            uint DeviceFeatures;
            GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetDeviceFeatures(out DeviceFeatures);

            // disable TouchScreen
            TouchScreenTimer.Enabled = false;

            PersonID = txtPesonalityName.Text;

            // set AfterStartCallback (if needed)
            //gcUserDataFormHandle = GCHandle.Alloc(this); // pass pointer to the form instance to callback
            //MyGUI.SetAfterStartCallback(AfterStartCallbackRef, GCHandle.ToIntPtr(gcUserDataFormHandle));
            // use non-static callback
            MyGUI.SetAfterStartCallback(AfterStartCallback, IntPtr.Zero);

            // set quality algorithm
            ret = MyGUI.SetQualityAlgorithm(DemoFormRef.DemoConfig.IAFIsQualityAlgorithm);
            if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
            {
                MessageBox.Show(MyGUI.GetErrorMessage() + " (SetQualityAlgorithm)",
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // set the thresholds according to the algorithm
            // IMPORTANT NOTE: the following thresholds are set more permissive for DEMO purpose;
            // don't use the values as an example, define your thresholds according to your application needs;
            if (DemoFormRef.DemoConfig.IAFIsQualityAlgorithm == GBMSGUI.QualityAlgorithms.NFIQAlgorithm)
            {
                //QualityThreshold1 = 3;
                //QualityThreshold2 = 4;
                // 2.0.0.0
                QualityThreshold1 = DemoFormRef.DemoConfig.NFIQQualityThreshold1;
                QualityThreshold2 = DemoFormRef.DemoConfig.NFIQQualityThreshold2;
            }
            else if (DemoFormRef.DemoConfig.IAFIsQualityAlgorithm == GBMSGUI.QualityAlgorithms.NFIQ2Algorithm)
            {
                // 2.0.0.0
                QualityThreshold1 = DemoFormRef.DemoConfig.NFIQ2QualityThreshold1;
                QualityThreshold2 = DemoFormRef.DemoConfig.NFIQ2QualityThreshold2;
            }
            else // GB algorithm
            {
                //QualityThreshold1 = 50;
                //QualityThreshold2 = 70;
                // 2.0.0.0
                QualityThreshold1 = DemoFormRef.DemoConfig.GBQualityThreshold1;
                QualityThreshold2 = DemoFormRef.DemoConfig.GBQualityThreshold2;
            }

            /*
            // other settings - for demo a little more permissive
            // see the NOTE above
            MyGUI.SetArtefactsThresholds(15, 30);
            MyGUI.SetLowerPalmCompletenessThresholds(70, 80);
            MyGUI.SetBlockAutoCaptureContrast(DemoFormRef.DemoConfig.BlockAutocaptureContrast);
            MyGUI.SetPatternValidityThreshold(65);
            MyGUI.SetPatternCompletenessThreshold(75);
            */
            // 2.0.0.0
            //MyGUI.SetArtefactsThresholds(DemoFormRef.DemoConfig.ArtefactsThreshold1, DemoFormRef.DemoConfig.ArtefactsThreshold2);
            // 2.0.1.0 - moved in the for, to adapt threshold for different objects

            MyGUI.SetArtefactsThresholds(DemoFormRef.DemoConfig.ArtefactsThreshold1, DemoFormRef.DemoConfig.ArtefactsThreshold2);
            MyGUI.SetLowerPalmCompletenessThresholds(DemoFormRef.DemoConfig.LowerPalmCompletenessThreshold1, DemoFormRef.DemoConfig.LowerPalmCompletenessThreshold2);
            // TODO ?
            MyGUI.SetBlockAutoCaptureContrast(DemoFormRef.DemoConfig.BlockAutocaptureContrast);
            MyGUI.SetPatternValidityThreshold(DemoFormRef.DemoConfig.PatternValidityThreshold);
            MyGUI.SetPatternCompletenessThreshold(DemoFormRef.DemoConfig.PatternCompletenessThreshold);

            // set window size and position
            ret = MyGUI.SetWindowSizeAndPosition(DemoFormRef.DemoConfig.WindowSize.X,
                DemoFormRef.DemoConfig.WindowSize.Y,
                DemoFormRef.DemoConfig.WindowSize.Width,
                DemoFormRef.DemoConfig.WindowSize.Height,
                DemoFormRef.DemoConfig.WindowMaximized);
            // ignore error
            /*
            if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
            {
                MessageBox.Show(MyGUI.GetErrorMessage() + " (SetWindowSizeAndPosition)",
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            */

            MyGUI.EnableBlockAutocaptureLedColorFeedback(Convert.ToInt32(DemoFormRef.DemoConfig.EnableBlockAutocaptureLedColorFeedback));

            // because acquisition form shown by GBMSGUI is not modal, disable buttons
            btnAcquire.Enabled = false;
            
            AcquisitionMode = GBMSGUI.AcquisitionModes.SingleAcquisition;

            // 2.0.0.0
            if (GBMSGUI.CheckMask(SessionOpt, GBMSGUI.SessionOption.SWFakeFingerDetection)
                && GBMSGUI.CheckMask(DeviceFeatures, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_SW_ANTIFAKE))
            {
                ret = GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_SetSoftwareFakeFingerDetectionThreshold(DemoFormRef.DemoConfig.SWFakeFingerDetectionThreshold);
                if (ret != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    MessageBox.Show(MyGUI.GetMSAPIErrorMessage("GBMSAPI_NET_SetSoftwareFakeFingerDetectionThreshold", ret),
                            Application.ProductName,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            // 2.0.0.0
            if (GBMSGUI.CheckMask(DeviceFeatures, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_HW_ANTIFAKE))
            {
                ret = GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_SetHardwareFakeFingerDetectionThreshold(DemoFormRef.DemoConfig.HWFakeFingerDetectionThreshold);
                if (ret != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                {
                    MessageBox.Show(MyGUI.GetMSAPIErrorMessage("GBMSAPI_NET_SetHardwareFakeFingerDetectionThreshold", ret),
                            Application.ProductName,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // start the acquisition session
            ret = MyGUI.StartSession(AcquisitionMode, PersonID, SessionOpt);
            if (ret != GBMSGUI.ReturnCodes.Ret_Success)
            {
                // re-enable our controls
                btnAcquire.Enabled = true;

                if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
                    MessageBox.Show(MyGUI.GetErrorMessage() + " (StartSession)",
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("StartSession error " + ret.ToString(),
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MyGUI.SelectFingerContactEvaluationMode(DemoFormRef.DemoConfig.FingerContactEvaluationMode);

            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                MyGUI.SetAdaptiveRollDirection(DemoFormRef.DemoConfig.RollDirection);

            MyGUI.SetBlockAutoCaptureMask(DemoFormRef.DemoConfig.BlockAutocaptureMask);

            MyGUI.SetIgnoreDiagnosticMask(DemoFormRef.DemoConfig.IgnoredDiagnosticMask);

            // get object Id from list
            //foreach (ScanItem Item in lstScannedObjects.CheckedItems)
            ScanItem Item;
            for (Index = 0; Index < lstScannedObjects.CheckedItems.Count; Index++)
            {
                Item = (ScanItem)lstScannedObjects.CheckedItems[Index];

                // skip items not supported by the scanner
                if (!Item.Supported)
                    continue;

                // skip Two thumbs for DS40
                if ((DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I) &&
                    (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS))
                    continue;

                // save current item (used in AfterStartCallback)
                CurrentScanItem = Item;

                // set roll area size (if supported)
                if (GBMSGUI.IsRolled(Item.ScanObjID) &&
                    (GBMSGUI.CheckMask(DeviceFeatures, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_GA)))
                {
                    if (DemoFormRef.DemoConfig.RollAreaSize == GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_IQS)
                        //GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_SetRollAreaStandard(GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_IQS);
                        MyGUI.SetRollArea(GBMSGUI.RollAreaType.RollAreaIQS);
                    else
                        //GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_SetRollAreaStandard(GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_GA);
                        MyGUI.SetRollArea(GBMSGUI.RollAreaType.RollAreaGA);
                }

                // 2.0.1.0 - adapt threshold for different objects
                if (GBMSGUI.IsRolledJoint(Item.ScanObjID))
                    MyGUI.SetArtefactsThresholds(DemoFormRef.DemoConfig.ArtefactsThreshold1 * 3, DemoFormRef.DemoConfig.ArtefactsThreshold2 * 3);
                else if (GBMSGUI.IsRolledThenar(Item.ScanObjID) || GBMSGUI.IsRolledHypothenar(Item.ScanObjID))
                    MyGUI.SetArtefactsThresholds(DemoFormRef.DemoConfig.ArtefactsThreshold1 * 5, DemoFormRef.DemoConfig.ArtefactsThreshold2 * 5);
                else
                    MyGUI.SetArtefactsThresholds(DemoFormRef.DemoConfig.ArtefactsThreshold1, DemoFormRef.DemoConfig.ArtefactsThreshold2);

                // set image size
                ret = SetImageSize(Item.ScanObjID);
                if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
                {
                    MessageBox.Show(MyGUI.GetErrorMessage() + " (SetImageSize)",
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //break;      // break sequence
                }

                // set size for segments
                // 1.14.0.0 - also for upper palm
                if (GBMSGUI.IsSlap(Item.ScanObjID) || GBMSGUI.IsUpperPalm(Item.ScanObjID))
                {
                    ret = SetSegmentsImageSize(Item.ScanObjID);
                    if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
                    {
                        MessageBox.Show(MyGUI.GetErrorMessage() + " (SetSegmentsImageSize)",
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        //MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //break;      // break sequence
                    }
                }

                // quality thresholds for each object
                ret = MyGUI.SetQualityThresholds(Item.ScanObjID, QualityThreshold1, QualityThreshold2);
                if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
                {
                    MessageBox.Show(MyGUI.GetErrorMessage() + " (SetQualityThresholds)",
                        Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;      // break sequence
                }

                // 1.14.0.0 - also for upper palm
                if (GBMSGUI.IsSlap(Item.ScanObjID) || GBMSGUI.IsUpperPalm(Item.ScanObjID))
                {
                    // quality thresholds for segments
                    MyGUI.SetSegmentQualityThresholds(Item.ScanObjID, 0, QualityThreshold1, QualityThreshold2);
                    MyGUI.SetSegmentQualityThresholds(Item.ScanObjID, 1, QualityThreshold1, QualityThreshold2);
                    if ((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                        (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                        // 2.0.1.0 - was missing
                        (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT) ||
                        (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
                    {
                        MyGUI.SetSegmentQualityThresholds(Item.ScanObjID, 2, QualityThreshold1, QualityThreshold2);
                        MyGUI.SetSegmentQualityThresholds(Item.ScanObjID, 3, QualityThreshold1, QualityThreshold2);
                    }

                    // missing fingers
                    SetMissingFingers();
                }

                //if (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT)
                //    // TEST - set left ring as unavailable
                //    MyGUI.SetSegmentUnavailabilityReason(Item.ScanObjID, 2, GBMSGUI.UnavailabilityReason.Unprintable);

                // 2.3.0.0
                // 2.3.0.1 - moved before SetFrameRate (it affects the frame rate used)
                if (GBMSGUI.CheckMask(DeviceFeatures, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_USB_3_0_SUPPORT))
                {
                    // get link speed
                    byte USBLinkSpeed = 0;
                    GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_GetUsbLinkSpeed(out USBLinkSpeed);
                    if (USBLinkSpeed == GBMSAPI_NET_UsbLinkValues.GBMSAPI_NET_USB_LINK_SUPER_SPEED)
                    {
                        // if connected in USB3, force full-res preview
                        AcqOptions |= GBMSGUI.AcquisitionOption.FullResPreview;
                    }
                }

                // set frame rate
                if (GBMSGUI.CheckMask(DeviceFeatures, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_FRAME_RATE_SETTING))
                {
                    //double FrameRate = 5;
                    double FrameRate = 0;
                    uint FrameRateOptions = 0;

                    if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS26)
                        FrameRate = DemoFormRef.DemoConfig.DS26FrameRate;
                    else if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84)
                    {
                        if (GBMSGUI.IsRolled(Item.ScanObjID))
                            FrameRate = DemoFormRef.DemoConfig.DS84PartialFrameRate;
                        else
                            FrameRate = DemoFormRef.DemoConfig.DS84FullFrameRate;
                    }
                    else if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I)
                    {
                        if (GBMSGUI.IsRolled(Item.ScanObjID))
                            FrameRate = DemoFormRef.DemoConfig.DS40iPartialFrameRate;
                        else
                            FrameRate = DemoFormRef.DemoConfig.DS40iFullFrameRate;
                    }
                    else if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C)
                    {
                        if (GBMSGUI.IsRolled(Item.ScanObjID))
                            FrameRate = DemoFormRef.DemoConfig.DS84cPartialFrameRate;
                        else
                        {
                            // hi or low res
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.FullResPreview))
                                FrameRate = DemoFormRef.DemoConfig.DS84cFullHiResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.DS84cFullLowResFrameRate;
                        }
                    }
                    else if ((DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517) ||
                        (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MSC517))
                    {
                        if (GBMSGUI.IsRolled(Item.ScanObjID))
                        {
                            // AdaptRollArea has same setting as Full low res
                            if (DemoFormRef.DemoConfig.RollAreaSize == GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_IQS)
                            {
                                if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                    FrameRate = DemoFormRef.DemoConfig.MC517FullLowResFrameRate;
                                else
                                    FrameRate = DemoFormRef.DemoConfig.MC517PartialIQSFrameRate;
                            }
                            else
                            {
                                if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                    FrameRate = DemoFormRef.DemoConfig.MC517FullLowResFrameRate;
                                else
                                    FrameRate = DemoFormRef.DemoConfig.MC517PartialGAFrameRate;
                            }
                        }
                        else
                        {
                            // hi or low res
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.FullResPreview))
                                FrameRate = DemoFormRef.DemoConfig.MC517FullHiResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MC517FullLowResFrameRate;
                        }
                    }
                    else if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32)
                    {
                        if (GBMSGUI.IsRolled(Item.ScanObjID))
                            FrameRate = DemoFormRef.DemoConfig.DS32PartialFrameRate;
                        else
                            FrameRate = DemoFormRef.DemoConfig.DS32FullFrameRate;
                    }
                    else if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527)
                    {
                        // AdaptRollArea has same setting as Full low res
                        if (GBMSGUI.IsRolledThenar(Item.ScanObjID)
                            // 1.15.0.0
                            || GBMSGUI.IsRolledHypothenar(Item.ScanObjID))
                        {
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                FrameRate = DemoFormRef.DemoConfig.MS527FullLowResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527PartialThenarFrameRate;
                        }
                        else if (GBMSGUI.IsRolledJoint(Item.ScanObjID))
                        {
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                FrameRate = DemoFormRef.DemoConfig.MS527FullLowResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527PartialJointFrameRate;
                        }
                        else if (GBMSGUI.IsRolled(Item.ScanObjID))
                        {
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                FrameRate = DemoFormRef.DemoConfig.MS527FullLowResFrameRate;
                            else
                            {
                                if (DemoFormRef.DemoConfig.RollAreaSize == GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_IQS)
                                    FrameRate = DemoFormRef.DemoConfig.MS527PartialIQSFrameRate;
                                else
                                    FrameRate = DemoFormRef.DemoConfig.MS527PartialGAFrameRate;
                            }
                        }
                        else
                        {
                            // hi or low res
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.FullResPreview))
                                FrameRate = DemoFormRef.DemoConfig.MS527FullHiResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527FullLowResFrameRate;
                        }
                    }
                    else if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84t)
                    {
                        if (GBMSGUI.IsRolled(Item.ScanObjID))
                            FrameRate = DemoFormRef.DemoConfig.DS84tPartialFrameRate;
                        else
                        {
                            // hi or low res
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.FullResPreview))
                                FrameRate = DemoFormRef.DemoConfig.DS84tFullHiResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.DS84tFullLowResFrameRate;
                        }
                    }
                    // 2.0.1.0
                    else if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DSID20)
                        FrameRate = DemoFormRef.DemoConfig.DID20FrameRate;
                    // 2.3.0.0
                    else if (DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t)
                    {
                        // AdaptRollArea has same setting as Full low res
                        if (GBMSGUI.IsRolledThenar(Item.ScanObjID)
                            // 1.15.0.0
                            || GBMSGUI.IsRolledHypothenar(Item.ScanObjID))
                        {
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                FrameRate = DemoFormRef.DemoConfig.MS527tFullLowResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527tPartialThenarFrameRate;
                        }
                        else if (GBMSGUI.IsRolledJoint(Item.ScanObjID))
                        {
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                FrameRate = DemoFormRef.DemoConfig.MS527tFullLowResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527tPartialJointFrameRate;
                        }
                        else if (GBMSGUI.IsRolled(Item.ScanObjID))
                        {
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                FrameRate = DemoFormRef.DemoConfig.MS527tFullLowResFrameRate;
                            else
                            {
                                if (DemoFormRef.DemoConfig.RollAreaSize == GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_IQS)
                                    FrameRate = DemoFormRef.DemoConfig.MS527tPartialIQSFrameRate;
                                else
                                    FrameRate = DemoFormRef.DemoConfig.MS527tPartialGAFrameRate;
                            }
                        }
                        else
                        {
                            // hi or low res
                            if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.FullResPreview))
                                FrameRate = DemoFormRef.DemoConfig.MS527tFullHiResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527tFullLowResFrameRate;
                        }
                    }


                    //if (GBMSGUI.IsRolled(Item.ScanObjID))
                    //    FrameRateOptions |= GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ROLL_AREA;
                    //if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.FullResPreview))
                    //FrameRateOptions |= GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE;

                    uint SupportedScanAreas;
                    GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetSupportedScanAreas(out SupportedScanAreas);

                    uint ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;

                    if (GBMSGUI.IsRolledThenar(Item.ScanObjID)
                        // 1.15.0.0
                        || GBMSGUI.IsRolledHypothenar(Item.ScanObjID))
                    {
                        ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_THENAR;
                    }
                    else if (GBMSGUI.IsRolledJoint(Item.ScanObjID) ||
                         GBMSGUI.IsFlatJoint(Item.ScanObjID))
                    {
                        ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                    }
                    else if (GBMSGUI.IsRolled(Item.ScanObjID))
                    {
                        if (DemoFormRef.DemoConfig.RollAreaSize == GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_GA)
                        {
                            if (GBMSGUI.CheckMask(SupportedScanAreas, GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA))
                                ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                            else
                                ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                        }
                        else
                        {
                            if (GBMSGUI.CheckMask(SupportedScanAreas, GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS))
                                ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                            else
                                ScanArea = GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                        }
                    }

                    if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                        FrameRateOptions |= GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ADAPT_ROLL_AREA_POSITION;

                    if ((ScanArea == GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME) &&
                        GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.FullResPreview))
                        FrameRateOptions |= GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_HIGH_RES_IN_PREVIEW;

                    // obsolete
                    /*
                    ret = GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_SetFrameRateComplete(FrameRateOptions, FrameRate);
                    if (ret != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        MessageBox.Show(MyGUI.GetMSAPIErrorMessage("GBMSAPI_NET_SetFrameRateComplete", ret),
                                Application.ProductName,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;      // break sequence
                    }
                    */

                    if (FrameRate != 0) // 2.2.0.0
                    {
                        ret = GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_SetFrameRate2(ScanArea, FrameRateOptions, FrameRate);
                        if (ret != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                        {
                            MessageBox.Show(MyGUI.GetMSAPIErrorMessage("GBMSAPI_NET_SetFrameRate2", ret),
                                    Application.ProductName,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // ignore error and continue
                            //break;      // break sequence
                        }
                    }
                }

                // set Block composition (if supported)
                if (GBMSGUI.IsRolled(Item.ScanObjID) &&
                    (GBMSGUI.CheckMask(DeviceFeatures, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ENABLE_BLOCK_ROLL_COMPOSITION)))
                {
                    ret = GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_ROLL_EnableBlockComposition(DemoFormRef.DemoConfig.EnableBlockComposition);
                    if (ret != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
                    {
                        MessageBox.Show(MyGUI.GetMSAPIErrorMessage("GBMSAPI_NET_ROLL_EnableBlockComposition", ret),
                                Application.ProductName,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // ignore error and continue
                    }
                }

                // manage joint segmentation option
                if (GBMSGUI.IsJoint(Item.ScanObjID) && !DemoFormRef.DemoConfig.JointSegmentation)
                    AcqOptions &= ~GBMSGUI.AcquisitionOption.Segmentation;

                if (GBMSGUI.CheckMask(AcqOptions, GBMSGUI.AcquisitionOption.LiveSegmentsEval))
                    MyGUI.SetLiveSegmEvalTimeout(DemoFormRef.DemoConfig.LiveSegmEvalTimeout);

                Repeat:
                // 2.0.1.0 - reset current item
                ResetScanItemData(Item);

                // acquire item
                ret = MyGUI.Acquire(Item.ScanObjID, AcqOptions, ImageBuffer, out ImageSizeX, out ImageSizeY, out Resolution);

                // if single acquisition (acquisition window is hidden after acquire), re-enable our window now
                if (AcquisitionMode == GBMSGUI.AcquisitionModes.SingleAcquisition)
                {
                    btnAcquire.Enabled = true;
                    //mnuAcquireItem.Enabled = true;
                }

                if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
                {
                    DialogResult Res = MessageBox.Show(MyGUI.GetErrorMessage() + " (Acquire)",
                        Application.ProductName,
                        MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation);
                    if (Res == DialogResult.Abort)
                        break;      // break sequence
                    else if (Res == DialogResult.Retry)
                    {
                        // repeat same acquisition
                        goto Repeat;
                    }
                    else if (Res == DialogResult.Ignore)
                        continue;   // go on
                }
                else if (ret == GBMSGUI.ReturnCodes.Ret_BreakSequence)
                {
                    break;
                }
                else if (ret == GBMSGUI.ReturnCodes.Ret_SkipImage)
                {
                    // if option is set, get unavailability reason
                    if (GBMSGUI.CheckMask(SessionOpt, GBMSGUI.SessionOption.AskUnavailabilityReason))
                    {
                        MyGUI.GetUnavailabilityReason(Item.ScanObjID, out Item.ItemData.UnavailabilityReason);
                        // 2.0.1.0
                        DisableScanItemSameFinger(Item.ScanObjID, -1, Item.ItemData.UnavailabilityReason);
                    }

                    continue;   // go on
                }
                else if (ret == GBMSGUI.ReturnCodes.Ret_GoBack)
                {
                    if (Index > 0)
                    {
                        Index--;
                        Index--;

                        continue;
                    }
                    else
                        break;
                }
                else if (ret == GBMSGUI.ReturnCodes.Ret_Success)
                {
                    // save image in Bmp format
                    bmp = GBMSGUI.RawImageToBitmap(ImageBuffer, ImageSizeX, ImageSizeY);
                    String FileName = BuildImageFileName(Item);
                    bmp.SetResolution((float)Resolution, (float)Resolution);
                    bmp.Save(FileName, ImageFormat.Bmp);
                    bmp.Dispose();
                    // display image
                    //Item.ImagePictureBox.Load(FileName);
                    // .NET 4.0 fix
                    LoadPictureBoxImage(Item.ImagePictureBox, FileName);

                    // save quality
                    ret = MyGUI.GetIAFISQuality(Item.ScanObjID, DemoFormRef.DemoConfig.IAFIsQualityAlgorithm,
                        out Item.ItemData.Quality);
                    Item.ItemData.QualityAlgorithm = DemoFormRef.DemoConfig.IAFIsQualityAlgorithm;
                }
            }

            // stop session
            MyGUI.StopSession();

            // save current window size and position
            int WLeft, WTop, WWidth, WHeight;
            bool WMaximized;
            MyGUI.GetWindowSizeAndPosition(out WLeft, out WTop, out WWidth, out WHeight, out WMaximized);
            Rectangle wRect = new Rectangle(WLeft, WTop, WWidth, WHeight);
            DemoFormRef.DemoConfig.WindowSize = wRect;
            DemoFormRef.DemoConfig.WindowMaximized = WMaximized;

            // re-enable our controls
            btnAcquire.Enabled = true;
            //mnuAcquireItem.Enabled = true;
        }

        private void ResetScanItemsData()
        {
            foreach (ScanItem Item in lstScannedObjects.Items)
            {
                Item.ItemData.Quality = 0;
                Item.ItemData.QualityAlgorithm = 0;
                Item.ItemData.UnavailabilityReason = 0;

                //if ((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                //    // 2.0.1.0
                //    (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT))
                //    ResetSlapSegmentsData(LeftSlapSegmentsData, 4);
                //else if ((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                //    // 2.0.1.0
                //    (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
                //    ResetSlapSegmentsData(RightSlapSegmentsData, 4);
                //else if (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS)
                //    ResetSlapSegmentsData(TwoThumbsSegmentsData, 2);
            }
        }

        private int SetImageSize(uint ScanObjID)
        {
            uint ObjectType = GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(ScanObjID);
            int ret;
            double ImageWidth, ImageHeight; // size in inches
            uint PixelWidth, PixelHeight;   // size in pixels

            ImageWidth = ImageHeight = 0;

            // read from configuration the size for the current object
            if (GBMSGUI.IsUpperPalm(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.UpperPalmWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.UpperPalmHeight;
            }
            else if (GBMSGUI.IsLowerPalm(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.LowerPalmWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.LowerPalmHeight;
            }
            else if (GBMSGUI.IsWritersPalm(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.WritersPalmWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.WritersPalmHeight;
            }
            else if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.FourFingersWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.FourFingersHeight;
            }
            else if (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS)
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.TwoThumbsWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.TwoThumbsHeight;
            }
            else if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_THUMB) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_THUMB))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.FlatThumbWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.FlatThumbHeight;
            }
            else if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledThumbWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledThumbHeight;
            }
            else if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledIndexWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledIndexHeight;
            }
            else if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledMiddleWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledMiddleHeight;
            }
            else if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledRingWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledRingHeight;
            }
            else if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledLittleWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledLittleHeight;
            }
            //V1.10 - start
            else if (GBMSGUI.IsRolledJoint(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledJointWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledJointHeight;
            }
            else if (GBMSGUI.IsFlatJoint(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.FlatJointWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.FlatJointHeight;
            }
            else if (GBMSGUI.IsRolledTip(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledTipWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledTipHeight;
            }
            else if (GBMSGUI.IsRolledThenar(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledThenarWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledThenarHeight;
            }
            //V1.10 - end
            // 1.15.0.0
            else if (GBMSGUI.IsRolledHypothenar(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.RolledHypothenarWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.RolledHypothenarHeight;
            }
            else if (GBMSGUI.IsFlatSingleFinger(ScanObjID))
            {
                ImageWidth = DemoFormRef.DemoConfig.ImageSize.FlatFingerWidth;
                ImageHeight = DemoFormRef.DemoConfig.ImageSize.FlatFingerHeight;
            }

            if ((ImageWidth != 0) && (ImageHeight != 0))
            {
                // convert to pixel
                if (DemoFormRef.Resolution1000Dpi)
                {
                    PixelWidth = (uint)(ImageWidth * 1000);
                    PixelHeight = (uint)(ImageHeight * 1000);
                }
                else
                {
                    PixelWidth = (uint)(ImageWidth * 500);
                    PixelHeight = (uint)(ImageHeight * 500);
                }

                // 2.0.0.0 - for DID20, set its own size (out of standard)
                if (DemoFormRef.CurrentDevice.DeviceID == GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DSID20)
                {
                    PixelWidth = 360;
                    PixelHeight = 400;
                }

                ret = MyGUI.SetImageSize(ScanObjID, PixelWidth, PixelHeight);
                return ret;
            }

            return GBMSGUI.ReturnCodes.Ret_Success;
        }

        private int SetSegmentsImageSize(uint ScanObjID)
        {
            double SegmWidth, SegmHeight;     // sizes for segments
            uint PixelWidth, PixelHeight;
            int ret = GBMSGUI.ReturnCodes.Ret_Success;

            SegmWidth = SegmHeight = 0;

            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                // 2.0.1.0 - was missing
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
            {
                // set sizes for segments using flat finger setting
                SegmWidth = DemoFormRef.DemoConfig.ImageSize.FlatFingerWidth;
                SegmHeight = DemoFormRef.DemoConfig.ImageSize.FlatFingerHeight;
            }
            else if (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS)
            {
                // set sizes for segments using flat thumb setting
                SegmWidth = DemoFormRef.DemoConfig.ImageSize.FlatThumbWidth;
                SegmHeight = DemoFormRef.DemoConfig.ImageSize.FlatThumbHeight;
            }

            if ((SegmWidth != 0) && (SegmHeight != 0))
            {
                // convert to pixel
                if (DemoFormRef.Resolution1000Dpi)
                {
                    PixelWidth = (uint)(SegmWidth * 1000);
                    PixelHeight = (uint)(SegmHeight * 1000);
                }
                else
                {
                    PixelWidth = (uint)(SegmWidth * 500);
                    PixelHeight = (uint)(SegmHeight * 500);
                }

                int num;
                if (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS)
                    num = 2;
                else
                    num = 4;
                for (int i = 0; i < num; i++)
                {
                    ret = MyGUI.SetSegmentImageSize(ScanObjID, i, PixelWidth, PixelHeight);
                    //if (ret != GBMSGUI.ReturnCodes.Ret_Success)
                    //    break;
                }

                return ret;
            }

            return GBMSGUI.ReturnCodes.Ret_Success;
        }

        private void TouchScreenTimer_Tick(object sender, EventArgs e)
        {
            Byte PressedButton;

            TouchScreenTimer.Enabled = false;

            // check LCD Start button
            GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_GetPressedButton(out PressedButton);
            if (PressedButton == GBMSAPI_NET_PressedButtonIDs.GBMSAPI_NET_VILCD_TOUCHSCREEN_START_BUTTON)
            {
                btnAcquire_Click(this, null);
                return;
            }

            TouchScreenTimer.Enabled = true;
        }

        private void btnAcquire_Click(object sender, EventArgs e)
        {
            // delete all files
            EmptyFolder(ImagesPath);

            // clear all pictureboxes
            foreach (ScanItem Item in lstScannedObjects.Items)
            {
                if (Item.ImagePictureBox.Image != null)
                    Item.ImagePictureBox.Image.Dispose();
                Item.ImagePictureBox.Image = null;
            }
            ResetScanItemsData();

            SelectAllScanItems();
            Acquire();

            //V1.10
            // for now not supported
            //EnableViewEJI();

            // after acquisition, re-enable timer
            GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_EnableStartButtonOnLogoScreen();
            TouchScreenTimer.Enabled = true;
        }

        private void EmptyFolder(String FolderName)
        {
            String[] Files = Directory.GetFiles(FolderName);

            foreach (String FileName in Files)
            {
                if (File.Exists(FileName))
                {
                    try
                    {
                        File.Delete(FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void ResetScanItemData(ScanItem Item)
        {
            // delete file
            String FileName = BuildImageFileName(Item);
            if (File.Exists(FileName))
                File.Delete(FileName);
            // clear picturebox
            if (Item.ImagePictureBox.Image != null)
                Item.ImagePictureBox.Image.Dispose();
            Item.ImagePictureBox.Image = null;

            Item.ItemData.Quality = 0;
            Item.ItemData.QualityAlgorithm = 0;
            //Item.ItemData.UnavailabilityReason = 0;
        }

        private void UserDataWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainWindow mainWin = new MainWindow();
            mainWin.Show();
            this.Hide();
            return;

            if (DialogResult == DialogResult.Cancel)
            {
                if (!ViewMode)
                {
                    if (Directory.Exists(ImagesPath))
                        // delete directory and contents
                        Directory.Delete(ImagesPath, true);
                }
            }

            TouchScreenTimer.Enabled = false;

            if (!ViewMode)
            {
                // save configuration of types for sequence
                DemoFormRef.DemoConfig.SequenceTypes |= MainWindow.SequenceType.Rolled;

                // save configuration
                try
                {
                    MainWindow.Configuration.Serialize(Path.ChangeExtension(Application.ExecutablePath, ".cfg"), DemoFormRef.DemoConfig);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                if (DemoFormRef.LCDPresent)
                    // return to Logo Screen
                    GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetLogoScreen();
            }

            // free images
            foreach (ScanItem Item in lstScannedObjects.Items)
                if (Item.ImagePictureBox.Image != null)
                    Item.ImagePictureBox.Image.Dispose();
            lstScannedObjects.Dispose();
        }

        private void SelectAllScanItems()
        {
            // select all items
            for (int i = 0; i < lstScannedObjects.Items.Count; i++)
            {
                ScanItem Item = (ScanItem)lstScannedObjects.Items[i];

                
                if ((GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(Item.ScanObjID) == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLL_SINGLE_FINGER)
                    && Item.Supported)
                    lstScannedObjects.SetItemChecked(i, true);

                // check if finger is marked as missing
                if (GBMSGUI.IsSingleFinger(Item.ScanObjID))
                {
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE))
                        && chkMissingLeftLittle.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING))
                        && chkMissingLeftRing.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE))
                        && chkMissingLeftMiddle.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX))
                        && chkMissingLeftIndex.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB))
                        && chkMissingLeftThumb.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB))
                        && chkMissingRightThumb.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX))
                        && chkMissingRightIndex.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE))
                        && chkMissingRightMiddle.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING))
                        && chkMissingRightRing.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                    if (((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE))
                        && chkMissingRightLittle.Checked)
                        lstScannedObjects.SetItemChecked(i, false);
                }
            }
        }

        private void UnselectAllScanItems()
        {
            // select all items
            for (int i = 0; i < lstScannedObjects.Items.Count; i++)
                lstScannedObjects.SetItemChecked(i, false);
        }

        private String BuildImageFileName(ScanItem Item)
        {
            String FileName = ImagesPath + Path.DirectorySeparatorChar + Item.Text + ".bmp";
            return FileName;
        }

        private String BuildSegmentFileName(ScanItem Item, int Index)
        {
            String FileName = ImagesPath + Path.DirectorySeparatorChar + Item.Text + "_" + Index.ToString() + ".bmp";
            return FileName;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            postFingers();
            return;
            if (!ViewMode)
            {
                if (txtPesonalityName.Text == "")
                {
                    MessageBox.Show("Specify at least Surname!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                UserData.Name = txtPesonalityName.Text;

                // save user data
                MainWindow.UserData.Serialize(ImagesPath + Path.DirectorySeparatorChar + "UserData.xml", UserData);

                // save other data
                SaveItemsData();
                //SaveSegmentsData();
                //SaveJointsData();

                // rename folder with User name
                String NewFolderName = txtPesonalityName.Text;
                //if (txtName.Text.Length != 0)
                //    NewFolderName = NewFolderName + " " + txtName.Text;

                // remove invalid chars (if any)
                NewFolderName = MakeValidFileName(NewFolderName);

                NewFolderName = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + NewFolderName;

                // check duplicate
                if (Directory.Exists(NewFolderName))
                {
                    MessageBox.Show("Duplicated name for folder! Specify a different Surname/Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    // rename folder
                    Directory.Move(ImagesPath, NewFolderName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            DialogResult = DialogResult.OK;
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            MainWindow mainWin = new MainWindow();
            mainWin.Show();
            this.Hide();
        }

        private void LoadSavedImages()
        {
            foreach (ScanItem Item in lstScannedObjects.Items)
            {
                String FileName = BuildImageFileName(Item);
                if (File.Exists(FileName))
                {
                    if (Item.ImagePictureBox.Image != null)
                        Item.ImagePictureBox.Image.Dispose();
                    //Item.ImagePictureBox.Load(FileName);
                    // .NET 4.0 fix
                    LoadPictureBoxImage(Item.ImagePictureBox, FileName);
                    Application.DoEvents();
                }

                // load segments
                // 1.14.0.0 - also for upper palm
                // TODO - verificare bene
                if (GBMSGUI.IsSlap(Item.ScanObjID) || GBMSGUI.IsUpperPalm(Item.ScanObjID))
                {
                    int i, num;
                    if (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS)
                        num = 2;
                    else
                        num = 4;
                    for (i = 0; i < num; i++)
                    {
                        FileName = BuildSegmentFileName(Item, i);
                        if (File.Exists(FileName))
                        {
                            // display image
                            ///PictureBox pbox = GetSegmentPictureBox(Item.ScanObjID, i);
                            //pbox.Load(FileName);
                            // .NET 4.0 fix
                            ///LoadPictureBoxImage(pbox, FileName);
                        }
                    }
                }
            }
        }

        public bool AfterStartCallback(IntPtr UserDefinedParameter)
        {
            return (bool)Invoke(AfterStartDelegate);
        }

        public bool OnAfterStart()
        {
            // for example
            /*
            if (GBMSGUI.IsRolled(CurrentScanItem.ScanObjID))
                GBMSAPI_NET_ScannerStartedRoutines.GBMSAPI_NET_ROLL_SetPreviewTimeout(1500);
            */

            return true;
        }

        // save acquired items data in a binary file
        private bool SaveItemsData()
        {
            int Count = 0;
            FileStream fs;
            BinaryWriter w;
            try
            {
                fs = new FileStream(MainWindow.selectedPersonality.id + Path.DirectorySeparatorChar + "ItemsData.dat", FileMode.Create);
                w = new BinaryWriter(fs);

                // count items to be written
                foreach (ScanItem Item in lstScannedObjects.Items)
                    if ((Item.ItemData.Quality != 0) || (Item.ItemData.UnavailabilityReason != 0))
                        Count++;

                // write number of items
                w.Write(Count);

                foreach (ScanItem Item in lstScannedObjects.Items)
                {
                    if ((Item.ItemData.Quality != 0) || (Item.ItemData.UnavailabilityReason != 0))
                    {
                        // save this item
                        w.Write(Item.ScanObjID);
                        w.Write(Item.ItemData.Quality);
                        w.Write(Item.ItemData.QualityAlgorithm);
                        w.Write(Item.ItemData.UnavailabilityReason);
                    }
                }

                w.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        // read saved items data from a binary file
        private bool ReadItemsData()
        {
            FileStream fs;
            BinaryReader r;
            int Count = 0;
            uint ScanObjID;
            int Quality;
            int QualityAlgorithm;
            int UnavailabilityReason;
            ScanItem Item;

            try
            {
                fs = new FileStream(ImagesPath + Path.DirectorySeparatorChar + "ItemsData.dat", FileMode.Open);
                r = new BinaryReader(fs);

                // read number of items
                Count = r.ReadInt32();

                // read items
                for (int i = 0; i < Count; i++)
                {
                    ScanObjID = (uint)r.ReadInt32();
                    Quality = r.ReadInt32();
                    QualityAlgorithm = r.ReadInt32();
                    UnavailabilityReason = r.ReadInt32();

                    // assign values to ScanItem
                    Item = FindScanItem(ScanObjID);
                    if (Item != null)
                    {
                        Item.ItemData.Quality = Quality;
                        Item.ItemData.QualityAlgorithm = QualityAlgorithm;
                        Item.ItemData.UnavailabilityReason = UnavailabilityReason;
                    }
                }

                r.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        

        // search an item in the list from the ID
        private ScanItem FindScanItem(uint ScanObjID)
        {
            foreach (ScanItem Item in lstScannedObjects.Items)
                if (Item.ScanObjID == ScanObjID)
                    return Item;

            return null;
        }

        // search an item in the list from the PictureBox
        private ScanItem FindScanItem(PictureBox pboxImage)
        {
            foreach (ScanItem Item in lstScannedObjects.Items)
                if (Item.ImagePictureBox == pboxImage)
                    return Item;

            return null;
        }

        private void LoadPictureBoxImage(PictureBox pBox, String FileName)
        {
            FileStream fs;
            fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            pBox.Image = System.Drawing.Image.FromStream(fs);
            fs.Close();
        }

        private void DisableScanItemSameFinger(uint ScanObjID, int Index, int UnavailabilityReason)
        {
            // disable same fingers in the list
            for (int i = 0; i < lstScannedObjects.Items.Count; i++)
            {
                ScanItem Item = (ScanItem)lstScannedObjects.Items[i];

                if (IsSameFinger(ScanObjID, Index, Item.ScanObjID, -1))
                    lstScannedObjects.SetItemChecked(lstScannedObjects.Items.IndexOf(Item), false);
            }

            // set checkboxes for slaps
            if (IsLeftThumb(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingLeftThumb.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingLeftThumb.CheckState = CheckState.Indeterminate;
            }
            if (IsLeftIndex(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingLeftIndex.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingLeftIndex.CheckState = CheckState.Indeterminate;
            }
            if (IsLeftMiddle(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingLeftMiddle.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingLeftMiddle.CheckState = CheckState.Indeterminate;
            }
            if (IsLeftRing(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingLeftRing.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingLeftRing.CheckState = CheckState.Indeterminate;
            }
            if (IsLeftLittle(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingLeftLittle.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingLeftLittle.CheckState = CheckState.Indeterminate;
            }
            if (IsRightThumb(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingRightThumb.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingRightThumb.CheckState = CheckState.Indeterminate;
            }
            if (IsRightIndex(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingRightIndex.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingRightIndex.CheckState = CheckState.Indeterminate;
            }
            if (IsRightMiddle(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingRightMiddle.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingRightMiddle.CheckState = CheckState.Indeterminate;
            }
            if (IsRightLittle(ScanObjID, Index))
            {
                if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Amputated)
                    chkMissingRightLittle.CheckState = CheckState.Checked;
                else if (UnavailabilityReason == GBMSGUI.UnavailabilityReason.Unprintable)
                    chkMissingRightLittle.CheckState = CheckState.Indeterminate;
            }
        }

        private bool IsLeftThumb(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB))
                return true;

            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS) && (Index == 0))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsLeftIndex(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX))
                return true;

            if (((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_LEFT))
                && (Index == 0))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsLeftMiddle(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE))
                return true;

            if (((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_LEFT))
                && (Index == 1))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsLeftRing(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING))
                return true;

            if (((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT))
                && (Index == 2))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsLeftLittle(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE))
                return true;

            if (((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT))
                && (Index == 3))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsRightThumb(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB))
                return true;

            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS) && (Index == 1))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsRightIndex(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX))
                return true;

            if (((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_RIGHT))
                && (Index == 0))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsRightMiddle(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE))
                return true;

            if (((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_RIGHT))
                && (Index == 1))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsRightRing(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING))
                return true;

            if (((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
                && (Index == 2))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsRightLittle(uint ScanObjID, int Index)
        {
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE))
                return true;

            if (((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
                && (Index == 3))
                return true;

            return false;
        }

        // 2.0.1.0
        private bool IsSameFinger(uint ScanObjID1, int Index1, uint ScanObjID2, int Index2)
        {
            if (IsLeftThumb(ScanObjID1, Index1) && IsLeftThumb(ScanObjID2, Index2))
                return true;
            if (IsLeftIndex(ScanObjID1, Index1) && IsLeftIndex(ScanObjID2, Index2))
                return true;
            if (IsLeftMiddle(ScanObjID1, Index1) && IsLeftMiddle(ScanObjID2, Index2))
                return true;
            if (IsLeftRing(ScanObjID1, Index1) && IsLeftRing(ScanObjID2, Index2))
                return true;
            if (IsLeftLittle(ScanObjID1, Index1) && IsLeftLittle(ScanObjID2, Index2))
                return true;
            if (IsRightThumb(ScanObjID1, Index1) && IsRightThumb(ScanObjID2, Index2))
                return true;
            if (IsRightIndex(ScanObjID1, Index1) && IsRightIndex(ScanObjID2, Index2))
                return true;
            if (IsRightMiddle(ScanObjID1, Index1) && IsRightMiddle(ScanObjID2, Index2))
                return true;
            if (IsRightRing(ScanObjID1, Index1) && IsRightRing(ScanObjID2, Index2))
                return true;
            if (IsRightLittle(ScanObjID1, Index1) && IsRightLittle(ScanObjID2, Index2))
                return true;

            return false;
        }

        private void popAcquireItemMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        private void SetMissingFingers()
        {
            //int Reason = GBMSGUI.UnavailabilityReason.Unprintable;
            // 2.0.1.0 - Reason is determined by the checkbox threestate
            int Reason;

            if ((CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                (CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_LEFT) ||
                // 2.0.1.0 - was missing
                (CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT))
            {
                if (chkMissingLeftIndex.Checked)
                {
                    // 2.0.1.0
                    if (chkMissingLeftIndex.CheckState == CheckState.Checked)
                        Reason = GBMSGUI.UnavailabilityReason.Amputated;
                    else
                        Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                    MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 0, Reason);
                }
                if (chkMissingLeftMiddle.Checked)
                {
                    // 2.0.1.0
                    if (chkMissingLeftMiddle.CheckState == CheckState.Checked)
                        Reason = GBMSGUI.UnavailabilityReason.Amputated;
                    else
                        Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                    MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 1, Reason);
                }
                if ((CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                    // 2.0.1.0
                    (CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT))
                {
                    if (chkMissingLeftRing.Checked)
                    {
                        // 2.0.1.0
                        if (chkMissingLeftRing.CheckState == CheckState.Checked)
                            Reason = GBMSGUI.UnavailabilityReason.Amputated;
                        else
                            Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                        MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 2, Reason);
                    }
                    if (chkMissingLeftLittle.Checked)
                    {
                        // 2.0.1.0
                        if (chkMissingLeftLittle.CheckState == CheckState.Checked)
                            Reason = GBMSGUI.UnavailabilityReason.Amputated;
                        else
                            Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                        MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 3, Reason);
                    }
                }
            }

            if ((CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                (CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_RIGHT) ||
                // 2.0.1.0 - was missing
                (CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
            {
                if (chkMissingRightIndex.Checked)
                {
                    // 2.0.1.0
                    if (chkMissingRightIndex.CheckState == CheckState.Checked)
                        Reason = GBMSGUI.UnavailabilityReason.Amputated;
                    else
                        Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                    MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 0, Reason);
                }
                if (chkMissingRightMiddle.Checked)
                {
                    // 2.0.1.0
                    if (chkMissingRightMiddle.CheckState == CheckState.Checked)
                        Reason = GBMSGUI.UnavailabilityReason.Amputated;
                    else
                        Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                    MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 1, Reason);
                }
                if ((CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                    // 2.0.1.0
                    (CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
                {
                    if (chkMissingRightRing.Checked)
                    {
                        // 2.0.1.0
                        if (chkMissingRightRing.CheckState == CheckState.Checked)
                            Reason = GBMSGUI.UnavailabilityReason.Amputated;
                        else
                            Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                        MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 2, Reason);
                    }
                    if (chkMissingRightLittle.Checked)
                    {
                        // 2.0.1.0
                        if (chkMissingRightLittle.CheckState == CheckState.Checked)
                            Reason = GBMSGUI.UnavailabilityReason.Amputated;
                        else
                            Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                        MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 3, Reason);
                    }
                }
            }

            if (CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS)
            {
                if (chkMissingLeftThumb.Checked)
                {
                    // 2.0.1.0
                    if (chkMissingLeftThumb.CheckState == CheckState.Checked)
                        Reason = GBMSGUI.UnavailabilityReason.Amputated;
                    else
                        Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                    MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 0, Reason);
                }
                if (chkMissingRightThumb.Checked)
                {
                    // 2.0.1.0
                    if (chkMissingRightThumb.CheckState == CheckState.Checked)
                        Reason = GBMSGUI.UnavailabilityReason.Amputated;
                    else
                        Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                    MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 1, Reason);
                }
            }

            if (CurrentScanItem.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_INDEXES)
            {
                if (chkMissingLeftIndex.Checked)
                {
                    // 2.0.1.0
                    if (chkMissingLeftIndex.CheckState == CheckState.Checked)
                        Reason = GBMSGUI.UnavailabilityReason.Amputated;
                    else
                        Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                    MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 0, Reason);
                }
                if (chkMissingRightIndex.Checked)
                {
                    // 2.0.1.0
                    if (chkMissingRightIndex.CheckState == CheckState.Checked)
                        Reason = GBMSGUI.UnavailabilityReason.Amputated;
                    else
                        Reason = GBMSGUI.UnavailabilityReason.Unprintable;
                    MyGUI.SetSegmentUnavailabilityReason(CurrentScanItem.ScanObjID, 1, Reason);
                }
            }
        }

        private void getFingers()
        {
            //request left thumb
            IRestResponse[] gl_response = new IRestResponse[5];
            IRestResponse[] gr_response = new IRestResponse[5];

            string[] gl = { "gl_thumb", "gl_point", "gl_mid", "gl_ring", "gl_pink" };
            string[] gr = { "gr_thumb", "gr_point", "gr_mid", "gr_ring", "gr_pink" };

            string[] JLeftFingers = new string[5];
            string[] JRightFingers = new string[5];

            //request get left fingers
            PictureBox[] leftFingers = { pboxLeftThumb, pboxLeftIndex, pboxLeftMiddle, pboxLeftRing, pboxLeftLittle };
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var client = new RestClient(Properties.Settings.Default.ip + "/api/personalities/" + gl[i]);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
                    request.AddHeader("Accept", "application/json");
                    request.AddParameter("person_id", MainWindow.selectedPersonality.id);
                    gl_response[i] = client.Execute(request);

                    if ((int)gl_response[i].StatusCode == 200)
                    {
                        leftFingers[i].Image = byteArrayToImage(gl_response[i].RawBytes);
                        Console.WriteLine(gl[i] + " fp pound");
                    }
                    if((int)gl_response[i].StatusCode == 404)
                    {
                        Console.WriteLine(gl[i] + " no fp found");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            //request get right fingers
            PictureBox[] rightFingers = { pboxRightThumb, pboxRightIndex, pboxRightMiddle, pboxRightRing, pboxRightLittle };
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var client = new RestClient(Properties.Settings.Default.ip + "/api/personalities/" + gr[i]);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
                    request.AddHeader("Accept", "application/json");
                    request.AddParameter("person_id", MainWindow.selectedPersonality.id);
                    gr_response[i] = client.Execute(request);

                    if ((int)gr_response[i].StatusCode == 200)
                    {
                        rightFingers[i].Image = byteArrayToImage(gr_response[i].RawBytes);
                        Console.WriteLine(gr[i] + " fp found");
                    }
                    if ((int)gr_response[i].StatusCode == 404)
                    {
                        Console.WriteLine(gr[i] + " no fp found");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void postFingers()
        {
            string url = "http://192.168.100.217:8000/api/personalities/";
            //request left thumb
            IRestResponse[] sl_response = new IRestResponse[5];
            IRestResponse[] sr_response = new IRestResponse[5];

            string[] sl = { "sl_thumb", "sl_point", "sl_mid", "sl_ring", "sl_pink" };
            string[] sr = { "sr_thumb", "sr_point", "sr_mid", "sr_ring", "sr_pink" };

            //request set left fingers
            PictureBox[] leftFingers = { pboxLeftThumb, pboxLeftIndex, pboxLeftMiddle, pboxLeftRing, pboxLeftLittle };
            for (int i = 0; i < 5; i++)
            {
                if (leftFingers[i].Image != null)
                {
                    try
                    {
                    var client = new RestClient(Properties.Settings.Default.ip + "/api/personalities/" + sl[i]);
                    var request = new RestRequest(Method.POST);
                    Bitmap bmp = new Bitmap(leftFingers[i].Image);
                    string base64 = ImageToBase64(bmp, ImageFormat.Bmp);
                    request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
                    request.AddHeader("Accept", "application/json");
                    request.AddParameter("fingerprint", base64);
                    request.AddParameter("person_id", MainWindow.selectedPersonality.id);
                    sl_response[i] = client.Execute(request);
                    Console.WriteLine(sl[i] + " success");
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            //request set right fingers
            PictureBox[] rightFingers = { pboxRightThumb, pboxRightIndex, pboxRightMiddle, pboxRightRing, pboxRightLittle };
            for (int i = 0; i < 5; i++)
            {
                if (rightFingers[i].Image != null)
                {
                    try
                    {
                        var client = new RestClient(Properties.Settings.Default.ip + "/api/personalities/" + sr[i]);
                        var request = new RestRequest(Method.POST);
                        Bitmap bmp = new Bitmap(rightFingers[i].Image);
                        string base64 = ImageToBase64(bmp, ImageFormat.Bmp);
                        request.AddHeader("Authorization", LoginWindow.apiCon.token_type + " " + LoginWindow.apiCon.access_token);
                        request.AddHeader("Accept", "application/json");
                        request.AddParameter("fingerprint", base64);
                        request.AddParameter("person_id", MainWindow.selectedPersonality.id);
                        sr_response[i] = client.Execute(request);
                        Console.WriteLine(sr[i] + " success");
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            MessageBox.Show("Success", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        string ImageToBase64(Image image, ImageFormat format)
        {
            var imageStream = new MemoryStream();
            image.Save(imageStream, format);
            imageStream.Position = 0;
            var imageBytes = imageStream.ToArray();
            var ImageBase64 = Convert.ToBase64String(imageBytes);
            return ImageBase64;
        }

        Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            var ms = new MemoryStream(imageBytes);
            Image image = Image.FromStream(ms);
            return image;
        }

        Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;

        }

    }
}
