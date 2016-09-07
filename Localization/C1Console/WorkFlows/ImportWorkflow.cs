using System;
using System.IO;
using System.Linq;
using System.Text;

using Composite.C1Console.Events;
using Composite.C1Console.Forms.CoreUiControls;

using CompositeC1Contrib.Localization.ImportExport;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Localization.C1Console.Workflows
{
    public sealed class ImportWorkflow : Basic1StepDialogWorkflow
    {
        public ImportWorkflow() : base("\\InstalledPackages\\CompositeC1Contrib.Localization\\Import.xml") { }

        public override void OnInitialize(object sender, EventArgs e)
        {
            if (BindingExist("UploadedFile"))
            {
                return;
            }

            Bindings.Add("UploadedFile", new UploadedFile());
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            var uploadedFile = GetBinding<UploadedFile>("UploadedFile");

            using (var sr = new StreamReader(uploadedFile.FileStream))
            {
                var txt = sr.ReadToEnd();

                var importModel = ImportExportModel.FromXml(txt);
                var importer = new ResourceImporter(importModel);

                var result = importer.Import();

                var sb = new StringBuilder();

                sb.AppendLine("Languages: " + String.Join(", ", result.Languages.Select(l => l.Name)));
                sb.AppendLine("ResourceSets: " + String.Join(", ", result.ResourceSets.Select(s => s == null ? "Website" : s)));
                sb.AppendLine();
                sb.AppendLine("Keys added: " + result.KeysAdded);
                sb.AppendLine("Values added: " + result.ValuesAdded);
                sb.AppendLine("Values updated: " + result.ValuesUpdated + " (were the same: " + result.ValuesWereTheSame + ")");

                ShowMessage(DialogType.Message, "Import result", sb.ToString());
            }
        }
    }
}
