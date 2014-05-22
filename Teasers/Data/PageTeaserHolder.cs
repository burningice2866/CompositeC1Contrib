using System;

namespace CompositeC1Contrib.Teasers.Data
{
    public class PageTeaserHolder
    {
        public string Position { get; set; }
        public string Design { get; set; }
        public Guid SharedTeaserGuid { get; set; }
        public PageTeaserWrapper AdditionalInfo { get; set; }

        public PageTeaserHolder(PageTeaserWrapper additionalInfo)
        {
            Position = additionalInfo.Teaser.Position;
            Design = additionalInfo.Design;
            SharedTeaserGuid = additionalInfo.Teaser.Id;
            AdditionalInfo = additionalInfo;
        }

        public PageTeaserHolder(string position, string design, Guid sharedTeaserGuid, PageTeaserWrapper additionalInfo)
        {
            Position = position;
            Design = design;
            SharedTeaserGuid = sharedTeaserGuid;
            AdditionalInfo = additionalInfo;
        }
    }
}
