﻿using CSDLGia_ASP.Models.Manages.DinhGia;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSDLGia_ASP.Models.Manages.ThamDinhGia
{
    public class ThamDinhGia
    {
        [Key]
        public int Id { get; set; }
        public string Mahs { get; set; }
        public string Madiaban { get; set; }
        public string Maxp { get; set; }
        public string Diadiem { get; set; }
        public string Ppthamdinh { get; set; }
        public string Mucdich { get; set; }
        public string Dvyeucau { get; set; }
        public string Dvthamdinh { get; set; }
        public DateTime Thoihan { get; set; }
        public string Sotbkl { get; set; }
        public string Hosotdgia { get; set; }
        public string Nguonvon { get; set; }
        public string Phanloai { get; set; }
        public string Soqdpheduyet { get; set; }
        public DateTime Ngayqdpheduyet { get; set; }
        public string Quy { get; set; }
        public string Thuevat { get; set; }
        public int Songaykq { get; set; }
        public string Tttstd { get; set; }
        public string Congbo { get; set; }
        public string Lichsu { get; set; }
        public string Thaotac { get; set; }
        public string Ghichu { get; set; }
        public DateTime Thoidiem { get; set; }
        public string Macqcq { get; set; }
        public string Madv { get; set; }
        public string Lydo { get; set; }
        public string Thongtin { get; set; }
        public string Trangthai { get; set; }
        public DateTime Thoidiem_h { get; set; }
        public string Macqcq_h { get; set; }
        public string Madv_h { get; set; }
        public string Lydo_h { get; set; }
        public string Thongtin_h { get; set; }
        public string Trangthai_h { get; set; }
        public DateTime Thoidiem_t { get; set; }
        public string Macqcq_t { get; set; }
        public string Madv_t { get; set; }
        public string Lydo_t { get; set; }
        public string Thongtin_t { get; set; }
        public string Trangthai_t { get; set; }
        public DateTime Thoidiem_ad { get; set; }
        public string Macqcq_ad { get; set; }
        public string Madv_ad { get; set; }
        public string Lydo_ad { get; set; }
        public string Thongtin_ad { get; set; }
        public string Trangthai_ad { get; set; }
        //Trang thái kết nối CSDLQG
        public string TrangThaiCSDLQG { get; set; }
        public DateTime NgayKetNoi { get; set; }
        public string Ipf1 { get; set; }
        [NotMapped]
        public IFormFile Ipf1upload { get; set; }
        public string Ipf2 { get; set; }
        [NotMapped]
        public IFormFile Ipf2upload { get; set; }
        public string Ipf3 { get; set; }
        [NotMapped]
        public IFormFile Ipf3upload { get; set; }
        public string Ipf4 { get; set; }
        [NotMapped]
        public IFormFile Ipf4upload { get; set; }
        public string Ipf5 { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        [NotMapped]
        public List<ThamDinhGiaCt> ThamDinhGiaCt { get; set; }
        [NotMapped]
        public List<ThongTinGiayTo> ThongTinGiayTo { get; set; }
        [NotMapped]
        public string MadvCh { get; set; }
        [NotMapped]
        public string TendvCh { get; set; }
        [NotMapped]
        public string Tencqcq { get; set; }
        [NotMapped]
        public string Tendb { get; set; }
        [NotMapped]
        public string Level { get; set; }
        [NotMapped]
        public string Tennhomhh { get; set; }

        [NotMapped]
        public string TenDonVi { get; set; }       
    }
}
