// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyProvider.cs" company="Clued In">
//   Copyright (c) 2019 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the dummy provider class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Crawling;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;
using CluedIn.Core.Webhooks;
using CluedIn.Providers.Models;

namespace CluedIn.CluedIn.Custom.Metrics.Tests.Infrastructure
{
    public class DummyProvider : ProviderBase
    {
        public DummyProvider([NotNull] ApplicationContext appContext, [NotNull] IProviderMetadata metadata)
            : base(appContext, metadata)
        {
        }

        public override Task<CrawlJobData> GetCrawlJobData(
            ProviderUpdateContext context,
            IDictionary<string, object> configuration,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> TestAuthentication(
            ProviderUpdateContext context,
            IDictionary<string, object> configuration,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override Task<ExpectedStatistics> FetchUnSyncedEntityStatistics(
            ExecutionContext context,
            IDictionary<string, object> configuration,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override Task<IDictionary<string, object>> GetHelperConfiguration(
            ProviderUpdateContext context,
            CrawlJobData jobData,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override Task<IDictionary<string, object>> GetHelperConfiguration(
            ProviderUpdateContext context,
            CrawlJobData jobData,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId,
            string folderId)
        {
            throw new NotImplementedException();
        }

        public override Task<AccountInformation> GetAccountInformation(
            ExecutionContext context,
            CrawlJobData jobData,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override string Schedule(DateTimeOffset relativeDateTime, bool webHooksEnabled)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<WebHookSignature>> CreateWebHook(
            ExecutionContext context,
            CrawlJobData jobData,
            IWebhookDefinition webhookDefinition,
            IDictionary<string, object> config)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<WebhookDefinition>> GetWebHooks(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteWebHook(ExecutionContext context, CrawlJobData jobData, IWebhookDefinition webhookDefinition)
        {
            throw new NotImplementedException();
        }

        public override Task<CrawlLimit> GetRemainingApiAllowance(
            ExecutionContext context,
            CrawlJobData jobData,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> WebhookManagementEndpoints(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }
    }
}
