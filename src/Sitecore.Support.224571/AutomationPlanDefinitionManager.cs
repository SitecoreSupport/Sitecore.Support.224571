namespace Sitecore.Support
{
  using System.Globalization;
  using Sitecore;
  using Sitecore.Framework.Conditions;
  using Sitecore.Marketing.Definitions;
  using Sitecore.Marketing.Definitions.AutomationPlans.Data;
  using Sitecore.Marketing.Definitions.AutomationPlans.Model;
  using Sitecore.Marketing.ObservableFeed.Activation;
  using Sitecore.Marketing.ObservableFeed.DeleteDefinition;
  using Sitecore.Marketing.Search;
  using Condition = Sitecore.Framework.Conditions.Condition;

  public class AutomationPlanDefinitionManager : DefinitionManagerBase<IAutomationPlanDefinition, AutomationPlanDefinitionRecord>
  {
		public AutomationPlanDefinitionManager(
        [NotNull] IAutomationPlanDefinitionRepository repository,
        [NotNull] ITaxonomyClassificationResolver<IAutomationPlanDefinition> classificationResolver,
        [NotNull] IDefinitionSearchProvider<IAutomationPlanDefinition> searchProvider,
        [NotNull] IActivationObservableFeed<IAutomationPlanDefinition> activationFeed,
        [NotNull] IDeleteDefinitionObservableFeed<IAutomationPlanDefinition> deleteDefinitionFeed,
        [CanBeNull] IDefinitionManagerSettings settings)
        : base(repository, classificationResolver, null, searchProvider, settings?.IsReadOnly ?? false, activationFeed, deleteDefinitionFeed)
    {
      Condition.Requires(repository, nameof(repository)).IsNotNull();
      Condition.Requires(classificationResolver, nameof(classificationResolver)).IsNotNull();
      Condition.Requires(searchProvider, nameof(searchProvider)).IsNotNull();
      Condition.Requires(activationFeed, nameof(activationFeed)).IsNotNull();
      Condition.Requires(deleteDefinitionFeed, nameof(deleteDefinitionFeed)).IsNotNull();
    }

    [NotNull]
    protected override IAutomationPlanDefinition ConvertRecordToDefinition([NotNull] AutomationPlanDefinitionRecord record, [CanBeNull] CultureInfo culture = null)
    {
      Condition.Requires(record, "record").IsNotNull();

      var definition = new AutomationPlanDefinition(record.Id, record.Alias, culture ?? record.Culture, record.Name, record.CreatedDate, record.CreatedBy);
      PopulateCommonFields(record, definition);

      definition.StartDate = record.StartDate;
      definition.EndDate = record.EndDate;
      definition.ContextKeyFactoryType = record.ContextKeyFactoryType;
      definition.EntryActivityId = record.EntryActivityId;
      definition.ReentryMode = record.ReentryMode;

      foreach (var activity in record.GetActivities())
      {
        var activityDefinition = CreateFromRecord(activity);
        definition.AddActivity(activityDefinition);
      }

      foreach (var activity in record.GetUniversalActivities())
      {
        var activityDefinition = CreateFromRecord(activity);
        definition.AddUniversalActivity(activityDefinition);
      }

      return definition;
    }

    [NotNull]
    protected virtual IAutomationActivityDefinition CreateFromRecord([NotNull] AutomationActivityDefinitionRecord record)
    {
      Condition.Requires(record, nameof(record)).IsNotNull();

      return new AutomationActivityDefinition
      {
        Id = record.Id,
        ActivityTypeId = record.ActivityTypeId,
        Parameters = record.Parameters,
        Paths = record.Paths
      };
    }

    [NotNull]
    protected virtual IAutomationUniversalActivityDefinition CreateFromRecord([NotNull] AutomationUniversalActivityDefinitionRecord record)
    {
      Condition.Requires(record, nameof(record)).IsNotNull();

      return new AutomationUniversalActivityDefinition
      {
        Id = record.Id,
        ActivityTypeId = record.ActivityTypeId,
        Parameters = record.Parameters,
        PlanProcessingPosition = record.PlanProcessingPosition,
        Order = record.Order
      };
    }

    protected override void SetCustomRecordFields([NotNull] IAutomationPlanDefinition source, [NotNull] AutomationPlanDefinitionRecord target)
    {
      Condition.Requires(source, "source").IsNotNull();
      Condition.Requires(target, "target").IsNotNull();

      target.ReentryMode = source.ReentryMode;
      target.ContextKeyFactoryType = source.ContextKeyFactoryType;
      target.EntryActivityId = source.EntryActivityId;
      target.StartDate = source.StartDate;
      target.EndDate = source.EndDate;

      foreach (var activity in source.GetActivities())
      {
        target.AddActivity(new AutomationActivityDefinitionRecord
        {
          Id = activity.Id,
          ActivityTypeId = activity.ActivityTypeId,
          Parameters = activity.Parameters,
          Paths = activity.Paths
        });
      }

      foreach (var activity in source.GetUniversalActivities())
      {
        target.AddUniversalActivity(new AutomationUniversalActivityDefinitionRecord
        {
          Id = activity.Id,
          ActivityTypeId = activity.ActivityTypeId,
          Parameters = activity.Parameters,
          PlanProcessingPosition = activity.PlanProcessingPosition,
          Order = activity.Order
        });
      }
    }
  }
}