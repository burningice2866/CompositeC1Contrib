using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Composite.Core.WebClient.UiControlLib;
using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.Web.UI
{
    public class MailLogPage : BasePage
    {
        protected Repeater rptLog;
        protected Selector ddlTemplates;

        protected void OnRefresh(object sender, EventArgs e)
        {
            BindControls();
        }

        protected void OnDeleteAll(object sender, EventArgs e)
        {
            using (var data = new DataConnection())
            {
                if (View == LogViewMode.Queued)
                {
                    var list = data.Get<IQueuedMailMessage>();

                    list = FilterQueue(list);
                    list = FilterTemplate(list);

                    data.Delete<IQueuedMailMessage>(list);
                }
            }

            BindControls();
            UpdateParents();
        }

        protected void OnTemplateChanged(object sender, EventArgs e)
        {
            var selected = ddlTemplates.SelectedValue;
            if (selected == "[]")
            {
                selected = String.Empty;
            }

            var url = BaseUrl.Replace("template=" + Request.QueryString["template"], "template=" + selected);

            Response.Redirect("log.aspx" + url);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!IsPostBack)
            {
                var cmd = Request.QueryString["cmd"];
                if (cmd == "delete")
                {
                    var id = new Guid(Request.QueryString["id"]);

                    using (var data = new DataConnection())
                    {
                        IData instance = null;

                        switch (View)
                        {
                            case LogViewMode.Queued: instance = data.Get<IQueuedMailMessage>().Single(m => m.Id == id); break;
                            case LogViewMode.Sent: instance = data.Get<ISentMailMessage>().Single(m => m.Id == id); break;
                        }

                        data.Delete(instance);

                        BindControls();
                        UpdateParents();
                    }
                }

                BindControls();
            }

            base.OnLoad(e);
        }

        private void BindControls()
        {
            using (var data = new DataConnection())
            {
                IQueryable<IMailMessage> query;
                if (View == LogViewMode.Queued)
                {
                    query = data.Get<IQueuedMailMessage>();
                }
                else
                {
                    query = data.Get<ISentMailMessage>();
                }

                query = FilterQueue(query);

                var templateFilter = SelectLogItems(query).Select(itm => itm.Template)
                    .Where(t => t != null)
                    .Select(t => t.Key)
                    .Distinct();

                ddlTemplates.Items.Clear();
                ddlTemplates.Items.Add(new ListItem("no filter", "[]"));
                ddlTemplates.Items.AddRange(templateFilter.Select(t => new ListItem(t, t)).ToArray());

                foreach (ListItem itm in ddlTemplates.Items)
                {
                    if (itm.Value == Request.QueryString["template"])
                    {
                        itm.Selected = true;
                    }
                }
                
                query = FilterTemplate(query);

                rptLog.DataSource = SelectLogItems(query);
            }

            DataBind();
        }

        private IQueryable<T> FilterQueue<T>(IQueryable<T> mailMessages) where T : IMailMessage
        {
            Guid queueId;
            if (Guid.TryParse(Request.QueryString["queue"], out queueId))
            {
                mailMessages = mailMessages.Where(m => m.QueueId == queueId);
            }

            return mailMessages;
        }

        private IQueryable<T> FilterTemplate<T>(IQueryable<T> mailMessages) where T : IMailMessage
        {
            var template = Request.QueryString["template"];
            if (!String.IsNullOrEmpty(template))
            {
                mailMessages = mailMessages.Where(m => m.MailTemplateKey == template);
            }

            return mailMessages;
        }

        private static IEnumerable<MailLogItem> SelectLogItems(IQueryable<IMailMessage> mailMessages)
        {
            return mailMessages
                .OrderByDescending(m => m.TimeStamp)
                .Take(100)
                .Select(MailLogItem.FromIMailMessage)
                .ToList();
        }
    }
}
