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
builder.Services.AddSingleton(new MySqlConnection(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NRL_Db_Context>();
    db.Database.Migrate();
}

app.Run();

