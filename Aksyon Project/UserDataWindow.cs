using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
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
        public UserDataWindow()
        {
            InitializeComponent();
        }

        public String ImagesPath;       // path where to store/read images
        public bool ViewMode;           // true = view mode; false = new mode
        public bool FingerprintCard;    // true = Acquire from Fingerprint Card

        private DemoForm.UserData UserData; // class where to store user data
        private DemoForm DemoFormRef;       // reference to main form
        private Byte[] ImageBuffer;         // buffer for images
        private GBMSGUI MyGUI = new GBMSGUI();  // GBMSGUI class
        //private GCHandle gcUserDataFormHandle;

        // AfterStartCallback, if needed
        //private static GBMSGUI.AfterStartCallback AfterStartCallbackRef = new GBMSGUI.AfterStartCallback(AfterStartCallback);

        //private static GCHandle gcUserDataFormHandle_St;
        /*
        public static bool AfterStartCallback(IntPtr Param)
        {
            bool ret;

            // retrieve form reference
            if (!gcUserDataFormHandle_St.IsAllocated)
                gcUserDataFormHandle_St = GCHandle.FromIntPtr(Param);

            UserDataForm UserDataFormRef = (UserDataForm)gcUserDataFormHandle_St.Target;

            // call form method
            ret = UserDataFormRef.AfterStartCallback();

            return ret;
        }
        */

        // class for scanned object list item
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

        // data for segments
        private class SegmentData
        {
            public ScanItemData ItemData = new ScanItemData();
            public Rectangle BoundingBox = new Rectangle();
        }
        private SegmentData[] LeftSlapSegmentsData = new SegmentData[4];
        private SegmentData[] RightSlapSegmentsData = new SegmentData[4];
        private SegmentData[] TwoThumbsSegmentsData = new SegmentData[2];

        // joint bounding boxes
        private class JointsData
        {
            public Rectangle[] PhalangeBBox = new Rectangle[3];
        }
        private JointsData[] JointsBBoxData = new JointsData[40]; // 10 fingers, 4 views
        private int IndexOfJointsData(uint ScannedObjectID)
        {
            int Index = -1;

            // assing an index of JointsBBoxData array to each joint object
            switch (ScannedObjectID)
            {
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_THUMB:
                    Index = 0;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_INDEX:
                    Index = 1;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_MIDDLE:
                    Index = 2;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_RING:
                    Index = 3;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_LITTLE:
                    Index = 4;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_THUMB:
                    Index = 5;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_INDEX:
                    Index = 6;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_MIDDLE:
                    Index = 7;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_RING:
                    Index = 8;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_LITTLE:
                    Index = 9;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_THUMB:
                    Index = 10;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_INDEX:
                    Index = 11;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_MIDDLE:
                    Index = 12;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_RING:
                    Index = 13;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_LITTLE:
                    Index = 14;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_THUMB:
                    Index = 15;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_INDEX:
                    Index = 16;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_MIDDLE:
                    Index = 17;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_RING:
                    Index = 18;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_LITTLE:
                    Index = 19;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_THUMB:
                    Index = 20;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_INDEX:
                    Index = 21;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_MIDDLE:
                    Index = 22;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_RING:
                    Index = 23;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_LITTLE:
                    Index = 24;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_THUMB:
                    Index = 25;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_INDEX:
                    Index = 26;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_MIDDLE:
                    Index = 27;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_RING:
                    Index = 28;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_LITTLE:
                    Index = 29;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_THUMB:
                    Index = 30;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_INDEX:
                    Index = 31;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_MIDDLE:
                    Index = 32;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_RING:
                    Index = 33;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_LITTLE:
                    Index = 34;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_THUMB:
                    Index = 35;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_INDEX:
                    Index = 36;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_MIDDLE:
                    Index = 37;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_RING:
                    Index = 38;
                    break;
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_LITTLE:
                    Index = 39;
                    break;
            }

            return Index;
        }
        private void ResetAllJointsData()
        {
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    JointsBBoxData[i].PhalangeBBox[j].X = 0;
                    JointsBBoxData[i].PhalangeBBox[j].Y = 0;
                    JointsBBoxData[i].PhalangeBBox[j].Width = 0;
                    JointsBBoxData[i].PhalangeBBox[j].Height = 0;
                }
            }
        }
        private void ResetJointsData(uint ScanObjID)
        {
            int Index = IndexOfJointsData(ScanObjID);
            for (int j = 0; j < 3; j++)
            {
                JointsBBoxData[Index].PhalangeBBox[j].X = 0;
                JointsBBoxData[Index].PhalangeBBox[j].Y = 0;
                JointsBBoxData[Index].PhalangeBBox[j].Width = 0;
                JointsBBoxData[Index].PhalangeBBox[j].Height = 0;
            }
        }

        // delegate for OnAfterStart
        public delegate bool AfterStartEvent();

        public AfterStartEvent AfterStartDelegate;

        public UserDataForm(DemoForm DemoFormRef)
        {
            // reference to main form
            this.DemoFormRef = DemoFormRef;

            FingerprintCard = false;

            InitializeComponent();

            // delegate for callback
            AfterStartDelegate = new AfterStartEvent(OnAfterStart);

            // assign popupmenu to items
            pboxLeftThumb.ContextMenu = popAcquireItemMenu;
            pboxLeftIndex.ContextMenu = popAcquireItemMenu;
            pboxLeftMiddle.ContextMenu = popAcquireItemMenu;
            pboxLeftRing.ContextMenu = popAcquireItemMenu;
            pboxLeftLittle.ContextMenu = popAcquireItemMenu;
            pboxRightThumb.ContextMenu = popAcquireItemMenu;
            pboxRightIndex.ContextMenu = popAcquireItemMenu;
            pboxRightMiddle.ContextMenu = popAcquireItemMenu;
            pboxRightRing.ContextMenu = popAcquireItemMenu;
            pboxRightLittle.ContextMenu = popAcquireItemMenu;

        }

        private void UserDataWindow_Load(object sender, EventArgs e)
        {
            
        }

        private void LoadScanObjectsList()
        {
            ScanItem Item;

            lstScannedObjects.Items.Clear();

            // fill list with scanned objects
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT, "Left Flat Four Fingers", pboxLeftFourFingers);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT, "Right Flat Four Fingers", pboxRightFourFingers);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS, "Flat Two Thumbs", pboxTwoThumbs);
            lstScannedObjects.Items.Add(Item);
            //V1.2 - DS40
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_LEFT, "Left Flat Two Fingers", pboxLeftTwoFingers);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_RIGHT, "Right Flat Two Fingers", pboxRightTwoFingers);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_INDEXES, "Flat Two Indexes", pboxTwoIndexes);
            lstScannedObjects.Items.Add(Item);

            // 1.14.0.0 - moved palms before rolled (upper palm can be used for sequence check)
            // Palms
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT, "Left Upper Half Palm", pboxLeftUpperPalm);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT, "Right Upper Half Palm", pboxRightUpperPalm);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_LEFT, "Left Lower Half Palm", pboxLeftLowerPalm);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_RIGHT, "Right Lower Half Palm", pboxRightLowerPalm);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_LEFT, "Left Writer's Palm", pboxLeftWritersPalm);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_RIGHT, "Right Writer's Palm", pboxRightWritersPalm);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_THENAR_LEFT, "Rolled Left Thenar", pboxLeftRolledThenar);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_THENAR_RIGHT, "Rolled Right Thenar", pboxRightRolledThenar);
            lstScannedObjects.Items.Add(Item);
            // 1.15.0.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_HYPOTHENAR_LEFT, "Rolled Left Hypothenar", pboxLeftRolledHypothenar);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_HYPOTHENAR_RIGHT, "Rolled Right Hypothenar", pboxRightRolledHypothenar);
            lstScannedObjects.Items.Add(Item);


            //V1.10 - group by finger
            // Left Thumb
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_THUMB, "Left Flat Thumb", pboxLeftFlatThumb);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB, "Left Rolled Thumb", pboxLeftRolledThumb);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_THUMB, "Rolled Joint - Left Thumb", pboxLeftThumbRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_THUMB, "Plain Joint Left Side - Left Thumb", pboxLeftThumbJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_THUMB, "Plain Joint Right Side - Left Thumb", pboxLeftThumbJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_THUMB, "Rolled Joint Center - Left Thumb", pboxLeftThumbJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_THUMB, "Rolled Tip - Left Thumb", pboxLeftThumbRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_THUMB, "Rolled Up - Left Thumb", pboxLeftRolledUpThumb);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_THUMB, "Rolled Down - Left Thumb", pboxLeftRolledDownThumb);
            lstScannedObjects.Items.Add(Item);

            // Left Index
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_INDEX, "Left Flat Index", pboxLeftFlatIndex);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX, "Left Rolled Index", pboxLeftRolledIndex);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_INDEX, "Rolled Joint - Left Index", pboxLeftIndexRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_INDEX, "Plain Joint Left Side - Left Index", pboxLeftIndexJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_INDEX, "Plain Joint Right Side - Left Index", pboxLeftIndexJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_INDEX, "Rolled Joint Center - Left Index", pboxLeftIndexJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_INDEX, "Rolled Tip - Left Index", pboxLeftIndexRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_INDEX, "Rolled Up - Left Index", pboxLeftRolledUpIndex);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_INDEX, "Rolled Down - Left Index", pboxLeftRolledDownIndex);
            lstScannedObjects.Items.Add(Item);

            // Left Middle
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_MIDDLE, "Left Flat Middle", pboxLeftFlatMiddle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE, "Left Rolled Middle", pboxLeftRolledMiddle);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_MIDDLE, "Rolled Joint - Left Middle", pboxLeftMiddleRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_MIDDLE, "Plain Joint Left Side - Left Middle", pboxLeftMiddleJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_MIDDLE, "Plain Joint Right Side - Left Middle", pboxLeftMiddleJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_MIDDLE, "Rolled Joint Center - Left Middle", pboxLeftMiddleJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_MIDDLE, "Rolled Tip - Left Middle", pboxLeftMiddleRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_MIDDLE, "Rolled Up - Left Middle", pboxLeftRolledUpMiddle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_MIDDLE, "Rolled Down - Left Middle", pboxLeftRolledDownMiddle);
            lstScannedObjects.Items.Add(Item);

            // Left Ring
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_RING, "Left Flat Ring", pboxLeftFlatRing);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING, "Left Rolled Ring", pboxLeftRolledRing);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_RING, "Rolled Joint - Left Ring", pboxLeftRingRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_RING, "Plain Joint Left Side - Left Ring", pboxLeftRingJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_RING, "Plain Joint Right Side - Left Ring", pboxLeftRingJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_RING, "Rolled Joint Center - Left Ring", pboxLeftRingJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_RING, "Rolled Tip - Left Ring", pboxLeftRingRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_RING, "Rolled Up - Left Ring", pboxLeftRolledUpRing);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_RING, "Rolled Down - Left Ring", pboxLeftRolledDownRing);
            lstScannedObjects.Items.Add(Item);

            // Left Little
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_LITTLE, "Left Flat Little", pboxLeftFlatLittle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE, "Left Rolled Little", pboxLeftRolledLittle);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_LEFT_LITTLE, "Rolled Joint - Left Little", pboxLeftLittleRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_LEFT_LITTLE, "Plain Joint Left Side - Left Little", pboxLeftLittleJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_LEFT_LITTLE, "Plain Joint Right Side - Left Little", pboxLeftLittleJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_LEFT_LITTLE, "Rolled Joint Center - Left Little", pboxLeftLittleJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_LEFT_LITTLE, "Rolled Tip - Left Little", pboxLeftLittleRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_LEFT_LITTLE, "Rolled Up - Left Little", pboxLeftRolledUpLittle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_LEFT_LITTLE, "Rolled Down - Left Little", pboxLeftRolledDownLittle);
            lstScannedObjects.Items.Add(Item);

            // Right Thumb
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_THUMB, "Right Flat Thumb", pboxRightFlatThumb);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB, "Right Rolled Thumb", pboxRightRolledThumb);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_THUMB, "Rolled Joint - Right Thumb", pboxRightThumbRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_THUMB, "Plain Joint Left Side - Right Thumb", pboxRightThumbJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_THUMB, "Plain Joint Right Side - Right Thumb", pboxRightThumbJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_THUMB, "Rolled Joint Center - Right Thumb", pboxRightThumbJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_THUMB, "Rolled Tip - Right Thumb", pboxRightThumbRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_THUMB, "Rolled Up - Right Thumb", pboxRightRolledUpThumb);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_THUMB, "Rolled Down - Right Thumb", pboxRightRolledDownThumb);
            lstScannedObjects.Items.Add(Item);

            // Right Index
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_INDEX, "Right Flat Index", pboxRightFlatIndex);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX, "Right Rolled Index", pboxRightRolledIndex);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_INDEX, "Rolled Joint - Right Index", pboxRightIndexRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_INDEX, "Plain Joint Left Side - Right Index", pboxRightIndexJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_INDEX, "Plain Joint Right Side - Right Index", pboxRightIndexJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_INDEX, "Rolled Joint Center - Right Index", pboxRightIndexJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_INDEX, "Rolled Tip - Right Index", pboxRightIndexRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_INDEX, "Rolled Up - Right Index", pboxRightRolledUpIndex);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_INDEX, "Rolled Down - Right Index", pboxRightRolledDownIndex);
            lstScannedObjects.Items.Add(Item);

            // Right Middle
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_MIDDLE, "Right Flat Middle", pboxRightFlatMiddle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE, "Right Rolled Middle", pboxRightRolledMiddle);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_MIDDLE, "Rolled Joint - Right Middle", pboxRightMiddleRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_MIDDLE, "Plain Joint Left Side - Right Middle", pboxRightMiddleJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_MIDDLE, "Plain Joint Right Side - Right Middle", pboxRightMiddleJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_MIDDLE, "Rolled Joint Center - Right Middle", pboxRightMiddleJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_MIDDLE, "Rolled Tip - Right Middle", pboxRightMiddleRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_MIDDLE, "Rolled Up - Right Middle", pboxRightRolledUpMiddle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_MIDDLE, "Rolled Down - Right Middle", pboxRightRolledDownMiddle);
            lstScannedObjects.Items.Add(Item);

            // Right Ring
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_RING, "Right Flat Ring", pboxRightFlatRing);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING, "Right Rolled Ring", pboxRightRolledRing);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_RING, "Rolled Joint - Right Ring", pboxRightRingRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_RING, "Plain Joint Left Side - Right Ring", pboxRightRingJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_RING, "Plain Joint Right Side - Right Ring", pboxRightRingJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_RING, "Rolled Joint Center - Right Ring", pboxRightRingJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_RING, "Rolled Tip - Right Ring", pboxRightRingRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_RING, "Rolled Up - Right Ring", pboxRightRolledUpRing);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_RING, "Rolled Down - Right Ring", pboxRightRolledDownRing);
            lstScannedObjects.Items.Add(Item);

            // Right Little
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_LITTLE, "Right Flat Little", pboxRightFlatLittle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE, "Right Rolled Little", pboxRightRolledLittle);
            lstScannedObjects.Items.Add(Item);
            //V1.10
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_RIGHT_LITTLE, "Rolled Joint - Right Little", pboxRightLittleRolledJoint);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_LEFT_SIDE_RIGHT_LITTLE, "Plain Joint Left Side - Right Little", pboxRightLittleJointLeft);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_PLAIN_JOINT_RIGHT_SIDE_RIGHT_LITTLE, "Plain Joint Right Side - Right Little", pboxRightLittleJointRight);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_JOINT_CENTER_RIGHT_LITTLE, "Rolled Joint Center - Right Little", pboxRightLittleJointCenter);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_TIP_RIGHT_LITTLE, "Rolled Tip - Right Little", pboxRightLittleRolledTip);
            lstScannedObjects.Items.Add(Item);
            // 1.13.4.0
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_UP_RIGHT_LITTLE, "Rolled Up - Right Little", pboxRightRolledUpLittle);
            lstScannedObjects.Items.Add(Item);
            Item = new ScanItem(GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLLED_DOWN_RIGHT_LITTLE, "Rolled Down - Right Little", pboxRightRolledDownLittle);
            lstScannedObjects.Items.Add(Item);
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


        private void Acquire()
        {
            int Index;
            int ret;
            int ImageSizeX, ImageSizeY;
            int Resolution;
            Bitmap bmp;
            uint AcqOptions = 0;
            uint SessionOpt = 0;
            String PersonID;
            int AcquisitionMode;
            int QualityThreshold1, QualityThreshold2;

            byte DeviceID = DemoFormRef.CurrentDevice.DeviceID;

            uint DeviceFeatures;
            GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetDeviceFeatures(out DeviceFeatures);

            // disable TouchScreen
            TouchScreenTimer.Enabled = false;

            PersonID = txtSurname.Text + " " + txtName.Text;

            // set language
            SetLanguage();

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
                QualityThreshold1 = 3;
                QualityThreshold2 = 4;
            }
            else // GB algorithm
            {
                QualityThreshold1 = 50;
                QualityThreshold2 = 70;
            }

            // other settings - for demo a little more permissive
            // see the NOTE above
            MyGUI.SetArtefactsThresholds(15, 30);
            MyGUI.SetLowerPalmCompletenessThresholds(70, 80);
            MyGUI.SetBlockAutoCaptureContrast(DemoFormRef.DemoConfig.BlockAutocaptureContrast);
            MyGUI.SetPatternValidityThreshold(65);
            MyGUI.SetPatternCompletenessThreshold(75);

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
            mnuAcquireItem.Enabled = false;

            SessionOpt = DemoFormRef.DemoConfig.SessionOptions;
            // if acquiring single item, disable sequence check
            if (lstScannedObjects.CheckedItems.Count == 1)
            {
                SessionOpt &= ~GBMSGUI.SessionOption.SequenceCheck;
                AcquisitionMode = GBMSGUI.AcquisitionModes.SingleAcquisition;
            }
            else
            {
                AcquisitionMode = GBMSGUI.AcquisitionModes.MultipleAcquisition;
            }

            // start the acquisition session
            ret = MyGUI.StartSession(AcquisitionMode, PersonID, SessionOpt);
            if (ret != GBMSGUI.ReturnCodes.Ret_Success)
            {
                // re-enable our controls
                btnAcquire.Enabled = true;
                mnuAcquireItem.Enabled = true;

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

            //MyGUI.SetFlatFingerOnRollArea(true);
            //MyGUI.SetIgnoreDiagnosticMask(
            //    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_DISPLACED_DOWN |
            //    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_LEFT |
            //    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_RIGHT |
            //    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_TOP
            //    );

            MyGUI.SelectFingerContactEvaluationMode(DemoFormRef.DemoConfig.FingerContactEvaluationMode);

            if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
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
                        (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT))
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

                // set frame rate
                if (GBMSGUI.CheckMask(DeviceFeatures, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_FRAME_RATE_SETTING))
                {
                    double FrameRate = 5;
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
                            if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.FullResPreview))
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
                                if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                    FrameRate = DemoFormRef.DemoConfig.MC517FullLowResFrameRate;
                                else
                                    FrameRate = DemoFormRef.DemoConfig.MC517PartialIQSFrameRate;
                            }
                            else
                            {
                                if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                    FrameRate = DemoFormRef.DemoConfig.MC517FullLowResFrameRate;
                                else
                                    FrameRate = DemoFormRef.DemoConfig.MC517PartialGAFrameRate;
                            }
                        }
                        else
                        {
                            // hi or low res
                            if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.FullResPreview))
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
                            if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                FrameRate = DemoFormRef.DemoConfig.MS527FullLowResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527PartialThenarFrameRate;
                        }
                        else if (GBMSGUI.IsRolledJoint(Item.ScanObjID))
                        {
                            if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                                FrameRate = DemoFormRef.DemoConfig.MS527FullLowResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527PartialJointFrameRate;
                        }
                        else if (GBMSGUI.IsRolled(Item.ScanObjID))
                        {
                            if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
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
                            if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.FullResPreview))
                                FrameRate = DemoFormRef.DemoConfig.MS527FullHiResFrameRate;
                            else
                                FrameRate = DemoFormRef.DemoConfig.MS527FullLowResFrameRate;
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

                    if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.AdaptRollAreaPosition))
                        FrameRateOptions |= GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ADAPT_ROLL_AREA_POSITION;

                    if ((ScanArea == GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME) &&
                        GBMSGUI.CheckMask(DemoFormRef.DemoConfig.AcquisitionOptions, GBMSGUI.AcquisitionOption.FullResPreview))
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

                AcqOptions = DemoFormRef.DemoConfig.AcquisitionOptions;

                // manage joint segmentation option
                if (GBMSGUI.IsJoint(Item.ScanObjID) && !DemoFormRef.DemoConfig.JointSegmentation)
                    AcqOptions &= ~GBMSGUI.AcquisitionOption.Segmentation;

                Repeat:
                // acquire item
                ret = MyGUI.Acquire(Item.ScanObjID, AcqOptions, ImageBuffer, out ImageSizeX, out ImageSizeY, out Resolution);

                // if single acquisition (acquisition window is hidden after acquire), re-enable our window now
                if (AcquisitionMode == GBMSGUI.AcquisitionModes.SingleAcquisition)
                {
                    btnAcquire.Enabled = true;
                    mnuAcquireItem.Enabled = true;
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
                    if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.SessionOptions, GBMSGUI.SessionOption.AskUnavailabilityReason))
                    {
                        MyGUI.GetUnavailabilityReason(Item.ScanObjID, out Item.ItemData.UnavailabilityReason);
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
                    Item.ImagePictureBox.Load(FileName);

                    // save quality
                    ret = MyGUI.GetIAFISQuality(Item.ScanObjID, DemoFormRef.DemoConfig.IAFIsQualityAlgorithm,
                        out Item.ItemData.Quality);
                    Item.ItemData.QualityAlgorithm = DemoFormRef.DemoConfig.IAFIsQualityAlgorithm;

                    int i, num;
                    int SegmLeft, SegmTop, SegmRight, SegmBottom, Quality, UnavailReason,
                        SegmSizeX, SegmSizeY;
                    bool SegmAvailable;
                    SegmentData[] SegmentsData;

                    // if slap, save also segments
                    // 1.14.0.0 - also for upper palm
                    // TODO - vedere se salvarli separatamente
                    if (GBMSGUI.IsSlap(Item.ScanObjID) || GBMSGUI.IsUpperPalm(Item.ScanObjID))
                    {
                        if ((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                            (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT))
                        {
                            num = 4;
                            SegmentsData = LeftSlapSegmentsData;
                        }
                        else if ((Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                            (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
                        {
                            num = 4;
                            SegmentsData = RightSlapSegmentsData;
                        }
                        else //if (Item.ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS)
                        {
                            num = 2;
                            SegmentsData = TwoThumbsSegmentsData;
                        }

                        for (i = 0; i < num; i++)
                        {
                            ret = MyGUI.GetSegmentationResult(i, DemoFormRef.DemoConfig.IAFIsQualityAlgorithm,
                                out SegmAvailable, out SegmLeft, out SegmTop, out SegmRight, out SegmBottom,
                                out UnavailReason, out Quality,
                                ImageBuffer, out SegmSizeX, out SegmSizeY, out Resolution);
                            if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
                            {
                                MessageBox.Show(MyGUI.GetErrorMessage() + " (GetSegmentationResult)",
                                    Application.ProductName,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            if (SegmAvailable)
                            {
                                // save segment image
                                bmp = GBMSGUI.RawImageToBitmap(ImageBuffer, SegmSizeX, SegmSizeY);
                                FileName = BuildSegmentFileName(Item, i);
                                bmp.SetResolution((float)Resolution, (float)Resolution);
                                bmp.Save(FileName, ImageFormat.Bmp);
                                bmp.Dispose();
                                // display image
                                PictureBox pbox = GetSegmentPictureBox(Item.ScanObjID, i);
                                pbox.Load(FileName);

                                // save segments data
                                SegmentsData[i].ItemData.Quality = Quality;
                                SegmentsData[i].ItemData.QualityAlgorithm = DemoFormRef.DemoConfig.IAFIsQualityAlgorithm;
                                SegmentsData[i].BoundingBox.X = SegmLeft;
                                SegmentsData[i].BoundingBox.Y = SegmTop;
                                SegmentsData[i].BoundingBox.Width = SegmRight - SegmLeft;
                                SegmentsData[i].BoundingBox.Height = SegmBottom - SegmTop;
                            }
                            else
                            {
                                // save unavailability reason
                                if (GBMSGUI.CheckMask(DemoFormRef.DemoConfig.SessionOptions, GBMSGUI.SessionOption.AskUnavailabilityReason))
                                    SegmentsData[i].ItemData.UnavailabilityReason = UnavailReason;
                            }
                        }
                    }
                    else if (GBMSGUI.IsJoint(Item.ScanObjID))
                    {
                        // get bounding boxes of phalanges
                        // in the next release will be saved in AN2K record
                        if (GBMSGUI.IsThumbJoint(Item.ScanObjID))
                            num = 2;
                        else
                            num = 3;
                        for (i = 0; i < num; i++)
                        {
                            ret = MyGUI.GetSegmentationResult(i, 0,
                                out SegmAvailable, out SegmLeft, out SegmTop, out SegmRight, out SegmBottom,
                                out UnavailReason, out Quality,
                                null, out SegmSizeX, out SegmSizeY, out Resolution);
                            if (ret == GBMSGUI.ReturnCodes.Ret_Failure)
                            {
                                MessageBox.Show(MyGUI.GetErrorMessage() + " (GetSegmentationResult)",
                                    Application.ProductName,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            // save data to write after in EBTS record
                            if (SegmAvailable)
                            {
                                JointsBBoxData[IndexOfJointsData(Item.ScanObjID)].PhalangeBBox[i].X = SegmLeft;
                                JointsBBoxData[IndexOfJointsData(Item.ScanObjID)].PhalangeBBox[i].Y = SegmTop;
                                JointsBBoxData[IndexOfJointsData(Item.ScanObjID)].PhalangeBBox[i].Width = SegmRight - SegmLeft;
                                JointsBBoxData[IndexOfJointsData(Item.ScanObjID)].PhalangeBBox[i].Height = SegmBottom - SegmTop;
                            }
                        }
                    }
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
            mnuAcquireItem.Enabled = true;
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
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT))
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

        // get the correct picturebox where display segment image (in the place of the flat fingerprints)
        private PictureBox GetSegmentPictureBox(uint ScanObjID, int Index)
        {
            // 1.14.0.0 - also upper palm
            if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT))
            {
                if (Index == 0)
                    return pboxLeftFlatIndex;
                if (Index == 1)
                    return pboxLeftFlatMiddle;
                if (Index == 2)
                    return pboxLeftFlatRing;
                if (Index == 3)
                    return pboxLeftFlatLittle;
            }
            // 1.14.0.0 - also upper palm
            else if ((ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT) ||
                (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT))
            {
                if (Index == 0)
                    return pboxRightFlatIndex;
                if (Index == 1)
                    return pboxRightFlatMiddle;
                if (Index == 2)
                    return pboxRightFlatRing;
                if (Index == 3)
                    return pboxRightFlatLittle;
            }
            else if (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS)
            {
                if (Index == 0)
                    return pboxLeftFlatThumb;
                if (Index == 1)
                    return pboxRightFlatThumb;
            }
            //V1.1
            else if (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_LEFT)
            {
                if (Index == 0)
                    return pboxLeftFlatIndex;
                if (Index == 1)
                    return pboxLeftFlatMiddle;
            }
            else if (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_RIGHT)
            {
                if (Index == 0)
                    return pboxRightFlatIndex;
                if (Index == 1)
                    return pboxRightFlatMiddle;
            }
            //V1.2
            else if (ScanObjID == GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_INDEXES)
            {
                if (Index == 0)
                    return pboxLeftFlatIndex;
                if (Index == 1)
                    return pboxRightFlatIndex;
            }

            return null;
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

        private void UserDataForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
            {
                if (!ViewMode)
                {
                    // delete directory and contents
                    Directory.Delete(ImagesPath, true);
                }
            }

            TouchScreenTimer.Enabled = false;

            if (!ViewMode)
            {
                // save configuration of types for sequence
                if (chkSlaps.Checked)
                    DemoFormRef.DemoConfig.SequenceTypes |= DemoForm.SequenceType.Slaps;
                else
                    DemoFormRef.DemoConfig.SequenceTypes &= ~DemoForm.SequenceType.Slaps;
                if (chkFlat.Checked)
                    DemoFormRef.DemoConfig.SequenceTypes |= DemoForm.SequenceType.SingleFlat;
                else
                    DemoFormRef.DemoConfig.SequenceTypes &= ~DemoForm.SequenceType.SingleFlat;
                if (chkRolled.Checked)
                    DemoFormRef.DemoConfig.SequenceTypes |= DemoForm.SequenceType.Rolled;
                else
                    DemoFormRef.DemoConfig.SequenceTypes &= ~DemoForm.SequenceType.Rolled;
                if (chkPalms.Checked)
                    DemoFormRef.DemoConfig.SequenceTypes |= DemoForm.SequenceType.Palms;
                else
                    DemoFormRef.DemoConfig.SequenceTypes &= ~DemoForm.SequenceType.Palms;
                //V1.10
                if (chkRolledJoints.Checked)
                    DemoFormRef.DemoConfig.SequenceTypes |= DemoForm.SequenceType.RolledJoints;
                else
                    DemoFormRef.DemoConfig.SequenceTypes &= ~DemoForm.SequenceType.RolledJoints;
                if (chkFlatJointSides.Checked)
                    DemoFormRef.DemoConfig.SequenceTypes |= DemoForm.SequenceType.FlatJointSides;
                else
                    DemoFormRef.DemoConfig.SequenceTypes &= ~DemoForm.SequenceType.FlatJointSides;
                if (chkRolledTips.Checked)
                    DemoFormRef.DemoConfig.SequenceTypes |= DemoForm.SequenceType.RolledTips;
                else
                    DemoFormRef.DemoConfig.SequenceTypes &= ~DemoForm.SequenceType.RolledTips;
                if (chkRolledThenars.Checked)
                    DemoFormRef.DemoConfig.SequenceTypes |= DemoForm.SequenceType.RolledThenars;
                else
                    DemoFormRef.DemoConfig.SequenceTypes &= ~DemoForm.SequenceType.RolledThenars;

                // save configuration
                DemoForm.Configuration.Serialize(Path.ChangeExtension(Application.ExecutablePath, ".cfg"), DemoFormRef.DemoConfig);

                if (DemoFormRef.LCDPresent)
                    // return to Logo Screen
                    GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetLogoScreen();
            }

            // free images
            foreach (ScanItem Item in lstScannedObjects.Items)
                if (Item.ImagePictureBox.Image != null)
                    Item.ImagePictureBox.Image.Dispose();
            lstScannedObjects.Dispose();

            //if (gcUserDataFormHandle.IsAllocated)
            //    gcUserDataFormHandle.Free();

            //if (UserDataForm.gcUserDataFormHandle_St.IsAllocated)
            //    UserDataForm.gcUserDataFormHandle_St.Free();
        }



    }
}
