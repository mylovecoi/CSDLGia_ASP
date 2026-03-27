using CSDLGia_ASP.Models.Manages.KeKhaiDangKyGia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSDLGia_ASP.Models.Systems.KetNoiGiaDichVu
{
    public class VMHoSoKeKhaiGia
    {
        public string mahs { get; set; }
        public string macskd { get; set; }
        public string masothue { get; set; }
        public DateTime? ngaynhap { get; set; }
        public string socv { get; set; }
        public string socvlk { get; set; }
        public DateTime? ngaycvlk { get; set; }
        public DateTime? ngayhieuluc { get; set; }
        public string ttnguoinop { get; set; }
        public DateTime? ngaynhan { get; set; }
        public string sohsnhan { get; set; }
        public string ghichu { get; set; }
        public string ngaychuyen { get; set; }
        public string lydo { get; set; }
        public string trangthai { get; set; }
        public string cqcq { get; set; }
        public string dvt { get; set; }
        public string phanloai { get; set; }
        public string plhs { get; set; }
        public string tendn { get; set; }
        public string tencskd { get; set; }
        public string loaihang { get; set; }
        public string thoihan { get; set; }
        public string giaycnhangcs { get; set; }
        public string filedk1 { get; set; }
        public string filedk2 { get; set; }
        public string filedk3 { get; set; }
        public string soqdgiaycnhangcs { get; set; }
        public DateTime? giaycnhangcstungay { get; set; }
        public DateTime? giaycnhangcsdenngaypublic { get; set; }
        public string Chucvuky { get; set; }
        public string Nguoiky { get; set; }
        public string Diachi { get; set; }
        public string Dtll { get; set; }
        public string Tendvhienthi { get; set; }
        public string Diadanh { get; set; }
        //[BindProperty]
        //public DateTime giaycnhangcstungay { get; set; } = DateTime.Parse("0001-01-01");
        //[BindProperty]
        //public DateTime giaycnhangcsdenngaypublic { get; set; } = DateTime.Parse("0001-01-01");
        public List<VMHoSoKeKhaiGia_ChiTiet> ds_cths { get; set;}
        public List<HoSoKeKhaiGia_ChiTiet> HoSoKeKhaiGia_ChiTiet { get; set; }
    }
}
