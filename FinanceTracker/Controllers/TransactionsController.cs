using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Data;
using FinanceTracker.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace FinanceTracker.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly FTDbContext _context;
        private readonly User _user;
        

        public TransactionsController(FTDbContext context, User user)
        {
            _context = context;
            _user = user;         
        }

        //List
        public IActionResult Index(string type, string category, DateTime? fromDate, DateTime? toDate)
        {
            var data = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(type))
                data = data.Where(x => x.Type == type);

            if (!string.IsNullOrEmpty(category))
                data = data.Where(x => x.Category.Contains(category));

            if (fromDate.HasValue)
                data = data.Where(x => x.Dates >= fromDate.Value);

            if (toDate.HasValue)
                data = data.Where(x => x.Dates <= toDate.Value);

            var result = data.OrderByDescending(x => x.Dates)
                        .Where(x => x.User_Id == _user.UserId)
                        .ToList();

            return View(result);
        }

        //Create (GET)
        public IActionResult Create()
        {
            return View();
        }

        //Create Post
        [HttpPost]
        public IActionResult Create(Transactions t)
        {
            if (ModelState.IsValid)
            {
                t.User_Id = _user.UserId;
                _context.Transactions.Add(t);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        //EDIT (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var data = _context.Transactions.Find(id);

            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }

        //Edit (Post)
        [HttpPost]
        public IActionResult Edit(Transactions t)
        {
            if (ModelState.IsValid)
            {
                t.User_Id = _user.UserId;
                _context.Transactions.Update(t);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View(t);
        }

        //Delete
        public IActionResult Delete(int id)
        {
            var data = _context.Transactions.Find(id);

            if(data != null)
            {
                _context.Transactions.Remove(data);
                _context.SaveChanges();
            }
            else
            {
                return NotFound();
            }
            return RedirectToAction("Dashboard");
        }

        //Dashboard
        public IActionResult Dashboard()
        {
            var userId = _user.UserId;

            var data = _context.Transactions
                .Where(x=> x.User_Id == userId)
                .ToList();

            var totalIncome = _context.Transactions         // if data grows large dont use tolist()
                .Where(x => x.Type == "Income" && x.User_Id == userId)
                .Sum(x => (decimal?)x.Amount)??0;

            var totalExpense = data
                .Where(x => x.Type == "Expense")
                .Sum(x => x.Amount);

            var balance = totalIncome - totalExpense;

            var highestExpense = data
                .Where(x => x.Type == "Expense")
                .Select(x => (decimal?)x.Amount)
                .Max() ?? 0;

            var topCategory = data
                .Where(x => x.Type == "Expense")
                .GroupBy(x => x.Category)
                .Select(x => new
                {
                    Category = x.Key,
                    Total = x.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault()?.Category ?? "N/A";

            var savingsRate = totalIncome == 0
                ? 0
                : Math.Round((decimal)balance / totalIncome * 
                100, 2);

            var model = new DashboardViewModel
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = balance,
                HighestExpense = highestExpense,
                TopCategory = topCategory,
                SavingsRate = savingsRate
            };

           /* ViewBag.Income = totalIncome;
            ViewBag.Expense = totalExpense;*/
            //Monthly Grouping
            var monthlyData = data
                .GroupBy(x => new { x.Dates.Year, x.Dates.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,

                    Income = g.Where(x => x.Type == "Income").Sum(x => x.Amount),
                    Expense = g.Where(x => x.Type == "Expense").Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            var labels = monthlyData
                .Select(x => new DateTime(x.Year, x.Month, 1).ToString("MMMM yyyy"))
                .ToList();

            var incomeList = monthlyData.Select(x => x.Income).ToList();
            var expenseList = monthlyData.Select(x => x.Expense).ToList();
            var savingList = monthlyData.Select(x => x.Income - x.Expense).ToList();

            //Budget warning
            decimal budget = 40000;
            var budgetUsedPercentage = budget == 0 ? 0 : Math.Round((totalExpense / budget) * 100, 2);

            string budgetStatus = "";

            if(totalExpense > budget)
            {
                budgetStatus = " Exceeded";
            }
            else if(budgetUsedPercentage > 80)
            {
                budgetStatus = "Warning";
            }
            else
            {
                budgetStatus = "Safe";
            }

            ViewBag.Months = labels;
            ViewBag.IncomeList = incomeList;
            ViewBag.ExpenseList = expenseList;
            ViewBag.SavingsList = savingList;

            ViewBag.Budget = budget;
            ViewBag.BudgetUsed = budgetUsedPercentage;
            ViewBag.BudgetStatus = budgetStatus;
            
            return View(model);
        }
    }
}
