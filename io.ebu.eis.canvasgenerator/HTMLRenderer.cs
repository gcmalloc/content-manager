﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace io.ebu.eis.canvasgenerator
{
    public static class HTMLRenderer
    {
        private const string PathToExe = "phantomjs.exe";
        //private string _pathToWorkingDir = "templates/";
        private const string PhantomPageGenerator = "sliderenderer.js";
        private const string PhantomArgumentProperties = "320px*240px"; //"640px*480px 2.0";//
        private const int TimeToExit = 500;

        public static BitmapImage Render(string file, string pathToWorkingDir)
        {
            try
            {
                var args = String.Format("{0} {1} {2}", PhantomPageGenerator, file, PhantomArgumentProperties);
                var startInfo = new ProcessStartInfo
                {
                    FileName = PathToExe,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    WorkingDirectory = pathToWorkingDir // PDF Tool Path
                };
                var p = new Process { StartInfo = startInfo };
                p.Start();
                p.WaitForExit(TimeToExit);
                // Read the Error:
                // var error = p.StandardError.ReadToEnd().Trim();
                // TODO If error not null or empty String the report error
                // Read the Output:
                var base64Image = p.StandardOutput.ReadToEnd().Trim();
                var bytes = Convert.FromBase64CharArray(base64Image.ToCharArray(), 0, base64Image.Length);
                if (bytes.Length > 0)
                {
                    var image = GetBitmapImage(bytes);
                    return image;
                }
            }
            catch (Exception)
            {
                // TODO Log
            }
            return null;
        }

        public static BitmapImage RenderHtml(string html, string pathToWorkingDir)
        {
            // TODO Pass as argument and decode
            var tempFilename = Guid.NewGuid() + ".html";
            var tempfile = Path.Combine(pathToWorkingDir, tempFilename);
            var file = File.Create(tempfile);
            var content = Encoding.UTF8.GetBytes(html);
            file.Write(content, 0, content.Length);
            file.Flush();
            file.Close();

            // TODO This is ugly but works : )
            var image = Render(Path.GetFileName(tempfile), pathToWorkingDir);

            // TODO Remove
            // Delete Temp file
            try
            {
                File.Delete(tempfile);
            }
            catch (Exception)
            {
                // TODO Log
            }

            return image;
        }

        public static BitmapImage GetBitmapImage(byte[] imageBytes)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageBytes);
            bitmapImage.EndInit();
            return bitmapImage;
        }


    }
}
