using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.DinhGia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaDatDiaBan
{
    public class GiaDatDiaBanExcelController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public GiaDatDiaBanExcelController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
        }


        public IActionResult Index(string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Create"))
                {
                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBan
                    {
                        Madv = Madv,
                        Mahs = Madv + "_" + DateTime.Now.ToString("yyMMddssmmHH"),
                        Thoidiem = DateTime.Now,
                        Sheet = 1,
                        LineStart = 5,
                        LineStop = 3000,
                    };
                    ViewData["Title"] = "Bảng giá các loại đất";
                    ViewData["MenuLv1"] = "menu_giadat";
                    ViewData["MenuLv2"] = "menu_giadatdiaban";
                    ViewData["MenuLv3"] = "menu_giadatdiaban_tt";
                    ViewData["DsDiaBanHuyen"] = _db.DsDiaBan;
                    ViewData["DsDiaBanXa"] = _db.DsDiaBan.Where(x => x.Level == "X");
                    ViewData["Soqd"] = _db.GiaDatDiaBanTt.ToList();
                    return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/Excels/Excel.cshtml", model);

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
        public async Task<IActionResult> Import(CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBan request, string MaDiaBanHuyen)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                request.LineStart = request.LineStart == 0 ? 1 : request.LineStart;
                var list_add = new List<CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt>();
                int sheet = request.Sheet == 0 ? 0 : (request.Sheet - 1);
                using (var stream = new MemoryStream())
                {
                    await request.FormFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        //ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[request.Sheet - 1];
                        var rowcount = worksheet.Dimension.Rows;
                        request.LineStop = request.LineStop > rowcount ? rowcount : request.LineStop;
                        int stt = 1;

                        for (int row = request.LineStart; row <= request.LineStop; row++)
                        {
                            //string MaDiaBan = request.MaXa == "all" ? (worksheet.Cells[row, 15].Value != null ? worksheet.Cells[row, 15].Value.ToString().Trim() : "") : request.Madiaban;
                            //MaDiaBan = (string.IsNullOrEmpty(MaDiaBan) || MaDiaBan =="all")? MaDiaBanHuyen :MaDiaBan;
                            list_add.Add(new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt
                            {
                                Madiaban = worksheet.Cells[row, 15].Value != null ? worksheet.Cells[row, 15].Value.ToString().Trim() : "",
                                Mahs = request.Mahs,
                                Created_at = DateTime.Now,
                                Sapxep = stt++,
                                HienThi = worksheet.Cells[row, 1].Value != null ? worksheet.Cells[row, 1].Value.ToString().Trim() : "",
                                Mota = worksheet.Cells[row, 2].Value != null ? worksheet.Cells[row, 2].Value.ToString().Trim() : "",
                                Diemdau = worksheet.Cells[row, 3].Value != null ? worksheet.Cells[row, 3].Value.ToString().Trim() : "",
                                Diemcuoi = worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString().Trim() : "",
                                Loaiduong = worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString().Trim() : "",
                                Giavt1 = worksheet.Cells[row, 6].Value != null ? Helpers.ConvertStrToDb(worksheet.Cells[row, 6].Value.ToString().Trim()) : 0,
                                Giavt2 = worksheet.Cells[row, 7].Value != null ? Helpers.ConvertStrToDb(worksheet.Cells[row, 7].Value.ToString().Trim()) : 0,
                                Giavt3 = worksheet.Cells[row, 8].Value != null ? Helpers.ConvertStrToDb(worksheet.Cells[row, 8].Value.ToString().Trim()) : 0,
                                Giavt4 = worksheet.Cells[row, 9].Value != null ? Helpers.ConvertStrToDb(worksheet.Cells[row, 9].Value.ToString().Trim()) : 0,
                                Giavt5 = worksheet.Cells[row, 10].Value != null ? Helpers.ConvertStrToDb(worksheet.Cells[row, 10].Value.ToString().Trim()) : 0,
                                Giavt6 = worksheet.Cells[row, 11].Value != null ? Helpers.ConvertStrToDb(worksheet.Cells[row, 11].Value.ToString().Trim()) : 0,
                                Giavt7 = worksheet.Cells[row, 12].Value != null ? Helpers.ConvertStrToDb(worksheet.Cells[row, 12].Value.ToString().Trim()) : 0,
                                Maloaidat = worksheet.Cells[row, 14].Value != null ? worksheet.Cells[row, 14].Value.ToString().Trim() : "",
                                Giavtconlai = worksheet.Cells[row, 13].Value != null ? Helpers.ConvertStrToDb(worksheet.Cells[row, 13].Value.ToString().Trim()) : 0,
                                Trangthai = "XD",
                                MaDv = request.Madv
                            });
                        }
                    }
                }

                var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBan
                {
                    NoiDungQDTT = request.NoiDungQDTT,
                    Noidung = request.Noidung,
                    Mahs = request.Mahs,
                    Madv = request.Madv,
                    Soqd = request.SoQDTT,
                    SoQDTT = request.SoQDTT,
                    Madiaban = MaDiaBanHuyen,
                    Thoidiem = request.Thoidiem,
                    Trangthai = "CC",
                    Congbo = "CHUACONGBO",
                    Created_at = DateTime.Now,
                    Updated_at = DateTime.Now,
                    MaXa = request.MaXa,
                    GhiChu = request.GhiChu,
                };
                _db.GiaDatDiaBan.Add(model);
                _db.GiaDatDiaBanCt.AddRange(list_add);
                _db.SaveChanges();
                return RedirectToAction("Edit", "GiaDatDiaBan", new { Mahs = request.Mahs });
            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }
        }

    }
}
