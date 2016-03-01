namespace CompositeC1Contrib.DataTypesSynchronization
{
    public interface IPostProcessor<TData>
    {
        void Process(DataUpdateCompareResult<TData> result);
    }
}
