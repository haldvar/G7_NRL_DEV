using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NRL_PROJECT.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add 
builder.Services.AddDbContext<NRL_Db_Context>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

//Henter connection string fra “appsettings.json” filen
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Oppretter en instans av MySqlConnection 
//builder.Services.AddSingleton(new MySqlConnection(connectionString));

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
app.UseAuthorization();

//app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

/*

// In-memory list to store todos - just testing
var todos = new List<Todo>();



public record Todo(int Id, string Name, DateTime DueDate, bool IsCompleted);
*/


/*
 * Documentation NRL_PROJECT - ASP.NET Core MVC with MySQL/MariaDB by Group7
 * 
 * 
 * 
 */
