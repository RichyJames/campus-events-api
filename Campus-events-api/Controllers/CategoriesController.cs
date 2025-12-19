using Campus_events_api.Data;
using Campus_events_api.Dtos;
using Campus_events_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace Campus_events_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto 
                { 
                    Id = c.Id, 
                    Name = c.Name 
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return new CategoryDto 
            { 
                Id = category.Id, 
                Name = category.Name 
            };
        }
        
        [Authorize(Roles = "Organiser,Admin")]
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
        {
            var category = new Category { Name = dto.Name };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                new CategoryDto { Id = category.Id, Name = category.Name }
            );
        }

        [Authorize(Roles = "Organiser,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            category.Name = dto.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
