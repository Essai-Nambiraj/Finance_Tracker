namespace FinanceTracker.Models
{
    public class DashboardViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }

        public decimal HighestExpense { get; set; }
        public string? TopCategory { get; set; }
        public decimal SavingsRate { get; set; }


        //for Budget Overview
        public decimal BudgetUsed { get; set; }
        public string BudgetStatus { get; set; }
        public decimal Budget { get; set; }
    }
}
