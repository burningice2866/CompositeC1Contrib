using System;

using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Security
{
    public class SecurityEvaluatorFactory
    {
        public static ISecurityEvaluator<T> GetEvaluatorFor<T>() where T : IData
        {
            if (typeof (T) == typeof (IPage))
            {
                return new EvaluatedPagePermissions() as ISecurityEvaluator<T>;
            }

            if (typeof(T) == typeof(IMediaFile))
            {
                return new EvaluatedMediaPermissions() as ISecurityEvaluator<T>;
            }

            if (typeof(T) == typeof(IMediaFileFolder))
            {
                return new EvaluatedMediaPermissions() as ISecurityEvaluator<T>;
            }

            throw new InvalidOperationException("No security evaluator found");
        }
    }
}
