using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Xml.Linq;

using Composite.C1Console.Security;
using Composite.Core;
using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.UI
{
    public class CompositeC1Page : Page
    {
        private IDisposable _dataScope;

        private string _cacheUrl = null;
        private bool _requestCompleted = false;
        private bool _isPreview = false;

        public IPage Document
        {
            get { return PageRenderer.CurrentPage; }
            private set
            {
                if (PageRenderer.CurrentPage == null)
                {
                    PageRenderer.CurrentPage = value;
                }
            }
        }

        public SiteMapNode SiteMapNode
        {
            get { return SiteMap.Provider.FindSiteMapNodeFromKey(Document.Id.ToString()); }
        }

        protected override void OnPreInit(EventArgs e)
        {
            if (Context.Items.Contains("SelectedPage") && Context.Items.Contains("NamedXhtmlFragments"))
            {
                _isPreview = true;

                Document = (IPage)Context.Items["SelectedPage"];                
            }
            else
            {
                var url = RequestInfo.Current.PageUrl;
                if (url != null)
                {
                    if (url.PublicationScope != PublicationScope.Published)
                    {
                        if (!UserValidationFacade.IsLoggedIn())
                        {
                            Response.Redirect(String.Format("/Composite/Login.aspx?ReturnUrl={0}", HttpUtility.UrlEncodeUnicode(Request.Url.OriginalString)), true);
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }

                    _dataScope = new DataScope(DataScopeIdentifier.FromPublicationScope(url.PublicationScope), url.Locale); // IDisposable, Disposed in OnUnload

                    Document = PageManager.GetPageById(url.PageId);
                }
            }

            if (Document == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Page not found");
            }

            InitializeCulture();

            IPageTemplate template = null;
            using (var conn = new DataConnection())
            {
                template = conn.Get<IPageTemplate>().Single(t => t.Id == Document.TemplateId);
            }

            string masterFile = String.Format("~/Renderers/Masters/{0}.master", template.Title);
            if (HostingEnvironment.VirtualPathProvider.FileExists(masterFile))
            {
                AppRelativeVirtualPath = "~/";
                MasterPageFile = masterFile;
            }

            if (!_isPreview)
            {
                _cacheUrl = Request.Url.PathAndQuery;

                var cachePolicy = Context.Response.Cache;
                cachePolicy.SetVaryByCustom("C1Page_ChangeDate");
                cachePolicy.SetExpires(DateTime.Now.AddSeconds(60));
                cachePolicy.VaryByParams["*"] = true;

                RewritePath();
            }

            base.OnPreInit(e);
        }

        protected override void InitializeCulture()
        {
            var doc = Document;
            if (doc != null)
            {
                this.Culture = this.UICulture = doc.CultureName;
            }

            base.InitializeCulture();
        }

        protected override void OnInit(EventArgs e)
        {
            IList<IPagePlaceholderContent> contents;

            if (!_isPreview)
            {
                if (RequestInfo.Current.PageUrl.PublicationScope != PublicationScope.Published)
                {
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                }

                var responseHandling = RenderingResponseHandlerFacade.GetDataResponseHandling(Document.GetDataEntityToken());

                if ((responseHandling != null) && (responseHandling.PreventPublicCaching == true))
                {
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                }

                if ((responseHandling != null) && (responseHandling.EndRequest || responseHandling.RedirectRequesterTo != null))
                {
                    if (responseHandling.RedirectRequesterTo != null)
                    {
                        Response.Redirect(responseHandling.RedirectRequesterTo.AbsoluteUri, false);
                    }

                    Context.ApplicationInstance.CompleteRequest();
                    _requestCompleted = true;

                    return;
                }

                contents = PageManager.GetPlaceholderContent(Document.Id);
            }
            else
            {
                contents = new List<IPagePlaceholderContent>();
                var namedXhtmlFragments = (Dictionary<string, string>)Context.Items["NamedXhtmlFragments"];

                foreach (var placeHolderContent in namedXhtmlFragments)
                {
                    var content = DataFacade.BuildNew<IPagePlaceholderContent>();
                    content.PageId = Document.Id;
                    content.PlaceHolderId = placeHolderContent.Key;
                    content.Content = placeHolderContent.Value;
                    contents.Add(content);
                }
            }

            if (Master != null)
            {
                var mi = typeof(PageRenderer).GetMethod("NormalizeXhtmlDocument", BindingFlags.Static | BindingFlags.NonPublic);

                foreach (var content in contents)
                {
                    var doc = XElement.Parse(content.Content);
                    var context = PageRenderer.GetPageRenderFunctionContextContainer();

                    PageRenderer.ExecuteEmbeddedFunctions(doc, context);
                    mi.Invoke(null, new[] { new XhtmlDocument(doc) });

                    var plc = FindControlRecursive(Master, content.PlaceHolderId);
                    if (plc != null)
                    {
                        var body = doc.Descendants().Single(el => el.Name.LocalName == "body");
                        var c = body.AsAspNetControl((IXElementToControlMapper)context.XEmbedableMapper);

                        plc.Controls.Add(c);
                    }

                    var head = doc.Descendants().SingleOrDefault(el => el.Name.LocalName == "head");
                    if (Header != null && head != null)
                    {
                        Header.Controls.Add(new LiteralControl(String.Concat(head.Elements())));
                    }
                }
            }
            else
            {
                var renderedPage = PageRenderer.Render(Document, contents);
                
                if (_isPreview)
                {
                    PageRenderer.DisableAspNetPostback(renderedPage);
                }

                Controls.Add(renderedPage);
            }

            if (Form != null)
            {
                Form.Action = Request.RawUrl;
            }

            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (_isPreview)
            {
                base.Render(writer);
            }
            else
            {

                if (_requestCompleted)
                {
                    return;
                }

                var scriptManager = ScriptManager.GetCurrent(this);
                bool isUpdatePanelPostback = scriptManager != null && scriptManager.IsInAsyncPostBack;

                if (isUpdatePanelPostback == true)
                {
                    base.Render(writer);
                    return;
                }

                var markupBuilder = new StringBuilder();
                var sw = new StringWriter(markupBuilder);
                try
                {
                    base.Render(new HtmlTextWriter(sw));
                }
                catch (HttpException ex)
                {
                    var setStringMethod = typeof(HttpContext).Assembly /* System.Web */
                        .GetType("System.Web.SR")
                        .GetMethod("GetString", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);

                    string multipleFormNotAllowedMessage = (string)setStringMethod.Invoke(null, new object[] { "Multiple_forms_not_allowed" });

                    bool multipleAspFormTagsExists = ex.Message == multipleFormNotAllowedMessage;
                    if (multipleAspFormTagsExists)
                    {
                        throw new HttpException("Multiple <asp:form /> elements exists on this page. ASP.NET only support one form. To fix this, insert a <asp:form> ... </asp:form> section in your template that spans all controls.");
                    }

                    throw;
                }

                string xhtml = PageUrlHelper.ChangeRenderingPageUrlsToPublic(markupBuilder.ToString());

                try
                {
                    xhtml = XhtmlPrettifier.Prettify(xhtml);
                }
                catch
                {
                    Log.LogWarning("/Renderers/Page.aspx", "Failed to format output xhtml. Url: " + (_cacheUrl ?? String.Empty));
                    throw;
                }

                writer.Write(xhtml);
            }
        }

        protected override void OnUnload(EventArgs e)
        {
            if (_dataScope != null)
            {
                _dataScope.Dispose();
            }

            if (_requestCompleted)
            {
                return;
            }

            if (_cacheUrl != null)
            {
                Context.RewritePath(_cacheUrl.Replace("%20", " "));
            }

            base.OnUnload(e);
        }

        private void RewritePath()
        {
            var structuredUrl = RequestInfo.Current.PageUrl.Build(PageUrlType.Public);
            if (structuredUrl == null)
            {
                return;
            }

            structuredUrl.AddQueryParameters(RequestInfo.Current.ForeignQueryStringParameters);

            string pathInfo = new UrlBuilder(_cacheUrl).PathInfo;
            Context.RewritePath(structuredUrl.FilePath, pathInfo, structuredUrl.QueryString);
        }

        public Control FindControlRecursive(Control current, string controlID)
        {
            if (current == null) throw new ArgumentNullException("current");
            if (controlID == null) throw new ArgumentNullException("controlID");

            if (current.ID == controlID)
            {
                return current;
            }

            foreach (Control c in current.Controls)
            {
                var t = FindControlRecursive(c, controlID);
                if (t != null) return t;
            }

            return null;
        }
    }
}
