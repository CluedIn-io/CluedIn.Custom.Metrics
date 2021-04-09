//// --------------------------------------------------------------------------------------------------------------------
//// <copyright file="MetricsImplementationTests.cs" company="Clued In">
////   Copyright (c) 2019 Clued In. All rights reserved.
//// </copyright>
//// <summary>
////   Implements the metrics implementation tests class.
//// </summary>
//// --------------------------------------------------------------------------------------------------------------------

//using System.IO;
//using CluedIn.CluedIn.Custom.Metrics.Tests.Infrastructure;
//using CluedIn.Core.Data;
//using CluedIn.Core.Data.Serialization;
//using CluedIn.Custom.Metrics.Implementations;
//using CluedIn.Metrics;
//using Xunit;
//using Xunit.Abstractions;

//namespace CluedIn.CluedIn.Custom.Metrics.Tests
//{
//    public class MetricsImplementationTestsFixture : TestContextFixture
//    {
//        public MetricsImplementationTestsFixture()
//        {
//            var serializer = new XmlSerializer(this.TestContext.Context, SerializationFlavor.Persisting);
//            this.Entity = serializer.Deserialize<Entity>(File.ReadAllText("TestData/Metrics/ann.entity.xml"));
//        }

//        public Entity Entity { get; set; }
//    }

//    public class MetricsImplementationTests : MetricsImplementationTestsBase, IClassFixture<MetricsImplementationTestsFixture>
//    {
//        private readonly MetricsImplementationTestsFixture fixture;

//        private readonly ITestOutputHelper outputHelper;

//        private readonly Entity entity;

//        public MetricsImplementationTests(MetricsImplementationTestsFixture fixture, ITestOutputHelper outputHelper)
//            : base(outputHelper)
//        {
//            this.fixture = fixture;
//            this.outputHelper = outputHelper;

//            this.entity = this.fixture.Entity;
//        }


//        [Fact]
//        public override void UniformityMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new UniformityMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0.5652f, ((Percentage)globalMetric).ToFloat());
//        }


//        [Fact]
//        public override void StewardshipMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new StewardshipMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0.2f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact]
//        public override void StalenessMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new StalenessMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(1f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact]
//        public override void SparsityMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new SparsityMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0.9166f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact]
//        public override void ReliabilityMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new ReliabilityMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact]
//        public override void OrderlinessMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new OrderlinessMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0.0263f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact]
//        public override void NoiseMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new NoiseMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact]
//        public override void InterpretabilityMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new InterpretabilityMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0.388f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact(Skip = "Requires provider vocabularies")]
//        public override void FlexibilityMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new FlexibilityMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0.004f, ((Percentage)globalMetric).ToFloat());
//        }


//        [Fact]
//        public override void DarkDataMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new DarkDataMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact]
//        public override void ComplexityMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new ComplexityMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(1f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact(Skip = "System.InvalidOperationException : Nullable object must have a value")]
//        public override void UsabilityMetricTest()
//        {
//            var (globalMetric, _) = this.TestMetric(new UsabilityMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0.0277f, ((Percentage)globalMetric).ToFloat());
//        }

//        [Fact]
//        public override void TimelinessMetric()
//        {
//            var (globalMetric, _) = this.TestMetric(new TimelinessMetric(this.ProviderResolver), this.entity);

//            Assert.Equal(0.5f, ((Percentage)globalMetric).ToFloat());
//        }
//    }
//}
