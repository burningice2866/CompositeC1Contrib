using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Composite.C1Console.Events;
using Composite.Core.PackageSystem;
using Composite.Core.PackageSystem.PackageFragmentInstallers;
using Composite.Data.DynamicTypes;
using Composite.Data.GeneratedTypes;

namespace CompositeC1Contrib.PackageFragmentInstallers
{
    public class DynamicDataTypeChangerFragmentInstaller : BasePackageFragmentInstaller
    {
        private Dictionary<DataTypeDescriptor, DataTypeDescriptor> _updates;

        public override IEnumerable<XElement> Install()
        {
            if (_updates == null)
            {
                throw new InvalidOperationException(GetType().Name + " has not been validated");
            }

            foreach (var kvp in _updates)
            {
                var oldDataTypeDescriptor = kvp.Key;
                var newDataTypeDescriptor = kvp.Value;

                var updateDescriptor = new UpdateDataTypeDescriptor(oldDataTypeDescriptor, newDataTypeDescriptor);

                GeneratedTypesFacade.UpdateType(updateDescriptor);
            }

            GlobalEventSystemFacade.FlushTheSystem();

            return Configuration;
        }

        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            var validationSummary = new List<PackageFragmentValidationResult>();
            _updates = new Dictionary<DataTypeDescriptor, DataTypeDescriptor>();

            var types = Configuration.Where(e => e.Name == "Type");
            foreach (var t in types)
            {
                var id = Guid.Parse(t.Attribute("id").Value);
                var dataTypeDescription = DynamicTypeManager.GetDataTypeDescriptor(id);

                if (dataTypeDescription == null)
                {
                    var validationMessage = String.Format("Invalid type id '{0}", id);

                    validationSummary.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, validationMessage, t));

                    continue;
                }

                var adds = t.Elements("Add");
                foreach (var add in adds)
                {
                    var fieldId = Guid.Parse(add.Attribute("id").Value);
                    if (dataTypeDescription.Fields.Any(f => f.Id == fieldId))
                    {
                        var validationMessage = String.Format("Field id '{0}' already exists", fieldId);

                        validationSummary.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, validationMessage, add));

                        continue;
                    }

                    var element = new XElement(add) { Name = "DataFieldDescriptor" };
                    var dataFieldDescriptor = DataFieldDescriptor.FromXml(element);

                    GetClone(dataTypeDescription).Fields.Add(dataFieldDescriptor);
                }

                var removes = t.Elements("Remove");
                foreach (var remove in removes)
                {
                    var fieldId = Guid.Parse(remove.Attribute("id").Value);
                    var dataFieldDescriptor = dataTypeDescription.Fields.SingleOrDefault(f => f.Id == fieldId);
                    if (dataFieldDescriptor == null)
                    {
                        var validationMessage = String.Format("Field id '{0}' doesn't exist", fieldId);

                        validationSummary.Add(new PackageFragmentValidationResult(PackageFragmentValidationResultType.Fatal, validationMessage, remove));

                        continue;
                    }

                    GetClone(dataTypeDescription).Fields.Remove(dataFieldDescriptor);
                }
            }

            return validationSummary;
        }

        private DataTypeDescriptor GetClone(DataTypeDescriptor dataTypeDescriptor)
        {
            DataTypeDescriptor clone;
            if (!_updates.TryGetValue(dataTypeDescriptor, out clone))
            {
                clone = dataTypeDescriptor.Clone();

                _updates.Add(dataTypeDescriptor, clone);
            }

            return clone;
        }
    }
}
