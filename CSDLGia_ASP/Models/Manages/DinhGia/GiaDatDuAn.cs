﻿using System;
using System.ComponentModel.DataAnnotations;

namespace CSDLGia_ASP.Models.Manages.DinhGia
{
    public class GiaDatDuAn
    {
        [Key]
        public int Id { get; set; }
        public string Mahuyen { get; set; }
        public string Maxa { get; set; }
        public string Tenduan { get; set; }
        public DateTime Thoidiem { get; set; }
        public string Dientich { get; set; }
        public string Soqd { get; set; }
        public string Loaidat { get; set; }
        public string Tenduong { get; set; }
        public string Loaiduong { get; set; }
        public string Vitri { get; set; }
        public string Qdgiadato { get; set; }
        public string Qdgiadattmdv { get; set; }
        public string Qdgiadatsxkd { get; set; }
        public string Qdgiadatnn { get; set; }
        public string Qdgiadatnuoits { get; set; }
        public string Qdgiadatmuoi { get; set; }
        public string Qdpddato { get; set; }
        public string Qdpddattmdv { get; set; }
        public string Qdpddatsxkd { get; set; }
        public string Qdpddatnn { get; set; }
        public string Qdpddatnuoits { get; set; }
        public string Qdpddatmuoi { get; set; }
        public string Manhomduan { get; set; }
        public string Trangthai { get; set; }
        public string Thaotac { get; set; }
        public string Ghichu { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public string PhanLoaiHoSo { get; set; }//0: Hồ sơ nhập chi tiết; 1: Hồ sơ nhận dữ liệu từ file excel
        public string CodeExcel { get; set; }
    }
}
