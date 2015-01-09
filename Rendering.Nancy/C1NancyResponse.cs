using Nancy;

namespace CompositeC1Contrib.Rendering.Nancy
{
    public class C1NancyResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }
}
