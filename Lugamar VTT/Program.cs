using LugamarVTT.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services and our XML data service
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<XmlDataService>();

var app = builder.Build();

// Configure error handling and static file serving
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Default route maps to the Charsheet controller's index action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Charsheet}/{action=Index}/{id?}");

app.Run();