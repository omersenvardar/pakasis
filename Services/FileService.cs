using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBGoreWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace DBGoreWebApp.Services
{
    public class FileService
    {
        private readonly ApplicationDbContext _context;

        public FileService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Genel amaçlı silme işlemi. Belirtilen modeldeki `Id` ile eşleşen kayıtları `IsDeleted` olarak işaretler.
        /// </summary>
        /// <typeparam name="T">Silme işlemi yapılacak Entity tipi</typeparam>
        /// <param name="ids">Silinecek kayıtların ID listesi</param>
        /// <returns></returns>
        public async Task SilKayitlariIsDeletedAsync<T>(string[] ids) where T : class
        {
            try
            {
                Console.WriteLine($"Gelen ID'ler: {string.Join(", ", ids)}");

                // ID'leri integer listesine çevir
                var silinecekIdler = ids
                    .SelectMany(idStr => idStr.Split(',')) // Virgülle ayrılmış ID'leri ayır
                    .Select(idStr => int.TryParse(idStr.Trim(), out int id) ? id : (int?)null) // Trim ve int parse işlemi
                    .Where(id => id.HasValue) // Null olanları filtrele
                    .Select(id => id.Value) // Nullable int'i normal int'e çevir
                    .ToList();

                if (!silinecekIdler.Any())
                {
                    Console.WriteLine("Silinecek ID'ler bulunamadı.");
                    return;
                }

                Console.WriteLine($"Silinecek ID'ler: {string.Join(", ", silinecekIdler)}");

                // Dinamik olarak sorgu oluştur
                var dbSet = _context.Set<T>();
                var kayitlar = await dbSet
                    .Where(e => silinecekIdler.Contains((int)EF.Property<object>(e, "Id"))) // Id'yi dinamik al
                    .ToListAsync();

                if (kayitlar.Any())
                {
                    foreach (var kayit in kayitlar)
                    {
                        // IsDeleted alanını byte olarak ayarla
                        var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
                        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(byte))
                        {
                            isDeletedProperty.SetValue(kayit, (byte)1); // Byte değer ataması
                        }
                        else if (isDeletedProperty != null)
                        {
                            isDeletedProperty.SetValue(kayit, 1); // Eğer int veya başka bir tipse
                        }

                        Console.WriteLine($"Kayit IsDeleted olarak işaretlendi: {kayit}");
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine("Silme işlemi başarıyla tamamlandı.");
                }
                else
                {
                    Console.WriteLine("Eşleşen kayıt bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }
        public async Task YukleYeniResimlerAsync<T>(List<IFormFile> yeniResimler, int arsaId, string uploadPath) where T : class
        {
            if (yeniResimler == null || !yeniResimler.Any())
            {
                Console.WriteLine("Yüklenecek yeni resim bulunamadı.");
                return;
            }

            foreach (var resim in yeniResimler)
            {
                try
                {
                    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(resim.FileName);
                    var relativePath = uploadPath.Replace("wwwroot/", ""); // wwwroot kısmını kaldırıyoruz
                    var path = Path.Combine(Directory.GetCurrentDirectory(), uploadPath, uniqueFileName);

                    // Resmi belirtilen dizine kaydet
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await resim.CopyToAsync(stream);
                    }

                    // Resim kaydını veritabanına ekle
                    var yeniResim = Activator.CreateInstance<T>();
                    var resimUrl = $"/{relativePath}/{uniqueFileName}".Replace("\\", "/");

                    var properties = typeof(T).GetProperties();

                    var arsaIdProperty = properties.FirstOrDefault(p => p.Name == "ArsaId" || p.Name == "ArabaId");
                    var resimUrlProperty = properties.FirstOrDefault(p => p.Name == "ResimArsaUrl" || p.Name == "ResimArabaUrl");
                    var resimAdiProperty = properties.FirstOrDefault(p => p.Name == "ResimAdi");
                    var isDeletedProperty = properties.FirstOrDefault(p => p.Name == "IsDeleted");

                    if (arsaIdProperty != null) arsaIdProperty.SetValue(yeniResim, arsaId);
                    if (resimUrlProperty != null) resimUrlProperty.SetValue(yeniResim, resimUrl);
                    if (resimAdiProperty != null) resimAdiProperty.SetValue(yeniResim, Path.GetFileName(resim.FileName));

                    // IsDeleted özelliği için veri tipi kontrolü ve atama
                    if (isDeletedProperty != null)
                    {
                        if (isDeletedProperty.PropertyType == typeof(byte))
                        {
                            isDeletedProperty.SetValue(yeniResim, (byte)0);
                        }
                        else if (isDeletedProperty.PropertyType == typeof(int))
                        {
                            isDeletedProperty.SetValue(yeniResim, 0);
                        }
                    }

                    _context.Set<T>().Add(yeniResim);
                    Console.WriteLine($"Yeni resim eklendi: {resimUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Resim yüklenirken hata oluştu: {ex.Message}");
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("Resim kayıtları başarıyla veritabanına kaydedildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veritabanına kaydedilirken hata oluştu: {ex.Message}");
            }
        }

    }
}
