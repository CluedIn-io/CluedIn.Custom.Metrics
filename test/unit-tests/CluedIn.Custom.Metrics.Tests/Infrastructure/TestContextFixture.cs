// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestContextFixture.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the test context fixture class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace CluedIn.CluedIn.Custom.Metrics.Tests.Infrastructure
{
    public class TestContextFixture : IDisposable
    {
        public TestContext TestContext { get; } = new TestContext();

        public void Dispose()
        {
            this.TestContext.Dispose();
        }
    }
}
