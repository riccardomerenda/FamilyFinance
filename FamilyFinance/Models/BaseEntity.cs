namespace FamilyFinance.Models;

/// <summary>
/// Base entity with audit trail and soft delete support
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key - auto-incremented
    /// </summary>
    public int Id { get; set; }
    
    // === Audit Trail ===
    
    /// <summary>
    /// When this entity was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User ID who created this entity
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// When this entity was last modified (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// User ID who last modified this entity
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    // === Soft Delete ===
    
    /// <summary>
    /// Whether this entity is soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// When this entity was soft-deleted (UTC)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// User ID who deleted this entity
    /// </summary>
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Interface for entities belonging to a family (multi-tenancy)
/// </summary>
public interface IFamilyOwned
{
    int FamilyId { get; set; }
    Family? Family { get; set; }
}

