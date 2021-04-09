//using System;
//using System.Collections.Generic;
//using System.Linq;
//using CluedIn.Core.Data;
//using CluedIn.Core.Metrics;
//using CluedIn.Core.Processing;
//using CluedIn.Metrics;
//using CluedIn.Processing;

//namespace CluedIn.Custom.Metrics.Implementations
//{
//    public class StalenessMetric : PercentageMetric
//    {
//        private readonly IMetricProviderResolver providerResolver;

//        public StalenessMetric(IMetricProviderResolver providerResolver)
//        {
//            this.providerResolver = providerResolver;
//        }

//        public override short ValueSize => sizeof(ushort);

//        public override Guid Id { get; } = new Guid("{066B48C6-A466-4E41-B091-C64CD8C1E593}");

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

   

//        private static double CalculateMetricValue(Entity entity)
//        {
//            //TODO: Calculate the Frequency of Modified Dates between Data Parts

//            var parts = entity.Details.DataEntries.Where(v => v != null);

//            var processor = (IClueToEntityMappingProcessor)new ClueToEntityMappingProcessor();

//            parts = processor.GetOrderedDataParts(parts);
//            var allModifiedDates = parts.Where(i => i.EntityData.ModifiedDate.HasValue);
//            var allRecentModifiedDates = parts.Where(i => i.EntityData.ModifiedDate.HasValue && i.EntityData.ModifiedDate >DateTimeOffset.UtcNow.AddDays(-30));

//            if (!allModifiedDates.Any())
//                return 0f;

//            return 1f - (allRecentModifiedDates.Count()  / allModifiedDates.Count());
//        }

//        public static IEnumerable<DateRange> FindGaps(IEnumerable<DateRange> baseCollection, IEnumerable<DateRange> testCollection)
//        {
//            var allBaseDates = baseCollection.SelectMany(o => o.GetDiscreetDates())
//                .Distinct()
//                .OrderBy(o => o.Ticks);

//            var missingInTest = (from d in allBaseDates
//                                 let inRange = testCollection.Any(o => d.IsInRange(o))
//                                 where !inRange
//                                 select d).ToArray();

//            var gaps = missingInTest.Select(o => new DateRange() { Start = o, End = o.AddDays(1) });

//            gaps = gaps.GroupConsecutive();

//            return gaps;

//        }
//    }

//    public class DateRange
//    {
//        protected bool Equals(DateRange other)
//        {
//            return Start.Equals(other.Start) && End.Equals(other.End);
//        }

//        public override int GetHashCode()
//        {
//            unchecked
//            {
//                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
//            }
//        }

//        public DateTime Start { get; set; }
//        public DateTime End { get; set; }

//        public IEnumerable<DateTime> GetDiscreetDates()
//        {
//            //Start is not allowed to equal end.
//            if (Start.Date == End.Date)
//                throw new ArgumentException("Start cannot equal end.");

//            var output = new List<DateTime>();

//            var current = Start.Date;

//            while (current < End.Date)
//            {
//                output.Add(current);
//                current = current.AddDays(1);
//            }

//            return output;
//        }

//        public override bool Equals(object obj)
//        {
//            if (ReferenceEquals(null, obj)) return false;
//            if (ReferenceEquals(this, obj)) return true;
//            if (obj.GetType() != this.GetType()) return false;
//            return Equals((DateRange)obj);
//        }
//    }

//    public static class Extensions
//    {
//        public static bool IsInRange(this DateTime testDate, DateRange range)
//        {
//            return range.Start <= testDate && range.End > testDate;
//        }

//        public static IEnumerable<DateRange> GroupConsecutive(this IEnumerable<DateRange> input)
//        {
//            var current = input.ToArray();
//            var nextIndex = 0;

//            //uses lookahead to figure out if gaps are consecutive.
//            for (var i = 0; i < current.Length - 1; i++)
//            {

//                //If the next range is consecutive to the current, skip;
//                if (!current[i].End.IsInRange(current[i + 1]))
//                {
//                    yield return new DateRange() {
//                        Start = current[nextIndex].Start,
//                        End = current[i].End
//                    };
//                    nextIndex = i + 1;
//                }
//            }

//            //If the last elements were consecutive, pull out the final item.
//            if (nextIndex != current.Length)
//            {
//                yield return new DateRange() {
//                    Start = current[nextIndex].Start,
//                    End = current[^1].End
//                };
//            }
//        }
//    }
//}
