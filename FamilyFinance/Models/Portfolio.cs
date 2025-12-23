using System.Text.Json.Serialization;

namespace FamilyFinance.Models;

public class Portfolio : IFamilyOwned
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? TimeHorizonYears { get; set; }
    public int? TargetYear { get; set; }
    public string Color { get; set; } = "#6366f1";
    public string Icon { get; set; } = "chart";
    public bool IsActive { get; set; } = true;
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
    [JsonIgnore]
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

