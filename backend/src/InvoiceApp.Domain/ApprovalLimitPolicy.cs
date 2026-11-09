namespace InvoiceApp.Domain;

public static class ApprovalRoles
{
    public const string Manager = "Manager";
    public const string Director = "Director";
}

/// <summary>
/// Decides whether a role may approve an invoice amount alone. Approver and Admin are
/// unrestricted, unchanged from their behavior before role-based limits existed; Manager and
/// Director are capped by amount.
/// </summary>
public static class ApprovalLimitPolicy
{
    public const decimal ManagerLimit = 50_000m;
    public const decimal DirectorLimit = 500_000m;

    public static bool CanApproveAlone(string role, decimal amount) => role switch
    {
        "Approver" or "Admin" => true,
        ApprovalRoles.Manager => amount <= ManagerLimit,
        ApprovalRoles.Director => amount <= DirectorLimit,
        _ => false
    };

    public static string DeclineReason(string role, decimal amount) => role switch
    {
        ApprovalRoles.Manager =>
            $"A Manager cannot approve invoices over {ManagerLimit:N0}; this invoice is {amount:N0}.",
        ApprovalRoles.Director =>
            $"A Director cannot approve invoices over {DirectorLimit:N0} alone; this invoice is " +
            $"{amount:N0} and needs a second Director approval.",
        _ => $"Role '{role}' is not permitted to approve invoices."
    };
}
