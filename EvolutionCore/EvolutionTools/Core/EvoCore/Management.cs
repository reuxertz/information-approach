using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EvolutionTools
{
    public static class Management
    {
        public delegate void HandlerCopyException(Exception ex);

        //Static Helper Functions
        public static string[] ConvertListCSVToArray(List<List<string>> l)
        {
            var r = new List<string>();

            for (int i = 0; i < l.Count; i++)
            {
                var t = "";

                for (int j = 0; j < l[i].Count; j++)
                {
                    t += l[i][j];

                    if (j < l[i].Count - 1)
                        t += ",";
                }
                r.Add(t);
            }
            return r.ToArray<string>();
        }
        
        //Static File Functions
        public static string[] GetTextFromFile(string path)
        {
            if (!System.IO.File.Exists(path))
                Management.WriteTextToFile(path, new string[] { }, false);

            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
                return new string[] { };
            }

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fs);
            }
            catch (Exception ex)
            {
                sr.Close();
                sr.Dispose();
                fs.Close();
                fs.Dispose();

                throw ex;
            }

            var tl = new List<string>();
            string t = "";

            while (true)
            {
                try
                {
                    t = sr.ReadLine();
                }
                catch (Exception ex)
                {
                    sr.Close();
                    sr.Dispose();
                    fs.Close();
                    fs.Dispose();
                }


                if (t == null)
                    break;

                tl.Add(t);
            }

            sr.Close();
            sr.Dispose();
            fs.Close();
            fs.Dispose();

            return tl.ToArray<string>();
        }
        public static void WriteTextToFile(string filePath, List<string> data, bool hidePath)
        {
            Management.WriteTextToFile(filePath, data.ToArray<string>(), false);
        }
        public static void WriteTextToFile(string filePath, string[] data, bool hidePath)
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            //CreateDateTime TextFile
            FileStream fs = null;
            try
            {
                //FileStream fs = new FileStream("blah", FileMode.Open, FileAccess.ReadWrite);
                fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            catch (Exception ex)
            {
                fs.Close();
                fs.Dispose();
                throw ex;
            }

            //Set DateTime in textfile
            StreamWriter sw = new StreamWriter(fs);

            foreach (string d in data)
            {

                try
                {
                    sw.WriteLine(d);
                }
                catch (Exception ex)
                {
                    sw.Close();
                    sw.Dispose();
                    fs.Close();
                    fs.Dispose();
                    throw ex;
                }
            }

            sw.Close();
            sw.Dispose();
            fs.Close();
            fs.Dispose();

            //Set TextFile to Hidden
            if (hidePath)
                System.IO.File.SetAttributes(filePath, FileAttributes.Hidden);
        }
        public static void WriteTextToCSVFile(string filePath, string[][] rawData, bool hidePath)
        {
            var data = new string[rawData.Length];
            for (int i = 0; i < rawData.Length; i++)
            {
                var curStr = "";
                for (int j = 0; j < rawData[i].Length; j++)
                {
                    curStr += rawData[i][j];

                    if (j < rawData[i].Length - 1)
                        curStr += ",";
                }
                data[i] = curStr;
            }

            Management.WriteTextToFile(filePath, data, hidePath);
        }
        public static void WriteTextToCSVFile(string filePath, string[] header, double[][] rawData, bool hidePath)
        {
            string[] d = new string[rawData.Length + 1];
            
            var h = "";
            for (int i = 0; i < header.Length; i++)
            {
                h += "" + header[i];

                if (i < header.Length - 1)
                    h += ", ";
            }
            d[0] = h;

            for (int i = 0; i < rawData.Length; i++)
            {
                var curS = "";
                for (int j = 0; j < rawData[i].Length; j++)
                {
                    curS += "" + rawData[i][j];

                    if (j < rawData[i].Length - 1)
                        curS += ", ";

                }
                d[i + 1] = curS;
            }

            Management.WriteTextToFile(filePath, d, hidePath);
        }
        public static void WriteTexttoTXTFile(string filePath, string[] header, double[][] rawData, bool hidePath)
        {

        }
        public static void AddTextLineToFile(string filePath, string newLine, bool hidePath)
        {
            var s = Management.GetTextFromFile(filePath);

            s.ToList<string>().Add(newLine);

            Management.WriteTextToFile(filePath, s.ToArray<string>(), hidePath);
        }
        public static void CopyFile(string sourceFilePath, string destFilePath, bool overwrite)
        {
            System.IO.File.Copy(sourceFilePath, destFilePath, overwrite);
        }
        public static void CopyFile(string sourceFolderPath, string destFolderPath, string fileName, bool overwrite)
        {
            Management.CopyFile(Management.AppendFilePath(sourceFolderPath, fileName), Management.AppendFilePath(destFolderPath, fileName), overwrite);
        }
        public static void CopyFiles(string sourceFolderPath, string destFolderPath, string[] allFiles, bool overwrite, HandlerCopyException copyEx)
        {
            foreach (string f in allFiles)
            {
                try
                {
                    Management.CopyFile(sourceFolderPath, destFolderPath, f, overwrite);
                }
                catch (Exception ex)
                {
                    copyEx(ex);
                }
            }
        }

        //File name Functions
        public static string AppendFilePath(string folderPath, string fileName)
        {
            return folderPath + @"\" + fileName;
        }
        public static string[] SplitDirectoryAndFile(string fullpath)
        {            
            var a = fullpath.Split('\\').ToList<string>();
            var b = a[a.Count - 1];
            a.RemoveAt(a.Count - 1);

            var c = "";
            for (int i = 0; i < a.Count; i++)
                c += a[i] + @"\";

            return new string[2] { c, b };

        }
        public static string NextPrefixNumber(string[] keys, string prefix)
        {
            var pref = "default";

            if (prefix != null && prefix != "")
                pref = prefix;

            var index = 0;
            var curName = "";
            while (true)
            {
                var found = false;
                index++;
                curName = pref + "" + "(" + index + ")";
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i].Contains(curName))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    break;
            }

            return curName;
        }
    
        //Drive Mappings
        public static string[] GetMappedNetworkDrives()
        {

            //Get Network
            IWshNetwork_Class network = new IWshNetwork_Class();
            WshNetwork net = new WshNetwork();

            //Get Network Drives
            IWshCollection colNetDrives = net.EnumNetworkDrives();

            //CYcle
            var enumerator = colNetDrives.GetEnumerator();
            var rll = new List<string>();
            while (enumerator.MoveNext())
            {

                //Get current item
                string item = enumerator.Current as string;

                //If Item is localname of drive and the local name is null, set onlocalname to false for next iteratoni and continue
                if (item == "")
                {
                    enumerator.MoveNext();
                    continue;
                }

                //item has name, store in return list
                enumerator.MoveNext();
                rll.Add(item + "?" + (enumerator.Current as string));
            }
           

            return rll.ToArray();
        }
        public static void MapNetworkDrive(String local, String remote)
        {
            WshNetwork net = new WshNetwork();
            net.MapNetworkDrive(local, remote);
        }
        public static void UnMapNetworkDrive(String localName)
        {
            WshNetwork net = new WshNetwork();
            net.RemoveNetworkDrive(localName, true, true);
        }
    }
}
