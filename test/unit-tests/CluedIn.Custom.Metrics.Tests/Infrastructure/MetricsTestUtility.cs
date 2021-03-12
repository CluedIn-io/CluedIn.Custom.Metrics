// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricsTestUtility.cs" company="Clued In">
//   Copyright (c) 2019 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the metrics test utility class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Globalization;
using Castle.MicroKernel.Registration;
using CluedIn.Core;
using CluedIn.Core.Models;
using CluedIn.Processing.Installers;
using CluedIn.Processing.Models;
using CluedIn.Processing.Processors;
using CluedIn.Processing.SimpleNameMatcher;
using Microsoft.Extensions.Logging;
using Moq;

namespace CluedIn.CluedIn.Custom.Metrics.Tests.Infrastructure
{
    public static class MetricsTestUtility
    {
        public static void SetupProcessingAndModelResolver(TestContext testContext)
        {
            var modelResolver = new Mock<INamedEntityExtractionModelResolver>();

            modelResolver
                .Setup(r => r.GetModels(It.IsAny<ExecutionContext>(), It.IsAny<CultureInfo>(), It.IsAny<NamedEntityClasses>()))
                .Returns<ExecutionContext, CultureInfo, NamedEntityClasses>(
                    (context, culture, namedEntityClasses) =>
                    {
                        var unwantedFirstNames = new BagOfWordsModel<string>();
                        var unwantedGeneratedNames = new BagOfWordsModel<string>(new[] { "Martin Ward", "Tim Hyldahl" });
                        var unwantedLastNamePostfix = new BagOfWordsModel<string>();

                        var personNameMatcherModel = new PersonNameMatcherModel(
                            new[]
                            {
                                new NamesModel(NameType.FirstName, new[] { "Martin", "Tim", "Philipp", "Andreas" }),
                                new NamesModel(NameType.LastName, new[] { "Hyldahl", "Ward", "Heltewig", "Nicolaisen" }),
                                new NamesModel(NameType.FullName)
                            },
                            new[] { unwantedFirstNames },
                            new[] { unwantedGeneratedNames },
                            new[] { unwantedLastNamePostfix },
                            Mock.Of<ILogger>());

                        personNameMatcherModel.EnsurePersonNameCache();

                        return new NamedEntityExtractionProcessing.Models()
                               {
                                   PersonNameModel = personNameMatcherModel, StopWords = new BagOfWordsModel<string>()
                               };
                    });

            testContext.Container.Register(Component.For<INamedEntityExtractionModelResolver>().Instance(modelResolver.Object));
            testContext.Container.Install(new ProcessingInstaller());
        }
    }
}
