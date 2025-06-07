using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;

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

            ViewBag.SelectedOption = option ?? "Last 7 Days";


            List<Transaction> transactions = await _context.Transactions
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
}
