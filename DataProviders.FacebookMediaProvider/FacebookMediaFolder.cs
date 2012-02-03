using System;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    public class FacebookMediaFolder : IMediaFileFolder
    {
        private Guid _id;
        public Guid Id
        {
            get { return _id; }
        }

        private DataSourceId _dataSourceId;
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
            set { throw new NotImplementedException(); }
        }

        public string Description { get; set; }
        public string Path { get; set; }
        public string StoreId { get; set; }
        public string Title { get; set; }

        public FacebookMediaFolder(IFacebookAlbum album, string storeId, DataSourceId dataSourceId)
        {
            _id = album.Id;

            Path = "/" + album.Title;
            Title = album.Title;
            Description = album.Title;

            StoreId = storeId;

            _dataSourceId = dataSourceId;
        }
    }
}
