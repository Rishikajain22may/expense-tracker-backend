using ExpensesWebApp_BE.Data;
using ExpensesWebApp_BE.DTOs;
using ExpensesWebApp_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpensesWebApp_BE.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController: ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;            
        }
        // Get all expenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            return await _context.Expenses.ToListAsync();
        }

        // Delete expense by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Add multiple expenses (logged-in user will be used as UserID)
        [HttpPost("bulk")]
        public async Task<ActionResult> AddMultipleExpenses([FromBody] List<ExpenseDTO> dtos)
        {
            int loggedInUserId = int.Parse(User.Claims.First(c => c.Type == "UserID").Value);
            if (dtos == null || dtos.Count == 0)
                return BadRequest("A non-empty list of expenses is required.");

           
            if (loggedInUserId == 0)
                return Unauthorized("Unable to determine logged-in user.");

            // Optionally verify logged-in user exists in DB
            var userExists = await _context.Users.AnyAsync(u => u.UserID == loggedInUserId);
            if (!userExists)
                return Unauthorized("Logged-in user not found.");

            // Validate categories in bulk to avoid per-item DB roundtrips
            var categoryIds = dtos.Select(d => d.CategoryId).Distinct().ToList();
            var existingCategoryIds = await _context.Categories
                .Where(c => categoryIds.Contains(c.CategoryId))
                .Select(c => c.CategoryId)
                .ToListAsync();

            var missingCategories = categoryIds.Except(existingCategoryIds).ToList();
            if (missingCategories.Any())
                return NotFound($"The following category IDs were not found: {string.Join(',', missingCategories)}");

            // Map DTOs to entities and assign logged-in user id for each expense
            var expenses = dtos.Select(dto => new Expense
            {
                Amount = dto.Amount,
                Description = dto.Description,
                ExpenseDate = dto.Date,
                CategoryId = dto.CategoryId,
                UserID = loggedInUserId
            }).ToList();

            // Use a transaction so the operation is atomic
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Expenses.AddRange(expenses);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(201, expenses);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
        // Add single expense
        [HttpPost("add")]
        public async Task<ActionResult> AddExpense(ExpenseDTO dto)
        {
            if (dto == null)
                return BadRequest("Expense data is required.");

            try
            {
                int loggedInUserId = int.Parse(User.Claims.First(c => c.Type == "UserID").Value);
                // Check if User and Category exist (foreign key validation)
                var userExists = await _context.Users.AnyAsync(u => u.UserID == loggedInUserId);
                var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);

                if (!userExists)
                    return NotFound($"User with ID {loggedInUserId} not found.");

                if (!categoryExists)
                    return NotFound($"Category with ID {dto.CategoryId} not found.");

                // Create expense object
                var expense = new Expense
                {
                    Amount = dto.Amount,
                    Description = dto.Description,
                    ExpenseDate = dto.Date,
                    CategoryId = dto.CategoryId,
                    UserID = loggedInUserId,
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                return StatusCode(201, expense);
            }
            catch (DbUpdateException ex)
            {
                // Catches DB-related issues like constraint violations
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                // General fallback for unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

    }
}
