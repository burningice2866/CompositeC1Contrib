using System;
using System.Collections.Generic;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(TaskAncestorProvider))]
    public class TaskEntityToken : EntityToken
    {
        public TaskType TaskType { get; }

        public override string Id { get; }
        public override string Source => TaskType.ToString();
        public override string Type => String.Empty;

        public TaskEntityToken(TaskType type, string id)
        {
            TaskType = type;
            Id = id;
        }

        public override string Serialize()
        {
            return DoSerialize();
        }

        public static EntityToken Deserialize(string serializedEntityToken)
        {
            DoDeserialize(serializedEntityToken, out _, out var source, out var id);

            var taskType = (TaskType)Enum.Parse(typeof (TaskType), source);

            return new TaskEntityToken(taskType, id);
        }
    }

    public class TaskAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var token = entityToken as TaskEntityToken;
            if (token == null)
            {
                yield break;
            }

            yield return new FolderEntityToken(token.TaskType);
        }
    }
}
