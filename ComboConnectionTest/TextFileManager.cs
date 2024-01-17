using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComboConnectionTest
{
    public class TextFileManager
    {
        // S/W 로그 남기기용 파일 Writer
        public class TextFileWriter
        {
            static StreamWriter writer = null;

            public static void MakeLogTextFile()
            {
                string curDirectory = Directory.GetCurrentDirectory();
                string txtFilePath = curDirectory + "\\" + "Log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                //if (!Directory.Exists(curDirectory))
                //{
                //    Directory.CreateDirectory(curDirectory);
                //}

                if (!File.Exists(txtFilePath))
                {
                    // Create File If not Exist
                    writer = new StreamWriter(txtFilePath) { AutoFlush = true };
                    // writer = File.CreateText(txtFilePath);
                }

                if (writer == null)
                {
                    writer = new StreamWriter(txtFilePath, true) { AutoFlush = true };
                }
            }

            public static void CloseFile()
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                    writer = null;
                }
            }
            
            public static void WriteLog(string strLog)
            {
                if (writer != null)
                {
                    writer.WriteLine(strLog);
                }
            }
        }
    }
}
