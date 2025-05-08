namespace Configuration
{

    /// Menyimpan konfigurasi aplikasi: Pengguna dan Pengajuan Cuti
    public class AppConfig
    {
        //  Konfigurasi User 

        public int MaxLoginAttempts { get; set; }

        public int PasswordExpiryDays { get; set; }

        public string DefaultUserRole { get; set; } = "User";


        public int MaxCutiTahunan { get; set; }

        public int MaxCutiPerPengajuan { get; set; }

        public bool AllowCutiBersama { get; set; }

        public int MinHariPengajuanSebelum { get; set; }

        public AppConfig() { }

        public AppConfig(int MaxLoginAttempts, int PasswordExpiryDays, string DefaultUserRole, int MaxCutiTahunan, int MaxCutiPerPengajuan, bool AllowCutiBersama, int MinHariPengajuanSebelum)
        {
            this.MaxLoginAttempts = MaxLoginAttempts;
            this.PasswordExpiryDays = PasswordExpiryDays;
            this.DefaultUserRole = DefaultUserRole;
            this.MaxCutiTahunan = MaxCutiTahunan;
            this.MaxCutiPerPengajuan = MaxCutiPerPengajuan;
            this.AllowCutiBersama = AllowCutiBersama;
            this.MinHariPengajuanSebelum = MinHariPengajuanSebelum;
        }
    }
}
