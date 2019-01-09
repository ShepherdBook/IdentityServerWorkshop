using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using IdentityProvider.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityProvider
{
  public class SeedData
  {
    public static void EnsureSeedData(string connectionString)
    {
      var services = new ServiceCollection();
      services.AddDbContext<ApplicationDbContext>(options =>
         options.UseSqlite(connectionString));

      services.AddDefaultIdentity<IdentityUser>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

      using (var serviceProvider = services.BuildServiceProvider())
      using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
      {
        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
        context.Database.Migrate();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var adminUser = userMgr.FindByNameAsync("admin@example.com").Result;
        if (adminUser != null)
        {
          Console.WriteLine("admin already exists");
          return;
        }

        adminUser = new IdentityUser
        {
          UserName = "admin@example.com"
        };
        var result = userMgr.CreateAsync(adminUser, "AwesomePassword4U!").Result;
        if (!result.Succeeded)
        {
          throw new Exception(result.Errors.First().Description);
        }

        result = userMgr.AddClaimsAsync(adminUser, new Claim[]{
            new Claim(JwtClaimTypes.Name, "Admin User"),
            new Claim(JwtClaimTypes.GivenName, "Admin"),
            new Claim(JwtClaimTypes.FamilyName, "User"),
            new Claim(JwtClaimTypes.Email, "admin@example.com"),
            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
            new Claim(JwtClaimTypes.WebSite, "http://example.com"),
            new Claim(JwtClaimTypes.Address, @"{ 'street_address': '350 Fifth Avenue', 'locality': 'New York', 'postal_code': 10118, 'country': 'United States' }", "json")
          }).Result;
        if (!result.Succeeded)
        {
          throw new Exception(result.Errors.First().Description);
        }
        Console.WriteLine("admin created");
      }
    }
  }
}