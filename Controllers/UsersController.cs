using ExpensesWebApp_BE.Data;
using ExpensesWebApp_BE.DTOs;
using ExpensesWebApp_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpensesWebApp_BE.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UsersController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.PasswordHash == dto.PasswordHash);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim("UserID", user.UserID.ToString()),
        new Claim(ClaimTypes.Name, user.UserName)
    };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }



        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }


        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> AddUser(UserDTO dto)
        {

            var user = new User
            {
                UserName = dto.UserName,
                PasswordHash = dto.PasswordHash,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, user);
        }

        [HttpDelete("{userName}")]
        public async Task<ActionResult> DeleteUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return BadRequest("userName is required.");

            if (_context.Users == null)
                return StatusCode(500, "Database context not available.");

            // Include related data if you want EF to cascade or validate relationships before delete
            var user = await _context.Users
                .Include(u => u.MappedExpenses)
                .Include(u => u.MappedTargets)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
                // 204 No Content is typical for successful DELETE
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                // Return 409 Conflict if there are FK constraint issues, or 500 for other DB errors
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }

        }
    }
}
