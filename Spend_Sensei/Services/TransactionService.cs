using System.Text.Json;
using Spend_Sensei.Models;

namespace Spend_Sensei.Services;

public class TransactionService
{
    private List<Transaction> transactions = new();
    private readonly string filePath;

    public TransactionService()
    {
        // Define file path for JSON storage
        filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "transactions.json");

        // Load transactions from the file during initialization
        LoadTransactions();
    }

    // Add a new transaction
    public void AddTransaction(Transaction transaction)
    {
        transactions.Add(transaction);
        SaveTransactions(); // Save transactions after adding
    }

    // Get total number of transactions
    public int GetTotalTransactions()
    {
        return transactions.Count;
    }

    // Get total amount for a specific type
    public decimal GetTotalByType(string type)
    {
        return transactions.Where(t => t.Type == type).Sum(t => t.Amount);
    }

    // Get total balance using the formula: Balance = Total Inflows + Cleared Debts - Total Outflows
    public decimal GetBalance()
    {
        return transactions.Where(t => t.Type == "Inflow").Sum(t => t.Amount) +
               transactions.Where(t => t.Type == "Debt" && t.IsCleared).Sum(t => t.Amount) -
               transactions.Where(t => t.Type == "Outflow").Sum(t => t.Amount);
    }

    // Get available balance (Inflow - Outflow)
    public decimal GetAvailableBalance()
    {
        return transactions.Where(t => t.Type == "Inflow").Sum(t => t.Amount) -
               transactions.Where(t => t.Type == "Outflow").Sum(t => t.Amount);
    }

    // Get total pending (uncleared) debt
    public decimal GetPendingDebtTotal()
    {
        return transactions.Where(t => t.Type == "Debt" && !t.IsCleared).Sum(t => t.Amount);
    }

    // Get total cleared (paid) debt
    public decimal GetClearedDebtTotal()
    {
        return transactions.Where(t => t.Type == "Debt" && t.IsCleared).Sum(t => t.Amount);
    }

    // Get transactions by tag
    public List<Transaction> GetTransactionsByTag(string tag)
    {
        return transactions.Where(t => t.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // Filter transactions by type, tags, and date range
    public List<Transaction> FilterTransactions(string type, DateTime? startDate = null, DateTime? endDate = null, string tag = null)
    {
        var filtered = transactions.AsEnumerable();

        if (!string.IsNullOrEmpty(type))
            filtered = filtered.Where(t => t.Type == type);

        if (startDate.HasValue)
            filtered = filtered.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            filtered = filtered.Where(t => t.Date <= endDate.Value);

        if (!string.IsNullOrEmpty(tag))
            filtered = filtered.Where(t => t.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase));

        return filtered.ToList();
    }

    // Get top N highest transactions by type
    public List<Transaction> GetHighestTransactions(string type, int count = 5)
    {
        return transactions.Where(t => t.Type == type).OrderByDescending(t => t.Amount).Take(count).ToList();
    }

    // Get top N lowest transactions by type
    public List<Transaction> GetLowestTransactions(string type, int count = 5)
    {
        return transactions.Where(t => t.Type == type).OrderBy(t => t.Amount).Take(count).ToList();
    }

    // Get pending debts
    public List<Transaction> GetPendingDebts()
    {
        return transactions.Where(t => t.Type == "Debt" && !t.IsCleared).ToList();
    }

    // Get cleared debts (Fixed method name)
    public decimal GetClearedDebts()
    {
        return transactions.Where(t => t.Type == "Debt" && t.IsCleared).Sum(t => t.Amount);
    }

    // Clear a debt
    public void ClearDebt(Transaction debt)
    {
        var availableBalance = GetAvailableBalance();

        if (availableBalance >= debt.Amount)
        {
            debt.IsCleared = true; // Mark the debt as cleared
            AddTransaction(new Transaction
            {
                Type = "Outflow",
                Amount = debt.Amount,
                Date = DateTime.Now,
                Tags = "Debt Payment"
            });
        }
        else
        {
            throw new InvalidOperationException("Insufficient balance to clear debt.");
        }
    }

    // Save transactions to a JSON file
    private void SaveTransactions()
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving transactions: {ex.Message}");
        }
    }

    // Load transactions from a JSON file
    private void LoadTransactions()
    {
        try
        {
            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);
                transactions = JsonSerializer.Deserialize<List<Transaction>>(jsonData) ?? new List<Transaction>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading transactions: {ex.Message}");
            transactions = new List<Transaction>();
        }
    }

    // Reset button for JSON file
    public void ResetTransactions()
    {
        transactions.Clear(); // Clear the in-memory list
        SaveTransactions();   // Overwrite the JSON file with an empty list
    }
}
