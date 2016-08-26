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
        public Rect rect;
        public CalibrationInfo()
        {
            size = new System.Windows.Size(0, 0);
            circles = new List<Circle>();
        }
        public CalibrationInfo(System.Windows.Size sz, List<Circle> cs,Rect rc)
        {
            size = sz;
            circles = cs;
            rect = rc;
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


        static public string GetCalibFileCPlusPlus(int cameraID)
        {
            return GetCalibFolder() + string.Format("ROIs_{0}.txt", cameraID);
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
            //bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            string subFolder = GlobalVars.UseTestImage ? "" : DateTime.Now.ToString("yyMMddhhmm") + "\\";
            CurrentAcquiredImageFolder = GetAcquiredImageRootFolder() + subFolder ;
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
        public int curBatchID;  //for sample's id between 1-8, if there are three tests, so the 1-3 plate are in the same batch.
     
        public int curPlateID;
        public int samplesPerCamera;
        //public int samplesThisBatch;
        public List<string> assays;
        public static int samplesPerPlate;
        private static AcquireInfo instance;
        private const int  horizontalSampleCnt = 8;
        
        static public AcquireInfo Instance
        {
            get
            {
                if (instance == null)
                    instance = new AcquireInfo();
                return instance;
            }
        }
        public bool IsHorizontal
        {
            get
            {
                return horizontalSampleCnt == samplesPerPlate;
            }
            
        }

        public int BatchStartID
        {
            get
            {
                int sampleRangeStart = (curBatchID - 1) * samplesPerPlate + 1;
                return sampleRangeStart;
            }
        }

        public int BatchEndID
        {
            get
            {

                return BatchStartID + CalculateSamplesThisBatch() - 1;
            }
        }
        public string CurrentAssay
        {
            get
            {
                return assays[(curPlateID-1) % assays.Count];
            }
        }
        public void NextPlate()
        {
            curPlateID++;
            if ((curPlateID-1) % assays.Count == 0)
                NextBatch();
            //samplesThisBatch = CalculateSamplesThisBatch();
        }

        private void NextBatch()
        {
            curBatchID++;
        }

        public int CalculateSamplesThisBatch()
        {
            int cnt = samplesPerPlate;
            if (totalSample < samplesPerPlate * curBatchID)
                cnt = totalSample % samplesPerPlate;
            return cnt;
        }

        public void SetSampleCount(int val)
        {
            totalSample = val;
            curBatchID = 1;
            curPlateID = 1;
            //samplesThisBatch = CalculateSamplesThisBatch();
        }

        public  void SetLayout(bool isHorizontal)
        {
            samplesPerPlate = isHorizontal ? 8 : 12;
        }


        public  int GetTotalPlateCnt()
        {
            return  (totalSample + samplesPerPlate - 1) / samplesPerPlate * assays.Count;
        }
    }

    public static class UIHelper
    {
        public static T TryFindParent<T>(this DependencyObject child)
where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }
    }

    public class RenderHelper
    {
        static public ImageBrush CreateBrushFromFile(string file,CalibrationInfo calibInfo)
        {
            System.Drawing.Bitmap bitmap;
            using (var bmpTemp = new Bitmap(file))
            {
                bitmap = new Bitmap(bmpTemp);
            }
            return CreateBrushFromBitmap(bitmap,calibInfo);
        }

        static private ImageBrush CreateBrushFromBitmap(Bitmap bitmap,CalibrationInfo calibInfo)
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
            if(!GlobalVars.IsCalibration && calibInfo != null)
            {
                double xStartRatio = calibInfo.rect.TopLeft.X / calibInfo.size.Width;
                double yStartRatio = calibInfo.rect.TopLeft.Y / calibInfo.size.Height;
                double widthRatio = calibInfo.rect.Width / calibInfo.size.Width;
                double heightRatio = calibInfo.rect.Height / calibInfo.size.Height;
                imgBrush.Viewbox = new Rect(xStartRatio, yStartRatio, widthRatio, heightRatio);
            }
            //var transform = Matrix.Identity;
            //transform.RotateAt(90, 0.5, 0.5);
            //imgBrush.RelativeTransform = new MatrixTransform(transform);
            imgBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            return imgBrush;
        }
    }
 
    public class GlobalVars
    {
        static bool useTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
        static bool isCalib;
        static List<CalibrationInfo> calibInfos = null;
        static public bool IsCalibration
        {
            get
            {
                return isCalib;
            }
            set
            {
                isCalib = value;
            }
        }
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

        //static public List<CalibrationInfo> CalibInfos
        //{
        //    get
        //    {
        //        if(calibInfos == null)
        //        {
        //            calibInfos = new List<CalibrationInfo>();
        //            for (int i = 0; i< 4; i++)
        //            {
        //                int cameraID = i + 1;
        //                string sFile = FolderHelper.GetCalibFile(cameraID);
        //                calibInfos.Add(SerializeHelper.LoadCalib(sFile));
        //            }
        //        }
        //        return calibInfos;
        //    }
        //}


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
