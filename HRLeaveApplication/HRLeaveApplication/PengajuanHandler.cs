using System;
using System.Collections.Generic;

namespace HRLeaveApplication
{
    public class PengajuanHandler
    {
        private List<Pengajuan<JenisCuti>> daftarPengajuan = new List<Pengajuan<JenisCuti>>();

        // Tabel mapping status (Table-driven)
        private Dictionary<StatusPengajuan, string> pesanStatus = new Dictionary<StatusPengajuan, string>
        {
            { StatusPengajuan.Menunggu, "Pengajuan sedang diproses." },
            { StatusPengajuan.Disetujui, "Pengajuan telah disetujui." },
            { StatusPengajuan.Ditolak, "Pengajuan ditolak." }
        };

        // Method untuk menambah pengajuan
        public void TambahPengajuan(Pengajuan<JenisCuti> pengajuan)
        {
            daftarPengajuan.Add(pengajuan);
            Console.WriteLine($"Pengajuan {pengajuan.Jenis} untuk {pengajuan.NamaKaryawan} berhasil ditambahkan.");
        }

        // Method untuk memproses pengajuan (ubah status)
        public void ProsesPengajuan(int id, StatusPengajuan statusBaru)
        {
            var pengajuan = daftarPengajuan.Find(p => p.Id == id);
            if (pengajuan != null)
            {
                pengajuan.Status = statusBaru;
                Console.WriteLine(pesanStatus[statusBaru]); // table-driven output
            }
            else
            {
                Console.WriteLine("Pengajuan tidak ditemukan.");
            }
        }

        // Tampilkan semua pengajuan
        public void TampilkanSemua()
        {
            foreach (var p in daftarPengajuan)
            {
                Console.WriteLine($"ID: {p.Id}, Nama: {p.NamaKaryawan}, Jenis: {p.Jenis}, Status: {p.Status}");
            }
        }
    }
}
