using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Data;
using SfaApp.Web.Models.Domain;

namespace SfaApp.Web.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ProductsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _dbContext.Products
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(products);
    }

    public IActionResult Create()
    {
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        var skuExists = await _dbContext.Products
            .AnyAsync(x => x.Sku == product.Sku);

        if (skuExists)
        {
            ModelState.AddModelError(nameof(product.Sku), "SKU already exists.");
            return View(product);
        }

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }
}