﻿using System.ComponentModel.DataAnnotations;
using System;

namespace CSDLGia_ASP.Models.Systems
{
    public class GiaGiaoDichBDSDm
    {
        [Key]
        public int Id { get; set; }
        public string Maloaidat { get; set; }
        public string Loaidat { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
    }
}