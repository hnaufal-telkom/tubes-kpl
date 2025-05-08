namespace Configuration
{

    /// Menyimpan konfigurasi aplikasi: Pengguna dan Pengajuan Cuti
    public class AppConfig
    {
        //  Konfigurasi User 

        /// Batas maksimal percobaan login
        public int MaxLoginAttempts { get; set; }

        /// Jumlah hari sebelum password user harus diganti
        public int PasswordExpiryDays { get; set; }

        /// Role default untuk user baru
        public string DefaultUserRole { get; set; } = "User";

        //  Konfigurasi Pengajuan Cuti 

        /// Maksimal cuti tahunan yang dimiliki user
        public int MaxCutiTahunan { get; set; }

        /// Maksimal cuti per pengajuan
        public int MaxCutiPerPengajuan { get; set; }

        /// Apakah cuti bersama diperbolehkan
        public bool AllowCutiBersama { get; set; }

        /// Minimal hari pengajuan cuti sebelum tanggal cuti
        public int MinHariPengajuanSebelum { get; set; }

        /// Constructor default
        public AppConfig() { }
    }
}
