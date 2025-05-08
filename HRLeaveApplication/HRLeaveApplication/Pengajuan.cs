using System;

namespace HRLeaveApplication
{
    // Class generik untuk pengajuan cuti/dinas
    public class Pengajuan<T>
    {
        public int Id { get; set; }
        public string NamaKaryawan { get; set; }
        public T Jenis { get; set; }
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalSelesai { get; set; }
        public StatusPengajuan Status { get; set; }
    }
}
