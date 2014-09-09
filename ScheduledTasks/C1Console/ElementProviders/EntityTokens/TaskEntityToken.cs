using System;
using System.Collections.Generic;

using Composite.C1Console.Security;

namespace CompositeC1Contrib.ScheduledTasks.C1Console.ElementProviders.EntityTokens
{
    [SecurityAncestorProvider(typeof(TaskAncestorProvider))]
    public class TaskEntityToken : EntityToken
    {
        private readonly string _id;
        public override string Id
        {
            get { return _id; }
        }

        public override string Source
        {
            get { return TaskType.ToString(); }
        }

        public override string Type
        {
            get { return String.Empty; }
        }

        public TaskType TaskType { get; private set; }

        public TaskEntityToken(TaskType type, string id)
        {
            TaskType = type;
            _id = id;
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
