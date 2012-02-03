using System;
using System.Collections.Generic;
using System.Linq;

using Composite.Data;
using Composite.Data.Plugins.DataProvider;
using Composite.Data.Types;

using Facebook;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    public class FacebookMediaProvider : IDataProvider
    {
        private static readonly object _lock = new object();

        private IQueryable<IMediaFile> _cachedFacebookPhotos;

        private DataProviderContext _context;
        public DataProviderContext Context
        {
            set { this._context = value; }
        }

        private IMediaFileStore _store;
        public IMediaFileStore Store
        {
            get
            {
                if (_store == null)
                {
                    var dataSourceId = _context.CreateDataSourceId(new FacebookMediaStoreId(), typeof(FacebookMediaStore));
                    _store = new FacebookMediaStore(dataSourceId);
                }

                return _store;
            }
        }

        public FacebookMediaProvider()
        {
            DataEvents<IFacebookAlbum>.OnAfterAdd += clearPhotosCache;
            DataEvents<IFacebookAlbum>.OnAfterUpdate += clearPhotosCache;
            DataEvents<IFacebookAlbum>.OnDeleted += clearPhotosCache;
        }

        private void clearPhotosCache(object sender, DataEventArgs dataEventArgs)
        {
            lock (_lock)
            {
                _cachedFacebookPhotos = null;
            }
        }

        public T GetData<T>(IDataId dataId) where T : class, IData
        {
            var folderId = dataId as FacebookMediaFolderId;
            if (folderId != null)
            {
                using (var data = new DataConnection(PublicationScope.Published))
                {
                    var album = data.Get<IFacebookAlbum>().Single(a => a.Id == folderId.Id);

                    var id = new FacebookMediaFolderId()
                    {
                        Id = album.Id
                    };

                    var dataSourceId = _context.CreateDataSourceId(id, typeof(IMediaFileFolder));

                    return (new FacebookMediaFolder(album, Store.Id, dataSourceId)) as T;
                }
            }

            var fileId = dataId as FacebookMediaFileId;
            if (fileId != null)
            {
                using (var data = new DataConnection(PublicationScope.Published))
                {
                    var photo = data.Get<IFacebookPhoto>().Single(p => p.Id == fileId.Id);
                    var album = data.Get<IFacebookAlbum>().Single(a => a.Id == fileId.AlbumId);
                    var dataSourceId = _context.CreateDataSourceId(new FacebookMediaFileId() { Id = photo.Id, AlbumId = album.Id }, typeof(IMediaFile));

                    return (new FacebookMediaFile(photo, album, Store.Id, dataSourceId)) as T;
                }
            }

            var photoId = dataId as FacebookPhotoId;
            if (photoId != null)
            {
                using (var data = new DataConnection(PublicationScope.Published))
                {
                    var album = data.Get<IFacebookAlbum>().SingleOrDefault(a => a.AlbumId == photoId.AlbumId);
                    if (album != null)
                    {
                        var token = album.AccessToken;
                        var client = new FacebookClient(token);

                        dynamic photo = client.Get(photoId.Id);

                        var id = new FacebookPhotoId()
                        {
                            Id = photo.Id,
                            AlbumId = album.AlbumId
                        };

                        var facebookPhoto = new FacebookPhoto(_context.CreateDataSourceId(id, typeof(IFacebookPhoto)))
                        {
                            Id = photo.id,
                            AlbumId = album.AlbumId,
                            Title = photo.name ?? String.Empty
                        };

                        return facebookPhoto as T;
                    }
                }
            }

            return default(T);
        }

        public IQueryable<T> GetData<T>() where T : class, IData
        {
            if (typeof(T) == typeof(IMediaFileFolder))
            {
                using (var data = new DataConnection(PublicationScope.Published))
                {
                    var list = new List<IMediaFileFolder>();

                    var albums = data.Get<IFacebookAlbum>();
                    foreach (var album in albums)
                    {
                        var id = new FacebookMediaFolderId()
                        {
                            Id = album.Id
                        };

                        var dataSourceId = _context.CreateDataSourceId(id, typeof(IMediaFileFolder));
                        var folder = new FacebookMediaFolder(album, Store.Id, dataSourceId);

                        list.Add(folder);
                    }

                    return (IQueryable<T>)list.AsQueryable();
                }
            }
            else if (typeof(T) == typeof(IMediaFile))
            {
                if (_cachedFacebookPhotos == null)
                {
                    lock (_lock)
                    {
                        if (_cachedFacebookPhotos == null)
                        {
                            var list = new List<IMediaFile>();

                            using (var data = new DataConnection(PublicationScope.Published))
                            {
                                var photos = data.Get<IFacebookPhoto>();
                                foreach (var photo in photos)
                                {
                                    var album = data.Get<IFacebookAlbum>().Single(a => a.AlbumId == photo.AlbumId);
                                    var dataSourceId = _context.CreateDataSourceId(new FacebookMediaFileId() { Id = photo.Id, AlbumId = album.Id }, typeof(IMediaFile));

                                    var file = new FacebookMediaFile(photo, album, Store.Id, dataSourceId);

                                    list.Add(file);
                                }
                            }

                            _cachedFacebookPhotos = list.AsQueryable();
                        }
                    }
                }

                return (IQueryable<T>)_cachedFacebookPhotos;
            }
            else if (typeof(T) == typeof(IFacebookPhoto))
            {
                var list = new List<IFacebookPhoto>();

                using (var data = new DataConnection(PublicationScope.Published))
                {
                    var albums = data.Get<IFacebookAlbum>();
                    foreach (var album in albums)
                    {
                        var token = album.AccessToken;
                        var client = new FacebookClient(token);

                        dynamic result = client.Get(album.AlbumId + "/photos");
                        foreach (var photo in result.data)
                        {
                            var id = new FacebookPhotoId()
                            {
                                Id = photo.Id,
                                AlbumId = album.AlbumId
                            };

                            var dataSourceId = _context.CreateDataSourceId(id, typeof(FacebookPhoto));
                            var facebookPhoto = new FacebookPhoto(dataSourceId)
                            {
                                Id = photo.id,
                                AlbumId = album.AlbumId,
                                CreatedTime = DateTime.Parse(photo.created_time),
                                UpdatedTime = DateTime.Parse(photo.updated_time),
                                Title = photo.name ?? photo.id
                            };

                            list.Add(facebookPhoto);
                        }

                    }

                    return (IQueryable<T>)list.AsQueryable();
                }
            }
            else if (typeof(T) == typeof(IMediaFileStore))
            {
                var store = new[] { Store }.AsQueryable();

                return (IQueryable<T>)store;
            }

            return Enumerable.Empty<T>().AsQueryable();
        }

        public IEnumerable<Type> GetSupportedInterfaces()
        {
            return new[] { typeof(IFacebookPhoto), typeof(IMediaFileStore), typeof(IMediaFile), typeof(IMediaFileFolder) };
        }
    }
}
