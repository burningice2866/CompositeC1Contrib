using System;

using CompositeC1Contrib.Teasers.Data.Types;

namespace CompositeC1Contrib.Teasers.Data
{
	public class PageTeaserWrapper
	{
		public IPageTeaser Teaser { get; set; }
		public int ReverseDepth { get; set; }
		public Type InterfaceType { get; set; }

		public PageTeaserWrapper() { }

        public PageTeaserWrapper(IPageTeaser teaser, int reverseDepth)
		{
			Teaser = teaser;
			ReverseDepth = reverseDepth;
            InterfaceType = teaser.DataSourceId.InterfaceType;
		}
	}
}
