using System;

namespace CompositeC1Contrib.Teasers.Data
{
	public class SharedTeaser
	{
		public Guid Id { get; set; }
		public Guid TeaserGroup { get; set; }
		public bool InclInRotation { get; set; }
		public string Type { get; set; }
		public string Title { get; set; }
		public DateTime PublishDate { get; set; }
		public DateTime UnpublishDate { get; set; }
		public string Html { get; set; }

		public SharedTeaser() { }

		public SharedTeaser(Guid id, Guid teaserGroup, bool inclInRotation, string type, string title, string html, DateTime publishDate, DateTime unpublishDate)
		{
			Id = id;
			TeaserGroup = teaserGroup;
			InclInRotation = inclInRotation;
			Type = type;
			Title = title;
			Html = html;
			PublishDate = publishDate;
			UnpublishDate = unpublishDate;
		}
	}
}
