using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DBGoreWebApp.Data;
using DBGoreWebApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace DBGoreWebApp.Services.Arabalar
{
    public class ArabaBilgileriServices
    {
        private readonly ApplicationDbContext _context;

        public ArabaBilgileriServices(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Belirtilen araba ID'sine göre marka bilgilerini getirir.
        /// </summary>
        /// <param name="arabaId">Araba ID'si</param>
        /// <returns>Marka bilgisi</returns>
        /// <summary>
        /// PHP serialize formatından C# sözlüğüne dönüştürür.
        /// </summary>
        /// <param name="serializedData">PHP serialized veri</param>
        /// <returns>Dictionary<string, object> olarak ayrıştırılmış veri</returns>
        public Dictionary<string, object> ParseSerializedData(string serializedData)
        {
            var result = new Dictionary<string, object>();
            var matches = Regex.Matches(serializedData, @"s:(\d+):""(.*?)"";|i:(\d+);|d:([\d.]+);|N;|b:(\d);");

            foreach (Match match in matches)
            {
                if (match.Groups[2].Success) // String değer
                {
                    result.Add(match.Groups[2].Value, match.Groups[2].Value);
                }
                else if (match.Groups[3].Success) // Integer değer
                {
                    result.Add($"key_{match.Index}", int.Parse(match.Groups[3].Value));
                }
                else if (match.Groups[4].Success) // Double değer
                {
                    result.Add($"key_{match.Index}", double.Parse(match.Groups[4].Value));
                }
                else if (match.Groups[5].Success) // Boolean değer
                {
                    result.Add($"key_{match.Index}", match.Groups[5].Value == "1");
                }
                else // Null değer
                {
                    result.Add($"key_{match.Index}", null);
                }
            }
                return result ?? default!;
        }

        /// <summary>
        /// Özelliklere göre istenilen bir bilgiyi getirir.
        /// </summary>
        /// <param name="parsedData">Ayrıştırılmış veri</param>
        /// <param name="key">Aranacak anahtar</param>
        /// <returns>Anahtara karşılık gelen değer</returns>
        public object GetValueByKey(Dictionary<string, object> parsedData, string key)
        {
            if (parsedData.ContainsKey(key))
            {
                return parsedData[key];
            }
            return null;
        }

        /// <summary>
        /// Örneğin ortalama yakıt tüketimini getirir.
        /// </summary>
        /// <param name="serializedData">PHP serialize edilmiş veri</param>
        /// <returns>Ortalama yakıt tüketimi</returns>
        public double? GetOrtalamaYakit(string serializedData)
        {
            var parsedData = ParseSerializedData(serializedData);
            if (parsedData.ContainsKey("ortalamaYakit"))
            {
                return Convert.ToDouble(parsedData["ortalamaYakit"]);
            }
            return null;
        }

        /// <summary>
        /// Örneğin hızlanma değerini getirir.
        /// </summary>
        /// <param name="serializedData">PHP serialize edilmiş veri</param>
        /// <returns>Hızlanma değeri</returns>
        public double? GetHizlanma(string serializedData)
        {
            var parsedData = ParseSerializedData(serializedData);
            if (parsedData.ContainsKey("hizlanma"))
            {
                return Convert.ToDouble(parsedData["hizlanma"]);
            }
            return null;
        }

        /// <summary>
        /// Tüm bilgileri bir sözlük olarak döner.
        /// </summary>
        /// <param name="serializedData">PHP serialized veri</param>
        /// <returns>Ayrıştırılmış veri</returns>
        public Dictionary<string, object> GetAllProperties(string serializedData)
        {
            return ParseSerializedData(serializedData);
        }
        public async Task<AracMarkalari> GetMarkaBilgileri(int arabaId)
        {
            var araba = await _context.Arabalar.Include(a => a.Marka)
                .FirstOrDefaultAsync(a => a.Id == arabaId);
            return araba?.Marka;
        }

        /// <summary>
        /// Belirtilen araba ID'sine göre model bilgilerini getirir.
        /// </summary>
        /// <param name="arabaId">Araba ID'si</param>
        /// <returns>Model bilgisi</returns>
        public async Task<AracModelListesi> GetModelBilgileri(int arabaId)
        {
            var araba = await _context.Arabalar.Include(a => a.Model)
                .FirstOrDefaultAsync(a => a.Id == arabaId);
            return araba?.Model;
        }

        /// <summary>
        /// Belirtilen araba ID'sine göre yıl bilgilerini getirir.
        /// </summary>
        /// <param name="arabaId">Araba ID'si</param>
        /// <returns>Yıl bilgisi</returns>
        public async Task<AracModelYillari> GetYilBilgileri(int arabaId)
        {
            var araba = await _context.Arabalar.Include(a => a.Yil)
                .FirstOrDefaultAsync(a => a.Id == arabaId);
            return araba?.Yil;
        }

        /// <summary>
        /// Belirtilen araba ID'sine göre versiyon bilgilerini getirir.
        /// </summary>
        /// <param name="arabaId">Araba ID'si</param>
        /// <returns>Versiyon bilgisi</returns>
        public async Task<AracYilVersiyon> GetVersiyonBilgileri(int arabaId)
        {
            var araba = await _context.Arabalar.Include(a => a.Versiyon)
                .FirstOrDefaultAsync(a => a.Id == arabaId);
            return araba?.Versiyon;
        }

        /// <summary>
        /// Tüm marka bilgilerini getirir.
        /// </summary>
        /// <returns>Marka bilgileri listesi</returns>
        public async Task<List<AracMarkalari>> GetAllMarkalar()
        {
            return await _context.AracMarkalaris.ToListAsync();
        }

        /// <summary>
        /// Belirli bir marka ID'sine göre tüm modelleri getirir.
        /// </summary>
        /// <param name="markaId">Marka ID'si</param>
        /// <returns>Model bilgileri listesi</returns>
        public async Task<List<AracModelListesi>> GetModelsByMarka(int markaId)
        {
            return await _context.AracModelListesis
                .Where(m => m.MarkaID == markaId)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir model ID'sine göre tüm yılları getirir.
        /// </summary>
        /// <param name="modelId">Model ID'si</param>
        /// <returns>Yıl bilgileri listesi</returns>
        public async Task<List<AracModelYillari>> GetYillarByModel(int modelId)
        {
            return await _context.AracModelYillaris
                .Where(y => y.ModelID == modelId)
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir yıl ID'sine göre tüm versiyonları getirir.
        /// </summary>
        /// <param name="yilId">Yıl ID'si</param>
        /// <returns>Versiyon bilgileri listesi</returns>
        public async Task<List<AracYilVersiyon>> GetVersiyonlarByYil(int yilId)
        {
            return await _context.AracYilVersiyons
                .Where(v => v.YilID == yilId.ToString())
                .ToListAsync();
        }
    }
}