// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoPropertiesMetricsTests.cs" company="Clued In">
//   Copyright (c) 2019 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the no properties metrics tests class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using CluedIn.CluedIn.Custom.Metrics.Tests.Infrastructure;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Serialization;
using CluedIn.Custom.Metrics.Implementations;
using CluedIn.Metrics;
using Xunit;
using Xunit.Abstractions;

namespace CluedIn.CluedIn.Custom.Metrics.Tests.EntityTests
{
    public class PhEntityMetricsTestsFixture : TestContextFixture
    {
        public PhEntityMetricsTestsFixture()
        {
            var serializer  = new XmlSerializer(this.TestContext.Context, SerializationFlavor.Persisting);
            this.Entity = serializer.Deserialize<Entity>(File.ReadAllText("TestData/Metrics/ph.entity.xml"));
        }

        public Entity Entity { get; set; }
    }

    public class PhEntityMetricsTests : MetricsImplementationTestsBase, IClassFixture<PhEntityMetricsTestsFixture>
    {
        private readonly PhEntityMetricsTestsFixture fixture;

        private readonly ITestOutputHelper outputHelper;

        private Entity entity;

        public PhEntityMetricsTests(PhEntityMetricsTestsFixture fixture, ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            this.fixture      = fixture;
            this.outputHelper = outputHelper;

            this.entity = this.fixture.Entity;
        }

        [Fact]
        public void LookupTest()
        {
            var data1 = new List<string>() { "Phil" };
            var data2 = new List<string>() { "Phil" };

            var data = new List<List<string>>() { data1, data2 };

            var groups = data.GroupBy(v => v[0]);

            var lookup = groups.ToLookup(g => g.Key);

            this.outputHelper.WriteLine(lookup["Phil"].Sum(g => g.Count()).ToString());
        }
        
        [Fact]
        public override void UniformityMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new UniformityMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0.5834f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void StewardshipMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new StewardshipMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void InterpretabilityMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new InterpretabilityMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0.2975f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact(Skip = "Requires provider vocabularies")]
        public override void ComplexityMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new ComplexityMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0.388f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void DarkDataMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new DarkDataMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void TimelinessMetric()
        {
            var (globalMetric, _) = this.TestMetric(new TimelinessMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void NoiseMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new NoiseMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void FlexibilityMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new FlexibilityMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void OrderlinessMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new OrderlinessMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void SparsityMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new SparsityMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void ReliabilityMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new ReliabilityMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void UsabilityMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new UsabilityMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }

        [Fact]
        public override void StalenessMetricTest()
        {
            var (globalMetric, _) = this.TestMetric(new StalenessMetric(this.ProviderResolver), this.entity);

            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
        }
    }
}
