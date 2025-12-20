namespace FamilyFinance.Models;

public class Portfolio
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? TimeHorizonYears { get; set; }
    public int? TargetYear { get; set; } // Anno obiettivo (es. 2045)
    public string Color { get; set; } = "#6366f1"; // Default indigo
    public string Icon { get; set; } = "chart"; // chart, crypto, piggy, rocket, shield
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

