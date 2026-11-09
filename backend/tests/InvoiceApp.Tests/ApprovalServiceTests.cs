using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ApprovalServiceTests
{
    private static Invoice InvoiceWith(string id, InvoiceStatus status) =>
        new() { Id = id, CustomerName = "Test School", Amount = 100m, Status = status };

    private static ApprovalService ServiceFor(params Invoice[] invoices) =>
        new(new FakeInvoiceRepository(invoices));

    [Fact]
    public void Submit_MovesDraftToPendingApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.Draft);

        var result = ServiceFor(invoice).Submit("INV-1");

        Assert.Equal(InvoiceStatus.PendingApproval, result.Status);
    }

    [Fact]
    public void Submit_RejectsInvoiceThatIsNotADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Approved));

        Assert.Throws<InvalidOperationException>(() => service.Submit("INV-1"));
    }

    [Fact]
    public void Approve_SetsStatusApproverAndTimestamp()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Approve("INV-1", "demo.approver", "Approver");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("demo.approver", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Approve_RejectsInvoiceThatIsStillADraft()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.Draft));

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.approver", "Approver"));
    }

    [Fact]
    public void Approve_ThrowsWhenInvoiceDoesNotExist()
    {
        var service = ServiceFor();

        Assert.Throws<KeyNotFoundException>(() => service.Approve("INV-404", "demo.approver", "Approver"));
    }

    [Theory]
    [InlineData("Approver")]
    [InlineData("Admin")]
    public void Approve_ApproverAndAdminApproveAnyAmount(string role)
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval);
        invoice.Amount = 900_000m;
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "demo.user", role);

        Assert.Equal(InvoiceStatus.Approved, result.Status);
    }

    [Theory]
    [InlineData(50_000)]
    [InlineData(1)]
    public void Approve_ManagerApprovesWithinLimit(decimal amount)
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval);
        invoice.Amount = amount;
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "demo.manager", "Manager");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
    }

    [Fact]
    public void Approve_ManagerDeclinedJustAboveLimit()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval);
        invoice.Amount = 50_000.01m;
        var service = ServiceFor(invoice);

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));
    }

    [Theory]
    [InlineData(500_000)]
    [InlineData(1)]
    public void Approve_DirectorApprovesWithinLimit(decimal amount)
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval);
        invoice.Amount = amount;
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "demo.director", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Director")]
    public void Approve_AboveDirectorLimitRegistersFirstApprovalInsteadOfDeclining(string role)
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval);
        invoice.Amount = 500_000.01m;
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "demo.first-approver", role);

        Assert.Equal(InvoiceStatus.PendingSecondApproval, result.Status);
        Assert.Equal("demo.first-approver", result.FirstApprovedBy);
        Assert.NotNull(result.FirstApprovedAt);
        Assert.Null(result.DecidedBy);
    }

    [Fact]
    public void Approve_SecondDirectorApprovalByDistinctUserCompletesApproval()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingSecondApproval);
        invoice.Amount = 600_000m;
        invoice.FirstApprovedBy = "demo.director1";
        invoice.FirstApprovedAt = DateTime.UtcNow.AddMinutes(-5);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "demo.director2", "Director");

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("demo.director2", result.DecidedBy);
        Assert.NotNull(result.DecidedAt);
    }

    [Fact]
    public void Approve_SecondApprovalDeclinedWhenSecondApproverIsNotDirector()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingSecondApproval);
        invoice.Amount = 600_000m;
        invoice.FirstApprovedBy = "demo.director1";
        invoice.FirstApprovedAt = DateTime.UtcNow.AddMinutes(-5);
        var repository = new FakeInvoiceRepository(invoice);
        var service = new ApprovalService(repository);

        var before = Snapshot(invoice);

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager2", "Manager"));

        AssertUnchanged(before, repository.GetById("INV-1")!);
    }

    [Fact]
    public void Approve_SameUserCannotGiveBothApprovals()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingSecondApproval);
        invoice.Amount = 600_000m;
        invoice.FirstApprovedBy = "demo.director1";
        invoice.FirstApprovedAt = DateTime.UtcNow.AddMinutes(-5);
        var repository = new FakeInvoiceRepository(invoice);
        var service = new ApprovalService(repository);

        var before = Snapshot(invoice);

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.director1", "Director"));

        AssertUnchanged(before, repository.GetById("INV-1")!);
    }

    [Theory]
    [InlineData("Approver")]
    [InlineData("Admin")]
    public void Approve_ApproverAndAdminFinalizePendingSecondApprovalInOneStep(string role)
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingSecondApproval);
        invoice.Amount = 900_000m;
        invoice.FirstApprovedBy = "demo.director1";
        invoice.FirstApprovedAt = DateTime.UtcNow.AddMinutes(-5);
        var service = ServiceFor(invoice);

        var result = service.Approve("INV-1", "demo.user", role);

        Assert.Equal(InvoiceStatus.Approved, result.Status);
        Assert.Equal("demo.user", result.DecidedBy);
    }

    [Fact]
    public void Reject_OnPendingSecondApprovalMovesStraightToRejected()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingSecondApproval);
        invoice.Amount = 600_000m;
        invoice.FirstApprovedBy = "demo.director1";
        invoice.FirstApprovedAt = DateTime.UtcNow.AddMinutes(-5);
        var service = ServiceFor(invoice);

        var result = service.Reject("INV-1", "demo.approver", "Pricing error");

        Assert.Equal(InvoiceStatus.Rejected, result.Status);
        Assert.Equal("Pricing error", result.DecisionNote);
    }

    private static object Snapshot(Invoice invoice) => new
    {
        invoice.Status,
        invoice.DecidedBy,
        invoice.DecidedAt,
        invoice.DecisionNote,
        invoice.FirstApprovedBy,
        invoice.FirstApprovedAt
    };

    private static void AssertUnchanged(object before, Invoice after)
    {
        var expected = (dynamic)before;
        Assert.Equal(expected.Status, after.Status);
        Assert.Equal(expected.DecidedBy, after.DecidedBy);
        Assert.Equal(expected.DecidedAt, after.DecidedAt);
        Assert.Equal(expected.DecisionNote, after.DecisionNote);
        Assert.Equal(expected.FirstApprovedBy, after.FirstApprovedBy);
        Assert.Equal(expected.FirstApprovedAt, after.FirstApprovedAt);
    }

    [Fact]
    public void Approve_DeclinedAttemptLeavesInvoiceCompletelyUnchanged()
    {
        var invoice = InvoiceWith("INV-1", InvoiceStatus.PendingApproval);
        invoice.Amount = 80_000m;
        var repository = new FakeInvoiceRepository(invoice);
        var service = new ApprovalService(repository);

        var before = new
        {
            invoice.Status,
            invoice.DecidedBy,
            invoice.DecidedAt,
            invoice.DecisionNote
        };

        Assert.Throws<InvalidOperationException>(() => service.Approve("INV-1", "demo.manager", "Manager"));

        var after = repository.GetById("INV-1")!;
        Assert.Equal(before.Status, after.Status);
        Assert.Equal(before.DecidedBy, after.DecidedBy);
        Assert.Equal(before.DecidedAt, after.DecidedAt);
        Assert.Equal(before.DecisionNote, after.DecisionNote);
    }

    [Fact]
    public void Reject_RecordsReasonAndApprover()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        var result = service.Reject("INV-1", "demo.approver", "Duplicate invoice");

        Assert.Equal(InvoiceStatus.Rejected, result.Status);
        Assert.Equal("Duplicate invoice", result.DecisionNote);
    }

    [Fact]
    public void Reject_RequiresAReason()
    {
        var service = ServiceFor(InvoiceWith("INV-1", InvoiceStatus.PendingApproval));

        Assert.Throws<ArgumentException>(() => service.Reject("INV-1", "demo.approver", "  "));
    }
}
