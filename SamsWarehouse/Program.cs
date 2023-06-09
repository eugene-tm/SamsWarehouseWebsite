using Microsoft.EntityFrameworkCore;
using SamsWarehouse.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MyContext>(c => c.UseSqlServer(builder.Configuration.GetConnectionString("SamsSQL")));

builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromSeconds(300);
    opts.Cookie.IsEssential = true;
    opts.Cookie.HttpOnly = true;
});

builder.Services.AddDistributedMemoryCache();

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

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
