namespace FamilyFinance.Models;

public class Account : IFamilyOwned
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public AccountCategory Category { get; set; }
    public string Owner { get; set; } = "Famiglia";
    public bool IsActive { get; set; } = true;
    public bool IsInterest { get; set; } = false;
    
    /// <summary>
    /// Live balance of the account, updated when transactions are imported
    /// </summary>
    public decimal CurrentBalance { get; set; }
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // === Audit Trail ===
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // === Soft Delete ===
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

