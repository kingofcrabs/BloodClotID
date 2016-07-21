using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BloodClotID
{
    public class FourCamera
    {
        public void TakePhote()
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
            string url = string.Format("http://192.168.1.{0}:8000/camera1.jpg", cameraIndex + 105);
            string imgFolder = FolderHelper.GetImageFolder();
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
    }
}
