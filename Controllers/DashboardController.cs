using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace ExpenseTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context) 
        {
            _context = context;

        }

        public async Task<ActionResult> Index(string? option)
        {
            SetOptionsList();

            DateTime startDate;
            DateTime endDate;

            switch (option)
            {
                case "Last 7 Days":
                    startDate = DateTime.Today.AddDays(-6);
                    endDate = DateTime.Today;
                    break;

                case "Last 30 Days":
                    startDate = DateTime.Today.AddDays(-29);
                    endDate = DateTime.Today;
                    break;

                case "Last 6 Months":
                    startDate = DateTime.Today.AddMonths(-6);
                    endDate = DateTime.Today;
                    break;

                case "Last Year":
                    startDate = DateTime.Today.AddYears(-1);
                    endDate = DateTime.Today;
                    break;

                default:
                    startDate = DateTime.Today.AddDays(-6);
                    endDate = DateTime.Today;
                    break;
            }


            int totalDays = (int)(endDate - startDate).TotalDays;

            ViewBag.SelectedOption = option ?? "Last 7 Days";


            List<Models.Transaction> transactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(x => x.Date >= startDate && x.Date <= endDate).ToListAsync();

            //Incomes

            double totalIncome = transactions.
                Where(x => x.Category?.Type == Models.Constants.ExpenseTypes.Income)
                .Sum(x => x.Amount);

            ViewBag.TotalIncome = totalIncome.ToString("C0");

            //Expenses

            double totalExpenses = transactions.
                Where(x => x.Category?.Type == Models.Constants.ExpenseTypes.Expense)
                .Sum(x => x.Amount);

            ViewBag.TotalExpense = totalExpenses.ToString("C0");

            double totalBalance = totalIncome - totalExpenses;

            ViewBag.Balance = totalBalance.ToString("C0");

            //Donut chart

            ViewBag.DonutChartData = transactions
                .Where(x => x.Category?.Type == Models.Constants.ExpenseTypes.Expense)
                .GroupBy(x => x.Category?.CategoryId)
                .Select(x => new
                {
                    CategoryTitleWithIcon = x.First().Category?.Icon + " " + x.First().Category?.Title,
                    Amount = x.Sum(x => x.Amount),
                    FormattedAmount = x.Sum(x => x.Amount).ToString("C0")

                })
                .OrderByDescending(x => x.Amount)
                .ToList();



            //Spline Line -- Income vs Expense

            //Income
            List<SplineChartData> incomeSummary = [.. transactions
                .Where(x => x.Category?.Type == Models.Constants.ExpenseTypes.Income)
                .GroupBy(x => x.Date)
                .Select(x => new SplineChartData()
                {
                    Day = x.First().Date.ToString("dd-MM"),
                    Income = x.Sum(x => x.Amount)
                })];

            //Income
            List<SplineChartData> expenseSummary = [.. transactions
                .Where(x => x.Category?.Type == Models.Constants.ExpenseTypes.Expense)
                .GroupBy(x => x.Date)
                .Select(x => new SplineChartData()
                {
                    Day = x.First().Date.ToString("dd-MM"),
                    Expense = x.Sum(x => x.Amount)
                })];


            string[] lastXDays = Enumerable.Range(0, totalDays + 1)
                .Select(i => startDate.AddDays(i).ToString("dd-MM")).ToArray();

            ViewBag.SplineChartData = from day in lastXDays
                                      join income in incomeSummary on day equals income.Day
                                      into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in expenseSummary on day equals expense.Day
                                      into dayExpenseJoined
                                      from expense in dayExpenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          Day = day,
                                          Income = income == null ? 0 : income.Income,
                                          Expense = expense == null ? 0 : expense.Expense,
                                      };




            //Recent Transactions

            ViewBag.RecentTransactions = await _context.Transactions
                .Include(x => x.Category)
                .OrderByDescending(x => x.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }

        [NonAction]
        private void SetOptionsList()
        {
            string[] options = ["Last 7 Days", "Last 30 Days", "Last 6 Months", "Last Year"];
            ViewBag.Options = options;
            ViewBag.SelectedOption = options[0];

        }
    }


    public class SplineChartData()
    {
        public string Day { get; set; }
        public double Income { get; set; }
        public double Expense { get; set; }
    }
}
