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

        //For Income wise
        public decimal TotalSalary { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalStocks { get; set; }
        public decimal TotalSIP { get; set; }
        public decimal TotalBusiness { get; set; }
        public decimal TotalWages { get; set; }
        public decimal TotalIncomeOthers { get; set; }

        //For Expense wise
        public decimal TotalShopping { get; set; }
        public decimal TotalFood { get; set; }
        public decimal TotalTravel { get; set; }
        public decimal TotalHome { get; set; }
        public decimal TotalBills { get; set; }
        public decimal TotalExpOthers { get; set; }
        
        public List<string> DateLabels { get; set; }
        public object ChartDataSets { get; set; }



    }
}
