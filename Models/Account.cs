namespace FamilyFinance.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public AccountCategory Category { get; set; }
    public string Owner { get; set; } = "Famiglia";
    public bool IsActive { get; set; } = true;
    public bool IsInterest { get; set; } = false; // Se true, appare nel tab Interessi
}

