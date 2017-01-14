using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Configuration;
using System.Threading;
using CameraHandle = System.Int32;
using System.Drawing;
using MVSDK;
using Utility;

namespace CameraControl
{
   
    public interface IImageAcquirer
    {
        event EventHandler onFinished;
        void Stop();
        void Start(string sFile, int cameraID, CameraSettings cameraSetting);
    }

    public class MyEventArgs : EventArgs
    {
        public MyEventArgs(string errMsg)
        {
            ErrMsg = errMsg;
        }
        public string ErrMsg { get; set; }
    }

    public class ImageAcquirerFactory
    {
        public static IImageAcquirer CreateImageAcquirer(string vendorName)
        {
            if(vendorName == "Do3Think")
            {
                return new Do3ThinkImageAcquirer();
            }
            else
            {
                return new MindVisionImageAcquirer();
            }
        }
    }


    public class MindVisionImageAcquirer : IImageAcquirer
    {
        #region variable

        //Camera1
        protected CameraHandle m_hCamera1 = 0;
        protected IntPtr m_ImageBuffer1;
        protected tSdkCameraCapbility tCameraCapability1;
        protected int m_iDisplayedFrames = 0;
        protected tSdkFrameHead m_tFrameHead1;


        //Camera2
        protected CameraHandle m_hCamera2 = 0;
        protected IntPtr m_ImageBuffer2;
        protected tSdkCameraCapbility tCameraCapability2;
        protected tSdkFrameHead m_tFrameHead2;

        #endregion

        const int cameraCnt = 2;
        bool bInitialized = false;
        public event EventHandler onFinished;

        public void Init()
        {
            tSdkCameraDevInfo[] tCameraDevInfoList = new tSdkCameraDevInfo[cameraCnt];
            IntPtr ptr;
            int i;

            ptr = Marshal.AllocHGlobal(Marshal.SizeOf(new tSdkCameraDevInfo()) * cameraCnt);
            int iCameraCounts = cameraCnt;//max 2 camera2

            if (MvApi.CameraEnumerateDevice(ptr, ref iCameraCounts) != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                throw new Exception("未能找到相机");
            if (iCameraCounts < cameraCnt)
                throw new Exception(string.Format("未能找到{0}台相机", iCameraCounts));
            for (i = 0; i < cameraCnt; i++)
            {
                tCameraDevInfoList[i] = (tSdkCameraDevInfo)Marshal.PtrToStructure((IntPtr)((int)ptr + i * Marshal.SizeOf(new tSdkCameraDevInfo())), typeof(tSdkCameraDevInfo));
            }

            Marshal.FreeHGlobal(ptr);
            bool containFirst = tCameraDevInfoList.Any(x => IsFirst(x.acFriendlyName));
            bool containSecond = tCameraDevInfoList.Any(x => IsSecond(x.acFriendlyName));

            if (!containFirst || (cameraCnt == 2 && !containSecond))
            {
                throw new Exception("程序未注册，请先注册！");
            }
            tSdkCameraDevInfo firstDevInfo = tCameraDevInfoList.Where(x => IsFirst(x.acFriendlyName)).First();
            InitCamera(firstDevInfo, true);
          

            tSdkCameraDevInfo secondDevInfo = tCameraDevInfoList.Where(x => IsSecond(x.acFriendlyName)).First();
            InitCamera(secondDevInfo, false);
          
        }

        public void Stop()
        {
            if (m_hCamera1 != 0)
            {
                MvApi.CameraUnInit(m_hCamera1);
                Marshal.FreeHGlobal(m_ImageBuffer1);
                m_hCamera1 = 0;
            }

            if (m_hCamera2 != 0)
            {
                MvApi.CameraUnInit(m_hCamera2);
                Marshal.FreeHGlobal(m_ImageBuffer2);
                m_hCamera2 = 0;
            }

        }

        private void InitCamera(tSdkCameraDevInfo devInfo, bool isFirst)
        {

            CameraHandle m_hCamera = 0;
            if (MvApi.CameraInit(ref devInfo, -1, -1, ref m_hCamera) == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            {
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(new tSdkCameraCapbility()));
                MvApi.CameraGetCapability(m_hCamera, ptr);
                tSdkCameraCapbility tCameraCapability = (tSdkCameraCapbility)Marshal.PtrToStructure(ptr, typeof(tSdkCameraCapbility));
                Marshal.FreeHGlobal(ptr);
                var tmpBuffer = Marshal.AllocHGlobal(tCameraCapability.sResolutionRange.iWidthMax * tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);
                if (isFirst)
                {
                    m_ImageBuffer1 = tmpBuffer;
                }
                else
                {
                    m_ImageBuffer2 = tmpBuffer;
                }

                tSdkImageResolution tResolution;
                tResolution.uSkipMode = 0;
                tResolution.uBinAverageMode = 0;
                tResolution.uBinSumMode = 0;
                tResolution.uResampleMask = 0;
                tResolution.iVOffsetFOV = 0;
                tResolution.iHOffsetFOV = 0;
                tResolution.iWidthFOV = tCameraCapability.sResolutionRange.iWidthMax;
                tResolution.iHeightFOV = tCameraCapability.sResolutionRange.iHeightMax;
                tResolution.iWidth = tCameraCapability.sResolutionRange.iWidthMax;
                tResolution.iHeight = tCameraCapability.sResolutionRange.iHeightMax; 
                tResolution.iIndex = 0xff;
                tResolution.acDescription = new byte[32];
                tResolution.iWidthZoomHd = 0;
                tResolution.iHeightZoomHd = 0;
                tResolution.iWidthZoomSw = 0;
                tResolution.iHeightZoomSw = 0;
                //MvApi.CameraShowSettingPage(m_hCamera, 1);
                //double exposureTime = 0;
                //MvApi.CameraGetExposureTime(m_hCamera, ref exposureTime);
                //MvApi.CameraSetExposureTime(m_hCamera, exposureTime/5);
                MvApi.CameraSetResolutionForSnap(m_hCamera, ref tResolution);
            }
            else
            {
                if (isFirst)
                    m_hCamera1 = 0;
                else
                    m_hCamera2 = 0;
                throw new Exception("初始化相机失败！");
            }
            if (isFirst)
                m_hCamera1 = m_hCamera;
            else
                m_hCamera2 = m_hCamera;
        }

        private bool IsFirst(byte[] byteArray)
        {
            string friendName = System.Text.Encoding.ASCII.GetString(byteArray);
            return friendName.Contains("rex001");
        }

        private bool IsSecond(byte[] byteArray)
        {
            string friendName = System.Text.Encoding.ASCII.GetString(byteArray);
            return friendName.Contains("rex002");
        }


        private string Convert2String(byte[] byteArray)
        {
            return System.Text.Encoding.ASCII.GetString(byteArray);
        }


        public void Start(string sFile, int cameraID, CameraSettings cameraSettings )
        {

            string errMsg = "";
            try
            {
                StartImpl(sFile, cameraID);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            if( onFinished != null)
                onFinished(this,new MyEventArgs(errMsg));
        }

        private void StartImpl(string sFile, CameraHandle cameraID)
        {
            
            if (!bInitialized)
            {
                Init();
                bInitialized = true;
            }

            tSdkFrameHead frameHead;
            uint uRawBuffer;
            CameraHandle cameraHandle = m_hCamera1;
            IntPtr imgBuffer = m_ImageBuffer1;
            PrepareCamera(cameraID == 1, ref cameraHandle, ref imgBuffer);
            if (cameraHandle <= 0)
            {
                throw new Exception("相机未初始化！");
            }
            MvApi.CameraPlay(cameraHandle);
            bool bContinue = true;
            int retryTimes = 10;
            while (bContinue)
            {
                var eStatus = MvApi.CameraGetImageBuffer(cameraHandle, out frameHead, out uRawBuffer, 500);
                int redGain = 0, greenGain = 0, blueGain = 0;
                MvApi.CameraGetGain(cameraHandle, ref redGain, ref greenGain, ref blueGain);
                if (eStatus == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    MvApi.CameraImageProcess(cameraHandle, uRawBuffer, imgBuffer, ref frameHead);
                    MvApi.CameraImageOverlay(cameraHandle, imgBuffer, ref frameHead);
                    MvApi.CameraDisplayRGB24(cameraHandle, imgBuffer, ref frameHead);
                    MvApi.CameraReleaseImageBuffer(cameraHandle, uRawBuffer);
                    int dotPosition = sFile.LastIndexOf('.');
                    string sOrgFile = sFile;
                    sFile = sFile.Substring(0, dotPosition);
                    byte[] file_path_bytes = Encoding.Default.GetBytes(sFile);
                    MvApi.CameraSaveImage(cameraHandle, file_path_bytes, imgBuffer, ref frameHead, emSdkFileType.FILE_BMP, 100);
                    Image img = Image.FromFile(sOrgFile);
                    var newImg = ImageProcessor.Resize(img, new Size(1280, 960));
                    img.Dispose();
                    newImg.Save(sOrgFile);
                    bContinue = false;
                }
                else
                {
                    retryTimes--;
                    Thread.Sleep(1000);
                    if (retryTimes == 0)
                        throw new Exception("采集图片失败！");
                }
            }
            MvApi.CameraPause(cameraHandle);
        }

        private void PrepareCamera(bool isFirst, ref CameraHandle cameraHanlde, ref IntPtr imgBuffer)
        {
            cameraHanlde = isFirst ? m_hCamera1 : m_hCamera2;
            imgBuffer = isFirst ? m_ImageBuffer1 : m_ImageBuffer2;
        }
        

    
    }

    public class ImageProcessor
    {
        public static Image Resize(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        
    }

    public class Do3ThinkImageAcquirer: IImageAcquirer
    {
        public string m_sCameraName;
        public emDSRunMode m_Runmode;
        public int[] m_iCameraIDs;
        private string[] m_sCameraExpectedNameList;
        private string[] m_sCameraRealNameList;
        string firstCamera = ConfigurationManager.AppSettings["FirstCameraID"];
        string secondCamera = ConfigurationManager.AppSettings["SecondCameraID"];
        string XResolution = ConfigurationManager.AppSettings["XResolution"];
        bool bInitialized = false;
        dvpCamera.DelegateProc psub = new dvpCamera.DelegateProc(SnapThreadCallback);
        static int frames = 0;
      

       
        public event EventHandler onFinished;

        public static int SnapThreadCallback(int m_iCam, IntPtr pbyBuffer, ref tDSFrameInfo sFrInfo)
        {
            frames++;
            //处理图像数据       
            IntPtr pBmp24 = dvpCamera.CameraISP(m_iCam, pbyBuffer, ref sFrInfo);

            //显示RGB24数据到画面
            dvpCamera.CameraDisplayRGB24(m_iCam, pBmp24, ref sFrInfo);

            return 0;
        }
        private void Init()
        {
            if (bInitialized)
                return;
            m_iCameraIDs = new int[2];
            m_sCameraExpectedNameList = new string[] { firstCamera,
                                                       secondCamera };    
            Stop();
            m_sCameraRealNameList = GetCameraList().ToArray();
            int cameraCnt = GetCameraCnt();
            CheckCameraNames(m_sCameraRealNameList.ToList());
            for (int i = 0; i < cameraCnt; i++)
            {
                string sName = m_sCameraRealNameList[i];
                emDSCameraStatus status = dvpCamera.CameraInit(psub, sName, IntPtr.Zero, ref m_iCameraIDs[i]);
                if (status != emDSCameraStatus.STATUS_OK)
                    throw new Exception(string.Format("无法初始化相机，原因是: {0}", status.ToString()));
               dvpCamera.CameraSetOnceWB(m_iCameraIDs[i]);
               int resIndex = GetIndex(m_iCameraIDs[i]);
               dvpCamera.CameraSetImageSizeSel(m_iCameraIDs[i], resIndex, true);
               var result = dvpCamera.CameraPlay(m_iCameraIDs[i]);
            }
            bInitialized = true;
            Thread.Sleep(1000);
        }

        private CameraHandle GetCameraCnt()
        {
            return secondCamera == "" ? 1 : 2;
        }

        private void CheckCameraNames(List<string> sCameraNames)
        {
            
            CheckAllowed();
            bool expectedCamerasFound = sCameraNames.Exists(x => x.Contains(firstCamera))
                && sCameraNames.Exists(x => x.Contains(secondCamera));
            if(!expectedCamerasFound)
            {
                throw new Exception("未能找到指定的摄像头！");
            }
                
        }

        private void CheckAllowed()
        {
            bool firstReg = Utility.GlobalVals.allowedCameraNames.Contains(firstCamera);
            bool secondReg = Utility.GlobalVals.allowedCameraNames.Contains(secondCamera);
            if (secondCamera == "")
                secondReg = true;
            string notFoundCameraName = "";
            if (!firstReg)
                notFoundCameraName = firstCamera;
            if (!secondReg)
                notFoundCameraName = secondCamera;
            if (!firstReg || !secondReg)
                throw new Exception(string.Format("摄像头：{0}未注册！", notFoundCameraName));
        }

        private int GetIndex(int id)
        {
            int iNum = 0;
            tDSCameraDevInfo[] sDevList = new tDSCameraDevInfo[4];
            dvpCamera.CameraGetDevList(ref sDevList, ref iNum);

            tDSCameraCapability capabilites = new tDSCameraCapability();
            //获取相机参数范围
            if (dvpCamera.CameraGetCapability(id, ref capabilites) != emDSCameraStatus.STATUS_OK)
            {
                throw new Exception("无法获取相机参数！");
            }
            for (int i = 0; i < capabilites.iImageSizeDec; i++)
            {
                if (capabilites.pImageSizeDesc[i].acDescription.IndexOf(XResolution) != -1)
                    return i;
            }

            
            throw new Exception("相机不支持该分辨率!");
        }

        public void Stop()
        {
            if (m_iCameraIDs == null)
                return;

            try
            {
                int cameraCnt = GetCameraCnt();
                for (int i = 0; i < cameraCnt; i++)
                {
                   dvpCamera.CameraStop(m_iCameraIDs[i]);
                   dvpCamera.CameraUnInit(m_iCameraIDs[i]);
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }


        public void Start(string sFile, int cameraID, CameraSettings cameraSetting)
        {
            string errMsg = "";
            try
            {
                StartImpl(sFile, cameraID, cameraSetting);
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
            }
            
            if(onFinished != null)
                onFinished(this,new MyEventArgs(errMsg));
        }

        private void StartImpl(string sFile, int cameraID, CameraSettings cameraSetting)
        {
            Init();
            
            if (!IsRightOrder(m_sCameraRealNameList[cameraID - 1], m_sCameraExpectedNameList[cameraID - 1]))
            {
                cameraID = 3 - cameraID;
            }
            int cameraHandle = m_iCameraIDs[cameraID-1];
            if (File.Exists(sFile))
                File.Delete(sFile);
            string sOrgFile = sFile;
            int pos = sFile.IndexOf(".jpg");
            if (pos == -1)
                throw new Exception("Invalid file name：" + sFile);
            sFile = sFile.Substring(0, pos);
            if(cameraSetting.IsAE)
            {
                dvpCamera.CameraSetAeState(cameraHandle, true);
            }
            else
            {
                ulong curExposeTime = 0;
                ulong maxExposeTime = 0;
                ulong minExposeTime = 0;
                ulong exposeTime = (ulong)(1000 * cameraSetting.ExposeTime); //convert to ms
                dvpCamera.CameraGetExposureTime(cameraHandle, ref curExposeTime, ref maxExposeTime, ref minExposeTime);
                if (exposeTime > maxExposeTime)
                    throw new Exception(string.Format("曝光时间:{0} > 最大曝光时间:{1}", exposeTime, maxExposeTime));
                if (exposeTime < minExposeTime)
                    throw new Exception(string.Format("曝光时间:{0} < 最小曝光时间:{1}", exposeTime, minExposeTime));
                dvpCamera.CameraSetAeState(cameraHandle, false);
                dvpCamera.CameraSetAnalogGain(cameraHandle, (float)cameraSetting.Gain);
                dvpCamera.CameraSetExposureTime(cameraHandle, exposeTime);
            }
           
            emDSCameraStatus status = dvpCamera.CameraCaptureFile(cameraHandle, sFile, (byte)emDSFileType.FILE_JPG, 100);
            while (true)
            {
                Thread.Sleep(50);
                if (File.Exists(sOrgFile))
                {
                    //Image img = Image.FromFile(sOrgFile);
                    //var newImg = ImageProcessor.Resize(img, new Size(1280, 960));
                    //img.Dispose();
                    //newImg.Save(sOrgFile);
                    break;
                }
            }
        }

        private bool IsRightOrder(string realName, string expectedName)
        {
            return realName.IndexOf(expectedName) != -1;
        }

        private List<string> GetCameraList()
        {

            int iNum = 0;
            tDSCameraDevInfo[] sDevList = new tDSCameraDevInfo[4];
            int cameraCnt = GetCameraCnt();
            //获取设备列表
            if (dvpCamera.CameraGetDevList(ref sDevList, ref iNum) != emDSCameraStatus.STATUS_OK)
                throw new Exception("无法找到相机！");

            if (iNum <= 0)
            {
                throw new Exception("没有找到相机！");
            }

            if (iNum < cameraCnt)
            {
                throw new Exception("只找到一个相机!");
            }
            List<string> sCameraList = new List<string>();
            for (int i = 0; i < iNum;i++ )
            {
                sCameraList.Add(sDevList[i].acFriendlyName);
            }
            return sCameraList;
        }
    }
}
