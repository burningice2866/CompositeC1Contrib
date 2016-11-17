using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

using Composite.Core.IO;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.DownloadFoldersAsZip.Web
{
    public class GenerateZipHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext ctx)
        {
            Action<ZipArchive> compress = null;

            var fileName = String.Empty;
            var mode = ctx.Request.QueryString["mode"];

            switch (mode)
            {
                case "media": compress = HandleCompressMedia(ctx, out fileName); break;
                case "file": compress = HandleCompressFile(ctx, out fileName); break;
            }

            if (compress == null)
            {
                return;
            }

            using (var ms = new MemoryStream())
            {
                var response = ctx.Response;

                response.Clear();
                response.BufferOutput = false;
                response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                response.AddHeader("Content-Type", "application/zip");

                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    compress(zipArchive);
                }

                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(response.OutputStream);

                response.Flush();
            }
        }

        private static Action<ZipArchive> HandleCompressFile(HttpContext ctx, out string fileName)
        {
            var relativePath = HttpUtility.UrlDecode(ctx.Request.QueryString["folder"]);
            var absolutePath = ctx.Server.MapPath("~" + relativePath);

            var folderName = Path.GetFileName(relativePath);
            if (folderName == String.Empty)
            {
                folderName = "root";
            }

            fileName = "files_" + folderName + ".zip";

            var folderOffset = (absolutePath.Length - relativePath.Length) + 1;

            return s => CompressFolder(absolutePath, s, folderOffset);
        }

        private static Action<ZipArchive> HandleCompressMedia(HttpContext ctx, out string fileName)
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

                    fileName = folder.StoreId + "_" + folder.Id + ".zip";
                }
                else if (!String.IsNullOrEmpty(archive))
                {
                    files = data.Get<IMediaFile>().Where(f => f.StoreId == archive);

                    fileName = "archive_" + archive + ".zip";
                }
                else
                {
                    fileName = String.Empty;
                }
            }

            return s => CompressMediaFiles(files, s);
        }

        private static void CompressFolder(string absolutePath, ZipArchive zipArchive, int folderOffset)
        {
            var files = C1Directory.GetFiles(absolutePath);
            foreach (string filename in files)
            {
                try
                {
                    using (var streamReader = File.OpenRead(filename))
                    {
                        var fi = new FileInfo(filename);

                        var entryName = filename.Remove(0, folderOffset);
                        var zipEntry = zipArchive.CreateEntry(entryName, CompressionLevel.Fastest);

                        zipEntry.LastWriteTime = fi.LastWriteTime;

                        using (var stream = zipEntry.Open())
                        {
                            streamReader.CopyTo(stream, 4096);
                        }
                    }
                }
                catch (IOException) { }
            }

            var folders = C1Directory.GetDirectories(absolutePath);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipArchive, folderOffset);
            }
        }

        private static void CompressMediaFiles(IEnumerable<IMediaFile> files, ZipArchive zipArchive)
        {
            foreach (var file in files)
            {
                var entryName = Path.Combine(file.FolderPath, file.FileName).Remove(0, 1);
                var zipEntry = zipArchive.CreateEntry(entryName, CompressionLevel.Fastest);

                zipEntry.LastWriteTime = file.LastWriteTime.Value;

                try
                {
                    using (var readStream = file.GetReadStream())
                    {
                        using (var stream = zipEntry.Open())
                        {
                            readStream.CopyTo(stream, 4096);
                        }
                    }
                }
                catch (FileNotFoundException) { }
            }
        }
    }
}
