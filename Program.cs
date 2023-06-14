using System.Configuration;
using System.Net;
using App.Data;
using App.ExtendedMethod;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<PlanetService>();
builder.Services.AddSingleton<ProductService>();

// Đăng ký AppDbContext, sử dụng kết nối đến MS SQL Server
builder.Services.AddDbContext<AppDbContext> (options => {
    string connectstring = builder.Configuration.GetConnectionString ("MyBlogContext");
    options.UseSqlServer (connectstring);
});
// Đăng ký các dịch vụ của Identity
builder.Services.AddIdentity<AppUser, IdentityRole> ()
    .AddEntityFrameworkStores<AppDbContext> ()
    .AddDefaultTokenProviders ();

// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions> (options => {
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true; // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false; // Xác thực số điện thoại

});

// Cấu hình Cookie
builder.Services.ConfigureApplicationCookie (options => {
    // options.Cookie.HttpOnly = true;  
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  
    options.LoginPath = $"/login/";                                 // Url đến trang đăng nhập
    options.LogoutPath = $"/logout/";   
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";   // Trang khi User bị cấm truy cập
});
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Trên 5 giây truy cập lại sẽ nạp lại thông tin User (Role)
    // SecurityStamp trong bảng User đổi -> nạp lại thông tinn Security
    options.ValidationInterval = TimeSpan.FromSeconds(5); 
});

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
