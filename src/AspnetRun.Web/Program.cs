using AspnetRun.Application.Interfaces;
using AspnetRun.Application.Mapper;
using AspnetRun.Application.Services;
using AspnetRun.Core.Configuration;
using AspnetRun.Core.Interfaces;
using AspnetRun.Core.Repositories;
using AspnetRun.Core.Repositories.Base;
using AspnetRun.Infrastructure.Data;
using AspnetRun.Infrastructure.Logging;
using AspnetRun.Infrastructure.Repository;
using AspnetRun.Infrastructure.Repository.Base;
using AspnetRun.Web.HealthChecks;
using AspnetRun.Web.Interfaces;
using AspnetRun.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using AspnetRun.Web.Mapper;

var builder = WebApplication.CreateBuilder(args);

// Add Core Layer
builder.Services.Configure<AspnetRunSettings>(builder.Configuration);

// Add Infrastructure Layer
builder.Services.AddDbContext<AspnetRunContext>(c =>
    c.UseInMemoryDatabase("AspnetRunConnection"));

//// use real database
//builder.Services.AddDbContext<AspnetRunContext>(c =>
//    c.UseSqlServer(builder.Configuration.GetConnectionString("AspnetRunConnection"), x => x.MigrationsAssembly("AspnetRun.Web")));

builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddDefaultUI()
    .AddEntityFrameworkStores<AspnetRunContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<ICompareRepository, CompareRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Add Application Layer
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistService, WishListService>();
builder.Services.AddScoped<ICompareService, CompareService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Add Web Layer
builder.Services.AddAutoMapper(cfg =>
{
    cfg.ShouldMapProperty = p => p.GetMethod!.IsPublic || p.GetMethod.IsAssembly;
    cfg.AddProfile<AspnetRunDtoMapper>();
    cfg.AddProfile<AspnetRunProfile>();
});
builder.Services.AddScoped<IIndexPageService, IndexPageService>();
builder.Services.AddScoped<IProductPageService, ProductPageService>();
builder.Services.AddScoped<ICategoryPageService, CategoryPageService>();
builder.Services.AddScoped<ICartComponentService, CartComponentService>();
builder.Services.AddScoped<IWishlistPageService, WishlistPageService>();
builder.Services.AddScoped<IComparePageService, ComparePageService>();
builder.Services.AddScoped<ICheckOutPageService, CheckOutPageService>();

// Add Miscellaneous
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks()
    .AddCheck<IndexPageHealthCheck>("home_page_health_check");

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var aspnetRunContext = services.GetRequiredService<AspnetRunContext>();
        AspnetRunContextSeed.SeedAsync(aspnetRunContext, loggerFactory).Wait();
    }
    catch (Exception exception)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(exception, "An error occurred seeding the DB.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
