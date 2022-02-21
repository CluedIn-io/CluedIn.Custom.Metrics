// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricTestBase.cs" company="Clued In">
//   Copyright (c) 2019 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the metric test base class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using CluedIn.CluedIn.Custom.Metrics.Tests.Infrastructure;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.Core.DataStore;
using CluedIn.Core.DataStore.Entities;
using CluedIn.Core.Diagnostics;
using CluedIn.Core.Metrics;
using CluedIn.Core.Processing;
using CluedIn.Core.Providers;
using CluedIn.Metrics;
using CluedIn.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace CluedIn.CluedIn.Custom.Metrics.Tests
{
    public class MetricTestBase
    {
        private ITestOutputHelper outputHelper;
        protected IMetricProviderResolver ProviderResolver = new MetricProviderResolver();

        protected MetricTestBase(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        public void Setup(TestContext testContext)
        {
            testContext.Logger = Mock.Of<ILogger>();
            SetupContext(testContext);
        }

        public static void SetupContext(TestContext testContext)
        {
            var proxyGenerator                      = new ProxyGenerator();
            var providerDefinitions                 = new Mock<IRelationalDataStore<ProviderDefinition>>();
            var organizationSpecificProviderInfo    = new Mock<IRelationalDataStore<OrganizationSpecificProviderInfo>>();
            var providerInfo                        = new Mock<IRelationalDataStore<ProviderInfo>>();
            var providerOwners                      = new Mock<IRelationalDataStore<OrganizationProviderOwner>>();

            var provider1 = new DummyProvider(testContext.Context.ApplicationContext, new ProviderMetadata() { Id = new Guid("{9E4CF6A2-82BD-4FFF-9CBB-3FA697AFCB2B}") });
            var provider2 = new DummyProvider(testContext.Context.ApplicationContext, new ProviderMetadata() { Id = new Guid("{1CCCDC20-82E2-49BA-972D-BE7D363CD435}") });
            var provider3 = new DummyProvider(testContext.Context.ApplicationContext, new ProviderMetadata() { Id = new Guid("{20F958AD-62D2-4138-91DC-3BC3FAC466C9}") });

            var providers = new List<IProvider>() { provider1, provider2, provider3 };
            var providerRanges = providers.Select(p => new ProviderRange { Provider = p, Range = new Range<int>(-1, -1) }).ToList();

            providerRanges.Aggregate(int.MinValue, (acc, p) =>
                            {
                                p.Range = new Range<int>(acc, acc + (2 * (int.MaxValue / providers.Count)));
                                return p.Range.Maximum + 1;
                            });

            providerRanges.Last().Range.Maximum = int.MaxValue;

            var streamLog = new StreamIngestionLog() {
                EntityId = new Guid("{afa55936-eace-5462-b4d0-c331edb465f5}"),
                StreamId = new Guid("{0B3183E6-EAB0-44DD-95FD-139C3EC03562}"),
                DateSent = DateTime.Parse("2019-05-20T04:19:38.2705147+00:00")
            };

            var streamLog2 = new StreamIngestionLog() {
                EntityId = new Guid("{afa55936-eace-5462-b4d0-c331edb465f5}"),
                StreamId = new Guid("{5242C262-C465-4B62-9C68-C09ECE9C65AD}"),
                DateSent = DateTime.Parse("2020-05-20T04:20:38.2705147+00:00")

            };

            IProvider GetProvider(Guid providerDefinitionId)
            {
                var hash = providerDefinitionId.GetHashCode();

                var provider = providerRanges.First(p => p.Range.Contains(hash));

                return provider.Provider;
            }

            var options = new DbContextOptionsBuilder<DbContext>()
             .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
             .Options;

            testContext.Container.Register(Component.For<DbContextOptions<DbContext>>().UsingFactoryMethod(() => options).OnlyNewServices());
            testContext.Container.Register(
                Component.For<CluedInEntities>()
                    .UsingFactoryMethod(() => new CluedInEntities(options))
                    .OnlyNewServices());

            var dbContext = testContext.Container.Resolve<CluedInEntities>();
            dbContext.StreamIngestionLog.Add(streamLog);
            dbContext.StreamIngestionLog.Add(streamLog2);


            dbContext.Streams.Add(new CluedInStream());
            dbContext.Streams.Add(new CluedInStream());
            dbContext.Streams.Add(new CluedInStream());
            dbContext.Streams.Add(new CluedInStream());

            dbContext.SaveChanges();

            providerDefinitions.Setup(d => d.GetById(It.IsAny<ExecutionContext>(), It.IsAny<Guid>()))
                               .Returns<ExecutionContext, Guid>((ctx, id) => new ProviderDefinition() { Id = id, ProviderId = GetProvider(id).Id, Context = testContext.Context.ApplicationContext });

            providerInfo.Setup(d => d.GetById(It.IsAny<ExecutionContext>(), It.IsAny<Guid>()))
                        .Returns<ExecutionContext, Guid>((ctx, id) => new ProviderInfo() { Id = id, IsEnabled = true, Context = testContext.Context.ApplicationContext });

            providerInfo.Setup(d => d.Select(It.IsAny<ExecutionContext>(), It.IsAny<Expression<Func<ProviderInfo, bool>>>()))
                        .Returns<ExecutionContext, Expression<Func<ProviderInfo, bool>>>((ctx, predicate) => providers.Select(p => new ProviderInfo() { Id = p.Id, IsEnabled = true, Context = testContext.Context.ApplicationContext }));


            testContext.Container.Register(Component.For<IRelationalDataStore<OrganizationProviderOwner>>().UsingFactoryMethod(() => proxyGenerator.CreateInterfaceProxyWithTarget(providerOwners.Object)));


            testContext.Container.Register(Component.For<IRelationalDataStore<ProviderDefinition>>().UsingFactoryMethod(() => proxyGenerator.CreateInterfaceProxyWithTarget(providerDefinitions.Object)));
            testContext.Container.Register(Component.For<IRelationalDataStore<OrganizationSpecificProviderInfo>>().UsingFactoryMethod(() => proxyGenerator.CreateInterfaceProxyWithTarget(organizationSpecificProviderInfo.Object)));
            testContext.Container.Register(Component.For<IRelationalDataStore<ProviderInfo>>().UsingFactoryMethod(() => proxyGenerator.CreateInterfaceProxyWithTarget(providerInfo.Object)));
            testContext.Container.Register(providers.Select(p => Component.For<IProvider>().Named(p.Id.ToString()).Forward<IProvider>().Instance(p)).ToArray());
            testContext.Container.Register(Types.FromAssemblyInDirectory(new AssemblyFilter(".", "CluedIn.*")).BasedOn<IVocabulary>().WithServiceFromInterface().If(t => !t.IsAbstract && !t.IsGenericTypeDefinition));

        }

        private class ProviderRange 
        {
            public IProvider Provider;
            public Range<int> Range;
        }

        public (T? globalMetricValue, MetricsModel<T> model) TestMetric<T>(Metric<T> metric, params Entity[] entities)
            where T : struct
        {
            return this.TestMetric(metric, globalMetricValues => globalMetricValues.SingleOrDefault(v => v.Dimension.DimensionType == MetricDimensionType.Global), entities);
        }

        public (T? globalMetricValue, MetricsModel<T> model) TestMetric<T>(Metric<T> metric, Func<MetricValueTable<T>, IMetricValue<T>> getGlobalMetricValueFunc, params Entity[] entities)
            where T : struct
        {
            // Setup
            var testContext = new TestContext();
            this.Setup(testContext);

            var (allDimensions, model, allValuesModel, globalMetricValues, globalDimensions) = CalculateMetric(testContext, metric, entities);

            var globalMetricValue = getGlobalMetricValueFunc(globalMetricValues);

            {
                this.DebugPrint(entities, model, allDimensions, allValuesModel, globalDimensions, globalMetricValue);

                this.DebugPrintStorageCostEstimation(metric, allDimensions);
            }

            return (globalMetricValue?.Value, model);
        }

        public static (HashSet<IMetricDimension> allDimensions, 
                        MetricsModel<T> model, 
                        MetricsModel<T> allValuesModel, 
                        MetricValueTable<T> globalMetricValues, 
                        IEnumerable<IMetricDimension> globalDimensions) CalculateMetric<T>(
                TestContext testContext,
                Metric<T> metric,
                Entity[] entities)
            where T : struct
        {
            var allDimensions = new HashSet<IMetricDimension>();

            var model = new MetricsModel<T>();
            var allValuesModel = new MetricsModel<T>();

            // Calculate Entity Metrics
            foreach (var entity in entities)
            {
                using (var context = testContext.Context.ApplicationContext.CreateExecutionContext(entity.OrganizationId).ToMetricsProcessingContext().WithExecutionOption(MetricsExecutionOption.Explanation))
                {
                    var existingMetricValues = new MetricValueTable<T>();
                    IEnumerable<IMetricDimension> dimensions;

                    using (new SimpleTimer(context, "Calculate Metrics"))
                    {
                        // Calculate which dimensions is available for the specified entity
                        dimensions = metric.GetDimensionsToCalculate(context, entity).ToList();

                        Assert.Equal(dimensions.Count(), dimensions.DistinctBy(d => d.Guid).Count());

                        foreach (var dimension in dimensions)
                        {
                            // Calculate value for dimension
                            //  - Note values can be calculated based on previous dimension values

                            var value = metric.Calculate(context, dimension, existingMetricValues, entity);

                            if (value != null)
                                existingMetricValues.Add(value);
                        }
                    }

                    Assert.Equal(existingMetricValues.Count(), existingMetricValues.Distinct().Count());
                    Assert.Equal(
                        existingMetricValues.Count(),
                        existingMetricValues.DistinctBy(v => v.EntityId + "|" + v.Dimension.Guid).Count());

                    // Persist Entity Metrics
                    //  - Simulate metric values being persisted to the EntityMetricValue table (a metric value table will be created pr metric value type, ie. percentages, integers etc..)
                    {
                        model.Add(metric);
                        allValuesModel.Add(metric);

                        foreach (var dimension in dimensions.Where(d => d.Persistence != MetricDimensionPersistence.None))
                            model.Add(dimension);

                        foreach (var dimension in dimensions)
                            allValuesModel.Add(dimension);

                        foreach (var value in existingMetricValues.Where(
                            v => v.Dimension.Persistence.HasFlag(MetricDimensionPersistence.EntityMetric)))
                            model.Add(value);

                        foreach (var value in existingMetricValues)
                            allValuesModel.Add(value);
                    }

                    allDimensions.AddRange(dimensions);
                }
            }

            // Calculate Global Metrics
            //  - Simulate batch job to calculate global metric values - global values and historic metric values will be stored in the MetricValueHistory table
            var globalMetricValues = new MetricValueTable<T>();

            IEnumerable<IMetricDimension> globalDimensions;

            using (var context = testContext.Context.ApplicationContext.CreateExecutionContext(entities.First().OrganizationId)
                .ToMetricsProcessingContext())
            using (new SimpleTimer(context, "Calculate Global Metrics"))
            {
                globalDimensions = metric.GetDimensions(context, model);

                foreach (var dimension in globalDimensions)
                {
                    var value = metric.Calculate(context, dimension, model.MetricValues, null);

                    if (value != null)
                        globalMetricValues.Add(value);
                }
            }

            // Persist Global Metrics in history table
            {
                // TODO
            }

            return (allDimensions, model, allValuesModel, globalMetricValues, globalDimensions);
        }

        private void DebugPrint<T>(
            Entity[] entities,
            MetricsModel<T> model,
            HashSet<IMetricDimension> allDimensions,
            MetricsModel<T> allValuesModel,
            IEnumerable<IMetricDimension> globalDimensions,
            IMetricValue<T> globalMetricValue)
            where T : struct
        {
            // Debug output
            this.outputHelper.WriteLine(string.Empty);
            this.outputHelper.WriteLine($"Entity Dimension Count: {allDimensions.Count()}");
            this.outputHelper.WriteLine($"Entity Dimension to Persist: {allDimensions.Count(d => d.Persistence != MetricDimensionPersistence.None)}");

            this.outputHelper.WriteLine(string.Empty);
            this.outputHelper.WriteLine($"Global Dimension Count: {globalDimensions.Count()}");
            this.outputHelper.WriteLine($"Global Dimension to Persist: {globalDimensions.Count(d => d.Persistence != MetricDimensionPersistence.None)}");

            this.outputHelper.WriteLine(string.Empty);
            this.outputHelper.WriteLine($"Global Metric Value: {globalMetricValue?.ToString()}");

            foreach (var entity in entities)
            {
                this.outputHelper.WriteLine($"Entity Metric Value: {model.MetricValues.Queryable.SingleOrDefault(v => v.Dimension.DimensionType == MetricDimensionType.Entity && v.Dimension.DimensionDetailType == MetricDimensionDetailType.None && v.EntityId == entity.Id)?.ToString()}");
                this.outputHelper.WriteLine(string.Empty);

                var valueDimensionFilters =
                    new
                    List<(string label, Expression<Func<IMetricValue<T>, bool>> filter, Expression<Func<IMetricValue<T>, bool>>
                        groupFilter, Func<IMetricValue<T>, string> writeValue)>()
                    {
                        ("Entity Value",
                            v => v.Dimension.DimensionType == MetricDimensionType.Entity
                                 && v.Dimension.DimensionDetailType == MetricDimensionDetailType.None
                                 && v.EntityId == entity.Id, v => v.Explanation != null,
                            v => $"Entity Metric Value - {v.Dimension.ProviderId} : {v}"),
                        ("Entity Integration Type Value",
                            v => v.Dimension.DimensionType == MetricDimensionType.EntityIntegrationType
                                 && v.Dimension.DimensionDetailType == MetricDimensionDetailType.None
                                 && v.EntityId == entity.Id, v => v.Explanation != null,
                            v => $"Entity Integration Type Metric Value - {v.Dimension.ProviderId} : {v}"),
                        ("Entity Integration Value",
                            v => v.Dimension.DimensionType == MetricDimensionType.EntityIntegration
                                 && v.Dimension.DimensionDetailType == MetricDimensionDetailType.None
                                 && v.EntityId == entity.Id, v => v.Explanation != null,
                            v =>
                                $"Entity Integration Metric Value - {v.Dimension.ProviderDefinitionId}, {v.Dimension.ProviderId} : {v}"),
                        ("Entity Property Metric Value",
                            v => v.Dimension.DimensionType == MetricDimensionType.Entity
                                 && v.Dimension.DimensionDetailType == MetricDimensionDetailType.Property
                                 && v.EntityId == entity.Id, v => true,
                            v => $"Entity Property Metric Value - {v.Dimension.DimensionDetail} : {v}")
                    };

                foreach (var valueDimensionFilter in valueDimensionFilters)
                {
                    var values = allValuesModel.MetricValues.Queryable.Where(valueDimensionFilter.filter);

                    if (values.Any(valueDimensionFilter.groupFilter))
                    {
                        this.outputHelper.WriteLine(new string('#', 80));
                        this.outputHelper.WriteLine(valueDimensionFilter.label);
                        this.outputHelper.WriteLine(new string('#', 80));
                        this.outputHelper.WriteLine(string.Empty);

                        this.outputHelper.WriteLine($"{valueDimensionFilter.label} Count: {values.Count()}");


                        foreach (var v in values)
                        {
                            this.outputHelper.WriteLine(valueDimensionFilter.writeValue(v));

                            if (v.Explanation != null)
                                this.outputHelper.WriteLine(v.Explanation);
                        }
                    }
                }
            }

            // outputHelper.WriteLine(string.Empty);
            // this.DebugPrint(dimensions);

            // outputHelper.WriteLine(string.Empty);
            // this.DebugPrint(globalDimensions);
        }

        private void DebugPrint(IEnumerable<MetricDimension> dimensions)
        {
            // | DimensionType         | DetailType | ProviderDefinitionId | ProviderId | Detail        | Persistence                       |
            foreach (var dimension in dimensions)
            {
                this.outputHelper.WriteLine($"DimensionType: {dimension.DimensionType};\t DetailType: {dimension.DimensionDetailType};\t ProviderDefinitionId: {dimension.ProviderDefinitionId};\t ProviderId: {dimension.ProviderId};\t Detail: {dimension.DimensionDetail};\t Persistence: {dimension.Persistence};");
            }

            this.outputHelper.WriteLine(string.Empty);
            this.outputHelper.WriteLine($"Dimension Count: {dimensions.Count()}");
        }

        private void DebugPrintStorageCostEstimation<T>(Metric<T> metric, HashSet<IMetricDimension> allDimensions)
            where T : struct
        {
            this.outputHelper.WriteLine(string.Empty);
            var bytes = metric.EstimateStorageCosts(54000000, 50, 10, 3);
            var bytes2 = metric.EstimateStorageCosts(54000000, allDimensions);

            this.outputHelper.WriteLine($"EstimateStorageCosts (1 Metric): {bytes} b, {bytes / 1024} kb, {bytes / 1024 / 1024} mb, {bytes / 1024 / 1024 / 1024} gb");
            this.outputHelper.WriteLine($"EstimateStorageCosts (1 Metric): {bytes2} b, {bytes2 / 1024} kb, {bytes2 / 1024 / 1024} mb, {bytes2 / 1024 / 1024 / 1024} gb");

            bytes *= 20;
            bytes2 *= 20;

            this.outputHelper.WriteLine($"EstimateStorageCosts (20 Metrics): {bytes} b, {bytes / 1024} kb, {bytes / 1024 / 1024} mb, {bytes / 1024 / 1024 / 1024} gb, {bytes / 1024 / 1024 / 1024 / 1024} tb");
            this.outputHelper.WriteLine($"EstimateStorageCosts (20 Metrics): {bytes2} b, {bytes2 / 1024} kb, {bytes2 / 1024 / 1024} mb, {bytes2 / 1024 / 1024 / 1024} gb, {bytes2 / 1024 / 1024 / 1024 / 1024} tb");
        }

        public Entity ReprocessEntity(Entity entity)
        {
            using (var testContext = new TestContext())
            {
                testContext.Context.ApplicationContext.System.Configuration.ClueProcessing.QueueToBus.EdgeProcessing = true;

                MetricsTestUtility.SetupProcessingAndModelResolver(testContext);

                var entityClone = EntityPartBase.Copy(entity);

                var processor = new ClueToEntityMappingProcessor();

                processor.ReprocessEntityDataParts(testContext.Context.ToProcessingContext().WithExecutionOption(ExecutionOptions.Force), entityClone.Details.DataEntries);
                processor.MergeDataAndProcessEntityData(entityClone, testContext.Context.ToProcessingContext(), new DataStoreOperations());

                return entityClone;
            }
        }
    }
}
