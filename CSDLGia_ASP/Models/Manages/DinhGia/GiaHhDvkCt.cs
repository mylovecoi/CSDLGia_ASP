﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CSDLGia_ASP.Models.Manages.DinhGia
{
    public class GiaHhDvkCt
    {
        [Key]
        public int Id { get; set; }
        public string Mahs { get; set; }
        public string Mahhdv { get; set; }
        public double Gialk { get; set; }
        public double Gia { get; set; }
        public string Loaigia { get; set; }
        public string Nguontt { get; set; }
        public string Ghichu { get; set; }
        public string Trangthai { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        [NotMapped]
        public string Tenhhdv { get; set; }
        [NotMapped]
        public string Dacdiemkt { get; set; }
        [NotMapped]
        public string Dvt { get; set; }
        //Show
        public string Manhom { get; set; }
        //Search
        [NotMapped]
        public string Madv { get; set; }
        [NotMapped]
        public string Tendv { get; set; }
        [NotMapped]
        public string Matt { get; set; }
        [NotMapped]
        public string Tentt { get; set; }
        [NotMapped]
        public DateTime Thoidiem { get; set; }
    }
}