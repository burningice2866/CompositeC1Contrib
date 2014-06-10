using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using ICSharpCode.SharpZipLib.Zip;

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
            Action<ZipOutputStream> compress = null;

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

            using (var zipStream = new ZipOutputStream(ctx.Response.OutputStream))
            {
                var response = ctx.Response;

                response.Clear();
                response.BufferOutput = false;
                response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                response.AddHeader("Content-Type", "application/zip");

                zipStream.SetLevel(3);

                compress(zipStream);

                response.Flush();
            }
        }

        private static Action<ZipOutputStream> HandleCompressFile(HttpContext ctx, out string fileName)
        {
            var path = HttpUtility.UrlDecode(ctx.Request.QueryString["folder"]);

            var folderName = Path.GetFileName(path);
            if (folderName == String.Empty)
            {
                folderName = "root";
            }

            fileName = "files_" + folderName + ".zip";

            int folderOffset = ctx.Server.MapPath("~").Length;

            return s => CompressFolder(path, s, folderOffset);
        }

        private static Action<ZipOutputStream> HandleCompressMedia(HttpContext ctx, out string fileName)
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

        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            var files = Directory.GetFiles(path);
            foreach (string filename in files)
            {
                try
                {
                    using (var streamReader = File.OpenRead(filename))
                    {
                        var fi = new FileInfo(filename);

                        var entryName = filename.Substring(folderOffset);
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
                CompressFolder(folder, zipStream, folderOffset);
            }
        }

        private static void CompressMediaFiles(IEnumerable<IMediaFile> files, ZipOutputStream zipStream)
        {
            foreach (var file in files)
            {
                string entryName = Path.Combine(file.FolderPath, file.FileName);
                entryName = ZipEntry.CleanName(entryName);

                var newEntry = new ZipEntry(entryName)
                {
                    DateTime = file.LastWriteTime.Value,
                };

                Stream readStream = null;
                try
                {
                    readStream = file.GetReadStream();

                    if (file.Length == null)
                    {
                        try
                        {
                            newEntry.Size = readStream.Length;
                        }
                        catch
                        {
                            Stream ms = null;
                            try
                            {
                                ms = new MemoryStream();

                                using (readStream)
                                {
                                    readStream.CopyTo(ms, 4096);
                                    newEntry.Size = ms.Length;
                                }

                                ms.Seek(0, SeekOrigin.Begin);
                                readStream = ms;
                            }
                            catch
                            {
                                if (ms != null)
                                {
                                    ms.Dispose();
                                }

                                throw;
                            }
                        }
                    }
                    else
                    {
                        newEntry.Size = file.Length.Value;
                    }

                    zipStream.PutNextEntry(newEntry);
                    readStream.CopyTo(zipStream, 4096);
                }
                finally
                {
                    if (readStream != null)
                    {
                        readStream.Dispose();
                    }
                }

                zipStream.CloseEntry();
            }
        }
    }
}
