namespace Spend_Sensei.Models;

public class Transaction
{
    public string Type { get; set; } = ""; // "Inflow", "Outflow", "Debt"
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string Tags { get; set; } = ""; // Custom tags for filtering
    public bool IsCleared { get; set; } = false; // For debts
}