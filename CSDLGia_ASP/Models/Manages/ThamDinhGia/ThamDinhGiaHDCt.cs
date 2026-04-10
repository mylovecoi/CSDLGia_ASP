using System;
using System.ComponentModel.DataAnnotations;

namespace CSDLGia_ASP.Models.Manages.ThamDinhGia
{
    public class ThamDinhGiaHDCt
    {
        [Key]
        public int Id { get; set; }
        public string MaHoiDong { get; set; }
        public int STT { get; set; }
        public string HoTen { get; set; }
        public string ChucVu { get; set; }      
        public string VaiTro { get; set; }       
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}
