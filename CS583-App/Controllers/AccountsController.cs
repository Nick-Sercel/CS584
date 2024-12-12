using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CS583_App.Data;
using CS583_App.Models;
using NuGet.DependencyResolver;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace CS583_App.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Accounts
        [HttpGet]
        [Route("Accounts/Index")]
        public IActionResult Index()
        {
            return Json(_context.Account);
        }

        // POST: Accounts/Create
        [HttpPost]
        [Route("Accounts/Create")]
        public async Task<IActionResult> Create()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var account = new Account
            {
                Id = 0,
                Username = data.GetProperty("Username").GetString(),
                Password = data.GetProperty("Password").GetString()
            };

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                System.Console.WriteLine(error.ErrorMessage);
            }
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Failed to create account" });
        }

        // GET: Accounts/Create
        [HttpPost]
        [Route("Accounts")]
        public async Task<IActionResult> GetAccount()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse the JSON manually
            var data = JsonSerializer.Deserialize<JsonElement>(body);

            // Extract fields from the JSON
            var username = data.GetProperty("Username").GetString();
            var password = data.GetProperty("Password").GetString();

            // Check if the account exists
            var existingAccount = await _context.Account
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (existingAccount != null)
            {
                // Account exists
                return Json(new { success = true, accountId = existingAccount.Id });
            }

            // Account does not exist
            return Json(new { success = false, message = "Account does not exist." });
        }
    }
}
