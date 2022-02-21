using System;
using CluedIn.Core;
using CluedIn.Core.Accounts;
using CluedIn.Core.Data.Relational;
using OrganizationAccount = CluedIn.Core.Accounts.OrganizationAccount;

namespace CluedIn.CluedIn.Custom.Metrics.Tests.Infrastructure
{
    public class DummyOrganization : Organization
    {
        public DummyOrganization(ApplicationContext context) 
            : base(
                new OrganizationAccount() { Id = Constants.SystemOrganizationId, ApplicationSubDomain = "dummy" }, 
                new OrganizationProfile() { Id = Constants.SystemOrganizationId, Name = "Dummy" }, 
                new OrganizationDataShard() { Id = Constants.SystemOrganizationId }, 
                context)
        {
        }

        public DummyOrganization(ApplicationContext context, Guid id) 
            : base(
                new OrganizationAccount() { Id = id, ApplicationSubDomain = "dummy" }, 
                new OrganizationProfile() { Id = id, Name = "Dummy" }, 
                new OrganizationDataShard() { Id = id }, 
                context)
        {
        }

        public DummyOrganization(ApplicationContext context, Guid id, string name) 
            : base(
                new OrganizationAccount() { Id = id, ApplicationSubDomain = name.ToLower() }, 
                new OrganizationProfile() { Id = id, Name = name }, 
                new OrganizationDataShard() { Id = id }, 
                context)
        {
        }
    }
}
