using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Data;
using FinanceTracker.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        public IActionResult Dashboard(int month, int year)
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


            //Income wise
            var totalSalary = data
                .Where(x => x.Type == "Income" && x.Category == "Salary")
                .Sum(x => x.Amount);
            var TotalCommission = data
                .Where(x => x.Type == "Income" && x.Category == "Commission")
                .Sum(x => x.Amount);
            var TotalStocks = data
                .Where(x => x.Type == "Income" && x.Category == "Stocks")
                .Sum(x => x.Amount);
            var TotalSIP = data
                .Where(x => x.Type == "Income" && x.Category == "SIP")
                .Sum(x => x.Amount);
            var TotalBusiness = data
                .Where(x => x.Type == "Income" && x.Category == "Business")
                .Sum(x => x.Amount);
            var TotalWages = data
                .Where(x => x.Type == "Income" && x.Category == "Wages")
                .Sum(x => x.Amount);
            var TotalIncomeOthers = data
                .Where(x => x.Type == "Income" && x.Category == "Others")
                .Sum(x => x.Amount);

            //Expense Wise
            var totalShopping = data
                .Where(x => x.Type == "Expense" && x.Category == "Shopping")
                .Sum(x => x.Amount);
            var totalFood = data
                .Where(x => x.Type == "Expense" && x.Category == "Food")
                .Sum(x => x.Amount);
            var totalTravel = data
                .Where(x => x.Type == "Expense" && x.Category == "Travel")
                .Sum(x => x.Amount);
            var totalHome = data
                .Where(x => x.Type == "Expense" && x.Category == "Home Expense")
                .Sum(x => x.Amount);
            var totalBills = data
                .Where(x => x.Type == "Expense" && x.Category == "Bills")
                .Sum(x => x.Amount);
            var totalExpOther = data
                .Where(x => x.Type == "Expense" && x.Category == "Others")
                .Sum(x => x.Amount);

            //Date wise category bar

            int years = 2026;
            int months = 4;

            var daysInMonth = DateTime.DaysInMonth(years, months);

            var allDates = Enumerable.Range(1, daysInMonth)
                .Select(day => new DateTime(years, months, day))
                .ToList();

            var dateData = data
                .Where(x => x.Dates.Year == years && x.Dates.Month == months)
                .ToList();

            var categories = dateData
                .Select(x => x.Category)
                .Distinct()
                .ToList();

            var datasets = new List<object>();

            foreach(var category in categories)
            {
                var values = new List<decimal>();
                foreach(var date in allDates)
                {
                    var total = dateData
                        .Where(x => x.Category == category && x.Dates.Date == date)
                        .Sum(x => x.Amount);

                    values.Add(total);  //if no data -> 0 automatically
                }

                datasets.Add(new
                {
                    label = category,
                    data = values
                });
            }

            var dateLabels = allDates
                .GroupJoin(
                data,
                d => d.Date,
                t => t.Dates.Date,
                (d, transactions) => new
                {
                    Date = d,
                    Income = transactions
                    .Where(x => x.Type == "Income")
                    .Sum(x => x.Amount),

                    Expense = transactions
                    .Where(x => x.Type == "Expense")
                    .Sum(x => x.Amount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            var dLabels = dateLabels
                .Select(X => X.Date.ToString("dd MMM"))
                .ToList();

            

            var model = new DashboardViewModel
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = balance,
                HighestExpense = highestExpense,
                TopCategory = topCategory,
                SavingsRate = savingsRate,

                //Income wise data to View
                TotalSalary = totalSalary,
                TotalCommission = TotalCommission,
                TotalStocks = TotalStocks,
                TotalSIP = TotalSIP,
                TotalBusiness = TotalBusiness,
                TotalWages = TotalWages,
                TotalIncomeOthers = TotalIncomeOthers,

                //Expense wise data to View
                TotalShopping = totalShopping,
                TotalFood = totalFood,
                TotalTravel = totalTravel,
                TotalHome = totalHome,
                TotalBills = totalBills,
                TotalExpOthers = totalExpOther,

                DateLabels = dLabels,
                ChartDataSets = datasets
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

            ViewBag.TotalSalary = totalSalary;
            
            return View(model);
        }

        //[HttpGet]
        //public JsonResult GetMonthlyData(int month, int year)
        //{

        //    var userId = _user.UserId;
        //    //Monthly Analytices for day wise for partcilar month and year
        //    int selectedMonth = month;
        //    int selectedYear = year;

        //    var transactions = _context.Transactions
        //        .Where(t => t.User_Id == userId && t.Dates.Month == selectedMonth && t.Dates.Year == selectedYear)
        //        .ToList();

        //    //Group by day nd calculate income and expense
        //    var result = transactions
        //        .GroupBy(t => t.Dates.Day)
        //        .Select(g => new
        //        {
        //            day = g.Key, //Day number(1-31)

        //            //Total income of the particular day
        //            income = g.Where(t => t.Type == "Income")
        //            .Sum(x => x.Amount),

        //            //TOtal expense
        //            expense = g.Where(t => t.Type == "Expense")
        //            .Sum(x => x.Amount),
        //        })
        //        .OrderBy(x => x.day)
        //        .ToList();

        //    return Json(result);
        //}

        public IActionResult Budget()
        {
            var userID = _user.UserId;
            if (userID == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.User_Id == userID);

            var data = _context.Transactions
               .Where(x => x.User_Id == userID)
               .ToList();

            var totalExpense = data
                .Where(x => x.Type == "Expense")
                .Sum(x => x.Amount);

            var bud = user.Budget;      //Users.Budget

            decimal budgetUsed = 0;
            if (bud > 0)
            {
                budgetUsed = Math.Round((totalExpense / bud) * 100, 2);
            }

            string status = "";
            if (totalExpense > bud)
            {
                status = "Exceeded";
            }
            else if (budgetUsed >= 80)
            {
                status = "Warning";
            }
            else if(budgetUsed < 80)
            {
                status = "Safe";
            }
            else
            {
                status = "Invalid Budget";
            }

            var model = new DashboardViewModel
            {
                TotalExpense = totalExpense,
                Budget = bud,         //DashboardViewModel
                BudgetUsed = budgetUsed,
                BudgetStatus = status
            };

            //var models = new DashboardViewModel { Budget = user.Budget };
            return View(model);
        }

        [HttpPost]
        public IActionResult Budget(DashboardViewModel b)
        {

            var userId = _user.UserId;                   
            var user = _context.Users.FirstOrDefault(u => u.User_Id == userId);

            if (user != null)
            {
                user.Budget = b.Budget;           //Users.Budget = Dashboard.ViewModel.Budget
                _context.SaveChanges();
            }
            return RedirectToAction("Budget");
        }
    }
}
