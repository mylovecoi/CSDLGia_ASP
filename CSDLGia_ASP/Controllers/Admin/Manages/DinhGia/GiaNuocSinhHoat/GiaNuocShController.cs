using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.DinhGia;
using CSDLGia_ASP.ViewModels.Manages.DinhGia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using CSDLGia_ASP.ViewModels.Systems;
using CSDLGia_ASP.Models.Systems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using OfficeOpenXml.Style;
using System.Text;
using CSDLGia_ASP.Services;
using System.Drawing;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaNuocSinhHoat
{
    public class GiaNuocShController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDsDonviService _dsDonviService;
        private readonly ITrangThaiHoSoService _trangThaiHoSoService;

        public GiaNuocShController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment, IDsDonviService dsDonviService, ITrangThaiHoSoService trangThaiHoSoService)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
            _dsDonviService = dsDonviService;
            _trangThaiHoSoService = trangThaiHoSoService;
        }

        [Route("GiaNuocSinhHoat")]
        [HttpGet]
        public IActionResult Index(string Madv, int Nam)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Index"))
                {
                    Madv = string.IsNullOrEmpty(Madv) ? Helpers.GetSsAdmin(HttpContext.Session, "Madv") : Madv;
                    var model_donvi = _dsDonviService.GetListDonvi(Helpers.GetSsAdmin(HttpContext.Session, "Madv"));
                    List<string> list_madv = model_donvi.Select(t => t.MaDv).ToList();

                    IEnumerable<CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocSh> model = _db.GiaNuocSh.Where(t => list_madv.Contains(t.Madv));
                    if (Madv != "all")
                    {
                        model = model.Where(t => t.Madv == Madv);
                    }

                    if (Nam != 0)
                    {
                        model = model.Where(t => t.Thoidiem.Year == Nam);
                    }


                    ViewData["Nam"] = Nam;
                    ViewData["Madv"] = Madv;
                    ViewData["DsDonvi"] = model_donvi;
                    ViewData["Title"] = " Thông tin giá nước sạch sinh hoạt";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgnsh";
                    ViewData["MenuLv3"] = "menu_dgnsh_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaNuocSh/Index.cshtml", model);
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


        [HttpGet("GiaNuocSh/Create")]
        public IActionResult Create(string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Create"))
                {
                    var modelcxd = _db.GiaNuocShCt.Where(t => t.Trangthai == "CXD" && t.Madv == Madv);

                    if (modelcxd.Any())
                    {
                        _db.GiaNuocShCt.RemoveRange(modelcxd);
                        _db.SaveChanges();
                    }

                    var model_file_cxd = _db.ThongTinGiayTo.Where(t => t.Status == "CXD" && t.Madv == Madv);

                    if (model_file_cxd.Any())
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        foreach (var file in model_file_cxd)
                        {
                            string path_del = Path.Combine(wwwRootPath + "/UpLoad/File/ThongTinGiayTo/", file.FileName);
                            FileInfo fi = new FileInfo(path_del);
                            if (fi != null)
                            {
                                System.IO.File.Delete(path_del);
                                fi.Delete();
                            }
                        }
                        _db.ThongTinGiayTo.RemoveRange(model_file_cxd);
                        _db.SaveChanges();
                    }

                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocSh
                    {
                        Madv = Madv,
                        Thoidiem = DateTime.Now,
                        Mahs = Madv + "_" + DateTime.Now.ToString("yyMMddssmmHH"),
                        Tunam = DateTime.Now.Year.ToString(),
                        Dennam = DateTime.Now.Year.ToString(),
                        PhanLoaiHoSo = "HOSOCHITIET",
                    };

                    var dm = _db.GiaNuocShDmKhung;
                    var ct = new List<CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocShCt>();
                    foreach (var item in dm)
                    {
                        ct.Add(new CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocShCt()
                        {
                            Mahs = model.Mahs,
                            Madv = Madv,
                            STTSapxep = item.STTSapxep,
                            STTHienthi = item.STTHienthi,
                            Doituongsd = item.Doituongsd,
                            Style = item.Style,
                            Trangthai = "CXD",
                            Created_at = DateTime.Now,
                            Updated_at = DateTime.Now,
                        });
                    }

                    _db.GiaNuocShCt.AddRange(ct);
                    _db.SaveChanges();

                    model.GiaNuocShCt = ct.Where(t => t.Mahs == model.Mahs).ToList();

                    ViewData["Madv"] = Madv;
                    ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                    ViewData["Danhmuc"] = _db.GiaNuocShDmKhung.ToList();
                    ViewData["Title"] = "Thêm mới giá nước sạch sinh hoạt";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgnsh";
                    ViewData["MenuLv3"] = "menu_dgnsh_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaNuocSh/Create.cshtml", model);
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

        [HttpGet("GiaNuocSh/NhanExcel")]
        public IActionResult NhanExcel(string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Create"))
                {
                    var modelcxd = _db.GiaNuocShCt.Where(t => t.Trangthai == "CXD" && t.Madv == Madv);
                    if (modelcxd.Any())
                    {
                        _db.GiaNuocShCt.RemoveRange(modelcxd);
                        _db.SaveChanges();
                    }
                    var model_file_cxd = _db.ThongTinGiayTo.Where(t => t.Status == "CXD" && t.Madv == Madv);
                    if (model_file_cxd.Any())
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        foreach (var file in model_file_cxd)
                        {
                            string path_del = Path.Combine(wwwRootPath + "/UpLoad/File/ThongTinGiayTo/", file.FileName);
                            FileInfo fi = new FileInfo(path_del);
                            if (fi != null)
                            {
                                System.IO.File.Delete(path_del);
                                fi.Delete();
                            }
                        }
                        _db.ThongTinGiayTo.RemoveRange(model_file_cxd);
                        _db.SaveChanges();
                    }
                    var model = new CSDLGia_ASP.ViewModels.VMImportExcel
                    {
                        MaDv = Madv,
                        LineStart = 2,
                        LineStop = 100,
                        Sheet = 1
                    };

                    ViewData["Madv"] = Madv;
                    ViewData["Title"] = "Thêm mới giá nước sạch sinh hoạt";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgnsh";
                    ViewData["MenuLv3"] = "menu_dgnsh_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaNuocSh/Excels/Excel.cshtml", model);
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
        public async Task<IActionResult> ImportExcel(CSDLGia_ASP.ViewModels.VMImportExcel requests)
        {
            requests.LineStart = requests.LineStart == 0 ? 1 : requests.LineStart;
            int sheet = requests.Sheet == 0 ? 0 : (requests.Sheet - 1);
            var list_add = new List<CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocShCt>();
            string Mahs = requests.MaDv + "_" + DateTime.Now.ToString("yyMMddssmmHH");
            using (var stream = new MemoryStream())
            {
                await requests.FormFile.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[sheet];
                    var rowcount = worksheet.Dimension.Rows;
                    requests.LineStop = requests.LineStop > rowcount ? rowcount : requests.LineStop;
                    Regex trimmer = new Regex(@"\s\s+"); // Xóa khoảng trắng thừa trong chuỗi
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
                        double percentage;
                        double PhanTram = 0.0;
                        if (double.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out percentage))
                        {
                            PhanTram = percentage;
                        }

                        int line = 1;
                        list_add.Add(new CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocShCt
                        {
                            Mahs = Mahs,
                            Madv = requests.MaDv,
                            Trangthai = "CXD",
                            STTSapxep = line,
                            STTHienthi = worksheet.Cells[row, 1].Value != null ?
                                        worksheet.Cells[row, 1].Value.ToString().Trim() : "",
                            Doituongsd = worksheet.Cells[row, 2].Value != null ?
                                        worksheet.Cells[row, 2].Value.ToString().Trim() : "",
                            TyTrongTieuThu = worksheet.Cells[row, 3].Value != null ?
                                        worksheet.Cells[row, 3].Value.ToString().Trim() : "",
                            SanLuong = worksheet.Cells[row, 4].Value != null ?
                                        worksheet.Cells[row, 4].Value.ToString().Trim() : "",
                            DonGia1 = Helper.Helpers.ConvertStrToDb(worksheet.Cells[row, 5].Value != null ?
                                        worksheet.Cells[row, 5].Value.ToString().Trim() : ""),
                            DonGia2 = PhanTram,
                            Style = strStyle.ToString()
                        });
                        line = line++;
                    }
                }
            }
            _db.GiaNuocShCt.AddRange(list_add);
            _db.SaveChanges();
            var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocSh
            {
                Madv = requests.MaDv,
                Thoidiem = DateTime.Now,
                Mahs = Mahs,
                Tunam = DateTime.Now.Year.ToString(),
                Dennam = DateTime.Now.Year.ToString(),
            };
            var modelct = _db.GiaNuocShCt.Where(t => t.Mahs == Mahs);
            model.GiaNuocShCt = modelct.ToList();
            ViewData["Madv"] = requests.MaDv;
            ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
            ViewData["Danhmuc"] = _db.GiaNuocShDmKhung.ToList();

            ViewData["Title"] = "Thêm mới giá nước sạch sinh hoạt";
            ViewData["MenuLv1"] = "menu_dg";
            ViewData["MenuLv2"] = "menu_dgnsh";
            ViewData["MenuLv3"] = "menu_dgnsh_tt";
            return View("Views/Admin/Manages/DinhGia/GiaNuocSh/Create.cshtml", model);
        }

        [Route("GiaNuocSh/Store")]
        [HttpPost]
        public IActionResult Store(CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocSh request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Create"))
                {
                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocSh
                    {
                        Mahs = request.Mahs,
                        Madv = request.Madv,
                        Madiaban = request.Madiaban,
                        Soqd = request.Soqd,
                        Thoidiem = request.Thoidiem,
                        Mota = request.Mota,
                        Ghichu = request.Ghichu,
                        Trangthai = "CC",
                        Congbo = "CHUACONGBO",
                        CodeExcel = request.CodeExcel,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                    };

                    var modelct = _db.GiaNuocShCt.Where(t => t.Mahs == request.Mahs);
                    if (modelct.Any())
                    {
                        foreach (var item in modelct)
                        {
                            item.Trangthai = "XD";
                        }
                        _db.GiaNuocShCt.UpdateRange(modelct);
                    }
                    var model_file = _db.ThongTinGiayTo.Where(t => t.Mahs == request.Mahs);
                    if (model_file.Any())
                    {
                        foreach (var file in model_file) { file.Status = "XD"; }
                        _db.ThongTinGiayTo.UpdateRange(model_file);
                    }
                    _db.GiaNuocSh.Add(model);
                    _db.SaveChanges();

                    // Sync spreadsheet to GiaNuocShCt
                    SaveSpreadsheetToCt(model.Mahs, model.Madv, model.CodeExcel);
                    _db.SaveChanges();
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Thêm mới");

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Store", "Thêm mới hồ sơ giá nước sinh hoạt");


                    return RedirectToAction("Index", "GiaNuocSh", new { request.Madv });
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

        [Route("GiaNuocSh/Edit")]
        [HttpGet]
        public IActionResult Edit(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Edit"))
                {
                    var model = _db.GiaNuocSh.FirstOrDefault(t => t.Mahs == Mahs);

                    var model_ct = _db.GiaNuocShCt.Where(t => t.Mahs == model.Mahs);
                    var model_file = _db.ThongTinGiayTo.Where(t => t.Mahs == model.Mahs);

                    model.GiaNuocShCt = model_ct.ToList();
                    model.ThongTinGiayTo = model_file.ToList();

                    ViewData["Madv"] = model.Madv;
                    ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                    ViewData["Title"] = "Chỉnh sửa giá nước sinh hoạt";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgnsh";
                    ViewData["MenuLv3"] = "menu_dgnsh_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaNuocSh/Edit.cshtml", model);
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

        [Route("GiaNuocSh/Update")]
        [HttpPost]
        public IActionResult Update(CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocSh request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Edit"))
                {
                    var model = _db.GiaNuocSh.FirstOrDefault(t => t.Mahs == request.Mahs);
                    model.Madiaban = request.Madiaban;
                    model.Soqd = request.Soqd;
                    model.Thoidiem = request.Thoidiem;
                    model.Thongtin = request.Thongtin;
                    model.Ghichu = request.Ghichu;
                    model.Mota = request.Mota;
                    model.CodeExcel = request.CodeExcel;
                    model.Updated_at = DateTime.Now;

                    var modelct = _db.GiaNuocShCt.Where(t => t.Mahs == request.Mahs);
                    if (modelct != null)
                    {
                        foreach (var item in modelct)
                        {
                            item.Mahs = model.Mahs;
                        }
                        _db.GiaNuocShCt.UpdateRange(modelct);
                    }
                    var model_file = _db.ThongTinGiayTo.Where(t => t.Mahs == request.Mahs);
                    if (model_file.Any())
                    {
                        foreach (var file in model_file) { file.Status = "XD"; }
                        _db.ThongTinGiayTo.UpdateRange(model_file);
                    }
                    _db.GiaNuocSh.Update(model);
                    _db.SaveChanges();

                    // Sync spreadsheet to GiaNuocShCt
                    SaveSpreadsheetToCt(model.Mahs, model.Madv, model.CodeExcel);
                    _db.SaveChanges();

                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Cập nhật");
                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Update", "Update hồ sơ giá nước sinh hoạt");

                    return RedirectToAction("Index", "GiaNuocSh", new { request.Madv });
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

        [Route("GiaNuocSh/Delete")]
        [HttpPost]
        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Delete"))
                {
                    var model = _db.GiaNuocSh.FirstOrDefault(t => t.Id == id_delete);
                    if (model != null)
                    {
                        var model_ct = _db.GiaNuocShCt.Where(t => t.Mahs == model.Mahs);
                        if (model_ct.Any())
                        {
                            _db.GiaNuocShCt.RemoveRange(model_ct);
                        }
                        var model_file = _db.ThongTinGiayTo.Where(t => t.Mahs == model.Mahs);
                        if (model_file.Any())
                        {
                            string wwwRootPath = _hostEnvironment.WebRootPath;
                            foreach (var file in model_file)
                            {
                                string path_del = Path.Combine(wwwRootPath + "/UpLoad/File/ThongTinGiayTo/", file.FileName);
                                FileInfo fi = new FileInfo(path_del);
                                if (fi != null)
                                {
                                    System.IO.File.Delete(path_del);
                                    fi.Delete();
                                }
                            }
                            _db.ThongTinGiayTo.RemoveRange(model_file);
                        }

                        _db.GiaNuocSh.Remove(model);
                        _db.SaveChanges();
                    }

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Delete", "Xóa hồ sơ giá nước sinh hoạt");

                    return RedirectToAction("Index", "GiaNuocSh", new { model.Madv });
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

        [Route("GiaNuocSh/Show")]
        [HttpGet]
        public IActionResult Show(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {

                var model = _db.GiaNuocSh.FirstOrDefault(t => t.Mahs == Mahs);
                model.GiaNuocShCt = _db.GiaNuocShCt.Where(t => t.Mahs == model.Mahs).ToList();

                ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                ViewData["DsDonVi"] = _db.DsDonVi.ToList();
                ViewData["Title"] = "Xem chi tiết giá nước sinh hoạt";
                ViewData["MenuLv1"] = "menu_dg";
                ViewData["MenuLv2"] = "menu_dgnsh";
                ViewData["MenuLv3"] = "menu_dgnsh_tt";
                return View("Views/Admin/Manages/DinhGia/GiaNuocSh/Show.cshtml", model);

            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }
        }

        [Route("GiaNuocSh/Printf")]
        [HttpGet]
        public IActionResult Printf(string Nam, string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Index"))
                {
                    var dsdonvi = (from db in _db.DsDiaBan.Where(t => t.Level != "H")
                                   join dv in _db.DsDonVi.Where(t => t.ChucNang != "QUANTRI") on db.MaDiaBan equals dv.MaDiaBan
                                   select new VMDsDonVi
                                   {
                                       Id = dv.Id,
                                       TenDiaBan = db.TenDiaBan,
                                       TenDv = dv.TenDv,
                                       MaDiaBan = dv.MaDiaBan,
                                       MaDv = dv.MaDv,
                                   }).ToList();

                    if (Helpers.GetSsAdmin(HttpContext.Session, "Madv") != null)
                    {
                        Madv = Helpers.GetSsAdmin(HttpContext.Session, "Madv");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Madv))
                        {
                            Madv = dsdonvi.OrderBy(t => t.Id).Select(t => t.MaDv).First();
                        }
                    }

                    var model = _db.GiaNuocSh.Where(t => t.Madv == Madv).ToList();

                    if (string.IsNullOrEmpty(Nam))
                    {
                        model = model.ToList();
                    }
                    else
                    {
                        if (Nam != "all")
                        {
                            model = model.Where(t => t.Thoidiem.Year == int.Parse(Nam)).ToList();
                        }
                        else
                        {
                            model = model.ToList();
                        }
                    }

                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgnsh";
                    ViewData["MenuLv3"] = "menu_dgnsh_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaNuocSh/Printf.cshtml", model);

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

        public IActionResult Complete(string mahs_complete, string trangthai_complete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.thongtin", "Approve"))
                {
                    var model = _db.GiaNuocSh.FirstOrDefault(p => p.Mahs == mahs_complete);
                    model.Trangthai = trangthai_complete;
                    model.Updated_at = DateTime.Now;

                    _db.GiaNuocSh.Update(model);
                    _db.SaveChanges();
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), trangthai_complete);

                    return RedirectToAction("Index", "Gianuocsh", new { Madv = model.Madv, Nam = model.Thoidiem.Year });
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

        [Route("GiaNuocSh/Search")]
        [HttpGet]
        public IActionResult Search(string Madv, DateTime? NgayTu, DateTime? NgayDen, string Mahs, double DonGiaTu, double DonGiaDen, string Doituongsd)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.timkiem", "Index"))
                {
                    DateTime nowDate = DateTime.Now;
                    DateTime firstDayCurrentYear = new DateTime(nowDate.Year, 1, 1);
                    DateTime lastDayCurrentYear = new DateTime(nowDate.Year, 12, 31);

                    Madv = string.IsNullOrEmpty(Madv) ? "all" : Madv;
                    NgayTu = NgayTu.HasValue ? NgayTu : firstDayCurrentYear;
                    NgayDen = NgayDen.HasValue ? NgayDen : lastDayCurrentYear;
                    Mahs = string.IsNullOrEmpty(Mahs) ? "all" : Mahs;
                    DonGiaTu = DonGiaTu == 0 ? 0 : DonGiaTu;
                    DonGiaDen = DonGiaDen == 0 ? 0 : DonGiaDen;
                    Doituongsd = string.IsNullOrEmpty(Doituongsd) ? "" : Doituongsd;

                    var model = (from hosoct in _db.GiaNuocShCt
                                 join hoso in _db.GiaNuocSh on hosoct.Mahs equals hoso.Mahs
                                 join donvi in _db.DsDonVi on hoso.Madv equals donvi.MaDv
                                 select new CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocShCt
                                 {
                                     Madv = hoso.Madv,
                                     Tendv = donvi.TenDv,
                                     SoQD = hoso.Soqd,
                                     ThoiDiem = hoso.Thoidiem,
                                     Doituongsd = hosoct.Doituongsd,
                                     TyTrongTieuThu = hosoct.TyTrongTieuThu,
                                     SanLuong = hosoct.SanLuong,
                                     DonGia1 = hosoct.DonGia1,
                                     DonGia2 = hosoct.DonGia2,
                                     Trangthai = hoso.Trangthai,
                                     Mahs = hoso.Mahs,
                                 });
                    List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
                    model = model.Where(t => t.ThoiDiem >= NgayTu && t.ThoiDiem <= NgayDen && t.DonGia1 >= DonGiaTu || t.DonGia2 >= DonGiaTu && list_trangthai.Contains(t.Trangthai));
                    if (Madv != "all") { model = model.Where(t => t.Madv == Madv); }
                    if (DonGiaDen > 0) { model = model.Where(t => t.DonGia1 <= DonGiaDen || t.DonGia2 <= DonGiaDen); }
                    if (!string.IsNullOrEmpty(Doituongsd))
                    {
                        model = model.Where(t => t.Doituongsd.ToLower().Contains(Doituongsd.ToLower()));
                    }

                    ViewData["Madv"] = Madv;
                    ViewData["NgayTu"] = NgayTu;
                    ViewData["NgayDen"] = NgayDen;
                    ViewData["Mahs"] = Mahs;
                    ViewData["DonGiaTu"] = Helpers.ConvertDbToStr(DonGiaTu);
                    ViewData["DonGiaDen"] = Helpers.ConvertDbToStr(DonGiaDen);
                    ViewData["Doituongsd"] = Doituongsd;
                    ViewData["DanhSachHoSo"] = _db.GiaNuocSh.Where(t => t.Thoidiem >= NgayTu && t.Thoidiem <= NgayDen && list_trangthai.Contains(t.Trangthai));

                    ViewData["DsDiaBan"] = _db.DsDiaBan;
                    ViewData["Cqcq"] = _db.DsDonVi.Where(t => t.ChucNang != "QUANTRI");
                    ViewData["Title"] = "Tìm kiếm thông tin hồ sơ giá nước sinh hoạt";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgnsh";
                    ViewData["MenuLv3"] = "menu_dgnsh_tk";
                    return View("Views/Admin/Manages/DinhGia/GiaNuocSh/TimKiem/Search.cshtml", model);

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

        [HttpGet("GiaNuocSh/PrintSearch")]
        public IActionResult PrintSearch(string Madv, DateTime? NgayTu, DateTime? NgayDen, string Mahs, double DonGiaTu, double DonGiaDen, string Doituongsd)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.nuocsh.timkiem", "Index"))
                {
                    var model = (from hosoct in _db.GiaNuocShCt
                                 join hoso in _db.GiaNuocSh on hosoct.Mahs equals hoso.Mahs
                                 join donvi in _db.DsDonVi on hoso.Madv equals donvi.MaDv
                                 select new CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocShCt
                                 {
                                     Madv = hoso.Madv,
                                     Tendv = donvi.TenDv,
                                     SoQD = hoso.Soqd,
                                     ThoiDiem = hoso.Thoidiem,
                                     Doituongsd = hosoct.Doituongsd,
                                     TyTrongTieuThu = hosoct.TyTrongTieuThu,
                                     SanLuong = hosoct.SanLuong,
                                     DonGia1 = hosoct.DonGia1,
                                     DonGia2 = hosoct.DonGia2,
                                     Trangthai = hoso.Trangthai,
                                     Mahs = hoso.Mahs,
                                 });
                    List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
                    model = model.Where(t => t.ThoiDiem >= NgayTu && t.ThoiDiem <= NgayDen && t.DonGia1 >= DonGiaTu || t.DonGia2 >= DonGiaTu && list_trangthai.Contains(t.Trangthai));
                    if (Madv != "all") { model = model.Where(t => t.Madv == Madv); }
                    if (DonGiaDen > 0) { model = model.Where(t => t.DonGia1 <= DonGiaDen || t.DonGia2 <= DonGiaDen); }
                    if (!string.IsNullOrEmpty(Doituongsd))
                    {
                        model = model.Where(t => t.Doituongsd.ToLower().Contains(Doituongsd.ToLower()));
                    }
                    ViewData["Title"] = "Thông tin tìm kiếm";
                    return View("Views/Admin/Manages/DinhGia/GiaNuocSh/TimKiem/Result.cshtml", model);
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

        [HttpGet("GiaNuocSh/ExportToExcel")]
        public IActionResult ExportToExcel(string Madv, DateTime? NgayTu, DateTime? NgayDen, string Mahs, double DonGiaTu, double DonGiaDen, string Doituongsd)
        {
            var model = (from hosoct in _db.GiaNuocShCt
                         join hoso in _db.GiaNuocSh on hosoct.Mahs equals hoso.Mahs
                         join donvi in _db.DsDonVi on hoso.Madv equals donvi.MaDv
                         select new CSDLGia_ASP.Models.Manages.DinhGia.GiaNuocShCt
                         {
                             Madv = hoso.Madv,
                             Tendv = donvi.TenDv,
                             SoQD = hoso.Soqd,
                             ThoiDiem = hoso.Thoidiem,
                             Doituongsd = hosoct.Doituongsd,
                             TyTrongTieuThu = hosoct.TyTrongTieuThu,
                             SanLuong = hosoct.SanLuong,
                             DonGia1 = hosoct.DonGia1,
                             DonGia2 = hosoct.DonGia2,
                             Trangthai = hoso.Trangthai,
                             Mahs = hoso.Mahs,
                         });

            List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
            model = model.Where(t => t.ThoiDiem >= NgayTu && t.ThoiDiem <= NgayDen && t.DonGia1 >= DonGiaTu || t.DonGia2 >= DonGiaTu && list_trangthai.Contains(t.Trangthai));
            if (Madv != "all") { model = model.Where(t => t.Madv == Madv); }
            if (DonGiaDen > 0) { model = model.Where(t => t.DonGia1 <= DonGiaDen || t.DonGia2 <= DonGiaDen); }
            if (!string.IsNullOrEmpty(Doituongsd))
            {
                model = model.Where(t => t.Doituongsd.ToLower().Contains(Doituongsd.ToLower()));
            }

            // Start exporting to excel
            var stream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var xlPackage = new ExcelPackage(stream))
            {
                // Define a worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Sheet1");

                // Styling
                var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");

                customStyle.Style.Font.UnderLine = true;
                customStyle.Style.Font.Color.SetColor(Color.Red);

                // 1st row
                var record_id = 1;
                var startRow = 3;
                var row = startRow;

                worksheet.Cells["A1"].Value = "Thông tin tìm kiếm hồ sơ giá nước sinh hoạt";
                using (var r = worksheet.Cells[1, 1, 1, 9])
                {
                    r.Merge = true;
                    r.Style.Font.Color.SetColor(Color.Green);
                    r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.Lavender);
                }

                worksheet.Cells["A2"].Value = "STT";
                worksheet.Cells["B2"].Value = "Đơn vị";
                worksheet.Cells["C2"].Value = "Số QĐ";
                worksheet.Cells["D2"].Value = "Thời điểm";
                worksheet.Cells["E2"].Value = "Mục đích sử dụng";
                worksheet.Cells["F2"].Value = "Tỷ trọng tiêu thụ (%)";
                worksheet.Cells["G2"].Value = "Sản lượng(m3)";
                worksheet.Cells["H2"].Value = "Đơn giá chưa bao gồm thuế GTGT(đồng/m3)";
                worksheet.Cells["I2"].Value = "Đơn giá đã bao gồm thuế GTGT(đồng/m3)";
                worksheet.Cells[2, 1, 2, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[2, 1, 2, 9].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                row = 3;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = record_id++;
                    worksheet.Cells[row, 2].Value = item.Tendv;
                    worksheet.Cells[row, 3].Value = item.SoQD;
                    worksheet.Cells[row, 4].Value = Helpers.ConvertDateToStr(item.ThoiDiem);
                    worksheet.Cells[row, 5].Value = item.Doituongsd;
                    worksheet.Cells[row, 6].Value = item.TyTrongTieuThu;
                    worksheet.Cells[row, 7].Value = item.SanLuong;
                    worksheet.Cells[row, 8].Value = Helpers.ConvertDbToStr(item.DonGia1);
                    worksheet.Cells[row, 9].Value = Helpers.ConvertDbToStr(item.DonGia2);

                    row++;
                }

                // Define border styles
                var borderStyle = ExcelBorderStyle.Thin;
                var borderColor = Color.Black;

                // Apply border to header cells
                using (var headerRange = worksheet.Cells["A2:I2"])
                {
                    headerRange.Style.Border.Top.Style = borderStyle;
                    headerRange.Style.Border.Bottom.Style = borderStyle;
                    headerRange.Style.Border.Left.Style = borderStyle;
                    headerRange.Style.Border.Right.Style = borderStyle;
                    headerRange.Style.Border.Top.Color.SetColor(borderColor);
                    headerRange.Style.Border.Bottom.Color.SetColor(borderColor);
                    headerRange.Style.Border.Left.Color.SetColor(borderColor);
                    headerRange.Style.Border.Right.Color.SetColor(borderColor);
                }

                // Apply border to data cells
                for (int i = startRow; i < row; i++)
                {
                    var dataRange = worksheet.Cells[i, 1, i, 9];
                    dataRange.Style.Border.Top.Style = borderStyle;
                    dataRange.Style.Border.Bottom.Style = borderStyle;
                    dataRange.Style.Border.Left.Style = borderStyle;
                    dataRange.Style.Border.Right.Style = borderStyle;
                    dataRange.Style.Border.Top.Color.SetColor(borderColor);
                    dataRange.Style.Border.Bottom.Color.SetColor(borderColor);
                    dataRange.Style.Border.Left.Color.SetColor(borderColor);
                    dataRange.Style.Border.Right.Color.SetColor(borderColor);
                }

                xlPackage.Workbook.Properties.Title = "Thông tin tìm kiếm hồ sơ giá nước sinh hoạt";
                //xlPackage.Workbook.Properties.Author = "Hùng Anh";


                var helpers = new Helpers(worksheet);

                // Căn chỉnh, độ rộng ô,  màu sắc
                helpers.CanChinhExCel(1, ExcelHorizontalAlignment.Center, 10, Color.Black);
                helpers.CanChinhExCel(2, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(3, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(4, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(5, ExcelHorizontalAlignment.Left, 40, Color.Black);
                helpers.CanChinhExCel(6, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(7, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(8, ExcelHorizontalAlignment.Right, 50, Color.Black);
                helpers.CanChinhExCel(9, ExcelHorizontalAlignment.Right, 50, Color.Black);

                xlPackage.Save();
            }

            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Thông tin tìm kiếm hồ sơ giá nước sinh hoạt.xlsx");
        }

        [HttpGet("GiaNuocSh/GetCodeExcel/{Mahs}")]
        public IActionResult GetCodeExcel(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                var model = _db.GiaNuocSh.FirstOrDefault(t => t.Mahs == Mahs);

                // Nếu đã có dữ liệu thì trả về luôn
                if (model != null && !string.IsNullOrEmpty(model.CodeExcel))
                    return Content(model.CodeExcel, "application/json");


                // Chưa có dữ liệu → tạo workbook mặc định từ GiaNuocShDmKhung
                var dmKhung = _db.GiaNuocShDmKhung.OrderBy(t => t.STTSapxep).ToList();

                // Build cellData: key = "rowIndex" (0-based), value = object chứa các cột
                // Cột 0: STTHienthi, Cột 1: Doituongsd, Cột 2: Dongia1, Cột 3: Dongia2
                var cellData = new System.Text.StringBuilder();
                cellData.Append("{");

                // Hàng 0: Header
                cellData.Append("\"0\":{");
                cellData.Append("\"0\":{\"v\":\"STT\",\"s\":{\"bl\":1,\"ht\":2,\"vt\":2,\"tb\":1}},");
                cellData.Append("\"1\":{\"v\":\"Mục đích sử dụng\",\"s\":{\"bl\":1,\"ht\":2,\"vt\":2,\"tb\":1}},");
                cellData.Append("\"2\":{\"v\":\"Tỷ trọng tiêu thụ (%)\",\"s\":{\"bl\":1,\"ht\":2,\"vt\":2,\"tb\":1}},");
                cellData.Append("\"3\":{\"v\":\"Sản lượng (m3)\",\"s\":{\"bl\":1,\"ht\":2,\"vt\":2,\"tb\":1}},");
                cellData.Append("\"4\":{\"v\":\"Đơn giá chưa thuế GTGT (đồng/m3)\",\"s\":{\"bl\":1,\"ht\":2,\"vt\":2,\"tb\":1}},");
                cellData.Append("\"5\":{\"v\":\"Đơn giá đã bao gồm thuế GTGT (đồng/m3)\",\"s\":{\"bl\":1,\"ht\":2,\"vt\":2,\"tb\":1}}");
                cellData.Append("},");

                // Các hàng dữ liệu
                for (int i = 0; i < dmKhung.Count; i++)
                {
                    var item = dmKhung[i];
                    bool isBold = !string.IsNullOrEmpty(item.Style) && item.Style.Contains("Chữ in đậm");
                    string styleJson = isBold ? ",\"s\":{\"bl\":1,\"vt\":2,\"tb\":1}" : ",\"s\":{\"vt\":2,\"tb\":1}";
                    string sttVal = string.IsNullOrEmpty(item.STTHienthi) ? "" : item.STTHienthi.Replace("\"", "\\\"");
                    string doiTuongVal = string.IsNullOrEmpty(item.Doituongsd) ? "" : item.Doituongsd.Replace("\"", "\\\"");

                    cellData.Append($"\"{i + 1}\":{{");
                    cellData.Append($"\"0\":{{\"v\":\"{sttVal}\"{styleJson}}},");
                    cellData.Append($"\"1\":{{\"v\":\"{doiTuongVal}\"{styleJson}}},");
                    cellData.Append($"\"2\":{{\"v\":\"\"{styleJson}}},");
                    cellData.Append($"\"3\":{{\"v\":\"\"{styleJson}}},");
                    cellData.Append($"\"4\":{{\"v\":\"\"{styleJson}}},");
                    cellData.Append($"\"5\":{{\"v\":\"\"{styleJson}}}");
                    cellData.Append("}");
                    if (i < dmKhung.Count - 1) cellData.Append(",");
                }

                cellData.Append("}");

                int totalRows = dmKhung.Count + 1;

                // Tăng chiều rộng cột để hiển thị đủ nội dung
                string defaultWorkbook = $@"{{
                                              ""id"": ""{Mahs}"",
                                              ""name"": ""Bảng giá nước sinh hoạt"",
                                              ""sheetOrder"": [""sheet1""],
                                              ""sheets"": {{
                                                ""sheet1"": {{
                                                  ""id"": ""sheet1"",
                                                  ""name"": ""Sheet1"",
                                                  ""rowCount"": {Math.Max(totalRows + 20, 50)},
                                                  ""columnCount"": 10,
                                                  ""cellData"": {cellData},
                                                  ""rowData"": {{
                                                    ""0"": {{""h"": 80}}
                                                  }},
                                                  ""columnData"": {{
                                                    ""0"": {{""w"": 60}},
                                                    ""1"": {{""w"": 400}},
                                                    ""2"": {{""w"": 150}},
                                                    ""3"": {{""w"": 150}},
                                                    ""4"": {{""w"": 200}},
                                                    ""5"": {{""w"": 200}}
                                                  }}
                                                }}
                                              }},
                                              ""locale"": ""vi-VN""
                                            }}";

                return Content(defaultWorkbook, "application/json");
            }
            else
            {
                return Json(new { status = "error", message = "Phiên đăng nhập kết thúc, Bạn cần đăng nhập lại!!!" });
            }
        }


        [HttpPost("GiaNuocSh/SaveCodeExcel/{Mahs}")]
        public async Task<IActionResult> SaveCodeExcel(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                var model = _db.GiaNuocSh.FirstOrDefault(t => t.Mahs == Mahs);
                if (model == null) return NotFound(new { success = false, message = "Report not found" });

                using (var reader = new StreamReader(Request.Body))
                {
                    var jsonString = await reader.ReadToEndAsync();
                    model.CodeExcel = jsonString;
                    model.Updated_at = DateTime.Now;

                    _db.GiaNuocSh.Update(model);
                    _db.SaveChanges();
                }

                return Json(new { success = true });
            }
            else
            {
                return Json(new { status = "error", message = "Phiên đăng nhập kết thúc, Bạn cần đăng nhập lại!!!" });
            }
        }

        [HttpPost("GiaNuocSh/ConvertExcelToUniver")]
        public async Task<IActionResult> ConvertExcelToUniver(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { status = "error", message = "File không hợp lệ" });

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                        if (worksheet.Dimension == null)
                             return Json(new { status = "error", message = "File Excel không có dữ liệu" });

                        var rowCount = worksheet.Dimension.Rows;
                        var colCount = worksheet.Dimension.Columns;

                        var cellData = new StringBuilder();
                        cellData.Append("{");

                        for (int row = 1; row <= rowCount; row++)
                        {
                            cellData.Append($"\"{row - 1}\":{{");
                            bool hasCell = false;
                            for (int col = 1; col <= colCount; col++)
                            {
                                var cell = worksheet.Cells[row, col];
                                if (cell.Value != null)
                                {
                                    if (hasCell) cellData.Append(",");
                                    string val = cell.Value.ToString().Replace("\"", "\\\"");
                                    bool isBold = cell.Style.Font.Bold;
                                    string styleJson = isBold ? ",\"s\":{\"bl\":1,\"vt\":2,\"tb\":1}" : ",\"s\":{\"vt\":2,\"tb\":1}";
                                    
                                    cellData.Append($"\"{col - 1}\":{{\"v\":\"{val}\"{styleJson}}}");
                                    hasCell = true;
                                }
                            }
                            cellData.Append("}");
                            if (row < rowCount) cellData.Append(",");
                        }
                        cellData.Append("}");

                        string workbookJson = $@"{{
                            ""id"": ""imported_{DateTime.Now.Ticks}"",
                            ""name"": ""Imported Sheet"",
                            ""sheetOrder"": [""sheet1""],
                            ""sheets"": {{
                                ""sheet1"": {{
                                    ""id"": ""sheet1"",
                                    ""name"": ""Sheet1"",
                                    ""rowCount"": {Math.Max(rowCount + 20, 50)},
                                    ""columnCount"": {Math.Max(colCount + 5, 10)},
                                    ""cellData"": {cellData},
                                    ""columnData"": {{
                                        ""0"": {{""w"": 60}},
                                        ""1"": {{""w"": 500}},
                                        ""2"": {{""w"": 300}},
                                        ""3"": {{""w"": 300}}
                                    }}
                                }}
                            }},
                            ""locale"": ""vi-VN""
                        }}";

                        return Content(workbookJson, "application/json");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Lỗi khi xử lý file: " + ex.Message });
            }
        }

        [HttpPost("GiaNuocSh/GetListHoSo")]
        public JsonResult GetListHoSo(DateTime ngaytu, DateTime ngayden)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
                var model = _db.GiaNuocSh.Where(t => t.Thoidiem >= ngaytu && t.Thoidiem <= ngayden && list_trangthai.Contains(t.Trangthai));
                string result = "<select class='form-control' id='Mahs_Search' name='Mahs_Search'>";
                result += "<option value='all'>--Tất cả---</option>";

                if (model.Any())
                {
                    foreach (var item in model)
                    {
                        result += "<option value='" + @item.Mahs + "'>Số QĐ: " + @item.Soqd + " - Thời điểm: " + @Helpers.ConvertDateToStr(item.Thoidiem) + "</option>";
                    }
                }
                result += "</select>";
                var data = new { status = "success", message = result };
                return Json(data);
            }
            else
            {
                var data = new { status = "error", message = "Phiên đăng nhập kết thúc, Bạn cần đăng nhập lại!!!" };
                return Json(data);
            }
        }

        private void SaveSpreadsheetToCt(string mahs, string madv, string codeExcel)
        {
            if (string.IsNullOrEmpty(codeExcel)) return;

            try
            {
                var data = JObject.Parse(codeExcel);
                var sheets = data["sheets"];
                if (sheets == null) return;

                // Lấy sheet đầu tiên
                var firstSheet = sheets.Children<JProperty>().FirstOrDefault()?.Value;
                if (firstSheet == null) return;

                var cellData = firstSheet["cellData"];
                if (cellData == null) return;

                // Xóa dữ liệu cũ của hồ sơ này
                var dataOld = _db.GiaNuocShCt.Where(t => t.Mahs == mahs);
                _db.GiaNuocShCt.RemoveRange(dataOld);

                var listCt = new List<GiaNuocShCt>();
                var rowKeys = cellData.Children<JProperty>()
                    .Select(p => {
                        int r;
                        return int.TryParse(p.Name, out r) ? (int?)r : null;
                    })
                    .Where(r => r.HasValue)
                    .Select(r => r.Value)
                    .OrderBy(k => k);

                // Xác định chỉ số cột dựa trên tiêu đề (dòng 0)
                int idxStt = 0;
                int idxDoituong = 1;
                int idxTyTrong = 2;
                int idxSanLuong = 3;
                int idxGia1 = 4;
                int idxGia2 = 5;

                var headerRow = cellData["0"];
                if (headerRow != null)
                {
                    foreach (JProperty col in headerRow.Children<JProperty>())
                    {
                        string headerText = col.Value["v"]?.ToString()?.ToLower() ?? "";
                        int colIdx = int.Parse(col.Name);

                        if (headerText.Contains("stt") || headerText.Contains("thứ tự")) idxStt = colIdx;
                        else if (headerText.Contains("đối tượng") || headerText.Contains("mục đích")) idxDoituong = colIdx;
                        else if (headerText.Contains("tỷ trọng") || headerText.Contains("tỷ lệ")) idxTyTrong = colIdx;
                        else if (headerText.Contains("sản lượng")) idxSanLuong = colIdx;
                        else if (headerText.Contains("đơn giá") && (headerText.Contains("chưa") || headerText.Contains("trước"))) idxGia1 = colIdx;
                        else if (headerText.Contains("đơn giá") && (headerText.Contains("có") || headerText.Contains("sau") || headerText.Contains("bao gồm"))) idxGia2 = colIdx;
                    }
                }

                foreach (var rowIndex in rowKeys)
                {
                    if (rowIndex == 0) continue; // Bỏ qua tiêu đề

                    var rowData = cellData[rowIndex.ToString()];
                    if (rowData == null) continue;

                    var sttVal = rowData[idxStt.ToString()]?["v"]?.ToString() ?? "";
                    var doituong = rowData[idxDoituong.ToString()]?["v"]?.ToString() ?? "";

                    if (string.IsNullOrEmpty(doituong)) continue;

                    double gia1 = 0;
                    double gia2 = 0;
                    int sttSapXep = 0;

                    var tytrong = rowData[idxTyTrong.ToString()]?["v"]?.ToString() ?? "";
                    var sanluong = rowData[idxSanLuong.ToString()]?["v"]?.ToString() ?? "";
                    var vFull1 = rowData[idxGia1.ToString()]?["v"]?.ToString();
                    var vFull2 = rowData[idxGia2.ToString()]?["v"]?.ToString();

                    if (!string.IsNullOrEmpty(vFull1)) double.TryParse(vFull1, out gia1);
                    if (!string.IsNullOrEmpty(vFull2)) double.TryParse(vFull2, out gia2);
                    // sttSapXep = rowIndex; 
                    sttSapXep = rowIndex; 

                    listCt.Add(new GiaNuocShCt
                    {
                        Mahs = mahs,
                        Madv = madv,
                        STTHienthi = sttVal,
                        STTSapxep = sttSapXep,
                        Doituongsd = doituong,
                        DonGia1 = gia1,
                        DonGia2 = gia2,
                        TyTrongTieuThu = tytrong,
                        SanLuong = sanluong,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                        Trangthai = "XD"
                    });
                }

                if (listCt.Any())
                {
                    _db.GiaNuocShCt.AddRange(listCt);
                }
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây nếu cần
                Console.WriteLine("Error syncing spreadsheet: " + ex.Message);
            }
        }
    }
}
