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
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Category - Danh sách danh mục
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = _db.Categories.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(c => c.Name.Contains(keyword)
                    || (c.Description != null && c.Description.Contains(keyword)));
            }

            if (TryParseStatus(status, out var isActive))
            {
                query = query.Where(c => c.IsActive == isActive);
            }

            var categories = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.ProductCounts = await _db.Products
                .AsNoTracking()
                .GroupBy(product => product.CategoryId)
                .Select(group => new { CategoryId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.CategoryId, item => item.Count);
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.StatusList = BuildStatusList(status);

            return View(categories);
        }

        // GET: /Admin/Category/Create
        public IActionResult Create()
        {
            var category = new Category { IsActive = true };
            FillStatusList(category);
            return View(category);
        }

        // POST: /Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            category.Name = (category.Name ?? string.Empty).Trim();

            // Không cho trùng tên danh mục
            if (await IsDuplicateNameAsync(category.Name, null))
            {
                ModelState.AddModelError(nameof(Category.Name), "Tên danh mục đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }

            FillStatusList(category);
            return View(category);
        }

        // GET: /Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            FillStatusList(category);
            return View(category);
        }

        // POST: /Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            category.Name = (category.Name ?? string.Empty).Trim();

            var existing = await _db.Categories.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (await IsDuplicateNameAsync(category.Name, id))
            {
                ModelState.AddModelError(nameof(Category.Name), "Tên danh mục đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                // Chỉ cập nhật các trường cho phép, tránh ghi đè dữ liệu khác
                existing.Name = category.Name;
                existing.Description = category.Description;
                existing.DisplayOrder = category.DisplayOrder;
                existing.IsActive = category.IsActive;

                await _db.SaveChangesAsync();
                TempData["Success"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }

            category.Id = id;
            FillStatusList(category);
            return View(category);
        }

        // GET: /Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var productCount = await _db.Products.CountAsync(p => p.CategoryId == id);

            ViewBag.ProductCount = productCount;
            return View(category);
        }

        // POST: /Admin/Category/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Danh mục còn sản phẩm thì không cho xóa
            if (await _db.Products.AnyAsync(p => p.CategoryId == id))
            {
                TempData["Error"] = "Danh mục đang có sản phẩm, không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xóa danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra trùng tên, bỏ qua chính bản ghi đang sửa
        private async Task<bool> IsDuplicateNameAsync(string categoryName, int? excludeId)
        {
            var query = _db.Categories.AsNoTracking()
                .Where(c => c.Name == categoryName);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        // Đưa danh sách trạng thái vào ViewBag cho thẻ <select>
        private void FillStatusList(Category category)
        {
            ViewBag.StatusList = BuildStatusList(category.IsActive.ToString());
        }

        private List<SelectListItem> BuildStatusList(string? selected = null)
        {
            bool? selectedStatus = TryParseStatus(selected, out var isActive) ? isActive : null;

            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = bool.TrueString,
                    Text = "Hiển thị",
                    Selected = selectedStatus == true
                },
                new SelectListItem
                {
                    Value = bool.FalseString,
                    Text = "Ẩn",
                    Selected = selectedStatus == false
                }
            };
        }

        private static bool TryParseStatus(string? status, out bool isActive)
        {
            if (bool.TryParse(status, out isActive))
            {
                return true;
            }

            if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                isActive = true;
                return true;
            }

            if (string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                isActive = false;
                return true;
            }

            isActive = false;
            return false;
        }
    }
}