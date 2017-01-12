using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AI_Conquer.BotLibrary
{
    class ExternalData
    {
        public static ExternalData.SavedData BinaryData = new ExternalData.SavedData();

        [Serializable]
        public class SavedData
        {
            internal DateTime DateTimeNow;
            internal List<string> ListRbName;
            internal List<Point> ListRbPoint;
        }
        public static void SaveData()
        {
            var binFormat = new BinaryFormatter();

            using (Stream streamObj = new FileStream("CO_Data.dat", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                binFormat.Serialize(streamObj, BinaryData);
            }
        }
        public static void LoadSavedFile()
        {
            if (!File.Exists("CO_Data.dat")) return;

            var binFormat = new BinaryFormatter();
            using (Stream streamObj = File.OpenRead("CO_Data.dat"))
            {
                BinaryData = (SavedData)binFormat.Deserialize(streamObj);
            }
        }
    }
}
