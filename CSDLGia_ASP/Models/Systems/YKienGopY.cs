﻿using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSDLGia_ASP.Models.Systems
{
    public class YKienGopY
    {
        public int Id { get; set; }
        public string MaDv { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string Ipf1 { get; set; }
        public DateTime NgayGopY { get; set; }
        public string TenDangNhap { get; set; }
        //Phản hồi
        public string NoiDungPhanHoi { get; set; }
        public string DonViPhanHoi { get; set; }
        public DateTime? NgayPhanHoi { get; set; }

        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}
