using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using GBMSGUI_NET;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_AcquisitionProcessDefines;
using WSQPACK_NET_WRAPPER;

namespace Aksyon_Project
{
    public partial class MainWindow
    {
        // type of objects to acquire in sequence
        public class SequenceType
        {
            public const uint Slaps = 0x00000001;
            public const uint SingleFlat = 0x00000002;
            public const uint Rolled = 0x00000004;
            public const uint Palms = 0x00000008;
            //V1.10
            public const uint RolledJoints = 0x00000010;
            public const uint FlatJointSides = 0x00000020;
            public const uint RolledTips = 0x00000040;
            public const uint RolledThenars = 0x00000080;
            // 1.15.1.0
            public const uint RolledHypothenars = 0x00000100;
            public const uint UpperPalms = 0x00000200;
            public const uint LowerPalms = 0x00000400;
            public const uint WritersPalms = 0x00000800;
        }

        // Standard used  for image sizes
        public class ImageSizeStandards
        {
            public const int Custom = 0;
            public const int ANSI_NIST_ITL_1_2000 = 1;
            public const int ANSI_NIST_ITL_1_2000_INTERPOL = 2;
            public const int ISO_IEC_FCD_19794_4 = 3;
            public const int ANSI_NIST_ITL_1_2007 = 4;
            public const int GA_CHINA = 5;
        }

        // Image sizes
        public struct ImageSizes
        {
            public double UpperPalmWidth;
            public double UpperPalmHeight;
            public double LowerPalmWidth;
            public double LowerPalmHeight;
            public double WritersPalmWidth;
            public double WritersPalmHeight;
            public double FourFingersWidth;
            public double FourFingersHeight;
            public double TwoThumbsWidth;
            public double TwoThumbsHeight;
            public double FlatThumbWidth;
            public double FlatThumbHeight;
            public double FlatFingerWidth;
            public double FlatFingerHeight;
            public double RolledThumbWidth;
            public double RolledThumbHeight;
            public double RolledIndexWidth;
            public double RolledIndexHeight;
            public double RolledMiddleWidth;
            public double RolledMiddleHeight;
            public double RolledRingWidth;
            public double RolledRingHeight;
            public double RolledLittleWidth;
            public double RolledLittleHeight;
            //V1.10 - new objects for MS527
            public double RolledTipWidth;
            public double RolledTipHeight;
            public double RolledJointWidth;
            public double RolledJointHeight;
            public double FlatJointWidth;
            public double FlatJointHeight;
            public double RolledThenarWidth;
            public double RolledThenarHeight;
            // 1.15.0.0
            public double RolledHypothenarWidth;
            public double RolledHypothenarHeight;
        }

        // Image compressions
        public class ImageCompressions
        {
            public const int WSQ = 1;
            public const int JPEG2000 = 2;
        }

        // Languages
        public class GUILanguages
        {
            public const int English = 0;
            public const int Italian = 1;
            public const int Chinese = 2;
        }

        // 2.0.0.0 - recommended settings
        public class BestPracticeSettings
        {
            public const uint SessionOptions = GBMSGUI.SessionOption.SequenceCheck |
                GBMSGUI.SessionOption.DryFingerImageEnhancement;
            public const uint AcquisitionOptions = GBMSGUI.AcquisitionOption.Segmentation |
                GBMSGUI.AcquisitionOption.Autocapture | GBMSGUI.AcquisitionOption.Sound |
                GBMSGUI.AcquisitionOption.BlockAutocapture | GBMSGUI.AcquisitionOption.RemoveHaloLatent |
                GBMSGUI.AcquisitionOption.DetectInvalidPattern | GBMSGUI.AcquisitionOption.DetectIncompletePattern |
                GBMSGUI.AcquisitionOption.DetectInclination | GBMSGUI.AcquisitionOption.SoundOnRollPreviewEnd |
                GBMSGUI.AcquisitionOption.AdaptRollAreaPosition;
            public const int ArtefactsThreshold1 = 7;
            public const int ArtefactsThreshold2 = 14;
            public const int PatternValidityThreshold = 75;
            public const int PatternCompletenessThreshold = 85;
            public const int LowerPalmCompletenessThreshold1 = 80;
            public const int LowerPalmCompletenessThreshold2 = 90;
            public const int NFIQQualityThreshold1 = 2;
            public const int NFIQQualityThreshold2 = 3;
            public const int NFIQ2QualityThreshold1 = 25;
            public const int NFIQ2QualityThreshold2 = 50;
            public const int GBQualityThreshold1 = 50;
            public const int GBQualityThreshold2 = 70;
            public const uint BlockAutocaptureMask = GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_SURFACE_NOT_NORMA |
                GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_SLIDING |
                GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_LEFT |
                GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_RIGHT |
                GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_TOP |
                GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_DISPLACED_DOWN;
            public const uint IgnoredDiagnosticMask = 0;
        }

        // 2.0.0.0 - more permissing for demonstration purposes
        public class DemoSettings
        {
            public const uint SessionOptions = GBMSGUI.SessionOption.SequenceCheck |
                GBMSGUI.SessionOption.DryFingerImageEnhancement;
            public const uint AcquisitionOptions = GBMSGUI.AcquisitionOption.Segmentation |
                GBMSGUI.AcquisitionOption.Autocapture | GBMSGUI.AcquisitionOption.Sound |
                GBMSGUI.AcquisitionOption.BlockAutocapture | GBMSGUI.AcquisitionOption.RemoveHaloLatent |
                GBMSGUI.AcquisitionOption.DetectInvalidPattern | GBMSGUI.AcquisitionOption.DetectIncompletePattern |
                GBMSGUI.AcquisitionOption.DetectInclination | GBMSGUI.AcquisitionOption.SoundOnRollPreviewEnd |
                GBMSGUI.AcquisitionOption.AdaptRollAreaPosition | GBMSGUI.AcquisitionOption.NoArtefactsDisplay;
            public const int ArtefactsThreshold1 = 15;
            public const int ArtefactsThreshold2 = 30;
            public const int PatternValidityThreshold = 65;
            public const int PatternCompletenessThreshold = 75;
            public const int LowerPalmCompletenessThreshold1 = 70;
            public const int LowerPalmCompletenessThreshold2 = 80;
            public const int NFIQQualityThreshold1 = 3;
            public const int NFIQQualityThreshold2 = 4;
            public const int GBQualityThreshold1 = 50;
            public const int GBQualityThreshold2 = 70;
            public const uint BlockAutocaptureMask = GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_SURFACE_NOT_NORMA;
            public const uint IgnoredDiagnosticMask = 0; // ???
        }

        // Configuration class
        [Serializable]
        public class Configuration
        {
            int mVersion;
            public int Version
            {
                get { return mVersion; }
                set { mVersion = value; }
            }

            // options
            uint mSessionOptions;
            public uint SessionOptions
            {
                get { return mSessionOptions; }
                set { mSessionOptions = value; }
            }

            uint mAcquisitionOptions;
            public uint AcquisitionOptions
            {
                get { return mAcquisitionOptions; }
                set { mAcquisitionOptions = value; }
            }

            uint mSequenceTypes;
            public uint SequenceTypes
            {
                get { return mSequenceTypes; }
                set { mSequenceTypes = value; }
            }

            // settings
            bool mBlockAutocaptureContrast;
            public bool BlockAutocaptureContrast
            {
                get { return mBlockAutocaptureContrast; }
                set { mBlockAutocaptureContrast = value; }
            }

            // Statndard for image sizes
            int mImageSizeStandard;
            public int ImageSizeStandard
            {
                get { return mImageSizeStandard; }
                set
                {
                    mImageSizeStandard = value;
                    // set values
                    SetImageSizeStandardValues();
                }
            }

            // size of images (stored in inches)
            ImageSizes mImageSize;
            public ImageSizes ImageSize
            {
                get { return mImageSize; }
                set { mImageSize = value; }
            }


            // IAFIS quality algorithm
            int mIAFIsQualityAlgorithm;
            public int IAFIsQualityAlgorithm
            {
                get { return mIAFIsQualityAlgorithm; }
                set { mIAFIsQualityAlgorithm = value; }
            }

            // window size
            Rectangle mWindowSize;
            public Rectangle WindowSize
            {
                get { return mWindowSize; }
                set { mWindowSize = value; }
            }

            // window state
            bool mWindowMaximized;
            public bool WindowMaximized
            {
                get { return mWindowMaximized; }
                set { mWindowMaximized = value; }
            }

            // Image compression
            int mImageCompression500;
            public int ImageCompression500
            {
                get { return mImageCompression500; }
                set { mImageCompression500 = value; }
            }

            int mImageCompression1000;
            public int ImageCompression1000
            {
                get { return mImageCompression1000; }
                set { mImageCompression1000 = value; }
            }

            // WSQ Bit rate for 500 dpi
            double mWQSBitRate500;
            public double WQSBitRate500
            {
                get { return mWQSBitRate500; }
                set { mWQSBitRate500 = value; }
            }

            // WSQ Bit rate for 1000 dpi
            double mWQSBitRate1000;
            public double WQSBitRate1000
            {
                get { return mWQSBitRate1000; }
                set { mWQSBitRate1000 = value; }
            }

            // JPEG2000 compression rate for 500 dpi
            int mJPEGRate500;
            public int JPEGRate500
            {
                get { return mJPEGRate500; }
                set { mJPEGRate500 = value; }
            }

            // JPEG2000 compression rate for 1000 dpi
            int mJPEGRate1000;
            public int JPEGRate1000
            {
                get { return mJPEGRate1000; }
                set { mJPEGRate1000 = value; }
            }

            // DactyScan26 Frame Rate
            double mDS26FrameRate;
            public double DS26FrameRate
            {
                get { return mDS26FrameRate; }
                set { mDS26FrameRate = value; }
            }

            // DactyScan84 Full Frame Rate
            double mDS84FullFrameRate;
            public double DS84FullFrameRate
            {
                get { return mDS84FullFrameRate; }
                set { mDS84FullFrameRate = value; }
            }

            // DactyScan84 Partial Frame Rate
            double mDS84PartialFrameRate;
            public double DS84PartialFrameRate
            {
                get { return mDS84PartialFrameRate; }
                set { mDS84PartialFrameRate = value; }
            }

            // DactyScan40i Full Frame Rate
            double mDS40iFullFrameRate;
            public double DS40iFullFrameRate
            {
                get { return mDS40iFullFrameRate; }
                set { mDS40iFullFrameRate = value; }
            }

            // DactyScan40i Partial Frame Rate
            double mDS40iPartialFrameRate;
            public double DS40iPartialFrameRate
            {
                get { return mDS40iPartialFrameRate; }
                set { mDS40iPartialFrameRate = value; }
            }

            // DactyScan84c Full Low Resolution Frame Rate
            double mDS84cFullLowResFrameRate;
            public double DS84cFullLowResFrameRate
            {
                get { return mDS84cFullLowResFrameRate; }
                set { mDS84cFullLowResFrameRate = value; }
            }

            // DactyScan84c Full High Resolution Frame Rate
            double mDS84cFullHiResFrameRate;
            public double DS84cFullHiResFrameRate
            {
                get { return mDS84cFullHiResFrameRate; }
                set { mDS84cFullHiResFrameRate = value; }
            }

            // DactyScan84c Partial Frame Rate
            double mDS84cPartialFrameRate;
            public double DS84cPartialFrameRate
            {
                get { return mDS84cPartialFrameRate; }
                set { mDS84cPartialFrameRate = value; }
            }

            // MC517 Full Low Resolution Frame Rate
            double mMC517FullLowResFrameRate;
            public double MC517FullLowResFrameRate
            {
                get { return mMC517FullLowResFrameRate; }
                set { mMC517FullLowResFrameRate = value; }
            }

            // MC517 Full High Resolution Frame Rate
            double mMC517FullHiResFrameRate;
            public double MC517FullHiResFrameRate
            {
                get { return mMC517FullHiResFrameRate; }
                set { mMC517FullHiResFrameRate = value; }
            }

            // MC517 Partial Frame Rate IQS
            double mMC517PartialIQSFrameRate;
            public double MC517PartialIQSFrameRate
            {
                get { return mMC517PartialIQSFrameRate; }
                set { mMC517PartialIQSFrameRate = value; }
            }

            // MC517 Partial Frame Rate GA
            double mMC517PartialGAFrameRate;
            public double MC517PartialGAFrameRate
            {
                get { return mMC517PartialGAFrameRate; }
                set { mMC517PartialGAFrameRate = value; }
            }

            // DactyScan32 Full Frame Rate
            double mDS32FullFrameRate;
            public double DS32FullFrameRate
            {
                get { return mDS32FullFrameRate; }
                set { mDS32FullFrameRate = value; }
            }

            // DactyScan40i Partial Frame Rate
            double mDS32PartialFrameRate;
            public double DS32PartialFrameRate
            {
                get { return mDS32PartialFrameRate; }
                set { mDS32PartialFrameRate = value; }
            }

            // MS527 Full Low Resolution Frame Rate
            double mMS527FullLowResFrameRate;
            public double MS527FullLowResFrameRate
            {
                get { return mMS527FullLowResFrameRate; }
                set { mMS527FullLowResFrameRate = value; }
            }

            // MS527 Full High Resolution Frame Rate
            double mMS527FullHiResFrameRate;
            public double MS527FullHiResFrameRate
            {
                get { return mMS527FullHiResFrameRate; }
                set { mMS527FullHiResFrameRate = value; }
            }

            // MS527 Partial Frame Rate IQS
            double mMS527PartialIQSFrameRate;
            public double MS527PartialIQSFrameRate
            {
                get { return mMS527PartialIQSFrameRate; }
                set { mMS527PartialIQSFrameRate = value; }
            }

            // MS527 Partial Frame Rate GA
            double mMS527PartialGAFrameRate;
            public double MS527PartialGAFrameRate
            {
                get { return mMS527PartialGAFrameRate; }
                set { mMS527PartialGAFrameRate = value; }
            }

            // MS527 Partial Frame Rate Joint area
            double mMS527PartialJointFrameRate;
            public double MS527PartialJointFrameRate
            {
                get { return mMS527PartialJointFrameRate; }
                set { mMS527PartialJointFrameRate = value; }
            }

            // MS527 Partial Frame Rate Thenar area
            double mMS527PartialThenarFrameRate;
            public double MS527PartialThenarFrameRate
            {
                get { return mMS527PartialThenarFrameRate; }
                set { mMS527PartialThenarFrameRate = value; }
            }

            // DactyScan84t Full Low Resolution Frame Rate
            double mDS84tFullLowResFrameRate;
            public double DS84tFullLowResFrameRate
            {
                get { return mDS84tFullLowResFrameRate; }
                set { mDS84tFullLowResFrameRate = value; }
            }

            // DactyScan84t Full High Resolution Frame Rate
            double mDS84tFullHiResFrameRate;
            public double DS84tFullHiResFrameRate
            {
                get { return mDS84tFullHiResFrameRate; }
                set { mDS84tFullHiResFrameRate = value; }
            }

            // DactyScan84t Partial Frame Rate
            double mDS84tPartialFrameRate;
            public double DS84tPartialFrameRate
            {
                get { return mDS84tPartialFrameRate; }
                set { mDS84tPartialFrameRate = value; }
            }

            // DactyID20 Frame Rate
            double mDID20FrameRate;
            public double DID20FrameRate
            {
                get { return mDID20FrameRate; }
                set { mDID20FrameRate = value; }
            }

            // 2.3.0.0
            // MS527t Full Low Resolution Frame Rate
            double mMS527tFullLowResFrameRate;
            public double MS527tFullLowResFrameRate
            {
                get { return mMS527tFullLowResFrameRate; }
                set { mMS527tFullLowResFrameRate = value; }
            }

            // MS527t Full High Resolution Frame Rate
            double mMS527tFullHiResFrameRate;
            public double MS527tFullHiResFrameRate
            {
                get { return mMS527tFullHiResFrameRate; }
                set { mMS527tFullHiResFrameRate = value; }
            }

            // MS527t Partial Frame Rate IQS
            double mMS527tPartialIQSFrameRate;
            public double MS527tPartialIQSFrameRate
            {
                get { return mMS527tPartialIQSFrameRate; }
                set { mMS527tPartialIQSFrameRate = value; }
            }

            // MS527t Partial Frame Rate GA
            double mMS527tPartialGAFrameRate;
            public double MS527tPartialGAFrameRate
            {
                get { return mMS527tPartialGAFrameRate; }
                set { mMS527tPartialGAFrameRate = value; }
            }

            // MS527t Partial Frame Rate Joint area
            double mMS527tPartialJointFrameRate;
            public double MS527tPartialJointFrameRate
            {
                get { return mMS527tPartialJointFrameRate; }
                set { mMS527tPartialJointFrameRate = value; }
            }

            // MS527t Partial Frame Rate Thenar area
            double mMS527tPartialThenarFrameRate;
            public double MS527tPartialThenarFrameRate
            {
                get { return mMS527tPartialThenarFrameRate; }
                set { mMS527tPartialThenarFrameRate = value; }
            }


            // Roll Area size (MC517 only)
            uint mRollAreaSize;
            public uint RollAreaSize
            {
                get { return mRollAreaSize; }
                set { mRollAreaSize = value; }
            }

            int mGUILanguage;
            public int GUILanguage
            {
                get { return mGUILanguage; }
                set { mGUILanguage = value; }
            }

            int mFingerContactEvaluationMode;
            public int FingerContactEvaluationMode
            {
                get { return mFingerContactEvaluationMode; }
                set { mFingerContactEvaluationMode = value; }
            }

            bool mJointSegmentation;
            public bool JointSegmentation
            {
                get { return mJointSegmentation; }
                set { mJointSegmentation = value; }
            }

            int mRollDirection;
            public int RollDirection
            {
                get { return mRollDirection; }
                set { mRollDirection = value; }
            }

            uint mBlockAutocaptureMask;
            public uint BlockAutocaptureMask
            {
                get { return mBlockAutocaptureMask; }
                set { mBlockAutocaptureMask = value; }
            }

            uint mIgnoredDiagnosticMask;
            public uint IgnoredDiagnosticMask
            {
                get { return mIgnoredDiagnosticMask; }
                set { mIgnoredDiagnosticMask = value; }
            }

            bool mEnableBlockComposition;
            public bool EnableBlockComposition
            {
                get { return mEnableBlockComposition; }
                set { mEnableBlockComposition = value; }
            }

            bool mEnableBlockAutocaptureLedColorFeedback;
            public bool EnableBlockAutocaptureLedColorFeedback
            {
                get { return mEnableBlockAutocaptureLedColorFeedback; }
                set { mEnableBlockAutocaptureLedColorFeedback = value; }
            }

            // 1.15.0.0
            int mLiveSegmEvalTimeout;
            public int LiveSegmEvalTimeout
            {
                get { return mLiveSegmEvalTimeout; }
                set { mLiveSegmEvalTimeout = value; }
            }

            // 2.0.0.0
            int mSWFakeFingerDetectionThreshold;
            public int SWFakeFingerDetectionThreshold
            {
                get { return mSWFakeFingerDetectionThreshold; }
                set { mSWFakeFingerDetectionThreshold = value; }
            }

            // 2.0.0.0
            int mHWFakeFingerDetectionThreshold;
            public int HWFakeFingerDetectionThreshold
            {
                get { return mHWFakeFingerDetectionThreshold; }
                set { mHWFakeFingerDetectionThreshold = value; }
            }

            // 2.0.0.0
            // values different in "Best practice" and "Demo" settings
            int mArtefactsThreshold1;
            public int ArtefactsThreshold1
            {
                get { return mArtefactsThreshold1; }
                set { mArtefactsThreshold1 = value; }
            }
            int mArtefactsThreshold2;
            public int ArtefactsThreshold2
            {
                get { return mArtefactsThreshold2; }
                set { mArtefactsThreshold2 = value; }
            }

            int mPatternValidityThreshold;
            public int PatternValidityThreshold
            {
                get { return mPatternValidityThreshold; }
                set { mPatternValidityThreshold = value; }
            }

            int mPatternCompletenessThreshold;
            public int PatternCompletenessThreshold
            {
                get { return mPatternCompletenessThreshold; }
                set { mPatternCompletenessThreshold = value; }
            }

            int mLowerPalmCompletenessThreshold1;
            public int LowerPalmCompletenessThreshold1
            {
                get { return mLowerPalmCompletenessThreshold1; }
                set { mLowerPalmCompletenessThreshold1 = value; }
            }

            int mLowerPalmCompletenessThreshold2;
            public int LowerPalmCompletenessThreshold2
            {
                get { return mLowerPalmCompletenessThreshold2; }
                set { mLowerPalmCompletenessThreshold2 = value; }
            }

            int mNFIQQualityThreshold1;
            public int NFIQQualityThreshold1
            {
                get { return mNFIQQualityThreshold1; }
                set { mNFIQQualityThreshold1 = value; }
            }
            int mNFIQQualityThreshold2;
            public int NFIQQualityThreshold2
            {
                get { return mNFIQQualityThreshold2; }
                set { mNFIQQualityThreshold2 = value; }
            }

            int mNFIQ2QualityThreshold1;
            public int NFIQ2QualityThreshold1
            {
                get { return mNFIQ2QualityThreshold1; }
                set { mNFIQ2QualityThreshold1 = value; }
            }
            int mNFIQ2QualityThreshold2;
            public int NFIQ2QualityThreshold2
            {
                get { return mNFIQ2QualityThreshold2; }
                set { mNFIQ2QualityThreshold2 = value; }
            }

            int mGBQualityThreshold1;
            public int GBQualityThreshold1
            {
                get { return mGBQualityThreshold1; }
                set { mGBQualityThreshold1 = value; }
            }
            int mGBQualityThreshold2;
            public int GBQualityThreshold2
            {
                get { return mGBQualityThreshold2; }
                set { mGBQualityThreshold2 = value; }
            }

            public Configuration()
            {
                mVersion = 1;

                // default values
                /*
                mSessionOptions = 0;
                mSessionOptions |= GBMSGUI.SessionOption.SequenceCheck;
                mSessionOptions |= GBMSGUI.SessionOption.DryFingerImageEnhancement;
                */
                // 2.0.0.0
                mSessionOptions = BestPracticeSettings.SessionOptions;

                /*
                mAcquisitionOptions = 0;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.Autocapture;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.BlockAutocapture;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.Sound;
                // no more active by default - they are time consuming...
                //mAcquisitionOptions |= GBMSGUI.AcquisitionOption.PalmPrintQualityCalculation;
                //mAcquisitionOptions |= GBMSGUI.AcquisitionOption.RotateFinger;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.DetectInclination;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.DetectIncompletePattern;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.DetectInvalidPattern;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.RemoveHaloLatent;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.Segmentation;
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.SoundOnRollPreviewEnd;
                // 1.15.0.0
                mAcquisitionOptions |= GBMSGUI.AcquisitionOption.DeletePalmFingerSegments;
                */
                // 2.0.0.0
                mAcquisitionOptions = BestPracticeSettings.AcquisitionOptions;

                mSequenceTypes = 0;
                mSequenceTypes |= SequenceType.Slaps;
                mSequenceTypes |= SequenceType.Rolled;
                mSequenceTypes |= SequenceType.Palms;

                mBlockAutocaptureContrast = false;

                // image size
                // set first values not defined in all standards
                mImageSize.FlatFingerWidth = 1;
                mImageSize.FlatFingerHeight = 1;
                mImageSize.TwoThumbsWidth = 3.2;
                mImageSize.TwoThumbsHeight = 3;
                mImageSize.FlatThumbWidth = 1;
                mImageSize.FlatThumbHeight = 2;
                //V1.10
                mImageSize.RolledTipWidth = 1.6;
                mImageSize.RolledTipHeight = 1;
                mImageSize.RolledJointWidth = 1.6;
                mImageSize.RolledJointHeight = 5;
                //mImageSize.FlatJointWidth = 0.966;
                mImageSize.FlatJointWidth = 1.6;
                mImageSize.FlatJointHeight = 5;
                mImageSize.RolledThenarWidth = 3;
                mImageSize.RolledThenarHeight = 4.5;
                // 1.15.0.0
                mImageSize.RolledHypothenarWidth = 3;
                mImageSize.RolledHypothenarHeight = 4.5;

                // set others
                mImageSizeStandard = ImageSizeStandards.ANSI_NIST_ITL_1_2007;
                SetImageSizeStandardValues();

                mIAFIsQualityAlgorithm = GBMSGUI.QualityAlgorithms.NFIQAlgorithm;

                mWindowSize = new Rectangle(0, 0, 1024, 768);
                mWindowMaximized = true;

                mImageCompression500 = ImageCompressions.WSQ;
                mWQSBitRate500 = 0.75;  // 15:1 compression
                //mWQSBitRate500 = NW_WSQPACK_RATE_DEFINITIONS.NW_WSQPACK_RECOMMENDED;
                mJPEGRate500 = 14;      // 14:1 compression
                mImageCompression1000 = ImageCompressions.JPEG2000;
                mWQSBitRate1000 = 0.75;  // 15:1 compression
                //mWQSBitRate1000 = NW_WSQPACK_RATE_DEFINITIONS.NW_WSQPACK_RECOMMENDED;
                mJPEGRate1000 = 14;     // 14:1 compression

                // default frame rates
                uint FrameRateOptions = 0;
                double MaxFrameRate;
                double MinFrameRate;
                double DefFrameRate;
                uint RollAreaStandard = 0;

                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS26, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS26,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS26FrameRate = DefFrameRate;

                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS84FullFrameRate = DefFrameRate;
                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ROLL_AREA, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS84PartialFrameRate = DefFrameRate;

                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS40iFullFrameRate = DefFrameRate;
                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ROLL_AREA, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS40I,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS40iPartialFrameRate = DefFrameRate;

                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C, 0, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS84cFullLowResFrameRate = DefFrameRate;
                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS84cFullHiResFrameRate = DefFrameRate;
                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ROLL_AREA, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84C,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS84cPartialFrameRate = DefFrameRate;

                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517, 0, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMC517FullLowResFrameRate = DefFrameRate;
                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMC517FullHiResFrameRate = DefFrameRate;

                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ROLL_AREA, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_IQS, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMC517PartialIQSFrameRate = DefFrameRate;

                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ROLL_AREA, GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_GA, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MC517,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMC517PartialGAFrameRate = DefFrameRate;

                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS32FullFrameRate = DefFrameRate;
                //GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange_Global(GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32, GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_ROLL_AREA, 0, out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS32,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS32PartialFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527FullLowResFrameRate = DefFrameRate;
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527FullHiResFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527PartialIQSFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527PartialGAFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527PartialJointFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_THENAR,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527PartialThenarFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84t,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS84tFullLowResFrameRate = DefFrameRate;
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84t,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS84tFullHiResFrameRate = DefFrameRate;
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DS84t,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDS84tPartialFrameRate = DefFrameRate;

                // 2.0.1.0
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_DSID20,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mDID20FrameRate = DefFrameRate;

                // 2.3.0.0
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527tFullLowResFrameRate = DefFrameRate;
                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t,
                    GBMSAPI_NET_FrameRateOptions.GBMSAPI_NET_FRO_FULL_RESOLUTION_MODE,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527tFullHiResFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527tPartialIQSFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527tPartialGAFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527tPartialJointFrameRate = DefFrameRate;

                GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_GetFrameRateRange2(
                    GBMSAPI_NET_DeviceName.GBMSAPI_NET_DN_MS527t,
                    0,
                    GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_THENAR,
                    out MaxFrameRate, out MinFrameRate, out DefFrameRate);
                mMS527tPartialThenarFrameRate = DefFrameRate;


                // Roll Area size (MC517 only)
                mRollAreaSize = GBMSAPI_NET_DeviceFeatures.GBMSAPI_NET_DF_ROLL_AREA_IQS;

                // Language
                mGUILanguage = GUILanguages.English;

                mFingerContactEvaluationMode = GBMSGUI.FingerContactEvaluationMode.DryWetWarning;

                mJointSegmentation = true;

                mRollDirection = GBMSGUI.AdaptiveRollDirection.ToCenter;

                mBlockAutocaptureMask = 0;
                mBlockAutocaptureMask |= GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_SURFACE_NOT_NORMA |
                    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_EXT_LIGHT_TOO_STRONG |
                    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_SLIDING |
                    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_DISPLACED_DOWN |
                    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_LEFT |
                    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_RIGHT |
                    GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_TOP;

                mIgnoredDiagnosticMask = 0;
                mEnableBlockComposition = false;
                mEnableBlockAutocaptureLedColorFeedback = false;
                // 1.15.0.0
                mLiveSegmEvalTimeout = 7;
                // 2.0.0.0
                mHWFakeFingerDetectionThreshold = 90;
                mSWFakeFingerDetectionThreshold = 30;

                // 2.0.0.0
                mArtefactsThreshold1 = BestPracticeSettings.ArtefactsThreshold1;
                mArtefactsThreshold2 = BestPracticeSettings.ArtefactsThreshold2;
                mPatternValidityThreshold = BestPracticeSettings.PatternValidityThreshold;
                mPatternCompletenessThreshold = BestPracticeSettings.PatternCompletenessThreshold;
                mLowerPalmCompletenessThreshold1 = BestPracticeSettings.LowerPalmCompletenessThreshold1;
                mLowerPalmCompletenessThreshold2 = BestPracticeSettings.LowerPalmCompletenessThreshold2;
                mNFIQQualityThreshold1 = BestPracticeSettings.NFIQQualityThreshold1;
                mNFIQQualityThreshold2 = BestPracticeSettings.NFIQQualityThreshold2;
                mNFIQ2QualityThreshold1 = BestPracticeSettings.NFIQ2QualityThreshold1;
                mNFIQ2QualityThreshold2 = BestPracticeSettings.NFIQ2QualityThreshold2;
                mGBQualityThreshold1 = BestPracticeSettings.GBQualityThreshold1;
                mGBQualityThreshold2 = BestPracticeSettings.GBQualityThreshold2;
            }

            public static void Serialize(string file, Configuration c)
            {
                // 1.13.5.0
                // use only one instance of serializer
                //System.Xml.Serialization.XmlSerializer xs
                //   = new System.Xml.Serialization.XmlSerializer(c.GetType());
                StreamWriter writer = File.CreateText(file);
                //xs.Serialize(writer, c);
                DemoConfigXmlSerializer.Serialize(writer, c);
                writer.Flush();
                writer.Close();
                writer.Dispose();
            }
            public static Configuration Deserialize(string file)
            {
                // 1.13.5.0
                // use only one instance of serializer
                //System.Xml.Serialization.XmlSerializer xs
                //   = new System.Xml.Serialization.XmlSerializer(typeof(Configuration));
                StreamReader reader = File.OpenText(file);
                //Configuration c = (Configuration)xs.Deserialize(reader);
                Configuration c = (Configuration)DemoConfigXmlSerializer.Deserialize(reader);
                reader.Close();
                reader.Dispose();

                //V1.10 - new added fields are set to 0
                Configuration Defaults = new Configuration();
                ImageSizes DefaultImageSize = Defaults.ImageSize;
                ImageSizes NewImageSize = c.ImageSize;
                if (NewImageSize.RolledTipWidth == 0)
                    NewImageSize.RolledTipWidth = Defaults.ImageSize.RolledTipWidth;
                if (NewImageSize.RolledTipHeight == 0)
                    NewImageSize.RolledTipHeight = Defaults.ImageSize.RolledTipHeight;
                if (NewImageSize.RolledJointWidth == 0)
                    NewImageSize.RolledJointWidth = Defaults.ImageSize.RolledJointWidth;
                if (NewImageSize.RolledJointHeight == 0)
                    NewImageSize.RolledJointHeight = Defaults.ImageSize.RolledJointHeight;
                if (NewImageSize.FlatJointWidth == 0)
                    NewImageSize.FlatJointWidth = Defaults.ImageSize.FlatJointWidth;
                if (NewImageSize.FlatJointHeight == 0)
                    NewImageSize.FlatJointHeight = Defaults.ImageSize.FlatJointHeight;
                if (NewImageSize.RolledThenarWidth == 0)
                    NewImageSize.RolledThenarWidth = Defaults.ImageSize.RolledThenarWidth;
                if (NewImageSize.RolledThenarHeight == 0)
                    NewImageSize.RolledThenarHeight = Defaults.ImageSize.RolledThenarHeight;
                // 1.15.0.0
                if (NewImageSize.RolledHypothenarWidth == 0)
                    NewImageSize.RolledHypothenarWidth = Defaults.ImageSize.RolledHypothenarWidth;
                if (NewImageSize.RolledHypothenarHeight == 0)
                    NewImageSize.RolledHypothenarHeight = Defaults.ImageSize.RolledHypothenarHeight;

                c.ImageSize = NewImageSize;

                return c;
            }

            private void SetImageSizeStandardValues()
            {
                // sizes are expressed in inches
                switch (mImageSizeStandard)
                {
                    case ImageSizeStandards.ANSI_NIST_ITL_1_2000:
                        mImageSize.UpperPalmWidth = 5;
                        mImageSize.UpperPalmHeight = 5;
                        mImageSize.LowerPalmWidth = 5;
                        mImageSize.LowerPalmHeight = 5;
                        mImageSize.WritersPalmWidth = 1.8;
                        mImageSize.WritersPalmHeight = 5;
                        mImageSize.FourFingersWidth = 3.2;
                        mImageSize.FourFingersHeight = 2;
                        //mImageSize.TwoThumbsWidth = 3.2;
                        //mImageSize.TwoThumbsHeight = 2;
                        mImageSize.FlatThumbWidth = 1;
                        mImageSize.FlatThumbHeight = 2;
                        //mImageSize.FlatFingerWidth = 1;
                        //mImageSize.FlatFingerHeight = 1;
                        mImageSize.RolledThumbWidth = 1.6;
                        mImageSize.RolledThumbHeight = 1.5;
                        mImageSize.RolledIndexWidth = 1.6;
                        mImageSize.RolledIndexHeight = 1.5;
                        mImageSize.RolledMiddleWidth = 1.6;
                        mImageSize.RolledMiddleHeight = 1.5;
                        mImageSize.RolledRingWidth = 1.6;
                        mImageSize.RolledRingHeight = 1.5;
                        mImageSize.RolledLittleWidth = 1.6;
                        mImageSize.RolledLittleHeight = 1.5;
                        break;
                    case ImageSizeStandards.ANSI_NIST_ITL_1_2000_INTERPOL:
                        mImageSize.UpperPalmWidth = 5;
                        mImageSize.UpperPalmHeight = 5;
                        mImageSize.LowerPalmWidth = 5;
                        mImageSize.LowerPalmHeight = 5;
                        mImageSize.WritersPalmWidth = 1.752;
                        mImageSize.WritersPalmHeight = 5;
                        mImageSize.FourFingersWidth = 2.756;
                        mImageSize.FourFingersHeight = 2.56;
                        //mImageSize.TwoThumbsWidth = 2.756;
                        //mImageSize.TwoThumbsHeight = 2.56;
                        mImageSize.FlatThumbWidth = 1.182;
                        mImageSize.FlatThumbHeight = 2.166;
                        //mImageSize.FlatFingerWidth = 1.182;
                        //mImageSize.FlatFingerHeight = 2.166;
                        mImageSize.RolledThumbWidth = 1.6;
                        mImageSize.RolledThumbHeight = 1.574;
                        mImageSize.RolledIndexWidth = 1.574;
                        mImageSize.RolledIndexHeight = 1.574;
                        mImageSize.RolledMiddleWidth = 1.574;
                        mImageSize.RolledMiddleHeight = 1.574;
                        mImageSize.RolledRingWidth = 1.574;
                        mImageSize.RolledRingHeight = 1.574;
                        mImageSize.RolledLittleWidth = 1.3;
                        mImageSize.RolledLittleHeight = 1.574;
                        break;
                    case ImageSizeStandards.ISO_IEC_FCD_19794_4:
                        mImageSize.UpperPalmWidth = 5;
                        mImageSize.UpperPalmHeight = 5;
                        mImageSize.LowerPalmWidth = 5;
                        mImageSize.LowerPalmHeight = 5;
                        mImageSize.WritersPalmWidth = 1.8;
                        mImageSize.WritersPalmHeight = 5;
                        mImageSize.FourFingersWidth = 3.2;
                        mImageSize.FourFingersHeight = 3;
                        mImageSize.TwoThumbsWidth = 2;
                        mImageSize.TwoThumbsHeight = 3;
                        //mImageSize.FlatThumbWidth = 1;
                        //mImageSize.FlatThumbHeight = 2;
                        //mImageSize.FlatFingerWidth = 1;
                        //mImageSize.FlatFingerHeight = 1;
                        mImageSize.RolledThumbWidth = 1.6;
                        mImageSize.RolledThumbHeight = 1.5;
                        mImageSize.RolledIndexWidth = 1.6;
                        mImageSize.RolledIndexHeight = 1.5;
                        mImageSize.RolledMiddleWidth = 1.6;
                        mImageSize.RolledMiddleHeight = 1.5;
                        mImageSize.RolledRingWidth = 1.6;
                        mImageSize.RolledRingHeight = 1.5;
                        mImageSize.RolledLittleWidth = 1.6;
                        mImageSize.RolledLittleHeight = 1.5;
                        break;
                    case ImageSizeStandards.ANSI_NIST_ITL_1_2007:
                        mImageSize.UpperPalmWidth = 5;
                        mImageSize.UpperPalmHeight = 5;
                        mImageSize.LowerPalmWidth = 5;
                        mImageSize.LowerPalmHeight = 5;
                        mImageSize.WritersPalmWidth = 1.8;
                        mImageSize.WritersPalmHeight = 5;
                        mImageSize.FourFingersWidth = 3.2;
                        mImageSize.FourFingersHeight = 3;
                        mImageSize.TwoThumbsWidth = 3.2;
                        mImageSize.TwoThumbsHeight = 3;
                        mImageSize.FlatThumbWidth = 1;
                        mImageSize.FlatThumbHeight = 2;
                        //mImageSize.FlatFingerWidth = 1;
                        //mImageSize.FlatFingerHeight = 1;
                        mImageSize.RolledThumbWidth = 1.6;
                        mImageSize.RolledThumbHeight = 1.5;
                        mImageSize.RolledIndexWidth = 1.6;
                        mImageSize.RolledIndexHeight = 1.5;
                        mImageSize.RolledMiddleWidth = 1.6;
                        mImageSize.RolledMiddleHeight = 1.5;
                        mImageSize.RolledRingWidth = 1.6;
                        mImageSize.RolledRingHeight = 1.5;
                        mImageSize.RolledLittleWidth = 1.6;
                        mImageSize.RolledLittleHeight = 1.5;
                        break;
                    case ImageSizeStandards.GA_CHINA:
                        mImageSize.UpperPalmWidth = 4.608;
                        mImageSize.UpperPalmHeight = 4.608;
                        mImageSize.LowerPalmWidth = 4.608;
                        mImageSize.LowerPalmHeight = 4.608;
                        mImageSize.WritersPalmWidth = 2.048;
                        mImageSize.WritersPalmHeight = 3.936;
                        mImageSize.FourFingersWidth = 3.2;
                        mImageSize.FourFingersHeight = 3;
                        mImageSize.TwoThumbsWidth = 3.2;
                        mImageSize.TwoThumbsHeight = 3;
                        mImageSize.FlatThumbWidth = 1.28;
                        mImageSize.FlatThumbHeight = 1.28;
                        //mImageSize.FlatFingerWidth = 1;
                        //mImageSize.FlatFingerHeight = 1;
                        mImageSize.RolledThumbWidth = 1.28;
                        mImageSize.RolledThumbHeight = 1.28;
                        mImageSize.RolledIndexWidth = 1.28;
                        mImageSize.RolledIndexHeight = 1.28;
                        mImageSize.RolledMiddleWidth = 1.28;
                        mImageSize.RolledMiddleHeight = 1.28;
                        mImageSize.RolledRingWidth = 1.28;
                        mImageSize.RolledRingHeight = 1.28;
                        mImageSize.RolledLittleWidth = 1.28;
                        mImageSize.RolledLittleHeight = 1.28;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
