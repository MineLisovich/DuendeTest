using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddW3CLogging(logging =>
{
    // Log all W3C fields
    logging.LoggingFields = W3CLoggingFields.Date | W3CLoggingFields.Time | W3CLoggingFields.Method
                            | W3CLoggingFields.Referer | W3CLoggingFields.Request;

    logging.FileSizeLimit = 5 * 1024 * 1024;
    logging.RetainedFileCountLimit = 2;
    logging.FileName = "LogFile_API_";
    logging.LogDirectory = @"D:\logs";
    logging.FlushInterval = TimeSpan.FromSeconds(2);
});
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(conf=>
    {
        conf.Authority = "https://localhost:5443";
        conf.Audience = "weatherApiResurs";

        conf.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
    });

var app = builder.Build();
app.UseW3CLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
