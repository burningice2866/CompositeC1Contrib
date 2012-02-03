using System;
using System.IO;
using System.Linq;
using System.Net;

using Composite.Data;
using Composite.Data.Streams;
using Composite.Data.Types;

using Facebook;

namespace CompositeC1Contrib.DataProviders.FacebookMediaProvider
{
    public class FacebookFileStreamManager : IFileStreamManager
    {
        public Stream GetNewWriteStream(IFile file)
        {
            throw new NotImplementedException();
        }

        public Stream GetReadStream(IFile file)
        {
            using (var data = new DataConnection(PublicationScope.Published))
            {
                var fileId = file.DataSourceId.DataId as FacebookMediaFileId;
                var album = data.Get<IFacebookAlbum>().Single(a => a.Id == fileId.AlbumId);

                var client = new FacebookClient(album.AccessToken);
                dynamic photo = client.Get(fileId.Id);
                var url = photo.source;

                var request = WebRequest.Create(url);
                return request.GetResponse().GetResponseStream();
            }
        }

        public void SubscribeOnFileChanged(IFile file, OnFileChangedDelegate handler)
        {
            throw new NotImplementedException();
        }
    }
}
