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
}
