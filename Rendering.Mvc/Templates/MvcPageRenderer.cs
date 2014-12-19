using System;
using System.Web.UI;

using Composite.Core.PageTemplates;

namespace CompositeC1Contrib.Rendering.Mvc.Templates
{
    public class MvcPageRenderer : IPageRenderer
    {
        private Page _page;

        public void AttachToPage(Page renderTaget, PageContentToRender contentToRender)
        {
            _page = renderTaget;

            renderTaget.Init += Renderer;
        }

        private void Renderer(object sender, EventArgs e)
        {
            _page.Controls.Add(new LiteralControl("TODO: Execute the Controller for this Page and insert the result here (good luck)."));
        }
    }
}
