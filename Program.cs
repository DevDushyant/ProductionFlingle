using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using API.Services;
using API.Interfaces;
using API.Helpers;
using Microsoft.AspNetCore.Identity;
using API.Entities;
using API.SignalR;
using API.Middlewares;
using System.Threading;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);
var policyName = "_myAllowSpecificOrigins";
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });

});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<DataContext>(options =>
            {
                var env=Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                 string connStr;
                 if(env=="Development")
                 {
                     connStr = builder.Configuration.GetConnectionString("DefaultConnection");
                 }   
                 else
                 {
                    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                  connUrl = connUrl.Replace("postgresql://", string.Empty);
                  var pgUserPass = connUrl.Split("@")[0];
                  var pgHostPortDb = connUrl.Split("@")[1];
                  var pgHostPort = pgHostPortDb.Split("/")[0];
                  var pgDb = pgHostPortDb.Split("/")[1];
                  var pgUser = pgUserPass.Split(":")[0];
                  var pgPass = pgUserPass.Split(":")[1];
                  var pgHost = pgHostPort.Split(":")[0];
                  var pgPort = pgHostPort.Split(":")[1];
                  connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;TrustServerCertificate=True";
              }
              options.UseNpgsql(connStr);

                 });


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
                      builder =>
                      {
                          builder
                            .WithOrigins("http://localhost:4200/") // specifying the allowed origin
                            .AllowAnyMethod() // defining the allowed HTTP method
                            .AllowAnyHeader()
                            .SetIsOriginAllowed(origin => true)
                            .AllowCredentials(); // allowing any header to be sent
                      });
});

builder.Services.AddIdentityCore<AppUser>(opt =>
           {
               opt.Password.RequireNonAlphanumeric = false;
           })
               .AddRoles<AppRole>()
               .AddRoleManager<RoleManager<AppRole>>()
               .AddSignInManager<SignInManager<AppUser>>()
               .AddRoleValidator<RoleValidator<AppRole>>()
               .AddEntityFrameworkStores<DataContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("TokenKey").Value)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];


            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }


            return Task.CompletedTask;
        }
    };


});
builder.Services.AddAuthorization(opt =>
            {
                opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });



builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));



builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<LogUserActivity>();

// builder.Services.AddScoped<IUserRepository, UserRepository>();
// builder.Services.AddScoped<ILikesRepository, LikesRepository>();
// builder.Services.AddScoped<IMessageRepository, MessageRepository>();

//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddSingleton<Cloudinary>();
//builder.Services.AddSignalR();
 builder.Services.AddSignalR(e => {
    e.MaximumReceiveMessageSize = 102400000;
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv7 v1"));
}
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.Use(async (context, next) =>
{
    Thread.CurrentPrincipal = context.User;
    await next(context);
});


Configure(app);

async void Configure(WebApplication host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
       
        var context = services.GetRequiredService<DataContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        if(roleManager.Roles==null)
        context.Database.Migrate();
        await Seed.SeedUsers(userManager, roleManager);
    }
    catch (Exception ex)
    {
       
        throw;
    }
}


app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(policyName);
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToController("Index", "Fallback");
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");




app.Run();
