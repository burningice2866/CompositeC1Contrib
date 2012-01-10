using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

using ICSharpCode.SharpZipLib.Zip;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.DownloadFoldersAsZip.Web
{
    public class GenerateZipHandler : IHttpHandler
    {
        static string tmpFolder = HostingEnvironment.MapPath("~/App_Data/Composite/Temp/DownloadFilesAsZip");

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext ctx)
        {
            Action<ZipOutputStream> compress = null;

            if (!Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }

            var mode = ctx.Request.QueryString["mode"];

            string folderName = String.Empty;
            var fileName = String.Empty;

            if (mode == "media")
            {
                var keyPath = ctx.Request.QueryString["keypath"];
                var archive = ctx.Request.QueryString["archive"];

                var files = Enumerable.Empty<IMediaFile>();

                using (var data = new DataConnection())
                {
                    if (!String.IsNullOrEmpty(keyPath))
                    {
                        var folder = data.Get<IMediaFileFolder>().Single(f => f.KeyPath == keyPath);
                        files = data.Get<IMediaFile>().Where(f => f.StoreId == folder.StoreId && f.FolderPath.StartsWith(folder.Path));

                        folderName = folder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                        fileName = Path.Combine(tmpFolder, folder.StoreId + "_" + folder.Id + ".zip");
                    }
                    else if (!String.IsNullOrEmpty(archive))
                    {
                        files = data.Get<IMediaFile>().Where(f => f.StoreId == archive);

                        folderName = archive;
                        fileName = Path.Combine(tmpFolder, "archive_" + archive + ".zip");
                    }
                }

                compress = (s) => compressMediaFiles(files, s);
            }
            else if (mode == "file")
            {
                var path = HttpUtility.UrlDecode(ctx.Request.QueryString["folder"]);

                folderName = Path.GetFileName(path);
                if (folderName == String.Empty)
                {
                    folderName = "root";
                }

                fileName = Path.Combine(tmpFolder, "files_" + folderName + ".zip");

                int folderOffset = ctx.Server.MapPath("~").Length;
                compress = (s) => compressFolder(path, s, folderOffset);
            }

            if (compress != null)
            {
                using (var zipFile = File.Create(fileName))
                {
                    using (var zipStream = new ZipOutputStream(zipFile))
                    {
                        zipStream.SetLevel(3);

                        compress(zipStream);

                        zipStream.IsStreamOwner = true;
                    }
                }

                using (var zipFile = File.OpenRead(fileName))
                {
                    ctx.Response.Clear();

                    ctx.Response.AddHeader("Content-Disposition", "filename=" + folderName + ".zip");
                    ctx.Response.AddHeader("Content-Length", zipFile.Length.ToString());
                    ctx.Response.AddHeader("Content-Type", "application/zip");

                    zipFile.CopyTo(ctx.Response.OutputStream);
                }

                File.Delete(fileName);
            }
        }

        private static void compressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string filename in files)
            {
                if (filename.StartsWith(tmpFolder))
                {
                    continue;
                }

                try
                {
                    using (var streamReader = File.OpenRead(filename))
                    {
                        var fi = new FileInfo(filename);

                        string entryName = filename.Substring(folderOffset);
                        entryName = ZipEntry.CleanName(entryName);

                        var newEntry = new ZipEntry(entryName)
                        {
                            DateTime = fi.LastWriteTime,
                            Size = fi.Length
                        };

                        zipStream.PutNextEntry(newEntry);

                        streamReader.CopyTo(zipStream, 4096);

                        zipStream.CloseEntry();
                    }
                }
                catch (IOException) { }                
            }

            var folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                compressFolder(folder, zipStream, folderOffset);
            }
        }

        private static void compressMediaFiles(IEnumerable<IMediaFile> files, ZipOutputStream zipStream)
        {
            foreach (var file in files)
            {
                string entryName = Path.Combine(file.FolderPath, file.FileName);
                entryName = ZipEntry.CleanName(entryName);

                var newEntry = new ZipEntry(entryName)
                {
                    DateTime = file.LastWriteTime.Value,
                    Size = file.Length.Value
                };

                zipStream.PutNextEntry(newEntry);

                using (var readStream = file.GetReadStream())
                {
                    readStream.CopyTo(zipStream, 4096);
                }

                zipStream.CloseEntry();
            }
        }
    }
}
