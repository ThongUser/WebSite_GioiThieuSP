using Microsoft.EntityFrameworkCore;
using WebSite_GioiThieuSP.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/Admin/Account/Login";
        options.AccessDeniedPath = "/Admin/Account/Login";
        options.Cookie.Name = "AdminCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin",
    defaults: new { area = "Admin", controller = "Dashboard", action = "Index" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "store-home-html",
    pattern: "index.html",
    defaults: new { controller = "Product", action = "Index" });

app.MapControllerRoute(
    name: "store-shop-html",
    pattern: "shop.html",
    defaults: new { controller = "Product", action = "Shop" });

app.MapControllerRoute(
    name: "store-product-details-html",
    pattern: "product-details.html",
    defaults: new { controller = "Product", action = "ProductDetails" });

app.MapControllerRoute(
    name: "store-cart-html",
    pattern: "cart.html",
    defaults: new { controller = "Product", action = "Cart" });

app.MapControllerRoute(
    name: "store-checkout-html",
    pattern: "checkout.html",
    defaults: new { controller = "Product", action = "Checkout" });

app.MapControllerRoute(
    name: "store-checkout-legacy-html",
    pattern: "checkout-1.html",
    defaults: new { controller = "Product", action = "Checkout" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
