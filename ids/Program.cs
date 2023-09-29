using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using ids;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

builder.Services.AddIdentityServer(conf =>
{
    conf.Events.RaiseErrorEvents = true;
    conf.Events.RaiseInformationEvents = true;
    conf.Events.RaiseFailureEvents = true;
    conf.Events.RaiseSuccessEvents = true;
    conf.EmitStaticAudienceClaim = true;
}).AddTestUsers(ConfigIDS.Users)
  .AddInMemoryClients(ConfigIDS.Clients)
  .AddInMemoryApiResources(ConfigIDS.ApiResources)
  .AddInMemoryApiScopes(ConfigIDS.ApiScopes)
  .AddInMemoryIdentityResources(ConfigIDS.IdentityResources);
var app = builder.Build();

app.UseIdentityServer();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

app.Run();
