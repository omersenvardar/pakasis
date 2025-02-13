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
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
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

// ✅ **Middleware Sıralaması (Doğru Kullanım)**
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

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

Console.WriteLine("✅ Uygulama başlatılıyor...");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"🚨 Uygulama başlatılırken hata oluştu: {ex.Message}");
}
