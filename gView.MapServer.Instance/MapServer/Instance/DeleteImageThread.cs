using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace gView.MapServer.Instance
{
    class DeleteImageThread
    {
        private string _imagePath = "";
        public DeleteImageThread(string path)
        {
            _imagePath = path;
        }

        public void Run()
        {
            while (true)
            {
                Thread.Sleep(10000);
                Delete("*.jpg");
                Delete("*.png");
                Delete("*.gif");
                Delete("*.tif");
                Delete("*.tiff");
                Delete("*.bmp");
            }
        }

        private void Delete(string filter)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(_imagePath);

                foreach (FileInfo fi in di.GetFiles(filter))
                {
                    TimeSpan ts = DateTime.Now - fi.CreationTime;
                    if (ts.TotalMinutes >= 5)
                    {
                        try
                        {
                            fi.Delete();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
