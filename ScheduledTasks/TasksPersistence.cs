using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CompositeC1Contrib.ScheduledTasks.Configuration;

namespace CompositeC1Contrib.ScheduledTasks
{
    public class TasksPersistence
    {
        private static readonly object Lock = new object();

        private static readonly string TasksDocumentPath = Path.Combine(ScheduledTasksWorkflow.RootPath, "tasks.xml");
        private static readonly XElement TasksDocument;

        static TasksPersistence()
        {
            TasksDocument = !File.Exists(TasksDocumentPath) ? new XElement("tasks") : XElement.Load(TasksDocumentPath);
        }

        public static ScheduledTask ParseTask(ScheduledTaskElement element)
        {
            ScheduledTask task;

            var taskElement = TasksDocument.Elements("task").SingleOrDefault(e => e.Attribute("name").Value == element.Name);
            if (taskElement != null)
            {
                var lastRun = DateTime.Parse(taskElement.Attribute("lastRun").Value, CultureInfo.InvariantCulture);
                var nextRun = DateTime.Parse(taskElement.Attribute("nextRun").Value, CultureInfo.InvariantCulture);

                task = new ScheduledTask(element.CronExpression, lastRun, nextRun);
            }
            else
            {
                task = new ScheduledTask(element.CronExpression);
            }

            task.Name = element.Name;
            task.Type = Type.GetType(element.Type);
            task.MethodName = element.Method;

            UpdateTasksDocument(task);

            return task;
        }

        public static void UpdateTasksDocument(ScheduledTask task)
        {
            var taskElement = TasksDocument.Elements("task").SingleOrDefault(el => el.Attribute("name").Value == task.Name);
            if (taskElement == null)
            {
                taskElement = new XElement("task",
                    new XAttribute("name", task.Name),
                    new XAttribute("cronExpression", String.Empty),
                    new XAttribute("lastRun", String.Empty),
                    new XAttribute("nextRun", String.Empty));

                TasksDocument.Add(taskElement);
            }

            taskElement.Attribute("cronExpression").Value = task.CronExpression;
            taskElement.Attribute("lastRun").Value = task.LastRun.ToString(CultureInfo.InvariantCulture);
            taskElement.Attribute("nextRun").Value = task.NextRun.ToString(CultureInfo.InvariantCulture);

            lock (Lock)
            {
                TasksDocument.Save(TasksDocumentPath);
            }
        }
    }
}
