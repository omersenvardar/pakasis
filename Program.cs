using DBGoreWebApp.Data;
using DBGoreWebApp.Services.Arabalar;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ **MySQL Bağlantısını Yapılandır**
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

// ✅ **Oturum ve Yetkilendirme Servisleri**
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(365); // **1 yıl boyunca oturumu açık tut**
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".Pakasis.Session"; // **Özel oturum cookie ismi**
    options.Cookie.SameSite = SameSiteMode.Strict; // **Güvenlik için stric olarak ayarla**
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(365);  // **1 yıl boyunca oturumu açık tut**
        options.SlidingExpiration = true; // **Kullanıcı her işlem yaptığında süresi yenilensin**
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = ".Pakasis.Auth"; // **Özel Authentication Cookie ismi**
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireClaim("KullaniciYetki", "admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireClaim("KullaniciYetki", "üye"));
});

// ✅ **Servisleri Ekleyin**
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ArabaBilgileriServices>();
var app = builder.Build();

// ✅ **Hata Yönetimi**
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ✅ **Middleware Sıralaması**
app.UseStaticFiles();
app.UseRouting();

// ✅ **Session ve Yetkilendirme Middleware'leri**
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ✅ **Resim Yükleme Boyutunu Arttır**
app.Use(async (context, next) =>
{
    var maxRequestSizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (maxRequestSizeFeature != null)
    {
        maxRequestSizeFeature.MaxRequestBodySize = 52428800; // 50 MB
    }
    await next.Invoke();
});

// ✅ **Veritabanından Kategorileri Al ve Cache'e Ekle**
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var kategoriler = dbContext.Kategoriler.ToList();
    app.Use(async (context, next) =>
    {
        context.Items["Kategoriler"] = kategoriler;
        await next.Invoke();
    });
}

// ✅ **Varsayılan Route Tanımlaması**
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{page?}/{pageSize?}");

app.Run();
