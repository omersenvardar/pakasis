using DBGoreWebApp.Data;
using DBGoreWebApp.Services.Arabalar;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Servisleri ekleyin
builder.Services.AddScoped<ArabaBilgileriServices>();
// Oturum hizmetlerini ekleyin
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Yetkilendirme ve kimlik doğrulama ekliyoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Kimlik doğrulama gerektiren yerlere yönlendirme
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetki olmayan yerlere yönlendirme
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Oturum süresi
    });

builder.Services.AddAuthorization(options =>
{
    // Özel yetkilendirme politikaları eklenebilir
    options.AddPolicy("AdminPolicy", policy => policy.RequireClaim("KullaniciYetki", "admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireClaim("KullaniciYetki", "üye"));
});


// Hizmetleri ekleyin
builder.Services.AddControllersWithViews();

// DbContext'i MySQL ile ekleyin
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21)))); // MySQL sürümünüzü belirtin

// Loglama
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();
// resimler 50 mb olacak
app.Use((context, next) =>
{
    context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 52428800; // 50 MB
    return next();
});


// Kategorileri otomatik olarak almak için
var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var kategoriler = dbContext.Kategoriler.ToList();  // Veritabanından kategorileri alın
app.Use(async (context, next) =>
{
    context.Items["Kategoriler"] = kategoriler;  // Kategorileri Items koleksiyonuna ekleyin
    await next.Invoke();
});

// HTTP istek hattını yapılandırın
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware'ları ekliyoruz
app.UseRouting();
app.UseAuthentication(); // Kimlik doğrulama
app.UseAuthorization();  // Yetkilendirme
app.UseSession();

// Varsayılan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
