using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalLimitPolicyTests
{
    [Theory]
    [InlineData(50_000)]
    [InlineData(1)]
    public void CanApproveAlone_ManagerWithinLimit(decimal amount)
    {
        Assert.True(ApprovalLimitPolicy.CanApproveAlone(ApprovalRoles.Manager, amount));
    }

    [Fact]
    public void CanApproveAlone_ManagerJustAboveLimit()
    {
        Assert.False(ApprovalLimitPolicy.CanApproveAlone(ApprovalRoles.Manager, 50_000.01m));
    }

    [Theory]
    [InlineData(500_000)]
    [InlineData(1)]
    public void CanApproveAlone_DirectorWithinLimit(decimal amount)
    {
        Assert.True(ApprovalLimitPolicy.CanApproveAlone(ApprovalRoles.Director, amount));
    }

    [Fact]
    public void CanApproveAlone_DirectorJustAboveLimit()
    {
        Assert.False(ApprovalLimitPolicy.CanApproveAlone(ApprovalRoles.Director, 500_000.01m));
    }

    [Theory]
    [InlineData("Approver")]
    [InlineData("Admin")]
    public void CanApproveAlone_ApproverAndAdminHaveNoLimit(string role)
    {
        Assert.True(ApprovalLimitPolicy.CanApproveAlone(role, 10_000_000m));
    }

    [Fact]
    public void CanApproveAlone_UnknownRoleIsDeclined()
    {
        Assert.False(ApprovalLimitPolicy.CanApproveAlone("Clerk", 1m));
    }

    [Fact]
    public void DeclineReason_IsNotEmptyForEveryDeclinedRole()
    {
        Assert.False(string.IsNullOrWhiteSpace(ApprovalLimitPolicy.DeclineReason(ApprovalRoles.Manager, 60_000m)));
        Assert.False(string.IsNullOrWhiteSpace(ApprovalLimitPolicy.DeclineReason(ApprovalRoles.Director, 600_000m)));
        Assert.False(string.IsNullOrWhiteSpace(ApprovalLimitPolicy.DeclineReason("Clerk", 1m)));
    }

    [Fact]
    public void DeclineReason_ForDirectorStatesTheLimitAndTheInvoiceAmount()
    {
        Assert.Equal(
            "A Director cannot approve invoices over 500,000 alone; this invoice is 600,000 and needs a second Director approval.",
            ApprovalLimitPolicy.DeclineReason(ApprovalRoles.Director, 600_000m));
    }

    [Fact]
    public void SecondApprovalDeclineReason_NamesTheRoleAndRequiresADirector()
    {
        Assert.Equal(
            "Role 'Manager' cannot give the second approval on this invoice; the second approval must come from a Director.",
            ApprovalLimitPolicy.SecondApprovalDeclineReason("Manager"));
    }

    [Fact]
    public void SameApproverDeclineReason_NamesTheApproverAndRequiresADifferentDirector()
    {
        Assert.Equal(
            "demo.director already gave the first approval on this invoice; the second approval must come from a different Director.",
            ApprovalLimitPolicy.SameApproverDeclineReason("demo.director"));
    }
}
