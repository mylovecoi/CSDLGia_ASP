﻿using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CSDLGia_ASP.ViewModels
{
    public class VMImportExcel
    {
        public int Sheet { get; set; }
        public int LineStart { get; set; }
        public int LineStop { get; set; }
        [Required(ErrorMessage = "Thông tin không được bỏ trống")]
        public IFormFile FormFile { get; set; }
        public string MaDv { get; set; }
    }
}