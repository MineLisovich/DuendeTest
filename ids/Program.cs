using ids;
using ids.Database;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddW3CLogging(logging =>
{
    // Log all W3C fields
    logging.LoggingFields = W3CLoggingFields.Date | W3CLoggingFields.Time | W3CLoggingFields.Method
                           | W3CLoggingFields.Referer | W3CLoggingFields.Request;

    logging.FileSizeLimit = 5 * 1024 * 1024;
    logging.RetainedFileCountLimit = 2;
    logging.FileName = "LogFile_IDS_";
    logging.LogDirectory = @"D:\logs";
    logging.FlushInterval = TimeSpan.FromSeconds(2);
});
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
var assembly = typeof(ConfigIDS).Assembly.GetName().Name;
builder.Services.AddControllersWithViews();

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
app.UseW3CLogging();
app.UseIdentityServer();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


if (args.Contains("/seed"))
{
    Console.WriteLine("Seeding database...");
    SeedData.EnsureSeedData(app);
    Console.WriteLine("Done seeding database...");
    return;
}

app.Run();
