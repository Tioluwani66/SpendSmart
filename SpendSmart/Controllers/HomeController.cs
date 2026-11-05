using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace SpendSmart.Controllers
{
    [Authorize] // 🔐 LOCK DOWN: Requires user to be logged in
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SpendSmartDbContext _context;

        public HomeController(ILogger<HomeController> logger, SpendSmartDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Helper method to extract the logged-in User's ID from authentication cookies
        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim ?? "0");
        }

        [AllowAnonymous] // Public: Anyone can visit the Home page
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Expenses()
        {
            int userId = GetUserId();

            // ONLY fetch expenses belonging to the currently logged-in user
            var allExpenses = _context.Expenses
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Id)
                .ToList();

            var totalExpenses = allExpenses.Sum(x => x.Value);

            ViewBag.Expenses = totalExpenses;

            return View(allExpenses);
        }

        public IActionResult CreateEditExpense(int? id)
        {
            int userId = GetUserId();

            if (id != null)
            {
                // Editing -> load an expense by id ONLY if it belongs to this user
                var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id && expense.UserId == userId);
                return View(expenseInDb);
            }

            // Creating new -> initialize with exact current UTC time
            var newExpense = new Expense
            {
                Date = DateTime.UtcNow
            };

            return View(newExpense);
        }

        public IActionResult DeleteExpense(int id)
        {
            int userId = GetUserId();

            // Delete ONLY if the expense belongs to this user
            var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id && expense.UserId == userId);

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
            int userId = GetUserId();

            // SAFETY CHECK: If description is empty or value is 0 or less, reload form
            if (string.IsNullOrWhiteSpace(model.Description) || model.Value <= 0)
            {
                return View("CreateEditExpense", model);
            }

            if (model.Id == 0)
            {
                // Create: Link the new expense to the logged-in user
                model.UserId = userId;
                model.Date = DateTime.UtcNow;
                _context.Expenses.Add(model);
            }
            else
            {
                // Edit: Ensure UserId remains linked to this user
                model.UserId = userId;
                model.Date = DateTime.SpecifyKind(model.Date, DateTimeKind.Utc);
                _context.Expenses.Update(model);
            }

            _context.SaveChanges();

            return RedirectToAction("Expenses");
        }

        [AllowAnonymous] // Public: Anyone can visit the Privacy page
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