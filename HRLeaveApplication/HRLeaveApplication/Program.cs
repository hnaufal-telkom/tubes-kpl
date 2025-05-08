using System;

namespace HRLeaveApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            PengajuanHandler handler = new PengajuanHandler();

            Console.WriteLine("=== Sistem Pengajuan Cuti ===");
            Console.Write("Masukkan Nama Karyawan: ");
            string nama = Console.ReadLine();

            Console.WriteLine("Pilih Jenis Cuti:");
            foreach (var jenis in Enum.GetValues(typeof(JenisCuti)))
            {
                Console.WriteLine($"{(int)jenis} - {jenis}");
            }

            Console.Write("Masukkan kode Jenis Cuti (angka): ");
            int jenisInput = int.Parse(Console.ReadLine());
            JenisCuti jenisCuti = (JenisCuti)jenisInput;

            Console.Write("Masukkan Tanggal Mulai (yyyy-MM-dd): ");
            DateTime tglMulai = DateTime.Parse(Console.ReadLine());

            Console.Write("Masukkan Tanggal Selesai (yyyy-MM-dd): ");
            DateTime tglSelesai = DateTime.Parse(Console.ReadLine());

            // Buat pengajuan baru
            var pengajuanBaru = new Pengajuan<JenisCuti>
            {
                Id = 1, // Bisa dikembangkan auto increment atau input manual
                NamaKaryawan = nama,
                Jenis = jenisCuti,
                TanggalMulai = tglMulai,
                TanggalSelesai = tglSelesai,
                Status = StatusPengajuan.Menunggu
            };

            handler.TambahPengajuan(pengajuanBaru);

            // Simulasi proses: minta input juga untuk update status
            Console.WriteLine("\n=== Proses Pengajuan ===");
            Console.WriteLine("Pilih Status Baru:");
            foreach (var status in Enum.GetValues(typeof(StatusPengajuan)))
            {
                Console.WriteLine($"{(int)status} - {status}");
            }

            Console.Write("Masukkan kode Status (angka): ");
            int statusInput = int.Parse(Console.ReadLine());
            StatusPengajuan statusBaru = (StatusPengajuan)statusInput;

            handler.ProsesPengajuan(1, statusBaru);

            // Tampilkan semua pengajuan
            Console.WriteLine("\n=== Daftar Semua Pengajuan ===");
            handler.TampilkanSemua();
        }
    }
}
