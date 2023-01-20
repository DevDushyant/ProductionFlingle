using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class Seed
{
    // public static async Task SeedUsers(DataContext context)
    // {
    //     if (await context.Users.AnyAsync()) return;
    //     var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
    //     var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
    //     if (users == null) return;

    //     foreach (var user in users)
    //     {
    //         using var hmac = new HMACSHA512();
    //         user.UserName = user.UserName.ToLower();
    //         user.PasswordSalt = hmac.Key;
    //         user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")).ToString();
    //         await context.Users.AddAsync(user);
    //     }
    //     await context.SaveChangesAsync();
    // }


     public static async Task SeedUsers(UserManager<AppUser> userManager,
           RoleManager<AppRole> roleManager)
       {
           if (await userManager.Users.AnyAsync()) return;


           var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
           var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
           if (users == null) return;


           var roles = new List<AppRole>
           {
               new AppRole{Name = "Member"},
               new AppRole{Name = "Admin"},
               new AppRole{Name = "Moderator"},
           };


           foreach (var role in roles)
           {
               await roleManager.CreateAsync(role);
           }


           foreach (var user in users)
           {
               user.UserName = user.UserName.ToLower();
               await userManager.CreateAsync(user, "Pa$$w0rd");
               await userManager.AddToRoleAsync(user, "Member");
           }


           var admin = new AppUser
           {
               UserName = "admin"
           };


           await userManager.CreateAsync(admin, "Pa$$w0rd");
           await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
       }


}