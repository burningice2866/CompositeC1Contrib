using System;
using System.Web;
using System.Web.Services;
using System.Web.UI;

using Hangfire;

namespace CompositeC1Contrib.DataTypesSynchronization.Web.UI
{
    public class SynchronizePage : Page
    {
        [WebMethod]
        public static string JobStatus(string jobId)
        {
            var log = GetJobLog(jobId);

            if (!String.IsNullOrEmpty(log))
            {
                return log.Contains("Job finished") ? "no" : "yes";
            }

            Guid guid;

            return Guid.TryParse(jobId, out guid) ? "pending" : "no";
        }

        [WebMethod]
        public static string GetJobLog(string jobId)
        {
            Guid guid;
            if (!Guid.TryParse(jobId, out guid))
            {
                return String.Empty;
            }

            var log = Logger.ReadLog(guid);

            return HttpUtility.HtmlEncode(log).Replace(Environment.NewLine, "<br />");
        }

        protected void UpdateData_Click(object sender, EventArgs e)
        {
            var jobId = Guid.NewGuid();

            BackgroundJob.Enqueue(() => DefaultJobs.SynchronizeData(jobId, SynchronizationFrequency.All, JobCancellationToken.Null));

            Response.Redirect("Synchronize.aspx?jobid=" + jobId);
        }
    }
}
