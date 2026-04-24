using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SfaApp.Web.Models.Domain;
using SfaApp.Web.Models.Identity;

namespace SfaApp.Web.Data;

public static class DbSeeder
{
    private static readonly string[] Roles = ["Admin", "TSI", "SalesRep", "DistributorUser"];

    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@sfa.local";
        const string adminPassword = "Admin@12345";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator",
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }
        }
        else if (!adminUser.IsActive)
        {
            adminUser.IsActive = true;
            var updateResult = await userManager.UpdateAsync(adminUser);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to update admin user: {errors}");
            }
        }

        foreach (var role in new[] { "Admin", "TSI" })
        {
            if (!await userManager.IsInRoleAsync(adminUser, role))
            {
                await userManager.AddToRoleAsync(adminUser, role);
            }
        }

        await dbContext.Database.ExecuteSqlRawAsync("UPDATE `AspNetUsers` SET `IsActive` = 1 WHERE `NormalizedEmail` = 'ADMIN@SFA.LOCAL'");

        var tsiUser = await EnsureUserAsync(userManager, "tsi@sfa.local", "Tsi@12345", "Territory Incharge", "TSI");
        var salesRepUser = await EnsureUserAsync(userManager, "rep@sfa.local", "Rep@12345", "Sales Representative", "SalesRep");
        var distributorUser = await EnsureUserAsync(userManager, "dist@sfa.local", "Dist@12345", "Distributor User", "DistributorUser");

        var distributor = await dbContext.Distributors.FirstOrDefaultAsync(x => x.DistributorCode == "DIST-001");
        if (distributor is null)
        {
            distributor = new Distributor
            {
                DistributorCode = "DIST-001",
                DistributorName = "Primary Distributor",
                ContactPerson = "Distributor Owner",
                MobileNumber = "9000000001",
                Address = "Main Warehouse"
            };

            dbContext.Distributors.Add(distributor);
            await dbContext.SaveChangesAsync();
        }

        var territory = await dbContext.Territories.FirstOrDefaultAsync(x => x.TerritoryCode == "TERR-001");
        if (territory is null)
        {
            territory = new Territory
            {
                TerritoryCode = "TERR-001",
                TerritoryName = "Central Territory",
                TsiUserId = tsiUser.Id
            };
            dbContext.Territories.Add(territory);
            await dbContext.SaveChangesAsync();
        }

        var route = await dbContext.SalesRoutes.FirstOrDefaultAsync(x => x.RouteCode == "RTE-001");
        if (route is null)
        {
            route = new SalesRoute
            {
                RouteCode = "RTE-001",
                RouteName = "Downtown Route",
                TerritoryId = territory.Id,
                DistributorId = distributor.Id
            };

            dbContext.SalesRoutes.Add(route);
            await dbContext.SaveChangesAsync();
        }

        var assignmentExists = await dbContext.RouteAssignments.AnyAsync(x => x.RouteId == route.Id && x.RepUserId == salesRepUser.Id && x.IsActive);
        if (!assignmentExists)
        {
            dbContext.RouteAssignments.Add(new RouteAssignment
            {
                RouteId = route.Id,
                RepUserId = salesRepUser.Id,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true
            });
        }

        var customer1 = await dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerCode == "CUST-001");
        if (customer1 is null)
        {
            customer1 = new Customer
            {
                CustomerCode = "CUST-001"
            };
            dbContext.Customers.Add(customer1);
        }
        customer1.Name = "A-One Retail";
        customer1.OutletCode = "OUT-001";
        customer1.ContactPerson = "Arun";
        customer1.Phone = "9000000002";
        customer1.City = "Mumbai";
        customer1.RouteId = route.Id;
        customer1.TerritoryId = territory.Id;
        customer1.DistributorId = distributor.Id;
        customer1.IsActive = true;
        customer1.Latitude = 19.0760900m;
        customer1.Longitude = 72.8774260m;
        customer1.CoordinateCaptureSource = "admin_manual";

        var customer2 = await dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerCode == "CUST-002");
        if (customer2 is null)
        {
            customer2 = new Customer
            {
                CustomerCode = "CUST-002"
            };
            dbContext.Customers.Add(customer2);
        }
        customer2.Name = "City Mart";
        customer2.OutletCode = "OUT-002";
        customer2.ContactPerson = "Rahul";
        customer2.Phone = "9000000003";
        customer2.City = "Mumbai";
        customer2.RouteId = route.Id;
        customer2.TerritoryId = territory.Id;
        customer2.DistributorId = distributor.Id;
        customer2.IsActive = true;

        if (!await dbContext.Products.AnyAsync(x => x.ProductCode == "PRD-001"))
        {
            dbContext.Products.Add(new Product
            {
                ProductCode = "PRD-001",
                Sku = "SKU-001",
                Name = "SFA Test Product 1",
                Uom = "PCS",
                UnitPrice = 120,
                Mrp = 140
            });
        }

        if (!await dbContext.Products.AnyAsync(x => x.ProductCode == "PRD-002"))
        {
            dbContext.Products.Add(new Product
            {
                ProductCode = "PRD-002",
                Sku = "SKU-002",
                Name = "SFA Test Product 2",
                Uom = "PCS",
                UnitPrice = 80,
                Mrp = 95
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }
}