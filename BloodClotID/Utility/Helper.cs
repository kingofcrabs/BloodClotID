using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Utility
{
    public class SerializeHelper
    {

        static public void SaveConfig(List<Circle> circles, string sFile)
        {
            int pos = sFile.LastIndexOf("\\");
            string sDir = sFile.Substring(0, pos);

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            if (File.Exists(sFile))
                File.Delete(sFile);

            XmlSerializer xs = new XmlSerializer(typeof(List<Circle>));
            Stream stream = new FileStream(sFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, circles);
            stream.Close();
        }

        static public void LoadConfig(ref List<Circle> circles, string sFile)
        {
            if (!File.Exists(sFile))
                throw new FileNotFoundException(string.Format("位于：{0}的配置文件不存在", sFile));
            Stream stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<Circle>));
                circles = xs.Deserialize(stream) as List<Circle>;
            }
            catch (Exception ex)
            {
                throw new Exception("Falied to load the setting: " + ex.Message);
            }
            finally
            {
                stream.Close();
            }
        }
    }
    public class FolderHelper
    {
        static public string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        static public string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }

        static public string GetConfigFolder()
        {
            string sConfigFolder = GetExeParentFolder() + "Config\\";
            CreateIfNotExist(sConfigFolder);
            return sConfigFolder;
        }

        public static string GetOutputFolder()
        {
            string sExeParent = GetExeParentFolder();
            string sOutputFolder = sExeParent + "Output\\";
            CreateIfNotExist(sOutputFolder);
            return sOutputFolder;
        }

        public static string GetAcquiredImageFolder()
        {
            string sExeParent = GetExeParentFolder();
            string sImageFolder = sExeParent + "AcquiredImages\\";
            CreateIfNotExist(sImageFolder);
            return sImageFolder;
        }

        static public string GetLatestImagePath()
        {
            bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            string s = GetLatestImagePathInner();
            if (bUseTestImage)
                s = GetTestImagePath();
            return s;
        }

        static public string GetDataFolder()
        {
            string sDataFolder = GetExeParentFolder() + "Data\\";
            CreateIfNotExist(sDataFolder);
            return sDataFolder;
        }


        //static public string GetLatestImage()
        //{
        //    string dir = ConfigurationManager.AppSettings["imageFolder"];
        //    var files = Directory.EnumerateFiles(dir, "*.jpg");
        //    List<FileInfo> fileInfos = new List<FileInfo>();
        //    foreach (var file in files)
        //    {
        //        fileInfos.Add(new FileInfo(file));
        //    }
        //    var latest = fileInfos.OrderBy(x => x.CreationTime).Last();
        //    return latest.FullName;
        //}

        private static void CreateIfNotExist(string sFolder)
        {
            if (!Directory.Exists(sFolder))
                Directory.CreateDirectory(sFolder);
        }

        private static string GetLatestImagePathInner()
        {

            return GetDataFolder() + "latest.jpg";
        }

        internal static string GetTestImagePath()
        {
            return GetDataFolder() + "test.jpg";
        }

        internal static string GetIconFolder()
        {
            string sDataFolder = GetExeParentFolder() + "Icons\\";
            return sDataFolder;

        }
    }



    public struct GlobalVals
    {
        static public int totalSample;
        static public int curBatch;
        static public int samplesPerCamera;
        static public int samplesThisBatch;
        static private int maxSamplesPerBatch = 8;

        static public List<string> allowedCameraNames = new List<string>()
        {

                "CC5010000642","CD501000241","CD501000064","CD501000068"
        };
        public static List<string> assays;

        static public void NextBatch()
        {
            curBatch++;
            samplesThisBatch = CalculateSamplesThisBatch();
          
        }

        private static int CalculateSamplesThisBatch()
        {
            int cnt = maxSamplesPerBatch;
            if (GlobalVals.totalSample < GlobalVals.maxSamplesPerBatch * GlobalVals.curBatch)
                cnt = GlobalVals.totalSample % GlobalVals.maxSamplesPerBatch;
            return cnt;
        }

        public static void SetSampleCount(int val)
        {
            totalSample = val;
            curBatch = 1;
            samplesThisBatch = CalculateSamplesThisBatch();
        }

        public static void SetLayout(bool isHorizontal)
        {
            maxSamplesPerBatch = isHorizontal ? 8 : 12;
        }
    }

    public class RenderHelper
    {
        static public ImageBrush CreateBrushFromFile(string file)
        {
            System.Drawing.Bitmap bitmap;
            using (var bmpTemp = new Bitmap(file))
            {
                bitmap = new Bitmap(bmpTemp);
            }
            return CreateBrushFromBitmap(bitmap);
        }

        static private ImageBrush CreateBrushFromBitmap(Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            System.Drawing.Bitmap cloneBitmpa = (System.Drawing.Bitmap)bitmap.Clone();
            using (MemoryStream memory = new MemoryStream())
            {
                cloneBitmpa.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = bitmapImage;
            imgBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            return imgBrush;
        }


        public static System.Windows.Media.Brush CreateBrushFromStream(MemoryStream memoryStream)
        {
            System.Drawing.Bitmap bitmap;
            using (var bmpTemp = new Bitmap(memoryStream))
            {
                bitmap = new Bitmap(bmpTemp);
            }
            return CreateBrushFromBitmap(bitmap);
        }
    }
 
    public class ConfigValues
    {
        static bool useTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
        
        static public string Vendor
        {
            get
            {
                return "Do3Think";
            }
        }
        static public bool UseTestImage
        {
            get
            {
                return useTestImage;
            }
        }
        public static List<AssayGroup> ReadGroups()
        {
            string s = FolderHelper.GetExeParentFolder();
            string sPanelFolder = s + "\\Groups\\";
            IEnumerable<string> allPanelFiles = Directory.EnumerateFiles(sPanelFolder, "*.txt");
            List<AssayGroup> assayGroups = new List<AssayGroup>();
            foreach (string sFile in allPanelFiles)
            {
                List<string> assays = new List<string>();
                int slashIndex = sFile.LastIndexOf("\\");
                string shortName = sFile.Substring(slashIndex + 1);
                shortName = shortName.Substring(0, shortName.Length - 4);
                assays = File.ReadAllLines(sFile).ToList();
                assayGroups.Add(new AssayGroup(shortName, assays));
            }
            return assayGroups;
        }

        //public static string PlateType
        //{
        //    get
        //    {
        //        return plateType;
        //    }
        //}
    }
}
