﻿using System;
using System.ComponentModel.DataAnnotations;

namespace CSDLGia_ASP.Models.Manages.DinhGia
{
    public class GiaThueTaiSanCongCt
    {
        [Key]
        public int Id { get; set; }
        public string Mataisan { get; set; }
        public double Dongiathue { get; set; }
        public string Dvthue { get; set; }
        public string Hdthue { get; set; }
        public string Ththue { get; set; }
        public double Sotienthuenam { get; set; }
        public string Mahs { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public string Dvt { get; set; }
        public string Diachi { get; set; }
        public string Soqdpd { get; set; }
        public DateTime Thoigianpd { get; set; }
        public string Soqddg { get; set; }
        public DateTime Thoigiandg { get; set; }
        public DateTime Thuetungay { get; set; }
        public DateTime Thuedenngay { get; set; }
    }
}