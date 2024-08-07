﻿using CSDLGia_ASP.Database;
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
using CSDLGia_ASP.Services;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaHangHoaTaiSieuThi
{
    public class GiaHangHoaTaiSieuThiController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDsDonviService _dsDonviService;
        private readonly ITrangThaiHoSoService _trangThaiHoSoService;

        public GiaHangHoaTaiSieuThiController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment, IDsDonviService dsDonviService, ITrangThaiHoSoService trangThaiHoSoService)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
            _dsDonviService = dsDonviService;
            _trangThaiHoSoService = trangThaiHoSoService;
        }

        [Route("GiaHangHoaTaiSieuThi")]
        [HttpGet]
        public IActionResult Index(string Madv, string Nam, string Thang)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Index"))
                {
                    Madv = string.IsNullOrEmpty(Madv) ? Helpers.GetSsAdmin(HttpContext.Session, "Madv") : Madv;
                    var model_donvi = _dsDonviService.GetListDonvi(Helpers.GetSsAdmin(HttpContext.Session, "Madv"));
                    List<string> list_madv = model_donvi.Select(t => t.MaDv).ToList();

                    IEnumerable<CSDLGia_ASP.Models.Manages.DinhGia.GiaHangHoaTaiSieuThi> model = _db.GiaHangHoaTaiSieuThi.Where(t => list_madv.Contains(t.Madv));

                    if (Madv != "all")
                    {
                        model = model.Where(t => t.Madv == Madv);
                    }

                    if (string.IsNullOrEmpty(Nam))
                    {
                        Nam = Helpers.ConvertYearToStr(DateTime.Now.Year);
                        model = model.Where(t => t.Nam == Nam);
                    }
                    else
                    {
                        if (Nam != "all")
                        {
                            model = model.Where(t => t.Nam == Nam);
                        }
                    }

                    if (string.IsNullOrEmpty(Thang))
                    {
                        Thang = Helpers.ConvertYearToStr(DateTime.Now.Month);
                        model = model.Where(t => t.Thang == Thang);
                    }
                    else
                    {
                        if (Thang != "all")
                        {
                            model = model.Where(t => t.Thang == Thang);
                        }
                    }

                    ViewData["Nam"] = Nam;
                    ViewData["Thang"] = Thang;
                    ViewData["Madv"] = Madv;
                    ViewData["DsDonvi"] = model_donvi;
                    ViewData["Dstt"] = _db.GiaHangHoaTaiSieuThiDm;
                    ViewData["Title"] = " Thông tin giá hàng hóa tại siêu thị";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgsieuthi";
                    ViewData["MenuLv3"] = "menu_dgsieuthi_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHangHoaTaiSieuThi/Index.cshtml", model);

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

        [Route("GiaHangHoaTaiSieuThi/Create")]
        [HttpGet]
        public IActionResult Create(string MadvBc, string ThangBc, string NamBc, string MattBc)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Create"))
                {
                    var check = _db.GiaHangHoaTaiSieuThiCt.Where(t => t.Trangthai == "CXD" && t.Madv == MadvBc);
                    if (check != null)
                    {
                        _db.GiaHangHoaTaiSieuThiCt.RemoveRange(check);
                        _db.SaveChanges();
                    }

                    var model_file_cxd = _db.ThongTinGiayTo.Where(t => t.Status == "CXD" && t.Madv == MadvBc);
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

                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaHangHoaTaiSieuThi
                    {
                        Matt = MattBc,
                        Madv = MadvBc,
                        Thang = ThangBc,
                        Nam = NamBc,
                        Thoidiem = DateTime.Now,
                        Mahs = MadvBc + "_" + DateTime.Now.ToString("yyMMddssmmHH"),
                    };

                    var dm = _db.GiaHangHoaTaiSieuThiDmCt.Where(t => t.Matt == MattBc).ToList();
                    var ct = new List<GiaHangHoaTaiSieuThiCt>();
                    foreach (var item in dm)
                    {
                        ct.Add(new GiaHangHoaTaiSieuThiCt()
                        {
                            Mahs = model.Mahs,
                            Madv = model.Madv,
                            Mahanghoa = item.Mahanghoa,
                            Tenhanghoa = item.Tenhanghoa,
                            Trangthai = "CXD",
                            Created_at = DateTime.Now,
                            Updated_at = DateTime.Now,
                        });
                    }

                    _db.GiaHangHoaTaiSieuThiCt.AddRange(ct);
                    _db.SaveChanges();

                    model.GiaHangHoaTaiSieuThiCt = ct.Where(t => t.Mahs == model.Mahs).ToList();

                    ViewData["Madv"] = MadvBc;
                    ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                    ViewData["Danhmuc"] = _db.GiaHangHoaTaiSieuThiDm.ToList();
                    ViewData["Title"] = "Thêm mới giá hàng hóa tại siêu thị";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgsieuthi";
                    ViewData["MenuLv3"] = "menu_dgsieuthi_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHangHoaTaiSieuThi/Create.cshtml", model);
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

        [Route("GiaHangHoaTaiSieuThi/Store")]
        [HttpPost]
        public IActionResult Store(CSDLGia_ASP.Models.Manages.DinhGia.GiaHangHoaTaiSieuThi request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Create"))
                {
                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaHangHoaTaiSieuThi
                    {
                        Matt = request.Matt,
                        Mahs = request.Mahs,
                        Madv = request.Madv,
                        Madiaban = request.Madiaban,
                        Soqd = request.Soqd,
                        Thang = request.Thang,
                        Nam = request.Nam,
                        Thoidiem = request.Thoidiem,
                        Mota = request.Mota,
                        Ghichu = request.Ghichu,
                        Trangthai = "CC",
                        Congbo = "CHUACONGBO",
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                    };

                    var modelct = _db.GiaHangHoaTaiSieuThiCt.Where(t => t.Mahs == request.Mahs);
                    if (modelct != null)
                    {
                        foreach (var item in modelct)
                        {
                            item.Trangthai = "XD";
                        }
                    }

                    var modelFile = _db.ThongTinGiayTo.Where(t => t.Mahs == request.Mahs);
                    if (modelFile.Any())
                    {
                        foreach (var file in modelFile) { file.Status = "XD"; }
                    }

                    _db.GiaHangHoaTaiSieuThi.Add(model);
                    _db.GiaHangHoaTaiSieuThiCt.UpdateRange(modelct);
                    _db.ThongTinGiayTo.UpdateRange(modelFile);
                    _db.SaveChanges();
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Thêm mới");

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Store", "Thêm mới hồ sơ giá hàng hóa tại siêu thị");


                    return RedirectToAction("Index", "GiaHangHoaTaiSieuThi", new { request.Madv });
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

        [Route("GiaHangHoaTaiSieuThi/Edit")]
        [HttpGet]
        public IActionResult Edit(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Edit"))
                {
                    var model = _db.GiaHangHoaTaiSieuThi.FirstOrDefault(t => t.Mahs == Mahs);

                    var model_ct = _db.GiaHangHoaTaiSieuThiCt.Where(t => t.Mahs == model.Mahs);
                    model.GiaHangHoaTaiSieuThiCt = model_ct.ToList();

                    var model_file = _db.ThongTinGiayTo.Where(t => t.Mahs == model.Mahs);
                    model.ThongTinGiayTo = model_file.ToList();

                    ViewData["Madv"] = model.Madv;
                    ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                    ViewData["Title"] = "Chỉnh sửa hàng hóa tại siêu thị";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgsieuthi";
                    ViewData["MenuLv3"] = "menu_dgsieuthi_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHangHoaTaiSieuThi/Edit.cshtml", model);
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

        [Route("GiaHangHoaTaiSieuThi/Update")]
        [HttpPost]
        public IActionResult Update(CSDLGia_ASP.Models.Manages.DinhGia.GiaHangHoaTaiSieuThi request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Edit"))
                {

                    var model = _db.GiaHangHoaTaiSieuThi.FirstOrDefault(t => t.Mahs == request.Mahs);
                    model.Madiaban = request.Madiaban;
                    model.Soqd = request.Soqd;
                    model.Thoidiem = request.Thoidiem;
                    model.Thang = request.Thang;
                    model.Nam = request.Nam;
                    model.Thongtin = request.Thongtin;
                    model.Ghichu = request.Ghichu;
                    model.Mota = request.Mota;
                    model.Updated_at = DateTime.Now;

                    var modelct = _db.GiaHangHoaTaiSieuThiCt.Where(t => t.Mahs == request.Mahs);
                    if (modelct.Any()) { foreach (var ct in modelct) { ct.Trangthai = "XD"; } }
                    var modelfile = _db.ThongTinGiayTo.Where(t => t.Mahs == request.Mahs);
                    if (modelfile.Any()) { foreach (var file in modelfile) { file.Status = "Enable"; } }

                    _db.GiaHangHoaTaiSieuThi.Update(model);
                    _db.GiaHangHoaTaiSieuThiCt.UpdateRange(modelct);
                    _db.ThongTinGiayTo.UpdateRange(modelfile);
                    _db.SaveChanges();
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Cập nhật");

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Update", " Update hồ sơ giá hàng hóa tại siêu thị");

                    return RedirectToAction("Index", "GiaHangHoaTaiSieuThi", new { request.Madv });
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

        [Route("GiaHangHoaTaiSieuThi/Delete")]
        [HttpPost]
        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Delete"))
                {
                    var model = _db.GiaHangHoaTaiSieuThi.FirstOrDefault(t => t.Id == id_delete);
                    var model_ct = _db.GiaHangHoaTaiSieuThiCt.Where(t => t.Mahs == model.Mahs);
                    var modelfile = _db.ThongTinGiayTo.Where(t => t.Mahs == model.Mahs);
                    if (modelfile.Any())
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        foreach (var file in modelfile)
                        {
                            string path_del = Path.Combine(wwwRootPath + "/UpLoad/File/ThongTinGiayTo/", file.FileName);
                            FileInfo fi = new FileInfo(path_del);
                            if (fi != null)
                            {
                                System.IO.File.Delete(path_del);
                                fi.Delete();
                            }
                        }
                    }

                    _db.GiaHangHoaTaiSieuThi.Remove(model);
                    _db.GiaHangHoaTaiSieuThiCt.RemoveRange(model_ct);
                    _db.ThongTinGiayTo.RemoveRange(modelfile);
                    _db.SaveChanges();

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Delete", "Xóa hồ sơ giá hàng hóa tại siêu thị");

                    return RedirectToAction("Index", "GiaHangHoaTaiSieuThi", new { model.Madv });
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

        [Route("GiaHangHoaTaiSieuThi/Show")]
        [HttpGet]
        public IActionResult Show(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Edit"))
                {
                    var model = _db.GiaHangHoaTaiSieuThi.FirstOrDefault(t => t.Mahs == Mahs);

                    var model_ct = _db.GiaHangHoaTaiSieuThiCt.Where(t => t.Mahs == model.Mahs);

                    model.GiaHangHoaTaiSieuThiCt = model_ct.ToList();

                    ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                    ViewData["Title"] = "Xem chi tiết hàng hóa tại siêu thị";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgsieuthi";
                    ViewData["MenuLv3"] = "menu_dgsieuthi_tt";
                    ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                    ViewData["DsDonVi"] = _db.DsDonVi.ToList();
                    return View("Views/Admin/Manages/DinhGia/GiaHangHoaTaiSieuThi/Show.cshtml", model);
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

        [Route("GiaHangHoaTaiSieuThi/Printf")]
        [HttpGet]
        public IActionResult Printf(string Nam, string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Index"))
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

                    var model = _db.GiaHangHoaTaiSieuThi.Where(t => t.Madv == Madv).ToList();

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
                    ViewData["MenuLv2"] = "menu_dgsieuthi";
                    ViewData["MenuLv3"] = "menu_dgsieuthi_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHangHoaTaiSieuThi/Printf.cshtml", model);

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
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.thongtin", "Approve"))
                {
                    var model = _db.GiaHangHoaTaiSieuThi.FirstOrDefault(p => p.Mahs == mahs_complete);
                    model.Trangthai = trangthai_complete;
                    model.Updated_at = DateTime.Now;
                   
                    _db.GiaHangHoaTaiSieuThi.Update(model);
                    _db.SaveChanges();
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), trangthai_complete);

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Chuyen", "Chuyển hồ sơ giá hàng hóa tại siêu thị");


                    return RedirectToAction("Index", "GiaHangHoaTaiSieuThi", new { Madv = model.Madv, Nam = model.Thoidiem.Year });
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

        [Route("GiaHangHoaTaiSieuThi/Search")]
        [HttpGet]
        public IActionResult Search(string Madv, DateTime? NgayTu, DateTime? NgayDen, string Mahs, double DonGiaTu, double DonGiaDen)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.timkiem", "Index"))
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

                    var model = (from hosoct in _db.GiaHangHoaTaiSieuThiCt
                                 join hoso in _db.GiaHangHoaTaiSieuThi on hosoct.Mahs equals hoso.Mahs
                                 join donvi in _db.DsDonVi on hoso.Madv equals donvi.MaDv
                                 select new CSDLGia_ASP.Models.Manages.DinhGia.GiaHangHoaTaiSieuThiCt
                                 {
                                     Madv = hoso.Madv,
                                     Tendv = donvi.TenDv,
                                     Soqd = hoso.Soqd,
                                     Thoidiem = hoso.Thoidiem,
                                     Trangthai = hoso.Trangthai,
                                     Mahs = hosoct.Mahs,
                                     Mahanghoa = hosoct.Mahanghoa,
                                     Tenhanghoa = hosoct.Tenhanghoa,
                                     Giatu = hosoct.Giatu,
                                     Giaden = hosoct.Giaden
                                 });
                    List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
                    model = model.Where(t => t.Thoidiem >= NgayTu && t.Thoidiem <= NgayDen && t.Giatu >= DonGiaTu && list_trangthai.Contains(t.Trangthai));
                    if (Madv != "all") { model = model.Where(t => t.Madv == Madv); }
                    if (DonGiaDen > 0) { model = model.Where(t => t.Giaden <= DonGiaDen); }

                    ViewData["Madv"] = Madv;
                    ViewData["NgayTu"] = NgayTu;
                    ViewData["NgayDen"] = NgayDen;
                    ViewData["Mahs"] = Mahs;
                    ViewData["DonGiaTu"] = Helpers.ConvertDbToStr(DonGiaTu);
                    ViewData["DonGiaDen"] = Helpers.ConvertDbToStr(DonGiaDen);
                    ViewData["DanhSachHoSo"] = _db.GiaHangHoaTaiSieuThi.Where(t => t.Thoidiem >= NgayTu && t.Thoidiem <= NgayDen && list_trangthai.Contains(t.Trangthai));

                    ViewData["DsDiaBan"] = _db.DsDiaBan.Where(t => t.Level != "H");
                    ViewData["Cqcq"] = _db.DsDonVi.Where(t => t.ChucNang != "QUANTRI");
                    ViewData["Title"] = "Tìm kiếm thông tin hồ sơ hàng hóa tại siêu thị";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgsieuthi";
                    ViewData["MenuLv3"] = "menu_dgsieuthi_tk";
                    return View("Views/Admin/Manages/DinhGia/GiaHangHoaTaiSieuThi/TimKiem/Search.cshtml", model);

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

        [Route("GiaHangHoaTaiSieuThi/Print")]
        [HttpPost]
        public IActionResult Print(string Madv_Search, DateTime? NgayTu_Search, DateTime? NgayDen_Search, double DonGiaTu_Search, double DonGiaDen_Search)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giasieuthi.timkiem", "Edit"))
                {

                    var model = (from hosoct in _db.GiaHangHoaTaiSieuThiCt
                                 join hoso in _db.GiaHangHoaTaiSieuThi on hosoct.Mahs equals hoso.Mahs
                                 join donvi in _db.DsDonVi on hoso.Madv equals donvi.MaDv
                                 select new CSDLGia_ASP.Models.Manages.DinhGia.GiaHangHoaTaiSieuThiCt
                                 {
                                     Madv = hoso.Madv,
                                     Tendv = donvi.TenDv,
                                     Soqd = hoso.Soqd,
                                     Thoidiem = hoso.Thoidiem,
                                     Giatu = hosoct.Giatu,
                                     Giaden = hosoct.Giaden,
                                     Mahanghoa = hosoct.Mahanghoa,
                                     Tenhanghoa = hosoct.Tenhanghoa,
                                     Trangthai = hoso.Trangthai,
                                     Mahs = hoso.Mahs
                                 });
                    List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
                    model = model.Where(t => t.Thoidiem >= NgayTu_Search && t.Thoidiem <= NgayDen_Search && t.Giatu >= DonGiaTu_Search && list_trangthai.Contains(t.Trangthai));
                    if (Madv_Search != "all") { model = model.Where(t => t.Madv == Madv_Search); }
                    if (DonGiaDen_Search > 0) { model = model.Where(t => t.Giaden <= DonGiaDen_Search); }

                    ViewData["Title"] = "Tìm kiếm thông tin hồ sơ hàng hóa tại siêu thị";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_dgsieuthi";
                    ViewData["MenuLv3"] = "menu_dgsieuthi_tk";
                    return View("Views/Admin/Manages/DinhGia/GiaHangHoaTaiSieuThi/TimKiem/Result.cshtml", model);
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

        [HttpPost("GiaHangHoaTaiSieuThi/GetListHoSo")]
        public JsonResult GetListHoSo(DateTime ngaytu, DateTime ngayden)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
                var model = _db.GiaHangHoaTaiSieuThi.Where(t => t.Thoidiem >= ngaytu && t.Thoidiem <= ngayden && list_trangthai.Contains(t.Trangthai));
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
    }
}
