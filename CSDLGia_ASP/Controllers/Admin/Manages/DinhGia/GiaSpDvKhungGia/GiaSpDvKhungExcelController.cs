﻿using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.DinhGia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaSpDvKhungGia
{
    public class GiaSpDvKhungExcelController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        public GiaSpDvKhungExcelController(CSDLGiaDBContext db)
        {
            _db = db;
        }


        public IActionResult Index(string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.spdvkhunggia.thongtin", "Create"))
                {
                    var model = new GiaSpDvKhungGiaCt
                    {
                        Manhom = "1",
                        Tennhom = "2",
                        Phanloaidv = "3",
                        Dvt = "4",
                        Giatoithieu = 5,
                        Giatoida = 6,

                        LineStart = 2,
                        LineStop = 1000,
                        Sheet = 1,
                    };
                    ViewData["MenuLv1"] = "menu_spdvkhunggia";
                    ViewData["MenuLv2"] = "menu_spdvkhunggia_thongtin";
                    ViewData["Madv"] = Madv;
                    ViewData["Title"] = "Thông tin hồ sơ giá sản phẩm dịch vụ khung giá";
                    return View("Views/Admin/Manages/DinhGia/GiaSpDvKhungGia/Excels/Excel.cshtml", model);

                }
                else
                {
                    ViewData["Messages"] = "Bạn không có quyền truy cập vào chức năng này!";
                    return View("Views/Admin/Error/Page.cshtml");
                }
            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }
        }


        [Route("GiaSpDvKhungGiaExcel/Create")]
        [HttpGet]
        public IActionResult Create(string Madv, string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.spdvkhunggia.thongtin", "Create"))
                {

                    ViewData["Title"] = "Thông tin hồ sơ giá sản phẩm dịch vụ khung giá";
                    ViewData["MenuLv1"] = "menu_spdvkhunggia";
                    ViewData["MenuLv2"] = "menu_spdvkhunggia_thongtin";
                    ViewData["Madv"] = Madv;
                    ViewData["Mahs"] = Mahs;
                    ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                    ViewData["modelct"] = _db.GiaSpDvKhungGiaCt.Where(t => t.Mahs == Mahs);

                    ViewData["GiaSpDvKhungGiaDm"] = _db.GiaSpDvKhungGiaDm.ToList();
                    return View("Views/Admin/Manages/DinhGia/GiaSpDvKhungGia/Excels/Create.cshtml");
                }
                else
                {
                    ViewData["Messages"] = "Bạn không có quyền truy cập vào chức năng này!";
                    return View("Views/Admin/Error/Page.cshtml");
                }
            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(GiaSpDvKhungGiaCt request, string Madv, string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {

                request.LineStart = request.LineStart == 0 ? 1 : request.LineStart;
                var list_add = new List<GiaSpDvKhungGiaCt>();
                int sheet = request.Sheet == 0 ? 0 : (request.Sheet - 1);
                using (var stream = new MemoryStream())
                {
                    await request.FormFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[sheet];
                        var rowcount = worksheet.Dimension.Rows;
                        request.LineStop = request.LineStop > rowcount ? rowcount : request.LineStop;
                        Mahs = request.Madv + "_" + DateTime.Now.ToString("yyMMddssmmHH");
                        for (int row = request.LineStart; row <= request.LineStop; row++)
                        {
  

                            list_add.Add(new GiaSpDvKhungGiaCt
                            {
                              
                                Mahs = Mahs,
                                Trangthai = "CXD",
                                Created_at = DateTime.Now,
                                Updated_at = DateTime.Now,

                                Manhom = worksheet.Cells[row, Int16.Parse(request.Manhom)].Value != null ?
                                            worksheet.Cells[row, Int16.Parse(request.Manhom)].Value.ToString().Trim() : "",

                                Phanloaidv = worksheet.Cells[row, Int16.Parse(request.Phanloaidv)].Value != null ?
                                            worksheet.Cells[row, Int16.Parse(request.Phanloaidv)].Value.ToString().Trim() : "",

                                Dvt = worksheet.Cells[row, Int16.Parse(request.Dvt)].Value != null ?
                                            worksheet.Cells[row, Int16.Parse(request.Dvt)].Value.ToString().Trim() : "",

                                Giatoithieu = worksheet.Cells[row, Int16.Parse(request.Giatoithieu.ToString())].Value != null ?
                                           Convert.ToInt32(worksheet.Cells[row, Int16.Parse(request.Giatoithieu.ToString())].Value) : 0,

                                Giatoida = worksheet.Cells[row, Int16.Parse(request.Giatoida.ToString())].Value != null ?
                                           Convert.ToInt32(worksheet.Cells[row, Int16.Parse(request.Giatoida.ToString())].Value) : 0,
                            });
                        }
                    }

                }
                _db.GiaSpDvKhungGiaCt.AddRange(list_add);
                _db.SaveChanges();
                return RedirectToAction("Create", "GiaSpDvKhungExcel", new { Madv = Madv, Mahs = Mahs });
            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }
        }
    }
}