using Microsoft.AspNetCore.Mvc;
using ExpensesWebApp_BE.Models;
using ExpensesWebApp_BE.Data;
using Microsoft.EntityFrameworkCore;
using ExpensesWebApp_BE.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ExpensesWebApp_BE.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController: ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // get categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCatgegories()
        {
            return await _context.Categories.ToListAsync();
        }


        //put category

        // post category
        [HttpPost]
        public async Task<ActionResult<Category>> AddCategory(CategoryDTO dto)
        {
            var category = new Category
            {
                CategoryName = dto.Name
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCatgegories", new { id = category.CategoryId }, category);
        }

        // delete category
        [HttpDelete("{catrgoryId}")]
        public async Task<ActionResult> RemoveCategory(int catrgoryId)
        {
            var category = await _context.Categories.FindAsync(catrgoryId);
            if (category == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch(DbUpdateException ex)
            {
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }

            
        }
    }
}
