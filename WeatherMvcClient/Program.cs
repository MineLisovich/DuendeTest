using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging;
using WeatherMvcClient.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddW3CLogging(logging =>
{
    // Log all W3C fields
    logging.LoggingFields = W3CLoggingFields.Date | W3CLoggingFields.Time | W3CLoggingFields.Method
                            | W3CLoggingFields.Referer | W3CLoggingFields.Request;
    


    logging.FileSizeLimit = 5 * 1024 * 1024;
    logging.RetainedFileCountLimit = 2;
    logging.FileName = "LogFile_MVC_";
    logging.LogDirectory = @"D:\logs";
    logging.FlushInterval = TimeSpan.FromSeconds(2);
});
builder.Services.AddControllersWithViews();
builder.Services.Configure<IdentityServerSettings>(builder.Configuration.GetSection("IdentityServerSettings"));
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddAuthentication(conf =>
{
    conf.DefaultScheme = "cookie";
    conf.DefaultChallengeScheme = "oidc";

}).AddCookie("cookie", conf => conf.ExpireTimeSpan = TimeSpan.FromDays(30))
  .AddOpenIdConnect("oidc", conf=>
  {
      conf.Authority = builder.Configuration["InteractiveServerSettings:AuthoriryUrl"];
      conf.ClientId = builder.Configuration["InteractiveServerSettings:ClientId"];
      conf.ClientSecret = builder.Configuration["InteractiveServerSettings:ClientSecret"];
      conf.Scope.Add(builder.Configuration["InteractiveServerSettings:Scopes:0"]);

      conf.ResponseType = "code";
      conf.UsePkce = true;
      conf.ResponseMode = "query";
      conf.SaveTokens = true;
  });
var app = builder.Build();
app.UseW3CLogging();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
