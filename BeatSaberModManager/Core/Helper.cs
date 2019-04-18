﻿using BeatSaberModManager.Dependencies;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Reflection;
using BeatSaberModManager.Dependencies.SimpleJSON;

namespace BeatSaberModManager.Core
{
    public static class Helper
    {

        private static void CreateHttpErrorLog(WebException ex)
        {
            string response = ex.Response != null ? new StreamReader(ex.Response.GetResponseStream()).ReadToEnd().ToString() : "none";

            string[] text =
            {
                "################################################################",
                "This is an automatic error log",
                $"generated by BeatSaberModManager {Assembly.GetEntryAssembly().GetName().Version}",
                $"on {DateTime.Now:s}",
                "################################################################",
                "Message :",
                ex.Message,
                "----------------------------------------------------------------",
                "Stack trace :",
                ex.StackTrace,
                "----------------------------------------------------------------",
                "Status :",
                ex.Status.ToString(),
                "----------------------------------------------------------------",
                "Complete error :",
                ex.ToString(),
                "----------------------------------------------------------------",
                "Complete response :",
                response,
                "----------------------------------------------------------------"
            };
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"BeatSaberModManager_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
            
            System.IO.File.WriteAllLines(filePath, text);
        }
        public static string Get(string URL) {

            try
            {
                HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(URL);
                request.Method = "GET";
                request.KeepAlive = true;
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = $"BeatSaberModManager/{Assembly.GetEntryAssembly().GetName().Version}";
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                using (StreamReader requestReader = new StreamReader(response.GetResponseStream()))
                {
                    return requestReader.ReadToEnd();
                }
            }
            catch (WebException ex) {
                MessageBox.Show($"Error trying to access: {URL}\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
#if DEBUG
                CreateHttpErrorLog(ex);
#endif
                Environment.Exit(0);
                return null;
            }
        }
        public static void UnzipFile(byte[] data, string directory)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (var zf = new ZipFile(ms))
                {
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                            continue;

                        string entryFileName = zipEntry.Name;

                        byte[] buffer = new byte[4096];
                        Stream zipStream = zf.GetInputStream(zipEntry);

                        string fullZipToPath = Path.Combine(directory, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);

                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
            }
        }
        public static byte[] GetFile(string url)
        {
            WebClient client = new WebClient();
            client.Proxy = null;
            return client.DownloadData(url);
        }
    }
}
