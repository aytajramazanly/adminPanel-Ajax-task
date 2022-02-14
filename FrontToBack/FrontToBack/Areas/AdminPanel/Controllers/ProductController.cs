using FrontToBack.Areas.AdminPanel.ViewModels;
using FrontToBack.DataAccessLayer;
using FrontToBack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _dbContext;

        public ProductController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(int page=1)
        {
            if (page < 1)
                return BadRequest();

            var totalPageCount = Math.Ceiling((decimal)await _dbContext.Products.CountAsync() / 10);
            if (page > totalPageCount)
                return NotFound();

            ViewBag.totalPageCount = totalPageCount;
            ViewBag.currentPage = page;
            var products = await _dbContext.Products.Include(x=>x.Category).Skip((page - 1) * 10).Take(10).ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var product = await _dbContext.Products.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _dbContext.Categories.ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
                return View();
            if(await _dbContext.Products.Include(x=>x.Category).AnyAsync(x=>x.Name.ToLower().Trim()==product.Name.ToLower().Trim()))
            {
                ModelState.AddModelError("Name", $"Product with this name already exist in {product.Category}");
                return View();
            }
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return Json(new { status = 404 });
             _dbContext.Remove(product);
            await _dbContext.SaveChangesAsync();
            return Json(new { status = 200 });
        }
    }
}
