using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebSite_GioiThieuSP.Data;
using WebSite_GioiThieuSP.Models;

namespace WebSite_GioiThieuSP.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
	private readonly ApplicationDbContext _db;

	public ProductController(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IActionResult> Index()
	{
		var products = await _db.Products
			.AsNoTracking()
			.Include(product => product.Category)
			.OrderByDescending(product => product.Id)
			.ToListAsync();

		return View(products);
	}

	public async Task<IActionResult> Create()
	{
		await LoadCategoriesAsync();
		return View(new Product());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(Product product)
	{
		if (!ModelState.IsValid)
		{
			await LoadCategoriesAsync(product.CategoryId);
			return View(product);
		}

		_db.Products.Add(product);
		await _db.SaveChangesAsync();
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Edit(int id)
	{
		var product = await _db.Products.FindAsync(id);
		if (product == null)
		{
			return NotFound();
		}

		await LoadCategoriesAsync(product.CategoryId);
		return View(product);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, Product input)
	{
		if (id != input.Id)
		{
			return NotFound();
		}

		if (!ModelState.IsValid)
		{
			await LoadCategoriesAsync(input.CategoryId);
			return View(input);
		}

		var product = await _db.Products.FindAsync(id);
		if (product == null)
		{
			return NotFound();
		}

		product.Name = input.Name;
		product.Price = input.Price;
		product.Description = input.Description;
		product.ImageUrl = input.ImageUrl;
		product.CategoryId = input.CategoryId;
		product.IsActive = input.IsActive;

		await _db.SaveChangesAsync();
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(int id)
	{
		var product = await _db.Products.FindAsync(id);
		if (product != null)
		{
			_db.Products.Remove(product);
			await _db.SaveChangesAsync();
		}

		return RedirectToAction(nameof(Index));
	}

	private async Task LoadCategoriesAsync(int? selectedCategoryId = null)
	{
		var categories = await _db.Categories
			.AsNoTracking()
			.Where(category => category.IsActive)
			.OrderBy(category => category.Name)
			.ToListAsync();

		ViewData["CategoryId"] = new SelectList(categories, nameof(Category.Id), nameof(Category.Name), selectedCategoryId);
	}
}
