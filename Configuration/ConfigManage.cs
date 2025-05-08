using System;
using System.IO;
using System.Text.Json;

namespace Configuration
{
    public class ConfigManage
    {
        public AppConfig Config { get; private set; }
        private const string configFilePath = "AppConfig.json";

        /// Constructor - Membaca atau membuat file konfigurasi
        public ConfigManage()
        {
            try
            {
                ReadConfigFile();
            }
            catch (Exception)
            {
                SetDefault();
                WriteConfigFile();
            }
        }

        /// Membaca AppConfig.json dan memuat ke Config
        private void ReadConfigFile()
        {
            string jsonData = File.ReadAllText(configFilePath);
            Config = JsonSerializer.Deserialize<AppConfig>(jsonData);
        }

        /// Menulis konfigurasi ke AppConfig.json
        private void WriteConfigFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Config, options);
            File.WriteAllText(configFilePath, json);
        }

        /// Set nilai default konfigurasi
        private void SetDefault()
        {
            Config = new AppConfig
            {
                // User Config
                MaxLoginAttempts = 5,
                PasswordExpiryDays = 90,
                DefaultUserRole = "User",

                // Cuti Config
                MaxCutiTahunan = 12,
                MaxCutiPerPengajuan = 5,
                AllowCutiBersama = true,
                MinHariPengajuanSebelum = 7
            };
        }
    }
}
