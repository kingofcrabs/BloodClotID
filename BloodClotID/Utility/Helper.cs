using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace Utility
{

    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
    public class SerializeHelper
    {

        static public void SaveCalib(PlatePositon platePosition, string sFile)
        {
            int pos = sFile.LastIndexOf("\\");
            string sDir = sFile.Substring(0, pos);

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            if (File.Exists(sFile))
                File.Delete(sFile);

            XmlSerializer xs = new XmlSerializer(typeof(PlatePositon));
            Stream stream = new FileStream(sFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, platePosition);
            stream.Close();
        }

        public static string Serialize<T>(T value)
        {
            if (value == null)
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();

            //StringWriter textWriter = new StringWriter()

            using (StringWriterWithEncoding textWriter = new StringWriterWithEncoding(Encoding.Default))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }

        public static T Deserialize<T>(string xml) where T : class
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }
            Object obj = new object();
            Stream stream = new FileStream(xml, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                obj = xs.Deserialize(stream) as T;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falied to load the setting: " + ex.Message);
            }
            stream.Close();
            return (T)obj;
        }

        static public PlatePositon LoadCalib(string sFile)
        {
            PlatePositon platePosition = new PlatePositon();
            if (!File.Exists(sFile))
                return platePosition;
            Stream stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(PlatePositon));
                platePosition = xs.Deserialize(stream) as PlatePositon;
            }
            catch (Exception ex)
            {
                throw new Exception("Falied to load the setting: " + ex.Message);
            }
            finally
            {
                stream.Close();
            }
            return platePosition;
        }
    }

    [Serializable]
    public class CalibrationInfo
    {
    
        public List<Circle> circles;
        //public Rect rect;
        public CalibrationInfo()
        {
            circles = new List<Circle>();
        }
        public CalibrationInfo(List<Circle> cs)
        {
            circles = cs;
          
        }
    }

    public class FolderHelper
    {

        static bool thisStartImageFolderCreated = false;
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
        public static string GetDataFolder()
        {
            string sExeParent = GetExeParentFolder();
            string sDataFolder = sExeParent + "Data\\";
            CreateIfNotExist(sDataFolder);
            return sDataFolder;
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
            string subFolder = bUseTestImage ? "TestImages\\" : "AcquiredImages\\";
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

    

        static private string GetCalibFolder()
        {
            string sFolder = GetExeParentFolder() + "\\Calib\\";
            if (!System.IO.Directory.Exists(sFolder))
                System.IO.Directory.CreateDirectory(sFolder);
            return sFolder;
        }

        static public string GetCalibFile()
        {
            return GetCalibFolder() + string.Format("calib.xml");
        }


      


        private static void CreateIfNotExist(string sFolder)
        {
            if (!Directory.Exists(sFolder))
                Directory.CreateDirectory(sFolder);
        }

        private static string GetLatestImagePathInner()
        {

            return GetImagePath() + "latest.jpg";
        }

        internal static string GetTestImagePath()
        {
            return GetAcquiredImageRootFolder() + "test2.jpg";
        }

        public static string GetImagePath()
        {
            return CurrentAcquiredImageFolder;
        }

        internal static string GetIconFolder()
        {
            string sDataFolder = GetExeParentFolder() + "Icons\\";
            return sDataFolder;

        }

        public static void CreateAcquiredImageFolder()
        {
            if (thisStartImageFolderCreated)
                return;
            //bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            string subFolder = GlobalVars.UseTestImage ? "" : DateTime.Now.ToString("yyyyMMdd") + "\\";
            CurrentAcquiredImageFolder = GetAcquiredImageRootFolder() + subFolder ;
            CreateIfNotExist(CurrentAcquiredImageFolder);
            thisStartImageFolderCreated = true;
        }
    }

  

    public class AcquireInfo
    {
        public int totalSample;
        public int curPlateID;
        public int samplesPerCamera;
        public bool hasControl;
        public bool isHITest;
        public string assayName;
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

        public bool HasControl
        {
            get
            {
                return hasControl;
            }
            set
            {
                hasControl = value;
            }
        }


        public bool IsHI
        {
            get
            {
                return isHITest;
            }
            set
            {
                isHITest = value;
            }
        }

    
     
        public string CurrentAssay
        {
            get
            {
                return assayName;
            }
        }

        public string SnapShot { get; set; }
        public string OriginalImage { get; set; }

        public void NextPlate()
        {
            curPlateID++;
       
            //samplesThisBatch = CalculateSamplesThisBatch();
        }

      

        public int CalculateSamplesThisBatch()
        {
            int cnt = samplesPerPlate;
            if (totalSample < samplesPerPlate)
                cnt = totalSample % samplesPerPlate;
            return cnt;
        }

        public void SetSampleCount(int val)
        {
            totalSample = val;
            curPlateID = 1;
            //samplesThisBatch = CalculateSamplesThisBatch();
        }

        public  void SetLayout(bool isHorizontal)
        {
            samplesPerPlate = isHorizontal ? 8 : 12;
        }


        public  int GetTotalPlateCnt()
        {
            return  (totalSample + samplesPerPlate - 1) / samplesPerPlate;
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
            //if(!GlobalVars.IsCalibration && calibInfo != null)
            //{
            
            //    double widthRatio = calibInfo.rect.Width / calibInfo.rect.Width;
            //    double heightRatio = calibInfo.rect.Height / calibInfo.rect.Height;
            //    imgBrush.Viewbox = new Rect(0, 1, widthRatio, heightRatio);
            //}
            //var transform = Matrix.Identity;
            //transform.RotateAt(90, 0.5, 0.5);
            //imgBrush.RelativeTransform = new MatrixTransform(transform);
            imgBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            return imgBrush;
        }

        public static System.Windows.Media.Brush CreateBrush(Bitmap bitmap, System.Windows.Size size)
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
            imgBrush.Stretch = Stretch.Fill;
            imgBrush.ImageSource = bitmapImage;
            return imgBrush;
        }
    }

    public class GlobalVars
    {
        static bool useTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
        static bool isCalib = false;
        private Dictionary<int, int> eachSampleResult = new Dictionary<int, int>();
        static GlobalVars instance;
        DateTime dateTime;
        public static GlobalVars Instance{
            get
            {
                if(instance == null)
                {
                    instance = new GlobalVars();
                }
                return instance;
            }
        }

        public DateTime AcquiredDateTime
        {
            get
            {
                return dateTime;
            }
        }

        public Dictionary<int,int> Result
        {
            get
            {
                return eachSampleResult;
            }
        }

        public void SetResult(List<int> indexs)
        {
            dateTime = DateTime.Now;
            int startWellID = AcquireInfo.Instance.curPlateID * AcquireInfo.Instance.samplesPerCamera;
            foreach (var result in indexs)
            {
                if (eachSampleResult.ContainsKey(startWellID))
                    eachSampleResult[startWellID] = result+1;
                else
                    eachSampleResult.Add(startWellID, result+1);
                startWellID++;
            }
        }

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


      
    }

    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            //TODO: when we remove the old OnPropertyChanged method we need to uncomment the below line
            //OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        [Obsolete("Please use the new RaisePropertyChanged method. This method will be removed to comply wth .NET coding standards. If you are overriding this method, you should overide the OnPropertyChanged(PropertyChangedEventArgs args) signature instead.")]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

    }
}
