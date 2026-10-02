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
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Admin/Order - Danh sách đơn hàng
        public async Task<IActionResult> Index()
        {
            var orders = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/Order/Details/5 - Chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Order/Edit/5 - Form cập nhật trạng thái
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Danh sách trạng thái có thể chọn
            FillStatusList(order.Status);

            return View(order);
        }

        // POST: Admin/Order/Edit/5 - Cập nhật trạng thái
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            if (!OrderStatuses.All.Contains(order.Status, StringComparer.Ordinal))
            {
                ModelState.AddModelError(nameof(Order.Status), "Trạng thái đơn hàng không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingOrder = await _db.Orders.FindAsync(id);
                    if (existingOrder == null)
                    {
                        return NotFound();
                    }

                    existingOrder.Status = order.Status;
                    await _db.SaveChangesAsync();
                    TempData["Success"] = "Order updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            FillStatusList(order.Status);

            return View(order);
        }

        // POST: Admin/Order/Delete/5 - Xóa đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Order deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _db.Orders.Any(e => e.Id == id);
        }

        private void FillStatusList(string selectedStatus)
        {
            ViewBag.StatusList = OrderStatuses.All
                .Select(status => new SelectListItem
                {
                    Value = status,
                    Text = status,
                    Selected = string.Equals(status, selectedStatus, StringComparison.Ordinal)
                })
                .ToList();
        }
    }
}