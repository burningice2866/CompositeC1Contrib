using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using ICSharpCode.SharpZipLib.Zip;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.MediaArchiveDownloader.Web
{
    public class DownloadFolderHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext ctx)
        {
            var tmpFolder = ctx.Server.MapPath("~/App_Data/Composite/Temp/MediaFolderDownload");

            var keyPath = ctx.Request.QueryString["keypath"];
            var archive = ctx.Request.QueryString["archive"];

            string folderName = String.Empty;
            var fileName = String.Empty;

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
                    fileName = Path.Combine(tmpFolder, "archive_"+ archive + ".zip");                            
                }
            }

            using (var zipFile = File.Create(fileName))
            {
                using (var zipStream = new ZipOutputStream(zipFile))
                {
                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                    compressFiles(files, zipStream, 0);

                    zipStream.IsStreamOwner = true;	// Makes the Close also Close the underlying stream
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

        private void compressFiles(IEnumerable<IMediaFile> files, ZipOutputStream zipStream, int folderOffset)
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
