using System;

namespace CompositeC1Contrib.Teasers.Data
{
    public class PageTeaserHolder
    {
        public string Position { get; set; }
        public Guid SharedTeaserGuid { get; set; }
        public PageTeaserWrapper AdditionalInfo { get; set; }

        public PageTeaserHolder(PageTeaserWrapper additionalInfo)
        {
            Position = additionalInfo.Teaser.Position;
            SharedTeaserGuid = additionalInfo.Teaser.Id;
            AdditionalInfo = additionalInfo;
        }

        public PageTeaserHolder(string position, Guid sharedTeaserGuid, PageTeaserWrapper additionalInfo)
        {
            Position = position;
            SharedTeaserGuid = sharedTeaserGuid;
            AdditionalInfo = additionalInfo;
        }
    }
}
