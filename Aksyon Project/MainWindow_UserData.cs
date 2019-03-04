using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBMSGUI_NET;
using System.IO;

namespace Aksyon_Project
{
    public partial class MainWindow
    {
        [Serializable]
        public class UserData
        {
            int mVersion;
            public int Version
            {
                get { return mVersion; }
                set { mVersion = value; }
            }
            String mPersonalityName;
            public String Name
            {
                get { return mPersonalityName; }
                set { mPersonalityName = value; }
            }
            int mAcquisitionDpi;
            public int AcquisitionDpi
            {
                get { return mAcquisitionDpi; }
                set { mAcquisitionDpi = value; }
            }
            String mDeviceName;
            public String DeviceName
            {
                get { return mDeviceName; }
                set { mDeviceName = value; }
            }
            String mDeviceSerialNumber;
            public String DeviceSerialNumber
            {
                get { return mDeviceSerialNumber; }
                set { mDeviceSerialNumber = value; }
            }

            public UserData()
            {
                mVersion = 1;
            }

            public static void Serialize(string file, UserData c)
            {
                // 1.13.5.0
                // use only one instance of serializer
                //System.Xml.Serialization.XmlSerializer xs
                //   = new System.Xml.Serialization.XmlSerializer(c.GetType());
                StreamWriter writer = File.CreateText(file);
                //xs.Serialize(writer, c);
                DemoUsersXmlSerializer.Serialize(writer, c);
                writer.Flush();
                writer.Close();
            }
            public static UserData Deserialize(string file)
            {
                // 1.13.5.0
                // use only one instance of serializer
                //System.Xml.Serialization.XmlSerializer xs
                //   = new System.Xml.Serialization.XmlSerializer(typeof(UserData));
                StreamReader reader = File.OpenText(file);
                //UserData c = (UserData)xs.Deserialize(reader);
                UserData c = (UserData)DemoUsersXmlSerializer.Deserialize(reader);
                reader.Close();
                return c;
            }
        }
    }
}
