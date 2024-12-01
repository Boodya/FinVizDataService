using Serilog;
using StockMarketAnalyticsService;

var builder = WebApplication.CreateBuilder(args);

ServiceConfigurator.Configure(builder);
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

ServiceConfigurator.PostConfigure(app.Services);

app.UseStaticFiles();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
