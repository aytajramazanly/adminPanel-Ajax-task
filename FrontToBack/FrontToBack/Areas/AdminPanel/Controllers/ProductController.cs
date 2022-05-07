using FrontToBack.Areas.AdminPanel.Data;
using FrontToBack.Areas.AdminPanel.ViewModels;
using FrontToBack.DataAccessLayer;
using FrontToBack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly string _defaultImg;

        public ProductController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _defaultImg = "defaultImagePerson.png";
        }

        public async Task<IActionResult> Index(int page=1)
        {
            if (page < 1)
                return BadRequest();

            if (((page - 1) * 10) >= await _dbContext.Products.CountAsync())
                page--;

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
            ViewBag.Categories = await _dbContext.Categories.ToListAsync();
            if (!ModelState.IsValid)
                return View();
            var exist = await _dbContext.Products.AnyAsync(x => x.Name.ToLower().Trim() == product.Name.ToLower().Trim());
            if (exist)
            {
                ModelState.AddModelError("Name", $"Product with this name already exist! ");
                return View();
            }
            if (product.Photo != null)
            {
                if (!product.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "File must to be a Photo!");
                    return View();
                }

                if (!product.Photo.IsAllowedSize(2))
                {
                    ModelState.AddModelError("Photo", "The size of the photo cannot be more than one MegaByte!");
                    return View();
                }
                product.Image = await product.Photo.GenerateFile(Constants.ImageFolderPath);
            }
            else
            {
                product.Image = _defaultImg;
            }

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _dbContext.Products.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                return NotFound();
            ViewBag.Categories = await _dbContext.Categories.ToListAsync();
            ViewBag.DefaultImg = _defaultImg;
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product)
        {
            ViewBag.Categories = await _dbContext.Categories.ToListAsync();
            if (id == null)
                return NotFound();

            if (id != product.Id)
                return BadRequest();
            var existProduct = await _dbContext.Products.FindAsync(id);
            if (existProduct == null)
                return NotFound();
            if (product.Photo != null)
            {
                if (!product.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "File must to be a Photo!");
                    return View(existProduct);
                }
                if (!product.Photo.IsAllowedSize(1))
                {
                    ModelState.AddModelError("Photo", "The size of the photo cannot be more than one MegaByte!");
                    return View(existProduct);
                }

                var existPath = Path.Combine(Constants.ImageFolderPath, existProduct.Image);
                if (System.IO.File.Exists(existPath) && existProduct.Image != _defaultImg)
                {
                    System.IO.File.Delete(existPath);
                }

                product.Image = await product.Photo.GenerateFile(Constants.ImageFolderPath);
                existProduct.Image = product.Image;
            }
            existProduct.Name = product.Name;
            existProduct.Description = product.Description;
            existProduct.Price = existProduct.Price;
            existProduct.CategoryId = product.CategoryId;
            existProduct.Category = product.Category;

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
            var path = Path.Combine(Constants.ImageFolderPath, "img", product.Image);
            if (System.IO.File.Exists(path) && product.Image != _defaultImg)
            {
                System.IO.File.Delete(path);
            }
            _dbContext.Remove(product);
            await _dbContext.SaveChangesAsync();
            return Json(new { status = 200 });
        }
        public async Task<IActionResult> DeletePhoto(int? id)
        {
            if (id == null)
                return BadRequest();
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return Json(new { status = 404 });
            var path = Path.Combine(Constants.ImageFolderPath, "img", product.Image);
            if (System.IO.File.Exists(path) && product.Image != _defaultImg)
            {
                System.IO.File.Delete(path);
            }
            product.Image = _defaultImg;
            await _dbContext.SaveChangesAsync();
            return Json(new { status = 200 });
        }
    }
}
