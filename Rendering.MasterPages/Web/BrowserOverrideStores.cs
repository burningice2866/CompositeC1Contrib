
namespace CompositeC1Contrib.Web
{
    class BrowserOverrideStores
    {
        private BrowserOverrideStore _currentOverrideStore;
        private static BrowserOverrideStores _instance = new BrowserOverrideStores();

        public static BrowserOverrideStore Current
        {
            get { return _instance.CurrentInternal; }
            set { _instance.CurrentInternal = value; }
        }

        internal BrowserOverrideStore CurrentInternal
        {
            get { return this._currentOverrideStore; }
            set { this._currentOverrideStore = value ?? new RequestBrowserOverrideStore(); }
        }
        
        public BrowserOverrideStores()
        {
            this._currentOverrideStore = new CookieBrowserOverrideStore();
        }
    }
}
