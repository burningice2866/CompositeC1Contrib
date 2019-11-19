using Composite.Data;

namespace CompositeC1Contrib.Data
{
    public static class DataConnectionExtensions
    {
        public static T AddOrUpdate<T>(this DataConnection data, T itm) where T : class, IData
        {
            if (itm.DataSourceId.ExistsInStore)
            {
                data.Update(itm);
            }
            else
            {
                itm = data.Add(itm);
            }

            return itm;
        }
    }
}
