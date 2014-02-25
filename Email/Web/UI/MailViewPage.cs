using System;
using System.Linq;
using System.Net.Mail;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.Web.UI
{
    public class MailViewPage : BasePage
    {
        protected DateTime TimeStamp { get; private set; }
        protected MailMessage Message { get; private set; }

        protected Guid Id
        {
            get
            {
                var id = new Guid(Request.QueryString["id"]);

                return id;
            }
        }

        protected void OnBack(object sender, EventArgs e)
        {
            GoBack();
        }

        protected void OnDelete(object sender, EventArgs e)
        {
            using (var data = new DataConnection())
            {
                IData instance = null;

                switch (View)
                {
                    case "queued":
                    instance = data.Get<IQueuedMailMessage>().Single(m => m.Id == Id);
                    break;

                    case "sent":
                    instance = data.Get<ISentMailMessage>().Single(m => m.Id == Id);
                    break;
                }

                data.Delete(instance);

                UpdateParents();
                GoBack();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            switch (View)
            {
                case "queued": GetQueuedMailMessage(Id); break;
                case "sent": GetSentMailMessage(Id); break;
            }

            base.OnLoad(e);
        }

        private void GoBack()
        {
            Response.Redirect("log.aspx" + BaseUrl);
        }

        private void GetQueuedMailMessage(Guid id)
        {
            using (var data = new DataConnection())
            {
                var instance = data.Get<IQueuedMailMessage>().Single(m => m.Id == id);

                TimeStamp = instance.TimeStamp.ToLocalTime();
                Message = MailsFacade.GetMailMessage(instance);
            }
        }

        private void GetSentMailMessage(Guid id)
        {
            using (var data = new DataConnection())
            {
                var instance = data.Get<ISentMailMessage>().Single(m => m.Id == id);

                TimeStamp = instance.TimeStamp.ToLocalTime();
                Message = MailMessageFileWriter.ReadMailMessage(id);
            }
        }
    }
}
