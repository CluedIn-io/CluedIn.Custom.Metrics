﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using CluedIn.Core.Data;
//using CluedIn.Core.Metrics;
//using CluedIn.Core.Processing;
//using CluedIn.Metrics;
//using CluedIn.Processing;

//namespace CluedIn.Custom.Metrics.Implementations
//{
//    public class StewardshipMetric : PercentageMetric
//    {
//        private readonly IMetricProviderResolver providerResolver;

//        public StewardshipMetric(IMetricProviderResolver providerResolver)
//        {
//            this.providerResolver = providerResolver;
//        }

//        public override short ValueSize => sizeof(ushort);

//        public override Guid Id { get; } = new Guid("{3279DBF5-89AD-4A2C-97FC-1966B3A67367}");

//        public override string[] Categories { get; } = { MetricCategories.DataQuality };

//        protected override PercentageMetricValue CalculatePct(
//            MetricsProcessingContext context,
//            IMetricDimension dimension,
//            IMetricValues<short> existingMetricValues,
//            Entity entity)
//        {
//            if (context == null)
//                throw new ArgumentNullException(nameof(context));

//            if (dimension == null)
//                throw new ArgumentNullException(nameof(dimension));

//            if (existingMetricValues == null)
//                throw new ArgumentNullException(nameof(existingMetricValues));

//            if (dimension.DimensionType.HasFlag(MetricDimensionType.Entity))
//            {
//                if (entity == null)
//                    throw new ArgumentNullException();

//                var hasProviderDefinition = dimension.ProviderDefinitionId.HasValue;
//                var hasProvider = dimension.ProviderId.HasValue;

//                // Entity Integration Dimension
//                if (hasProviderDefinition && hasProvider)
//                {
//                    var connectivity = CalculateMetricValue(entity);

//                    return new PercentageMetricValue(dimension, entity.Id, connectivity);
//                }

//                // Entity Integration Type
//                if (hasProvider)
//                {
//                    var connectivity = CalculateMetricValue(entity);

//                    return new PercentageMetricValue(dimension, entity.Id, connectivity);
//                }

//                // Entity
//                if (!hasProviderDefinition)
//                {
//                    var connectivity = CalculateMetricValue(entity);

//                    return new PercentageMetricValue(dimension, entity.Id, connectivity);
//                }
//            }

//            // Global
//            else if (dimension.DimensionType.HasFlag(MetricDimensionType.Global))
//            {
//                var dateDimension = MetricDateDimension.Today;

//                short average;
//                string explanation;

//                switch (dimension.DimensionType)
//                {
//                    case MetricDimensionType.Global:
//                        average = (short)existingMetricValues.Average(this, v => v.Dimension.DimensionType == MetricDimensionType.Entity);
//                        explanation = this.GetAggregatedValueExplanation(context, MetricDimensionType.Entity);
//                        break;

//                    case MetricDimensionType.GlobalIntegration:
//                        average = (short)existingMetricValues.Average(this, v => v.Dimension.DimensionType == MetricDimensionType.EntityIntegration && v.Dimension.ProviderId == dimension.ProviderId);
//                        explanation = this.GetAggregatedValueExplanation(context, MetricDimensionType.EntityIntegration, null, dimension.ProviderId);
//                        break;

//                    case MetricDimensionType.GlobalIntegrationType:
//                        average = (short)existingMetricValues.Average(this, v => v.Dimension.DimensionType == MetricDimensionType.EntityIntegrationType && v.Dimension.ProviderDefinitionId == dimension.ProviderDefinitionId);
//                        explanation = this.GetAggregatedValueExplanation(context, MetricDimensionType.EntityIntegrationType, null, null, dimension.ProviderDefinitionId);
//                        break;

//                    default:
//                        throw new Exception();
//                }

//                return new PercentageMetricValue(dimension, dateDimension, average).WithExplanation(explanation);
//            }

//            throw new Exception();
//        }

//        public override bool ShouldPersist(IMetricDimension dimension)
//        {
//            throw new NotImplementedException();
//        }

//        public override IEnumerable<IMetricDimension> GetDimensions(MetricsProcessingContext context, IMetricsModel model)
//        {
//            /*
//             *                                                                    Dimension Table:
//             *
//             *                                                                  | DimensionType         | DetailType | ProviderDefinitionId | ProviderId | Detail        | Persistence                       |
//             *                                                                  |-----------------------|------------|----------------------|------------|---------------|-----------------------------------|
//             * Global                                                           | Global                |            |                      |            |               |                                   |
//             *  │  Global Provider                                              | GlobalIntegrationType |            |                      | Id         |               |                                   |
//             *  │   │  Global Provider Definition                               | GlobalIntegration     |            | Id                   | Id         |               |                                   |
//             *  │   │   │                                                       |-----------------------|------------|----------------------|------------|---------------|-----------------------------------|
//             *  └───│───│── Entity                                              | Entity                |            |                      |            |               | Blob, Graph, Search, EntityMetric |
//             *      └───│── Entity Provider                                     | EntityIntegrationType |            |                      | Id         |               | EntityMetric                      |
//             *          └── Entity Provider Definition                          | EntityIntegration     |            | Id                   | Id         |               | EntityMetric                      |
//             *
//             */


//            var existingDimensions = model.MetricDimensions.Where(d => d.MetricId == this.Id);

//            var entityIntegrationDimensions = existingDimensions.Where(d => d.DimensionType == MetricDimensionType.EntityIntegration && d.DimensionDetailType == MetricDimensionDetailType.None && d.ProviderDefinitionId.HasValue && d.ProviderId.HasValue);

//            if (entityIntegrationDimensions.Any())
//            {
//                foreach (var entityDimension in entityIntegrationDimensions)
//                    yield return this.GetDefaultGlobalDimension(context, entityDimension.ProviderDefinitionId, entityDimension.ProviderId);
//            }

//            var entityIntegrationTypeDimensions = existingDimensions.Where(d => d.DimensionType == MetricDimensionType.EntityIntegrationType && d.DimensionDetailType == MetricDimensionDetailType.None && d.ProviderId.HasValue);

//            if (entityIntegrationTypeDimensions.Any())
//            {
//                foreach (var entityDimension in entityIntegrationTypeDimensions)
//                    yield return this.GetDefaultGlobalDimension(context, entityDimension.ProviderId);
//            }

//            if (existingDimensions.Any(d => d.DimensionType == MetricDimensionType.Entity))
//                yield return this.GetDefaultGlobalDimension(context);
//        }

//        public override IEnumerable<IMetricDimension> GetDimensionsToCalculate(MetricsProcessingContext context, Entity entity)
//        {
//            /*
//             *                                                                    Dimension Table:
//             *
//             *                                                                  | DimensionType         | DetailType | ProviderDefinitionId | ProviderId | Detail        | Persistence                       |
//             *                                                                  |-----------------------|------------|----------------------|------------|---------------|-----------------------------------|
//             * Global                                                           | Global                |            |                      |            |               |                                   |
//             *  │  Global Provider                                              | GlobalIntegrationType |            |                      | Id         |               |                                   |
//             *  │   │  Global Provider Definition                               | GlobalIntegration     |            | Id                   | Id         |               |                                   |
//             *  │   │   │                                                       |-----------------------|------------|----------------------|------------|---------------|-----------------------------------|
//             *  └───│───│── Entity                                              | Entity                |            |                      |            |               | Blob, Graph, Search, EntityMetric |
//             *      └───│── Entity Provider                                     | EntityIntegrationType |            |                      | Id         |               | EntityMetric                      |
//             *          └── Entity Provider Definition                          | EntityIntegration     |            | Id                   | Id         |               | EntityMetric                      |
//             *
//             */

//            // Provider Definition
//            foreach (var group in entity.Details.DataEntries.GroupBy(d => d.OriginProviderDefinitionId))
//            {
//                if (group.Key == null)
//                    continue;

//                var providerDefinition = context.Organization.Providers.GetProviderDefinition(context, group.Key.Value);

//                if (providerDefinition == null)
//                    break;

//                var globalDimension = this.GetDefaultGlobalDimension(context, group.Key, providerDefinition.ProviderId);
//                yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.EntityIntegration, MetricDimensionDetailType.None, providerDefinitionId: group.Key, providerId: providerDefinition.ProviderId, persistence: MetricDimensionPersistence.EntityMetric);
//            }

//            // Provider 
//            foreach (var group in entity.Details.DataEntries.Where(d => d.OriginProviderDefinitionId.HasValue)
//                .GroupBy(d => this.providerResolver.ResolveProvider(context, d.OriginProviderDefinitionId.Value)))
//            {
//                if (group.Key == null)
//                    continue;

//                var globalDimension = this.GetDefaultGlobalDimension(context, group.Key.Id);
//                yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.EntityIntegrationType, MetricDimensionDetailType.None, providerId: group.Key.Id, persistence: MetricDimensionPersistence.EntityMetric);
//            }

//            // Entity
//            {
//                var globalDimension = this.GetDefaultGlobalDimension(context);
//                yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.Entity, MetricDimensionDetailType.None, persistence: MetricDimensionPersistence.Blob | MetricDimensionPersistence.Graph | MetricDimensionPersistence.Search | MetricDimensionPersistence.EntityMetric);
//            }
//        }


//        private IEnumerable<EntityEdge> FilterEdges(IEnumerable<EntityEdge> edges, Func<EntityEdge, EntityReference> getReferencePoint)
//        {
//            return edges.Where(e => !getReferencePoint(e).Type.Is(EntityType.Temporal)
//                                 && !getReferencePoint(e).Type.Is(EntityType.Provider.Root));
//        }

//        private static double CalculateMetricValue(Entity entity)
//        {
//            //TODO: How many Data Parts where source is CluedIn Clean is present compared to the number that is not. 

//            var parts = entity.Details.DataEntries.Where(v => v != null);

//            var processor = (IClueToEntityMappingProcessor)new ClueToEntityMappingProcessor();

//            parts = processor.GetOrderedDataParts(parts);

//            var userInputCount = parts.Count(d => d.Attributes.ContainsKey("inputSource") && EntityUserInputSources.IsUserInputSource(d.Attributes["inputSource"]));
//            var cleanInputCount = parts.Count(d => d.Attributes.ContainsKey("inputSource") && EntityUserInputSources.IsUserInputSource(d.Attributes["inputSource"]));

//            var totalCounts = parts.Count();

//            var returnValue = (userInputCount + cleanInputCount) / (10f + ( userInputCount - cleanInputCount));

//            return returnValue;
//        }

//        private string GetExplanation(MetricsProcessingContext context, (IEnumerable<EntityEdge> outgoingEdges, IEnumerable<EntityEdge> incomingEdges) map)
//        {
//            if (!context.MetricsExecutionOptions.HasFlag(MetricsExecutionOption.Explanation))
//                return null;

//            var sb = new StringBuilder();

//            sb.AppendLine($"outgoingEdges count: {map.outgoingEdges.Distinct().Count()}");
//            sb.AppendLine($"incomingEdges count: {map.incomingEdges.Distinct().Count()}");

//            return sb.ToString();
//        }
//    }
//}
