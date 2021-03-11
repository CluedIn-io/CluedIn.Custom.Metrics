using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.Core.Metrics;
using CluedIn.Metrics;
using CluedIn.Metrics.Implementations;

namespace CluedIn.Custom.Metrics.Implementations
{
    public class InterpretabilityMetric : PercentageMetric
    {
        private readonly IMetricProviderResolver providerResolver;

        public InterpretabilityMetric(IMetricProviderResolver providerResolver)
        {
            this.providerResolver = providerResolver;
        }

        public override short ValueSize => sizeof(ushort);

        public override Guid Id { get; } = new Guid("{180C4274-5E1A-4F99-8259-6388B22EA452}");

        public override string[] Categories { get; } = { MetricCategories.DataQuality };

        protected override PercentageMetricValue CalculatePct(
            MetricsProcessingContext context,
            IMetricDimension dimension,
            IMetricValues<short> existingMetricValues,
            Entity entity)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (dimension == null)
                throw new ArgumentNullException(nameof(dimension));

            if (existingMetricValues == null)
                throw new ArgumentNullException(nameof(existingMetricValues));

            if (dimension.DimensionType.HasFlag(MetricDimensionType.Entity))
            {
                if (entity == null)
                    throw new ArgumentNullException();

                var hasProviderDefinition = dimension.ProviderDefinitionId.HasValue;
                var hasProvider = dimension.ProviderId.HasValue;

                // Entity Integration Dimension
                if (hasProviderDefinition && hasProvider)
                {
                    var map = this.GetValuesMap(context, entity, v => v.OriginProviderDefinitionId == dimension.ProviderDefinitionId.Value).ToList();
                    var average = this.CalculateMetricValue(map);

                    return new PercentageMetricValue(dimension, entity.Id, average).WithExplanation(this.GetExplanation(context, map));
                }

                // Entity Integration Type
                if (hasProvider)
                {
                    var map = this.GetValuesMap(context, entity, v => this.providerResolver.ResolveProvider(context, v.OriginProviderDefinitionId)?.Id == dimension.ProviderId.Value).ToList();
                    var average = this.CalculateMetricValue(map);

                    return new PercentageMetricValue(dimension, entity.Id, average).WithExplanation(this.GetExplanation(context, map));
                }

                // Entity
                if (!hasProviderDefinition)
                {
                    var map = this.GetValuesMap(context, entity, v => true).ToList();
                    var average = this.CalculateMetricValue(map);

                    return new PercentageMetricValue(dimension, entity.Id, average).WithExplanation(this.GetExplanation(context, map));
                }
            }

            // Global
            else if (dimension.DimensionType.HasFlag(MetricDimensionType.Global))
            {
                var dateDimension = MetricDateDimension.Today;

                short average;
                string explanation;

                switch (dimension.DimensionType)
                {
                    case MetricDimensionType.Global:
                        average = (short)existingMetricValues.Average(this, v => v.Dimension.DimensionType == MetricDimensionType.Entity);
                        explanation = this.GetAggregatedValueExplanation(context, MetricDimensionType.Entity);
                        break;

                    case MetricDimensionType.GlobalIntegration:
                        average = (short)existingMetricValues.Average(this, v => v.Dimension.DimensionType == MetricDimensionType.EntityIntegration && v.Dimension.ProviderId == dimension.ProviderId);
                        explanation = this.GetAggregatedValueExplanation(context, MetricDimensionType.EntityIntegration, null, dimension.ProviderId);
                        break;

                    case MetricDimensionType.GlobalIntegrationType:
                        average = (short)existingMetricValues.Average(this, v => v.Dimension.DimensionType == MetricDimensionType.EntityIntegrationType && v.Dimension.ProviderDefinitionId == dimension.ProviderDefinitionId);
                        explanation = this.GetAggregatedValueExplanation(context, MetricDimensionType.EntityIntegrationType, null, null, dimension.ProviderDefinitionId);
                        break;

                    default:
                        throw new Exception();
                }

                return new PercentageMetricValue(dimension, dateDimension, average).WithExplanation(explanation);
            }

            throw new Exception();
        }

        private IEnumerable<IGrouping<IVocabulary, KeyValuePair<string, VocabularyKey>>> GetValuesMap(MetricsProcessingContext context, Entity entity, Func<IDataPart, bool> dataPartFilter)
        {
            var dataParts = entity.Details.DataEntries.Where(dataPartFilter);

            // Note: Processed properties is used to get populated keys AFTER translation
            var properties = dataParts.SelectMany(d => d.ProcessedEntityData.Properties.Keys).Distinct().ToDictionary(v => v, v => "DUMMY");

            var usedVocabularyKeys = context.ApplicationContext.System.Vocabularies.GetUsedVocabularyKeys(properties)
                                                                                   .Where(kp => !(kp.Value.Vocabulary is DynamicVocabulary) && kp.Value.Vocabulary.IsCore)
                                                                                   .ToDictionary(kp => kp.Key, kp => kp.Value);

            var vocabularyGroups = usedVocabularyKeys.GroupBy(k => k.Value.Vocabulary);

            return vocabularyGroups;
        }

        private double CalculateMetricValue(IEnumerable<IGrouping<IVocabulary, KeyValuePair<string, VocabularyKey>>> vocabularyGroups)
        {
            var byVocabulary = new List<double>(vocabularyGroups.Count());

            foreach (var vocabularyGroup in vocabularyGroups)
            {
                var vocabulary = vocabularyGroup.Key;
                var relevance = (double)vocabulary.Keys.Count() / vocabularyGroup.Count();

                byVocabulary.Add(relevance);
            }

            var average = byVocabulary.Average();
            if (average > 100)
                average = 100;

            if (average < 0)
                average = 0;

            return average / 100f;
        }

        public override bool ShouldPersist(IMetricDimension dimension)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IMetricDimension> GetDimensions(MetricsProcessingContext context, IMetricsModel model)
        {
            /*
             *                                                                    Dimension Table:
             *
             *                                                                  | DimensionType         | DetailType | ProviderDefinitionId | ProviderId | Detail        | Persistence                       |
             *                                                                  |-----------------------|------------|----------------------|------------|---------------|-----------------------------------|
             * Global                                                           | Global                |            |                      |            |               |                                   |
             *  │  Global Provider                                              | GlobalIntegrationType |            |                      | Id         |               |                                   |
             *  │   │  Global Provider Definition                               | GlobalIntegration     |            | Id                   | Id         |               |                                   |
             *  │   │   │                                                       |-----------------------|------------|----------------------|------------|---------------|-----------------------------------|
             *  └───│───│── Entity                                              | Entity                |            |                      |            |               | Blob, Graph, Search, EntityMetric |
             *      └───│── Entity Provider                                     | EntityIntegrationType |            |                      | Id         |               | EntityMetric                      |
             *          └── Entity Provider Definition                          | EntityIntegration     |            | Id                   | Id         |               | EntityMetric                      |
             *
             */


            var existingDimensions = model.MetricDimensions.Where(d => d.MetricId == this.Id);

            var entityIntegrationDimensions = existingDimensions.Where(d => d.DimensionType == MetricDimensionType.EntityIntegration && d.DimensionDetailType == MetricDimensionDetailType.None && d.ProviderDefinitionId.HasValue && d.ProviderId.HasValue);

            if (entityIntegrationDimensions.Any())
            {
                foreach (var entityDimension in entityIntegrationDimensions)
                    yield return this.GetDefaultGlobalDimension(context, entityDimension.ProviderDefinitionId, entityDimension.ProviderId);
            }

            var entityIntegrationTypeDimensions = existingDimensions.Where(d => d.DimensionType == MetricDimensionType.EntityIntegrationType && d.DimensionDetailType == MetricDimensionDetailType.None && d.ProviderId.HasValue);

            if (entityIntegrationTypeDimensions.Any())
            {
                foreach (var entityDimension in entityIntegrationTypeDimensions)
                    yield return this.GetDefaultGlobalDimension(context, entityDimension.ProviderId);
            }

            if (existingDimensions.Any(d => d.DimensionType == MetricDimensionType.Entity))
                yield return this.GetDefaultGlobalDimension(context);
        }

        public override IEnumerable<IMetricDimension> GetDimensionsToCalculate(MetricsProcessingContext context, Entity entity)
        {
            /*
             *                                                                    Dimension Table:
             *
             *                                                                  | DimensionType         | DetailType | ProviderDefinitionId | ProviderId | Detail        | Persistence                       |
             *                                                                  |-----------------------|------------|----------------------|------------|---------------|-----------------------------------|
             * Global                                                           | Global                |            |                      |            |               |                                   |
             *  │  Global Provider                                              | GlobalIntegrationType |            |                      | Id         |               |                                   |
             *  │   │  Global Provider Definition                               | GlobalIntegration     |            | Id                   | Id         |               |                                   |
             *  │   │   │                                                       |-----------------------|------------|----------------------|------------|---------------|-----------------------------------|
             *  └───│───│── Entity                                              | Entity                |            |                      |            |               | Blob, Graph, Search, EntityMetric |
             *      └───│── Entity Provider                                     | EntityIntegrationType |            |                      | Id         |               | EntityMetric                      |
             *          └── Entity Provider Definition                          | EntityIntegration     |            | Id                   | Id         |               | EntityMetric                      |
             *
             */

            // Provider Definition
            foreach (var group in entity.Details.DataEntries.GroupBy(d => d.OriginProviderDefinitionId))
            {
                if (group.Key == null)
                    continue;

                var vocabKeys = context.ApplicationContext.System.Vocabularies.GetUsedVocabularyKeys(group.SelectMany(d => d.ProcessedEntityData.Properties.Keys).Distinct());
                var vocabKeysFiltered = vocabKeys.Where(kp => !(kp.Value.Vocabulary is DynamicVocabulary) && kp.Value.Vocabulary.IsCore);

                if (!vocabKeysFiltered.Any())
                    continue;

                var providerDefinition = context.Organization.Providers.GetProviderDefinition(context, group.Key.Value);

                if (providerDefinition == null)
                    break;

                var hasKeys = group.Any(d => d.ProcessedEntityData.Properties.Keys.Any());

                if (hasKeys)
                {
                    var globalDimension = this.GetDefaultGlobalDimension(context, group.Key, providerDefinition.ProviderId);
                    yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.EntityIntegration, MetricDimensionDetailType.None, providerDefinitionId: group.Key, providerId: providerDefinition.ProviderId, persistence: MetricDimensionPersistence.EntityMetric);
                }
            }

            // Provider 
            foreach (var group in entity.Details.DataEntries.Where(d => d.OriginProviderDefinitionId.HasValue)
                                                            .GroupBy(d => this.providerResolver.ResolveProvider(context, d.OriginProviderDefinitionId.Value)))
            {
                if (group.Key == null)
                    continue;

                var vocabKeys = context.ApplicationContext.System.Vocabularies.GetUsedVocabularyKeys(group.SelectMany(d => d.ProcessedEntityData.Properties.Keys).Distinct());
                var vocabKeysFiltered = vocabKeys.Where(kp => !(kp.Value.Vocabulary is DynamicVocabulary) && kp.Value.Vocabulary.IsCore);

                if (!vocabKeysFiltered.Any())
                    continue;

                var hasKeys = group.Any(d => d.ProcessedEntityData.Properties.Keys.Any());

                if (hasKeys)
                {
                    var globalDimension = this.GetDefaultGlobalDimension(context, group.Key.Id);
                    yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.EntityIntegrationType, MetricDimensionDetailType.None, providerId: group.Key.Id, persistence: MetricDimensionPersistence.EntityMetric);
                }
            }

            // Entity
            if (entity.Properties.Any())
            {
                var vocabKeys = context.ApplicationContext.System.Vocabularies.GetUsedVocabularyKeys(entity.Properties);
                var vocabKeysFiltered = vocabKeys.Where(kp => !(kp.Value.Vocabulary is DynamicVocabulary) && kp.Value.Vocabulary.IsCore);

                if (vocabKeysFiltered.Any())
                {
                    var globalDimension = this.GetDefaultGlobalDimension(context);
                    yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.Entity, MetricDimensionDetailType.None, persistence: MetricDimensionPersistence.Blob | MetricDimensionPersistence.Graph | MetricDimensionPersistence.Search | MetricDimensionPersistence.EntityMetric);
                }
            }
        }

        private string GetExplanation(MetricsProcessingContext context, IEnumerable<IGrouping<IVocabulary, KeyValuePair<string, VocabularyKey>>> vocabularyGroups)
        {
            if (!context.MetricsExecutionOptions.HasFlag(MetricsExecutionOption.Explanation))
                return null;

            var sb = new StringBuilder();

            var completenessByVocabulary = new List<(string vocabularyName, int populatedCount, int vocabularyKeyCount, double result)>(vocabularyGroups.Count());

            foreach (var vocabularyGroup in vocabularyGroups)
            {
                var vocabulary = vocabularyGroup.Key;
                var completeness = vocabularyGroup.Count() / (double)vocabulary.Keys.Count();

                completenessByVocabulary.Add((vocabulary.VocabularyName, vocabularyGroup.Count(), vocabulary.Keys.Count(), completeness));
            }

            var average = completenessByVocabulary.Average(v => v.result);

            var table = AsciiTableGenerator.GenerateTable(
                new[] { "Vocabulary", "PopulatedCount", "VocabularyKeyCount", "Calculation", "Result" },
                completenessByVocabulary,
                v => v.vocabularyName,
                v => v.populatedCount.ToString(CultureInfo.InvariantCulture),
                v => v.vocabularyKeyCount.ToString(CultureInfo.InvariantCulture),
                v => $"{v.populatedCount} / {v.vocabularyKeyCount}",
                v => v.result.ToString("0.0000", CultureInfo.InvariantCulture));

            sb.AppendLine(table);
            sb.AppendLine();
            sb.AppendLine("Average: " + average.ToString("0.0000", CultureInfo.InvariantCulture));

            return sb.ToString();
        }
    }
}
