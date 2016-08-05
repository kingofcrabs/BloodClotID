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

        static public void SaveCalib(CalibrationInfo calibInfo, string sFile)
        {
            int pos = sFile.LastIndexOf("\\");
            string sDir = sFile.Substring(0, pos);

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            if (File.Exists(sFile))
                File.Delete(sFile);

            XmlSerializer xs = new XmlSerializer(typeof(CalibrationInfo));
            Stream stream = new FileStream(sFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, calibInfo);
            stream.Close();
        }

        static public CalibrationInfo LoadCalib(string sFile)
        {
            CalibrationInfo calibInfo;
            if (!File.Exists(sFile))
                throw new FileNotFoundException(string.Format("位于：{0}的配置文件不存在", sFile));
            Stream stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(CalibrationInfo));
                calibInfo = xs.Deserialize(stream) as CalibrationInfo;
            }
            catch (Exception ex)
            {
                throw new Exception("Falied to load the setting: " + ex.Message);
            }
            finally
            {
                stream.Close();
            }
            return calibInfo;
        }
    }

    [Serializable]
    public class CalibrationInfo
    {
        public System.Windows.Size size;
        public List<Circle> circles;
        public CalibrationInfo()
        {
            size = new System.Windows.Size(0, 0);
            circles = new List<Circle>();
        }
        public CalibrationInfo(System.Windows.Size sz, List<Circle> cs)
        {
            size = sz;
            circles = cs;
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
        static public string CurrentAcquiredImageFolder { get; set; }
        public static string GetAcquiredImageRootFolder()
        {
            string sExeParent = GetExeParentFolder();
            bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            string subFolder = bUseTestImage ? "TestImages\\" : "AcquiredImages";
            string sImageFolder = sExeParent + subFolder;
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

        static private string GetCalibFolder()
        {
            string sFolder = GetExeParentFolder() + "\\Calib\\";
            if (!System.IO.Directory.Exists(sFolder))
                System.IO.Directory.CreateDirectory(sFolder);
            return sFolder;
        }

        static public string GetCalibFile(int cameraID)
        {
            return GetCalibFolder() + string.Format("ROIs_{0}.xml", cameraID);
        }


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

        public static string GetImagePath(int cameraId)
        {
            return CurrentAcquiredImageFolder + string.Format("{0}.jpg", cameraId);
        }

        internal static string GetIconFolder()
        {
            string sDataFolder = GetExeParentFolder() + "Icons\\";
            return sDataFolder;

        }

        public static void CreateAcquiredImageFolder()
        {
            bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            string subFolder = bUseTestImage ? "" : DateTime.Now.ToString("yyMMddhhmm") + "\\";
            CurrentAcquiredImageFolder = GetAcquiredImageRootFolder() + subFolder;
            CreateIfNotExist(CurrentAcquiredImageFolder);
        }
    }

    public struct RegistInfo
    {
        public static List<string> allowedCameraNames = new List<string>()
        {

              "CC5010000642","CD501000241","CD501000064","CD501000068","CD500002346","CD500002342"
        };
    }

    public class AcquireInfo
    {
        public int totalSample;
        public int curBatch;
        public int curPlate;
        public int samplesPerCamera;
        public int samplesThisBatch;
        public List<string> assays;
        private int maxSamplesPerBatch = 8;
        private static AcquireInfo instance;
       
        
        static public AcquireInfo Instance
        {
            get
            {
                if (instance == null)
                    instance = new AcquireInfo();
                return instance;
            }
        }

        public void NextPlate()
        {
            if (curPlate % assays.Count == 0)
                NextBatch();
            curPlate++;
        }

        private void NextBatch()
        {
            curBatch++;
        }

        private int CalculateSamplesThisBatch()
        {
            int cnt = maxSamplesPerBatch;
            if (totalSample < maxSamplesPerBatch * curBatch)
                cnt = totalSample % maxSamplesPerBatch;
            return cnt;
        }

        public void SetSampleCount(int val)
        {
            totalSample = val;
            curBatch = 1;
            curPlate = 1;

        }

        public  void SetLayout(bool isHorizontal)
        {
            maxSamplesPerBatch = isHorizontal ? 8 : 12;
        }


        public  int GetTotalPlateCnt()
        {
            return  (totalSample + maxSamplesPerBatch - 1) / maxSamplesPerBatch * assays.Count;
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
            System.Drawing.Bitmap cloneBitmap = (System.Drawing.Bitmap)bitmap.Clone();
            using (MemoryStream memory = new MemoryStream())
            {
                cloneBitmap.Save(memory, ImageFormat.Jpeg);
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
                return "OV";
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
