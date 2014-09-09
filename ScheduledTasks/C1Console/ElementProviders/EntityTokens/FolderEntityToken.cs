using System;
using System.Collections.Generic;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(FolderAncestorProvider))]
    public class FolderEntityToken : EntityToken
    {
        public override string Id
        {
            get { return "FolderEntityToken"; }
        }

        public override string Source
        {
            get { return String.Empty; }
        }

        public override string Type
        {
            get { return TaskType.ToString(); }
        }

        public TaskType TaskType { get; private set; }

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
            string type;
            string source;
            string id;

            DoDeserialize(serializedEntityToken, out type, out source, out id);

            var taskType = (TaskType)Enum.Parse(typeof (TaskType), type);

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
