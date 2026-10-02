using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebSite_GioiThieuSP.Data;
using WebSite_GioiThieuSP.Models;

namespace WebSite_GioiThieuSP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Product - Danh sách sản phẩm
        public async Task<IActionResult> Index(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _db.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(p => p.Name.Contains(keyword)
                    || (p.Description != null && p.Description.Contains(keyword)));
            }

            if (categoryId is > 0)
            {
                var catId = categoryId.Value;
                query = query.Where(p => p.CategoryId == catId);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var products = await query
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            await FillCategoryList();

            return View(products);
        }

        // GET: /Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            await FillCategoryList();
            return View(new Product());
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            product.Name = (product.Name ?? string.Empty).Trim();

            if (ModelState.IsValid)
            {
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            await FillCategoryList(product.CategoryId);
            return View(product);
        }

        // GET: /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            await FillCategoryList();
            return View(product);
        }

        // POST: /Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var existing = await _db.Products.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            product.Name = (product.Name ?? string.Empty).Trim();

            if (ModelState.IsValid)
            {
                existing.Name = product.Name;
                existing.CategoryId = product.CategoryId;
                existing.Price = product.Price;
                existing.Description = product.Description;
                existing.ImageUrl = product.ImageUrl;
                existing.IsActive = product.IsActive;
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            product.Category = await _db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == product.CategoryId);

            await FillCategoryList(product.CategoryId);
            return View(product);
        }

        // GET: /Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.OrderCount = await _db.OrderDetails.CountAsync(od => od.ProductId == id);
            return View(product);
        }

        // POST: /Admin/Product/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Sản phẩm đã có trong đơn hàng thì không cho xóa
            if (await _db.OrderDetails.AnyAsync(od => od.ProductId == id))
            {
                TempData["Error"] = "Sản phẩm đã có trong đơn hàng, không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Đổ danh mục vào ViewBag dùng cho thẻ <select>
        private async Task FillCategoryList(int? selectedCategoryId = null)
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .Where(category => category.IsActive)
                .OrderBy(category => category.Name)
                .ToListAsync();

            ViewData["CategoryId"] = new SelectList(categories, nameof(Category.Id), nameof(Category.Name), selectedCategoryId);
        }
    }
}