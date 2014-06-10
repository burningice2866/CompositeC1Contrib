using System;
using System.Security.Cryptography;
using System.Text;

using Composite.Data;
using Composite.Data.Streams;
using Composite.Data.Types;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    [FileStreamManager(typeof(FacebookFileStreamManager))]
    public class FacebookMediaFile : IMediaFile
    {
        private readonly Guid _id;
        public Guid Id
        {
            get { return _id; }
        }

        private readonly DataSourceId _dataSourceId;
        public DataSourceId DataSourceId
        {
            get { return _dataSourceId; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public string KeyPath
        {
            get { return StoreId + ":" + Id; }
        }

        public string CompositePath
        {
            get { return KeyPath; }
            set { throw new NotSupportedException(); }
        }

        private readonly DateTime _creationTime;
        public DateTime? CreationTime
        {
            get { return _creationTime; }
        }

        private readonly DateTime _lastWriteTime;
        public DateTime? LastWriteTime
        {
            get { return _lastWriteTime; }
        }

        public int? Length
        {
            get { return null; }
        }

        public string MimeType
        {
            get { return "image/jpeg"; }
        }

        public string StoreId { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string FolderPath { get; set; }
        public string Culture { get; set; }
        public string Description { get; set; }

        public FacebookMediaFile(IFacebookPhoto photo, IFacebookAlbum album, string storeId, DataSourceId dataSourceId)
        {
            _id = MakeGuidFromString(photo.Id);
            _creationTime = photo.CreatedTime;
            _lastWriteTime = photo.UpdatedTime;

            FolderPath = "/" + album.Title;
            
            Culture = "en-US";
            Title = photo.Title;
            Description = photo.Title;
            FileName = photo.Title + ".jpg";

            StoreId = storeId;

            _dataSourceId = dataSourceId;
        }

        private static Guid MakeGuidFromString(string input)
        {
            var provider = new MD5CryptoServiceProvider();
            var inputBytes = Encoding.Default.GetBytes(input);

            var hashBytes = provider.ComputeHash(inputBytes);
            var hashGuid = new Guid(hashBytes);

            return hashGuid;
        }
    }
}
