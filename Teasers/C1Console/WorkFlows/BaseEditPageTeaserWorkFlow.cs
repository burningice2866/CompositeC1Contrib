using System;

using Composite.Core.Serialization;
using Composite.Data;

using CompositeC1Contrib.Teasers.C1Console.EntityTokens;
using CompositeC1Contrib.Teasers.Data.Types;
using CompositeC1Contrib.Workflows;

namespace CompositeC1Contrib.Teasers.C1Console.WorkFlows
{
    public abstract class BaseEditPageTeaserWorkFlow<T> : Basic1StepDocumentWorkflow where T : class, IPageTeaser
    {
        [NonSerialized]
        protected T Teaser;

        protected BaseEditPageTeaserWorkFlow(string formDefinitionFile) : base(formDefinitionFile) { }

        public override void OnInitialize(object sender, EventArgs e)
        {
            var pageTeaserInstanceEntityToken = EntityToken as PageTeaserInstanceEntityToken;
            if (pageTeaserInstanceEntityToken != null)
            {
                Teaser = (T)pageTeaserInstanceEntityToken.Teaser;
            }
            else
            {
                var pageTeaserPositionFolderEntityToken = (PageTeaserPositionFolderEntityToken)EntityToken;
                var payload = StringConversionServices.ParseKeyValueCollection(Payload);

                var teaserType = Type.GetType(StringConversionServices.DeserializeValue<string>(payload["teaserType"]));
                var name = StringConversionServices.DeserializeValue<string>(payload["name"]);

                Teaser = (T)DataFacade.BuildNew(teaserType);

                Teaser.Name = name;
                Teaser.Position = pageTeaserPositionFolderEntityToken.Id;
            }

            if (BindingExist("Label"))
            {
                return;
            }

            var page = PageManager.GetPageById(new Guid(EntityToken.Source));

            Bindings.Add("Label", Teaser.GetLabel());
            Bindings.Add("Positions", FormHelpers.GetPositions(page.TemplateId));

            Bindings.Add("Name", Teaser.Name);
            Bindings.Add("Position", Teaser.Position);
            Bindings.Add("AdditionalHeader", Teaser.AdditionalHeader);
            Bindings.Add("ShowOnDescendants", Teaser.ShowOnDescendants);
            Bindings.Add("PublishDate", Teaser.PublishDate);
            Bindings.Add("UnpublishDate", Teaser.UnpublishDate);

            LoadBindings();
        }

        public override void OnFinish(object sender, EventArgs e)
        {
            Teaser.Name = GetBinding<string>("Name");
            Teaser.Position = GetBinding<string>("Position");
            Teaser.AdditionalHeader = GetBinding<string>("AdditionalHeader");
            Teaser.ShowOnDescendants = GetBinding<bool>("ShowOnDescendants");
            Teaser.PublishDate = GetBinding<DateTime?>("PublishDate");
            Teaser.UnpublishDate = GetBinding<DateTime?>("UnpublishDate");

            SaveBindings();

            if (EntityToken is PageTeaserInstanceEntityToken)
            {
                DataFacade.Update(Teaser);
            }
            else
            {
                var page = PageManager.GetPageById(new Guid(EntityToken.Source));

                Teaser.Id = Guid.NewGuid();
                Teaser.PageId = page.Id;

                DataFacade.AddNew(Teaser);
            }

            CreateSpecificTreeRefresher().PostRefreshMesseges(EntityToken);
            SetSaveStatus(true);
        }

        protected abstract void LoadBindings();
        protected abstract void SaveBindings();
    }
}
