﻿using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.DinhGia;
using CSDLGia_ASP.ViewModels.Manages.DinhGia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using CSDLGia_ASP.Models.Systems;
using CSDLGia_ASP.Services;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaDatPl
{
    public class GiaDatPlExcelController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IDsDiaBanService _IDsDiaBan;

        public GiaDatPlExcelController(CSDLGiaDBContext db,IDsDiaBanService IDsDiaBan)
        {
            _db = db;
            _IDsDiaBan = IDsDiaBan;
        }


        public IActionResult Index(string MadvExcel, string PhanloaiExcel)
        {
            var model = new CSDLGia_ASP.ViewModels.VMImportExcel
            {
                LineStart = 3,
                LineStop = 1000,
                Sheet = 1,
                MaDv = MadvExcel,
                Phanloai = PhanloaiExcel,
            };
            ViewData["DsDiaBanHuyen"] = _db.DsDiaBan;
            ViewData["DsDiaBanXa"] = _db.DsDiaBan.Where(x => x.Level == "X");
            ViewData["MenuLv1"] = "menu_giadat";
            ViewData["MenuLv2"] = "menu_dgdct";
            ViewData["MenuLv3"] = "menu_dgdct_tt";
            ViewData["Title"] = "Thông tin hồ sơ giá các loại đất";
            return View("Views/Admin/Manages/DinhGia/GiaDatPhanLoai/Excels/Excel.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Import(CSDLGia_ASP.ViewModels.VMImportExcel requests,string MaDiaBanGiaDatPhanLoai)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                requests.LineStart = requests.LineStart == 0 ? 1 : requests.LineStart;
                int sheet = requests.Sheet == 0 ? 0 : (requests.Sheet - 1);

                string Mahs = requests.MaDv + "_" + DateTime.Now.ToString("yyMMddssmmHH");
                using (var stream = new MemoryStream())
                {
                    await requests.FormFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[sheet];
                        if (worksheet != null)
                        {
                            var rowcount = worksheet.Dimension.Rows;
                            requests.LineStop = requests.LineStop > rowcount ? rowcount : requests.LineStop;
                            Regex trimmer = new Regex(@"\s\s+"); // Xóa khoảng trắng thừa trong chuỗi
                            var list_add = new List<CSDLGia_ASP.Models.Manages.DinhGia.GiaDatPhanLoaiCt>();
                            int line = 1;
                            for (int row = requests.LineStart; row <= requests.LineStop; row++)
                            {
                                ExcelStyle style = worksheet.Cells[row, 2].Style;
                                // Kiểm tra xem font chữ có được đánh dấu là đậm không
                                bool isBold = style.Font.Bold;
                                // Kiểm tra xem font chữ có được đánh dấu là nghiêng không
                                bool isItalic = style.Font.Italic;
                                StringBuilder strStyle = new StringBuilder();
                                if (isBold) { strStyle.Append("Chữ in đậm,"); }
                                if (isItalic) { strStyle.Append("Chữ in nghiêng,"); }
                                var MaDiaBan = requests.MadiabanBc == "all" ? (worksheet.Cells[row, 4].Value != null ?
                                                worksheet.Cells[row, 4].Value.ToString().Trim() : "") :requests.MadiabanBc;
                                MaDiaBan = (string.IsNullOrEmpty(MaDiaBan) || MaDiaBan == "all") ? MaDiaBanGiaDatPhanLoai : MaDiaBan;

                                list_add.Add(new GiaDatPhanLoaiCt
                                {
                                    MaDiaBan=MaDiaBan,                                    
                                    Mahs = Mahs,
                                    Madv = requests.MaDv,
                                    Trangthai = "CXD",
                                    STTSapXep = line,
                                    Created_at = DateTime.Now,
                                    Updated_at = DateTime.Now,

                                    STTHienThi = worksheet.Cells[row, 1].Value != null ?
                                                worksheet.Cells[row, 1].Value.ToString().Trim() : "",

                                    Khuvuc = worksheet.Cells[row, 2].Value != null ?
                                             worksheet.Cells[row, 2].Value.ToString().Trim() : "",                                 

                                    Giacuthe = worksheet.Cells[row, 3].Value != null ?
                                            Helpers.ConvertStrToDb(worksheet.Cells[row, 3].Value.ToString()) : 0,

                                    Style = strStyle.ToString(),
                                });
                                line++;
                            }
                            _db.GiaDatPhanLoaiCt.AddRange(list_add);
                            _db.SaveChanges();
                        }
                    }
                }

                var model = new CSDLGia_ASP.ViewModels.Manages.DinhGia.VMDinhGiaDat
                {
                    Madiaban= MaDiaBanGiaDatPhanLoai,
                    Maxp=requests.Maxp,
                    Madv = requests.MaDv,
                    Thoidiem = DateTime.Now,
                    Mahs = Mahs,
                    Phanloai = requests.Phanloai,
                };

                var modelct = _db.GiaDatPhanLoaiCt.Where(t => t.Mahs == Mahs);
                model.GiaDatPhanLoaiCt = modelct.ToList();
                var DsXaPhuong = _IDsDiaBan.GetListDsDiaBan(MaDiaBanGiaDatPhanLoai);
                ViewData["DsXaPhuong"] = DsXaPhuong;
                ViewData["TenDiaBan"] = _db.DsDiaBan.FirstOrDefault(x => x.MaDiaBan == MaDiaBanGiaDatPhanLoai).TenDiaBan;
                ViewData["Mahs"] = model.Mahs;
                ViewData["DsDonVi"] = _db.DsDonVi.Where(t => t.ChucNang != "QUANTRI");               
                ViewData["Dmloaidat"] = _db.DmLoaiDat.ToList();                
                ViewData["Title"] = "Thông tin hồ sơ giá các loại đất";
                ViewData["MenuLv1"] = "menu_giadat";
                ViewData["MenuLv2"] = "menu_dgdct";
                ViewData["MenuLv3"] = "menu_dgdct_tt";
                return View("Views/Admin/Manages/DinhGia/GiaDatPhanLoai/Create.cshtml", model);
            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }
        }
    }
}
