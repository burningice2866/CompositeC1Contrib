using System;
using System.Linq;
using System.Net.Mail;
using System.Web.UI.WebControls;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.Web.UI
{
    public class MailViewPage : BasePage
    {
        protected Repeater rptAttachments;

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
            }

            UpdateParents();
            GoBack();
        }

        protected void OnDownload(object sender, EventArgs e)
        {
            var eml = MailMessageFileWriter.ToEml(Message);

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Id + ".eml");
            Response.AddHeader("Content-Type", "message/rfc822");
            Response.Write(eml);
            Response.End();
        }

        protected override void OnLoad(EventArgs e)
        {
            switch (View)
            {
                case "queued": GetQueuedMailMessage(Id); break;
                case "sent": GetSentMailMessage(Id); break;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["cmd"] == "download")
                {
                    var attachmentId = Request.QueryString["attachmentId"];
                    var attachment = Message.Attachments.Single(a => a.ContentId == attachmentId);

                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + attachment.Name);
                    Response.AddHeader("Content-Type", attachment.ContentType.MediaType);
                    attachment.ContentStream.CopyTo(Response.OutputStream);
                    Response.End();

                }
                else
                {
                    if (Message.Attachments.Any())
                    {
                        var list = Message.Attachments.Select(attachment => new MailAttachmentItem()
                        {
                            Id = attachment.ContentId,
                            Name = attachment.Name,
                            Size = attachment.ContentStream.Length
                        });

                        rptAttachments.DataSource = list;
                        rptAttachments.DataBind();
                    }
                }
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
                Message = MailMessageFileWriter.DeserializeFromBase64(instance.SerializedMessage);
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
