using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebSite_GioiThieuSP.Data;
using WebSite_GioiThieuSP.Models;

namespace WebSite_GioiThieuSP.Controllers
{
    
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const string CartSessionKey = "Cart";

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /  - home page (karl/index.html)
        public async Task<IActionResult> Index()
        {
            var products = await LoadProductsAsync();
            ViewBag.Categories = await LoadCategoriesAsync();

            ViewBag.Products = products.OrderByDescending(p => p.Id).ToList();

            // The view file is named "index.cshtml" (lowercase) - name it explicitly
            // so it does not depend on the OS being case sensitive.
            return View("index");
        }

        // GET: Product/Shop?categoryId=3&search=dress - product listing, filtered
        public async Task<IActionResult> Shop(int? categoryId, string? search)
        {
            var products = await LoadProductsAsync();
            IEnumerable<Product> query = products;

            if (categoryId is > 0)
            {
                var catId = categoryId.Value;
                query = query.Where(p => p.CategoryId == catId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.Categories = await LoadCategoriesAsync();
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.Search = search;
            ViewBag.Products = query.OrderBy(p => p.Id).ToList();

            return View();
        }

        // GET: Product/ProductDetails/5
        public async Task<IActionResult> ProductDetails(int id)
        {
            // The header menu passes no id -> send the customer to the shop page
            // instead of returning 404.
            if (id <= 0)
            {
                return RedirectToAction(nameof(Shop));
            }

            var products = await LoadProductsAsync();
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Product = product;
            ViewBag.Categories = await LoadCategoriesAsync();

            // Related products: same category, excluding the current one.
            ViewBag.RelatedProducts = products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .OrderBy(p => p.Id)
                .Take(8)
                .ToList();

            // The view file is named "product-details.cshtml" (contains a dash) so the
            // view name must be specified explicitly.
            return View("product-details");
        }

        // ==================== GIỎ HÀNG (CART) ====================

        // GET: Product/Cart - Hiển thị giỏ hàng
        public IActionResult Cart()
        {
            var cart = GetCart();
            ViewBag.CartCount = GetCartCount();
            return View(cart);
        }

        // GET/POST: Product/AddToCart - Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1 || quantity > 99)
            {
                return BadRequest("Số lượng phải từ 1 đến 99");
            }

            var product = (await LoadProductsAsync()).FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound("Sản phẩm không tồn tại");
            }

            var cart = GetCart();

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Image = product.Image,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            SaveCart(cart);

            // Nếu là AJAX request, trả về JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount = GetCartCount(), cartTotal = cart.Sum(c => c.Total) });
            }

            // Quay lại trang trước đó (giữ nguyên trang khi thêm sản phẩm)
            // Nếu không có Referer (ví dụ mở trực tiếp URL), fallback về trang shop.
            return Redirect(GetSafeRedirectUrl(nameof(Shop)));
        }

        // GET: Product/GetCartCount - Lấy số lượng sản phẩm trong giỏ (cho AJAX)
        [HttpGet]
        public IActionResult GetCartCount()
        {
            return Json(new { cartCount = GetCartItemCount(), cartTotal = GetCart().Sum(c => c.Total) });
        }

        // GET: Product/DebugCart - Debug session cart
        [HttpGet]
        public IActionResult DebugCart()
        {
            var cart = GetCart();
            return Json(new { cartCount = cart.Count, items = cart.Select(c => new { c.ProductId, c.ProductName, c.Quantity, c.Price }) });
        }

        // POST: Product/UpdateCart - Cập nhật số lượng (form duy nhất cho toàn bộ giỏ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(int? removeProductId)
        {
            var cart = GetCart();

            if (removeProductId is > 0)
            {
                cart.RemoveAll(item => item.ProductId == removeProductId.Value);
            }

            // Đọc tất cả các giá trị quantity_X từ form
            foreach (var key in Request.Form.Keys)
            {
                if (key.ToString().StartsWith("quantity_"))
                {
                    if (!int.TryParse(key.ToString().Replace("quantity_", ""), out var productId) ||
                        !int.TryParse(Request.Form[key].ToString(), out var quantity))
                    {
                        continue;
                    }

                    var item = cart.FirstOrDefault(c => c.ProductId == productId);
                    if (item != null)
                    {
                        if (quantity > 0)
                        {
                            item.Quantity = Math.Min(quantity, 99);
                        }
                        else
                        {
                            cart.Remove(item);
                        }
                    }
                }
            }

            SaveCart(cart);
            return RedirectToAction(nameof(Cart));
        }

        // POST: Product/RemoveFromCart - Xóa sản phẩm khỏi giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            // Nếu đến từ header dropdown, trở về trang trước đó
            var referer = Request.Headers.Referer.ToString();
            if (!string.IsNullOrWhiteSpace(referer) && referer.Contains("Cart", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction(nameof(Cart));

            return RedirectToAction(nameof(Cart));
        }

        // POST: Product/ClearCart - Xóa toàn bộ giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction(nameof(Cart));
        }

        // POST: Product/RemoveFromCartHeader - Xóa sản phẩm từ header dropdown
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCartHeader(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return Redirect(GetSafeRedirectUrl(nameof(Cart)));
        }

        // GET: Product/Checkout - Hiển thị form checkout
        public IActionResult Checkout()
        {
            var cart = GetCart();

            // Nếu giỏ hàng trống, chuyển về trang giỏ hàng
            if (cart.Count == 0)
            {
                return RedirectToAction(nameof(Cart));
            }

            ViewBag.CartItems = cart;
            ViewBag.CartTotal = cart.Sum(c => c.Total);

            return View();
        }

        // POST: Product/Checkout - Xử lý đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();

            // Nếu giỏ hàng trống, chuyển về trang giỏ hàng
            if (cart.Count == 0)
            {
                return RedirectToAction(nameof(Cart));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CartItems = cart;
                ViewBag.CartTotal = cart.Sum(c => c.Total);
                return View(model);
            }

            // Tạo đơn hàng mới
            var order = new Order
            {
                CustomerName = model.CustomerName,
                Phone = model.PhoneNumber,
                ShippingAddress = model.Address,
                Note = model.Note,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatuses.Pending
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Tạo chi tiết đơn hàng từ giỏ hàng
            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = (int)item.Price
                };

                _db.OrderDetails.Add(orderDetail);
            }

            await _db.SaveChangesAsync();

            // Xóa giỏ hàng sau khi đặt hàng thành công
            HttpContext.Session.Remove(CartSessionKey);

            // Chuyển đến trang xác nhận đơn hàng
            return RedirectToAction(nameof(OrderConfirmation), new { orderId = order.OrderId });
        }

        // GET: Product/OrderConfirmation/5 - Trang xác nhận đơn hàng
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ==================== PRIVATE METHODS ====================

        /// <summary>
        /// Lấy giỏ hàng từ session
        /// </summary>
        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        /// <summary>
        /// Lưu giỏ hàng vào session
        /// </summary>
        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }

        /// <summary>
        /// Trả về Referer an toàn, tránh lỗi khi request mở trực tiếp từ URL không có lịch sử trình duyệt.
        /// </summary>
        private string GetSafeRedirectUrl(string fallbackAction)
        {
            var referer = Request.Headers.Referer.ToString();
            if (Url.IsLocalUrl(referer))
            {
                return referer;
            }

            if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) &&
                string.Equals(refererUri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
            {
                return refererUri.PathAndQuery;
            }

            var fallbackUrl = Url.Action(fallbackAction, "Product");
            return !string.IsNullOrWhiteSpace(fallbackUrl) ? fallbackUrl : "/Product/Shop";
        }

        /// <summary>
        /// Lấy tổng số lượng sản phẩm trong giỏ
        /// </summary>
        private int GetCartItemCount()
        {
            return GetCart().Sum(c => c.Quantity);
        }

        /// <summary>
        /// Danh mục dùng cho sidebar và bộ lọc của trang chủ / shop.
        /// </summary>
        private async Task<List<Category>> LoadCategoriesAsync()
        {
            return await _db.Categories
                .AsNoTracking()
                .OrderBy(category => category.DisplayOrder)
                .ThenBy(category => category.Id)
                .ToListAsync();
        }

        private async Task<List<Product>> LoadProductsAsync()
        {
            return await _db.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .ToListAsync();
        }
    }
}