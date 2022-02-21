// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricsImplementationTestsBase.cs" company="Clued In">
//   Copyright (c) 2019 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the metrics implementation tests base class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Xunit;
using Xunit.Abstractions;

namespace CluedIn.CluedIn.Custom.Metrics.Tests
{
    public abstract class MetricsImplementationTestsBase : MetricTestBase
    {
        private readonly ITestOutputHelper outputHelper;

        protected MetricsImplementationTestsBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public abstract void UniformityMetricTest();

        [Fact]
        public abstract void StewardshipMetricTest();

        [Fact]
        public abstract void StalenessMetricTest();

        [Fact]
        public abstract void UsabilityMetricTest();

        [Fact]
        public abstract void SparsityMetricTest();

        [Fact]
        public abstract void OrderlinessMetricTest();

        [Fact]
        public abstract void NoiseMetricTest();

        [Fact]
        public abstract void DarkDataMetricTest();

        [Fact]
        public abstract void InterpretabilityMetricTest();

        [Fact]
        public abstract void FlexibilityMetricTest();

        [Fact]
        public abstract void ComplexityMetricTest();

        [Fact]
        public abstract void ReliabilityMetricTest();

        [Fact]
        public abstract void TimelinessMetric();

    }
}
