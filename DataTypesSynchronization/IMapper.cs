namespace CompositeC1Contrib.DataTypesSynchronization
{
    public interface IMapper<in TModel, in TData>
    {
        void Map(TModel model, TData data);
    }
}
