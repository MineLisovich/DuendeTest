using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using ids;
using ids.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
var assembly = typeof(ConfigIDS).Assembly.GetName().Name;
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(conf =>
{
    conf.UseSqlServer(connStr, sql=>sql.MigrationsAssembly(assembly));
});
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer(conf =>
{
    conf.Events.RaiseErrorEvents = true;
    conf.Events.RaiseInformationEvents = true;
    conf.Events.RaiseFailureEvents = true;
    conf.Events.RaiseSuccessEvents = true;
    conf.EmitStaticAudienceClaim = true;
}).AddConfigurationStore(conf => conf.ConfigureDbContext = b => b.UseSqlServer(connStr, sql=> sql.MigrationsAssembly(assembly)))
  .AddOperationalStore(conf => conf.ConfigureDbContext = b => b.UseSqlServer(connStr, sql => sql.MigrationsAssembly(assembly)))
  .AddAspNetIdentity<IdentityUser>();

var app = builder.Build();

app.UseIdentityServer();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

if (args.Contains("/seed"))
{
    Console.WriteLine("Seeding database...");
    SeedData.EnsureSeedData(app);
    Console.WriteLine("Done seeding database...");
    return;
}

app.Run();
