using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.DataStore.Entities;
using CluedIn.Core.Metrics;
using CluedIn.Core.Processing;
using CluedIn.Metrics;
using CluedIn.Processing;

namespace CluedIn.Custom.Metrics.Implementations
{
    public class FlexibilityMetric : PercentageMetric
    {
        private readonly IMetricProviderResolver providerResolver;

        public FlexibilityMetric(IMetricProviderResolver providerResolver)
        {
            this.providerResolver = providerResolver;
        }

        public override short ValueSize => sizeof(ushort);

        public override Guid Id { get; } = new Guid("{6E72202B-83DE-4C3B-8FA7-CB8B1DFA826E}");

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
                    var map = this.GetMergedData(entity, v => v.OriginProviderDefinitionId == dimension.ProviderDefinitionId.Value);
                    var connectivity = CalculateMetricValue(context, entity);

                    return new PercentageMetricValue(dimension, entity.Id, connectivity).WithExplanation(this.GetExplanation(context, map));
                }

                // Entity Integration Type
                if (hasProvider)
                {
                    var map = this.GetMergedData(entity, v => this.providerResolver.ResolveProvider(context, v.OriginProviderDefinitionId)?.Id == dimension.ProviderId.Value);
                    var connectivity = CalculateMetricValue(context, entity);

                    return new PercentageMetricValue(dimension, entity.Id, connectivity).WithExplanation(this.GetExplanation(context, map));
                }

                // Entity
                if (!hasProviderDefinition)
                {
                    var map = (entity.OutgoingEdges, entity.IncomingEdges);
                    var connectivity = CalculateMetricValue(context, entity);

                    return new PercentageMetricValue(dimension, entity.Id, connectivity).WithExplanation(this.GetExplanation(context, map));
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

                var providerDefinition = context.Organization.Providers.GetProviderDefinition(context, group.Key.Value);

                if (providerDefinition == null)
                    break;

                var globalDimension = this.GetDefaultGlobalDimension(context, group.Key, providerDefinition.ProviderId);
                yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.EntityIntegration, MetricDimensionDetailType.None, providerDefinitionId: group.Key, providerId: providerDefinition.ProviderId, persistence: MetricDimensionPersistence.EntityMetric);
            }

            // Provider 
            foreach (var group in entity.Details.DataEntries.Where(d => d.OriginProviderDefinitionId.HasValue)
                .GroupBy(d => this.providerResolver.ResolveProvider(context, d.OriginProviderDefinitionId.Value)))
            {
                if (group.Key == null)
                    continue;

                var globalDimension = this.GetDefaultGlobalDimension(context, group.Key.Id);
                yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.EntityIntegrationType, MetricDimensionDetailType.None, providerId: group.Key.Id, persistence: MetricDimensionPersistence.EntityMetric);
            }

            // Entity
            {
                var globalDimension = this.GetDefaultGlobalDimension(context);
                yield return new MetricDimension(context, this, globalDimension, MetricDimensionType.Entity, MetricDimensionDetailType.None, persistence: MetricDimensionPersistence.Blob | MetricDimensionPersistence.Graph | MetricDimensionPersistence.Search | MetricDimensionPersistence.EntityMetric);
            }
        }

        private (IEnumerable<EntityEdge> outgoingEdges, IEnumerable<EntityEdge> incomingEdges) GetMergedData(Entity entity, Func<IDataPart, bool> filter)
        {
            var parts = entity.Details.DataEntries.Where(filter);

            var processor = (IClueToEntityMappingProcessor)new ClueToEntityMappingProcessor();

            parts = processor.GetOrderedDataParts(parts);

            // Enable to get accurate merged list of edges
            //var parent   = entity.ProcessedData;
            //var outgoing = new EntityEdgeCollection(parent, edge => edge.FromReference, edge => edge.ToReference, new VersionedCollection<EntityEdge>(parent, new HashSet<EntityEdge>()), () => null);
            //var incoming = new EntityEdgeCollection(parent, edge => edge.ToReference, edge => edge.FromReference, new VersionedCollection<EntityEdge>(parent, new HashSet<EntityEdge>()), () => null);

            IEnumerable<EntityEdge> outgoing = new EntityEdge[0];
            IEnumerable<EntityEdge> incoming = new EntityEdge[0];

            foreach (var dataPart in parts)
            {
                // Enable to get accurate merged list of edges
                //outgoing.AddOnlyValid(this.FilterEdges(dataPart.ProcessedEntityData.OutgoingEdges, edge => edge.ToReference));
                //incoming.AddOnlyValid(this.FilterEdges(dataPart.ProcessedEntityData.IncomingEdges, edge => edge.FromReference));

                outgoing = outgoing.Concat(this.FilterEdges(dataPart.ProcessedEntityData.OutgoingEdges, edge => edge.ToReference));
                incoming = incoming.Concat(this.FilterEdges(dataPart.ProcessedEntityData.IncomingEdges, edge => edge.FromReference));
            }

            return (outgoing, incoming);
        }

        private IEnumerable<EntityEdge> FilterEdges(IEnumerable<EntityEdge> edges, Func<EntityEdge, EntityReference> getReferencePoint)
        {
            return edges.Where(e => !getReferencePoint(e).Type.Is(EntityType.Temporal)
                                 && !getReferencePoint(e).Type.Is(EntityType.Provider.Root));
        }

        private static double CalculateMetricValue(MetricsProcessingContext context, Entity entity)
        {
            //Go get all streams. Start to calculate how many streams are of different types (e.g. BI, Machine Learning) and the mroe variery, the higher the score. 

            var dbContext = context.ApplicationContext.Container.Resolve<CluedInEntities>();

            double cc = dbContext.StreamIngestionLog.Where(i => i.EntityId == entity.Id).Select(g => g.StreamId).Distinct().Count();
            var defs = dbContext.Streams.Select(i => dbContext.OrganizationProviders.Single(r => r.Id == i.ConnectorProviderDefinitionId));
            var blah = defs.Select(g => g.ProviderInfo);
            var maxCount = blah.Select(r => r.Category).Count();
            return (cc / maxCount);
        }

        private string GetExplanation(MetricsProcessingContext context, (IEnumerable<EntityEdge> outgoingEdges, IEnumerable<EntityEdge> incomingEdges) map)
        {
            if (!context.MetricsExecutionOptions.HasFlag(MetricsExecutionOption.Explanation))
                return null;

            var sb = new StringBuilder();

            sb.AppendLine($"outgoingEdges count: {map.outgoingEdges.Distinct().Count()}");
            sb.AppendLine($"incomingEdges count: {map.incomingEdges.Distinct().Count()}");

            return sb.ToString();
        }
    }
}
