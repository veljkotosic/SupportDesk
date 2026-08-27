using SupportDesk.Domain.Models.Organization.Enums;
using OrganizationModel = SupportDesk.Domain.Models.Organization.Organization;

namespace SupportDesk.UnitTests.Domain.Models.Organization;

[TestFixture]
internal sealed class OrganizationTests
{
    [TestCase("A.T.R. Doo")]
    public void Create_WithValidData_ShouldCreateOrganization(string validName)
    {
        var timeProvider = TimeProvider.System;
        
        var organization = OrganizationModel.Create(validName, timeProvider);

        Assert.Multiple(() =>
        {
            Assert.That(organization.Id.IdValue, Is.Not.EqualTo(Guid.Empty));
            Assert.That(organization.Name.NameValue, Is.EqualTo(validName));
            Assert.That(organization.Status, Is.EqualTo(OrganizationStatus.Active));
            Assert.That(organization.CreatedAt.CreatedAtValue, Is.Not.EqualTo(default(DateTime)));
            Assert.That(organization.DeletedAt, Is.Null);
        });
    }
}