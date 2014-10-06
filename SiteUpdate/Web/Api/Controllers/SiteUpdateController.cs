using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using CompositeC1Contrib.SiteUpdate.Configuration;

namespace CompositeC1Contrib.SiteUpdate.Web.Api.Controllers
{
    public class SiteUpdateController : ApiController
    {
        private static readonly SiteUpdateConfigurationSection Config = SiteUpdateConfigurationSection.GetSection();

        [Route("api/siteupdate/{installationId}")]
        public IHttpActionResult Get(Guid installationId)
        {
            var location = Config.StoreLocation;
            var store = new SiteUpdateStore(location);

            return Ok(store.GetUpdateSummaries(installationId));
        }

        [Route("api/siteupdate/{installationId}/{updateId}/stream")]
        public HttpResponseMessage GetStream(Guid installationId, Guid updateId)
        {
            var location = Config.StoreLocation;
            var store = new SiteUpdateStore(location);
            var updates = store.GetUpdateSummaries(installationId);
            var update = updates.Single(u => u.Id == updateId);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(store.GetZipStream(update))
            };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = update.FileName
            };

            return result;
        }
    }
}
