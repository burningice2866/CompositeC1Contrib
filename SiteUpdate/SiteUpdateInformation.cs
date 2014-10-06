using System;

namespace CompositeC1Contrib.SiteUpdate
{
    public class SiteUpdateInformation
    {
        public Guid Id { get; set; }
        public Guid InstallationId { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime ReleasedDate { get; set; }
        public string ChangeLog { get; set; }
    }
}
