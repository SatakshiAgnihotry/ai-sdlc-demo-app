namespace InvoiceApp.Domain;

/// <summary>
/// Moves an invoice through its lifecycle: Draft -> PendingApproval -> (PendingSecondApproval ->)
/// Approved, or Rejected. Invoices over the Director limit need two distinct approvals; Approver
/// and Admin finalize any amount in a single step, unchanged from before this rule existed.
/// </summary>
public class ApprovalService
{
    private readonly IInvoiceRepository _repository;

    public ApprovalService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public Invoice Submit(string invoiceId)
    {
        var invoice = Find(invoiceId);

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Invoice {invoiceId} is {invoice.Status}; only drafts can be submitted.");
        }

        invoice.Status = InvoiceStatus.PendingApproval;
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Approve(string invoiceId, string approver, string role)
    {
        var invoice = Find(invoiceId);
        EnsureDecidable(invoice);

        if (role is "Approver" or "Admin")
        {
            FinalizeApproval(invoice, approver);
            return invoice;
        }

        if (invoice.Status == InvoiceStatus.PendingSecondApproval)
        {
            if (role != ApprovalRoles.Director)
            {
                throw new InvalidOperationException(ApprovalLimitPolicy.SecondApprovalDeclineReason(role));
            }

            if (approver == invoice.FirstApprovedBy)
            {
                throw new InvalidOperationException(ApprovalLimitPolicy.SameApproverDeclineReason(approver));
            }

            FinalizeApproval(invoice, approver);
            return invoice;
        }

        if (invoice.Amount > ApprovalLimitPolicy.DirectorLimit)
        {
            if (role != ApprovalRoles.Manager && role != ApprovalRoles.Director)
            {
                throw new InvalidOperationException(ApprovalLimitPolicy.DeclineReason(role, invoice.Amount));
            }

            invoice.Status = InvoiceStatus.PendingSecondApproval;
            invoice.FirstApprovedBy = approver;
            invoice.FirstApprovedAt = DateTime.UtcNow;
            _repository.Update(invoice);
            return invoice;
        }

        if (!ApprovalLimitPolicy.CanApproveAlone(role, invoice.Amount))
        {
            throw new InvalidOperationException(ApprovalLimitPolicy.DeclineReason(role, invoice.Amount));
        }

        FinalizeApproval(invoice, approver);
        return invoice;
    }

    public Invoice Reject(string invoiceId, string approver, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required to reject an invoice.", nameof(reason));
        }

        var invoice = Find(invoiceId);
        EnsureDecidable(invoice);

        invoice.Status = InvoiceStatus.Rejected;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        invoice.DecisionNote = reason;
        _repository.Update(invoice);
        return invoice;
    }

    private void FinalizeApproval(Invoice invoice, string approver)
    {
        invoice.Status = InvoiceStatus.Approved;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        _repository.Update(invoice);
    }

    private Invoice Find(string invoiceId) =>
        _repository.GetById(invoiceId)
        ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");

    private static void EnsureDecidable(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.PendingApproval && invoice.Status != InvoiceStatus.PendingSecondApproval)
        {
            throw new InvalidOperationException(
                $"Invoice {invoice.Id} is {invoice.Status}; only pending invoices can be decided.");
        }
    }
}
