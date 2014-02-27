using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.Web.UI
{
    public class MailLogPage : BasePage
    {
        protected Repeater rptLog;

        protected IMailQueue Queue
        {
            get
            {
                var queueId = Guid.Parse(Request.QueryString["queue"]);
                using (var data = new DataConnection())
                {
                    var queue = data.Get<IMailQueue>().Single(q => q.Id == queueId);

                    return queue;
                }
            }
        }

        protected void OnRefresh(object sender, EventArgs e)
        {
            BindControls();
        }

        protected void OnDeleteAll(object sender, EventArgs e)
        {
            using (var data = new DataConnection())
            {
                if (View == "queued")
                {
                    var list = data.Get<IQueuedMailMessage>().Where(m => m.QueueId == Queue.Id).ToList();

                    data.Delete<IQueuedMailMessage>(list);
                }
            }

            BindControls();
            UpdateParents();
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
                            case "queued":
                            instance = data.Get<IQueuedMailMessage>().Single(m => m.Id == id);
                            break;

                            case "sent":
                            instance = data.Get<ISentMailMessage>().Single(m => m.Id == id);
                            break;
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
                if (View == "queued")
                {
                    query = data.Get<IQueuedMailMessage>();
                }
                else
                {
                    query = data.Get<ISentMailMessage>();
                }

                rptLog.DataSource = SelectLogItems(query, Queue);
            }

            DataBind();
        }

        private static IList<MailLogItem> SelectLogItems(IQueryable<IMailMessage> mailMessages, IMailQueue queue)
        {
            var now = DateTime.Now;

            return mailMessages.Where(m => m.QueueId == queue.Id)
                .OrderByDescending(m => m.TimeStamp)
                .Select(m => new MailLogItem()
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    TimeStamp = m.TimeStamp.ToLocalTime(),
                    TimeStampString = FormatTimeStamp(m.TimeStamp.ToLocalTime(), now)
                }).ToList();
        }

        private static string FormatTimeStamp(DateTime dt, DateTime now)
        {
            if (dt.Date == now.Date)
            {
                return dt.ToString("HH:mm:ss");
            }

            if (dt.Year == now.Year)
            {
                return dt.ToString("dd-MM HH:mm:ss");
            }

            return dt.ToString("dd-MM-yyyy HH:mm:ss");
        }
    }
}
