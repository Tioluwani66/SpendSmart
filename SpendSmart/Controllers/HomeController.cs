using Microsoft.AspNetCore.Mvc;
using SpendSmart.Models;
using System.Diagnostics;

namespace SpendSmart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SpendSmartDbContext _context;

        public HomeController(ILogger<HomeController> logger, SpendSmartDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Expenses()
        {
            var allExpenses = _context.Expenses.OrderBy(x => x.Id).ToList();

            var totalExpenses = allExpenses.Sum(x => x.Value);

            ViewBag.Expenses = totalExpenses;

            return View(allExpenses);
        }

        public IActionResult CreateEditExpense(int? id)
        {
            if (id != null)
            {
                // Editing -> load an expense by id
                var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
                return View(expenseInDb);
            }

            // FIXED: Use DateTime.UtcNow instead of DateTime.Today so hours/minutes/seconds are preserved
            var newExpense = new Expense
            {
                Date = DateTime.UtcNow
            };

            return View(newExpense);
        }

        public IActionResult DeleteExpense(int id)
        {
            var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);

            if (expenseInDb != null)
            {
                _context.Expenses.Remove(expenseInDb);
                _context.SaveChanges();
            }

            return RedirectToAction("Expenses");
        }

        [HttpPost]
        public IActionResult CreateEditExpenseForm(Expense model)
        {
            // SAFETY CHECK: If description is empty or value is 0 or less, reload form instead of crashing
            if (string.IsNullOrWhiteSpace(model.Description) || model.Value <= 0)
            {
                return View("CreateEditExpense", model);
            }

            if (model.Id == 0)
            {
                // FIXED: Automatically stamp the exact current UTC time at the moment of submission
                model.Date = DateTime.UtcNow;
                _context.Expenses.Add(model);
            }
            else
            {
                // Edit: Preserve existing date as UTC for PostgreSQL
                model.Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Utc);
                _context.Expenses.Update(model);
            }

            _context.SaveChanges();

            return RedirectToAction("Expenses");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}