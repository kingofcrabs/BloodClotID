using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Configuration;
using System.Threading;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;
using MVSDK;
using System.Drawing;
using Utility;
using System.Threading.Tasks;
using System.Net;
using System.Linq;

namespace CameraControl
{
    public interface IImageAcquirer
    {
        void TakePhoto();
        void Stop();
    }


    public class ImageAcquirerFactory
    {
        public static IImageAcquirer CreateImageAcquirer(string vendorName)
        {
            if(vendorName == "Do3Think")
            {
                return new Do3ThinkImageAcquirer();
            }
            else if(vendorName == "OV")
            {
                return new FourCamera();
            }
            else
            {
                throw new Exception("不支持的相机！");
            }
        }
    }


        public class FourCamera : IImageAcquirer
        {
       
        
            public void TakePhoto()
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Factory.StartNew(DownloadFile1));
                tasks.Add(Task.Factory.StartNew(DownloadFile2));
                tasks.Add(Task.Factory.StartNew(DownloadFile3));
                tasks.Add(Task.Factory.StartNew(DownloadFile4));
                tasks.ForEach(x => x.Wait());
            }

            private void HttpDownloadFile(int cameraIndex)
            {
                bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
                if (bUseTestImage)
                {
                    Thread.Sleep(1000);
                    return;
                }
                    
                string url = string.Format("http://192.168.1.{0}:8000/camera1.jpg", cameraIndex + 105);
                string imgFolder = FolderHelper.CurrentAcquiredImageFolder;
                string path = imgFolder + string.Format("{0}.jpg", cameraIndex + 1);
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();

                //创建本地文件写入流
                Stream stream = new FileStream(path, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    stream.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
             
            
                stream.Close();
                responseStream.Close();

            }

            private void DownloadFile1()
            {
                HttpDownloadFile(0);
            }

            private void DownloadFile2()
            {
                HttpDownloadFile(1);
            }
            private void DownloadFile3()
            {
                HttpDownloadFile(2);
            }
            private void DownloadFile4()
            {
                HttpDownloadFile(3);
            }



            public void Stop()
            {
                
            }
        }



 

    public class Do3ThinkImageAcquirer: IImageAcquirer
    {
        public string m_sCameraName;
        public emDSRunMode m_Runmode;
        public int[] m_iCameraIDs;
        private string[] m_sCameraExpectedNameList;
        private string[] m_sCameraRealNameList;
        string firstCamera = ConfigurationManager.AppSettings["firstCameraID"];
        string secondCamera = ConfigurationManager.AppSettings["secondCameraID"];
        bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
        tDSCameraDevInfo[] pCameraInfo;
        bool bInitialized = false;
        Camera.DelegateProc psub = new Camera.DelegateProc(SnapThreadCallback);
        
      
        static int frames = 0;

        

        public static int SnapThreadCallback(int m_iCam, ref Byte pbyBuffer, ref tDSFrameInfo sFrInfo)
        {
            frames++;
            int pBmp24 = Camera.CameraISP(m_iCam, ref pbyBuffer, ref sFrInfo);
            Camera.CameraDisplayRGB24(m_iCam, pBmp24, ref sFrInfo);
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
            CheckCameraNames(m_sCameraRealNameList);
            for( int i = 0; i< 2; i++)
            {
                string sName = m_sCameraRealNameList[i];
                emDSCameraStatus status = Camera.CameraInit(psub, sName, IntPtr.Zero, ref m_iCameraIDs[i]);
                if (status != emDSCameraStatus.STATUS_OK)
                    throw new Exception(string.Format("无法初始化相机，原因是: {0}", status.ToString()));
                Camera.CameraSetOnceWB(m_iCameraIDs[i]);
                //Camera.CameraSetMirror(m_iCameraIDs[i], emDSMirrorDirection.MIRROR_DIRECTION_HORIZONTAL, true);
                int resIndex = GetIndex(m_iCameraIDs[i]);
                //Camera.CameraSetImageSize(m_iCameraID, true, 0, 0, 1280, 960,1);
                Camera.CameraSetImageSizeSel(m_iCameraIDs[i], resIndex, true);
                Camera.CameraPlay(m_iCameraIDs[i]);
            }
            bInitialized = true;
            Thread.Sleep(1000);
        }

        private void CheckCameraNames(string[] namesArray)
        {
            CheckAllowed();
            var cameraNames = namesArray.ToList();
            bool expectedCamerasFound = cameraNames.Exists(x => x.Contains(firstCamera))
                && cameraNames.Exists(x => x.Contains(secondCamera));
            if (!expectedCamerasFound)
            {
                throw new Exception("未能找到指定的摄像头！");
            }
        }

        private void CheckAllowed()
        {
            bool firstReg = RegistInfo.allowedCameraNames.Contains(firstCamera);
            bool secondReg = RegistInfo.allowedCameraNames.Contains(secondCamera);
            string notFoundCameraName = "";
            if(!firstReg)
                notFoundCameraName = firstCamera;
            if(!secondReg)
                notFoundCameraName = secondCamera;
            if(!firstReg || !secondReg)
                throw new Exception(string.Format("摄像头：{0}未注册！", notFoundCameraName));
            
        }

        private int GetIndex(int id)
        {

            tDSCameraCapability dsCapbility = new tDSCameraCapability();
            tDSImageSize[] pImagesize = new tDSImageSize[8];
            Camera.CameraGetCapability(id, ref dsCapbility);
            List<string> capabilites = new List<string>();
            for (int i = 0; i < 8; i++)
            {
                pImagesize[i].acDescription = new byte[64];
            }
            int pAddress = dsCapbility.pImageSizeDesc + 4;
            for (int j = 0; j < dsCapbility.iImageSizeDec; j++)
            {
                Camera.CopyMemory(Marshal.UnsafeAddrOfPinnedArrayElement(pImagesize[j].acDescription, 0), pAddress, 32);
                string sCapability = System.Text.Encoding.GetEncoding("GB2312").GetString(pImagesize[j].acDescription);
                if (sCapability.IndexOf("bin") != -1)
                    continue;
                capabilites.Add(sCapability);
                pAddress = pAddress + Marshal.SizeOf(pImagesize[j]);
            }

            for (int i = 0; i < capabilites.Count; i++)
            {
                if (capabilites[i].IndexOf("1920") != -1)
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
                for (int i = 0; i < 2; i++)
                {
                    Camera.CameraStop(m_iCameraIDs[i]);
                    Camera.CameraUnInit(m_iCameraIDs[i]);
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        
        public void  TakePhoto()
        {
            Init();
            string errMsg = "";
            try
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Factory.StartNew(TakePhoto1));
                tasks.Add(Task.Factory.StartNew(TakePhoto2));
                //tasks.Add(Task.Factory.StartNew(TakePhoto3));
                //tasks.Add(Task.Factory.StartNew(TakePhoto4));
                tasks.ForEach(x => x.Wait());
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
            }
        }

        private void TakePhoto1()
        {
            TakePhoto(FolderHelper.CurrentAcquiredImageFolder + "1.jpg", 1);
        }

        private void TakePhoto2()
        {
            TakePhoto(FolderHelper.CurrentAcquiredImageFolder + "2.jpg", 2);
        }
        private void TakePhoto3()
        {
            TakePhoto(FolderHelper.CurrentAcquiredImageFolder + "3.jpg", 3);
        }
        private void TakePhoto4()
        {
            TakePhoto(FolderHelper.CurrentAcquiredImageFolder + "4.jpg", 4);
        }
         

        private void TakePhoto(string sFile, int cameraID)
        {
            if (bUseTestImage)
            {
                Thread.Sleep(1000);
                return;
            }

            cameraID = ConvertID(cameraID);
            if (File.Exists(sFile))
                File.Delete(sFile);
            string sOrgFile = sFile;
            int pos = sFile.IndexOf(".jpg");
            if (pos == -1)
                throw new Exception("Invalid file name：" + sFile);
            sFile = sFile.Substring(0, pos);
            emDSCameraStatus status = Camera.CameraCaptureFile(cameraID, sFile, (byte)emDSFileType.FILE_JPG, 100);
            while (true)
            {
                Thread.Sleep(50);
                if (File.Exists(sOrgFile))
                    break;
            }
        }

        private int ConvertID(int id)
        {
            string expectedName = m_sCameraExpectedNameList[id - 1];
            int convertedID = 0;
            for (int cameraID = 1; cameraID <= 4; cameraID++)
            {
                if(IsRightOrder(m_sCameraRealNameList[cameraID - 1],expectedName))
                {
                    convertedID = cameraID;
                    break;
                }
            }
            if (convertedID == -1)
                throw new Exception(string.Format("找不到相机:{0}", expectedName));
            return convertedID;
        }

        private bool IsRightOrder(string realName, string expectedName)
        {
            return realName.IndexOf(expectedName) != -1;
        }

        private List<string> GetCameraList()
        {
            pCameraInfo = new tDSCameraDevInfo[5]; //发送缓冲区大小可根据需要设置；
            for (int yy = 0; yy < 5; yy++)
            {
                pCameraInfo[yy] = new tDSCameraDevInfo();
                pCameraInfo[yy].acVendorName = new Byte[64];
                pCameraInfo[yy].acProductSeries = new Byte[64];
                pCameraInfo[yy].acProductName = new char[64];
                pCameraInfo[yy].acFriendlyName = new char[64];
                pCameraInfo[yy].acDevFileName = new Byte[64];
                pCameraInfo[yy].acFileName = new Byte[64];
                pCameraInfo[yy].acFirmwareVersion = new Byte[64];
                pCameraInfo[yy].acSensorType = new Byte[64];
                pCameraInfo[yy].acPortType = new Byte[64];
            }
            IntPtr[] ptArray = new IntPtr[1];
            ptArray[0] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(tDSCameraDevInfo)) * 5);
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(tDSCameraDevInfo))*5);
            Marshal.Copy(ptArray, 0, pt, 1);
            
            int iNum = 0;
            Camera.CameraGetDevList(pt, ref iNum);
            if (iNum <= 0)
            {
                throw new Exception("没有找到相机！");
            }

            if (iNum < 2)
            {
                throw new Exception("只找到一个相机!");
            }
            List<string> sCameraList = new List<string>();
            for( int i = 0; i< 2; i++)
            {
                string sCameraName = "";
                pCameraInfo[i] = (tDSCameraDevInfo)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(tDSCameraDevInfo))), typeof(tDSCameraDevInfo));
                for (int j = 0; pCameraInfo[i].acFriendlyName[j] != '\0'; j++)
                {
                    sCameraName = sCameraName + pCameraInfo[i].acFriendlyName[j];
                }
                sCameraList.Add(sCameraName);
            }

            return sCameraList;
        }

        
    }
}
