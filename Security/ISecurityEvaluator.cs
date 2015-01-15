using Composite.Data;

namespace CompositeC1Contrib.Security
{
    public interface ISecurityEvaluator<T> where T : IData
    {
        bool HasAccess(T data);
        EvaluatedPermissions GetEvaluatedPermissions(T data);
    }
}
