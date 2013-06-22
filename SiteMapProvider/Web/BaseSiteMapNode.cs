using System;
using System.Globalization;
using System.Web;

namespace CompositeC1Contrib.Web
{
    public static class SiteMapNodeExtension
    {
        public static bool IsEqualToOrDescendantOf(this SiteMapNode node, SiteMapNode other)
        {
            if (node == null)
            {
                return false;
            }

            return node.Equals(other) || node.IsDescendantOf(other);
        }
    }

    public class BaseSiteMapNode : SiteMapNode
    {
        public CultureInfo Culture { get; protected set; }
        public int? Priority { get; protected set; }
        public int Depth { get; protected set; }
        public DateTime LastModified { get; protected set; }
        public SitemapNodeChangeFrequency? ChangeFrequency { get; protected set; }
        public string DocumentTitle { get; protected set; }

        public BaseSiteMapNode(SiteMapProvider provider, string key, CultureInfo culture)
            : this(provider, key, String.Empty, String.Empty, String.Empty, culture, DateTime.MinValue) { }

        public BaseSiteMapNode(SiteMapProvider provider, string key, string url, string title, string description, CultureInfo culture, DateTime lastModified)
            : base(provider, key, url, title, description)
        {
            Culture = culture;
            LastModified = lastModified;
        }

        public bool Equals(BaseSiteMapNode obj)
        {
            return Key == obj.Key && Culture.Equals(obj.Culture);
        }

        public override bool Equals(object obj)
        {
            if (obj is BaseSiteMapNode)
            {
                return Equals((BaseSiteMapNode)obj);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode() + Culture.GetHashCode();
        }

        public static bool operator ==(BaseSiteMapNode a, BaseSiteMapNode b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(BaseSiteMapNode a, BaseSiteMapNode b)
        {
            return !(a == b);
        }
    }
}
