using System.Net;
using App.ExtendedMethod;
using App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<PlanetService>();
builder.Services.AddSingleton<ProductService>();

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
// app.AddStatusCodePage();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(enpoint =>{
    // /hello
    enpoint.MapGet("/Hello", async (context) => {
        await context.Response.WriteAsync("Hello");
    });

    enpoint.MapRazorPages();
    enpoint.MapControllerRoute(
        name: "First",
        pattern: "productInfo/{id?}",
        defaults: new {
            controller = "First",
            action = "View"
        }

    );

});

app.MapAreaControllerRoute(
    name: "product",
    pattern: "/{area}/{controller}/{action=Index}/{id?}",
    areaName: "ProductManage"
    
);
app.MapControllerRoute(
    name: "default",
    pattern: "/{controller=Home}/{action=Index}/{id?}");

app.Run();
