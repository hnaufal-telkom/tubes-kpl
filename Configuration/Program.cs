using System;
using Configuration;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigManage configManager = new ConfigManage();

            AppConfig config = configManager.Config;

            Console.WriteLine("==== Konfigurasi Aplikasi ====");
            Console.WriteLine($"Max Login Attempts: {config.MaxLoginAttempts}");
            Console.WriteLine($"Password Expiry (Days): {config.PasswordExpiryDays}");
            Console.WriteLine($"Default User Role: {config.DefaultUserRole}");
            Console.WriteLine($"Max Cuti Tahunan: {config.MaxCutiTahunan}");
            Console.WriteLine($"Max Cuti Per Pengajuan: {config.MaxCutiPerPengajuan}");
            Console.WriteLine($"Allow Cuti Bersama: {config.AllowCutiBersama}");
            Console.WriteLine($"Minimal Hari Pengajuan Sebelum: {config.MinHariPengajuanSebelum}");


            if (config.AllowCutiBersama)
            {
                Console.WriteLine("Cuti bersama diperbolehkan.");
            }
            else
            {
                Console.WriteLine("Cuti bersama tidak diperbolehkan.");
            }

            Console.WriteLine("Program selesai.");
        }
    }
}

