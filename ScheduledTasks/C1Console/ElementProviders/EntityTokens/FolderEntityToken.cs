using System;
using System.Collections.Generic;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(FolderAncestorProvider))]
    public class FolderEntityToken : EntityToken
    {
        public TaskType TaskType { get; }

        public override string Id => nameof(FolderEntityToken);

        public override string Source => TaskType.ToString();

        public override string Type => String.Empty;

        public FolderEntityToken(TaskType type)
        {
            TaskType = type;
        }

        public override string Serialize()
        {
            return DoSerialize();
        }

        public static EntityToken Deserialize(string serializedEntityToken)
        {
            DoDeserialize(serializedEntityToken, out _, out var source, out _);

            var taskType = (TaskType)Enum.Parse(typeof(TaskType), source);

            return new FolderEntityToken(taskType);
        }
    }

    public class FolderAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var token = entityToken as FolderEntityToken;
            if (token == null)
            {
                yield break;
            }

            yield return new ScheduledTasksElementProviderEntityToken();
        }
    }
}
