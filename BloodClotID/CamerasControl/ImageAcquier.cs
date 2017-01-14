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
        void Init();
        void Stop();
        void Start(int cameraID);
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
        public static IImageAcquirer CreateImageAcquirer()
        {
            return new Do3ThinkImageAcquirer();
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
        string thirdCamera = ConfigurationManager.AppSettings["thirdCamera"];
        string fourthCamera = ConfigurationManager.AppSettings["fourthCamera"];
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
        public void Init()
        {
            if (bInitialized)
                return;
            m_iCameraIDs = new int[4];
            m_sCameraExpectedNameList = new string[] { firstCamera,
                                                       secondCamera,
                                                        thirdCamera,
                                                        fourthCamera};    
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
            return 4;
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
            bool firstReg = RegistInfo.allowedCameraNames.Contains(firstCamera);
            bool secondReg = RegistInfo.allowedCameraNames.Contains(secondCamera);
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
            return 0;
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


        public void Start( int cameraID)
        {
            CameraSettings cameraSetting = GlobalVars.CameraSettings;
            string sFile = FolderHelper.GetImagePath(cameraID);
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
                    break;
                }
            }
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
