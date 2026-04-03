using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Systems;
using CSDLGia_ASP.Services;
using CSDLGia_ASP.ViewModels.Manages.DinhGia;
using CSDLGia_ASP.ViewModels.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Drawing;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaDatDiaBan
{
    public class GiaDatDiaBanController : Controller
    {

        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDsDiaBanService _IDsDiaBan;
        private readonly IDsDonviService _dsDonviService;
        private readonly ITrangThaiHoSoService _trangThaiHoSoService;

        public GiaDatDiaBanController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment, IDsDiaBanService IDsDiaBan, IDsDonviService dsDonviService, ITrangThaiHoSoService trangThaiHoSoService)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
            _IDsDiaBan = IDsDiaBan;
            _dsDonviService = dsDonviService;
            _trangThaiHoSoService = trangThaiHoSoService;
        }

        [Route("GiaDatDiaBan")]
        [HttpGet]
        public IActionResult Index(string Madv, int Nam)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Index"))
                {

                    Madv = string.IsNullOrEmpty(Madv) ? "all" : Madv;

                    var model_donvi = _dsDonviService.GetListDonvi(Helpers.GetSsAdmin(HttpContext.Session, "Madv"));
                    List<string> list_madv = model_donvi.Select(t => t.MaDv).ToList();

                    var model = _db.GiaDatDiaBan.Where(t => list_madv.Contains(t.Madv));
                    if (Madv != "all")
                    {
                        model = model.Where(t => t.Madv == Madv);
                    }

                    if (Nam != 0)
                    {
                        model = model.Where(t => t.Thoidiem.Year == Nam);
                    }
                    ViewData["DsDiaBan"] = _db.DsDiaBan.Where(x=>x.Level !="X");
                    ViewData["DsDonVi"] = model_donvi;
                    ViewData["Madv"] = Madv;
                    ViewData["Nam"] = Nam;
                    ViewData["Soqd"] = _db.GiaDatDiaBanTt.ToList();
                    ViewData["Title"] = "Thông tin hồ sơ bảng giá đất";
                    ViewData["MenuLv1"] = "menu_giadat";
                    ViewData["MenuLv2"] = "menu_giadatdiaban";
                    ViewData["MenuLv3"] = "menu_giadatdiaban_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/Index.cshtml", model);

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

        [Route("GiaDatDiaBan/Create")]
        [HttpGet]
        public IActionResult Create(string soqd, string madv, string MaDiaBan)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Create"))
                {
                    this.RemoveDataCXD(madv);
                    var model = _db.GiaDatDiaBan.Where(t => t.Soqd == soqd && t.MaHuyen == MaDiaBan).FirstOrDefault();

                    if (model == null)
                    {
                        var ndqd = _db.GiaDatDiaBanTt.Where(t => t.Soqd == soqd).FirstOrDefault();
                        var m_qd = new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBan
                        {
                            SoQDTT = soqd,
                            NoiDungQDTT = ndqd.Mota,
                            Soqd = soqd,
                            Madv = madv,
                            Mahs = madv + "_" + DateTime.Now.ToString("yyMMddssmmHH"),
                            Thoidiem = DateTime.Now,
                            Madiaban = MaDiaBan,
                        };
                        var diaban = _db.DsDiaBan.FirstOrDefault(x => x.MaDiaBan == MaDiaBan);

                        var DsXaPhuong = _IDsDiaBan.GetListDsDiaBan(MaDiaBan);

                        ViewData["TenDiaBan"] = _db.DsDiaBan.FirstOrDefault(x => x.MaDiaBan == MaDiaBan).TenDiaBan;
                        //ViewData["DsDiaBan"] = _db.DsDiaBan;
                        ViewData["DsXaPhuong"] = DsXaPhuong;
                        ViewData["Khuvuc"] = _db.GiaDatDiaBanCt.ToList();
                        ViewData["DsDonVi"] = _db.DsDonVi;
                        ViewData["MaDv"] = madv;
                        ViewData["Dmloaidat"] = _db.DmLoaiDat.ToList();
                        ViewData["DsDiaBanHuyen"] = _db.DsDiaBan.Where(t => t.Level == "H");
                        ViewData["Title"] = "Thông tin hồ sơ bảng giá đất";
                        ViewData["MenuLv1"] = "menu_giadat";
                        ViewData["MenuLv2"] = "menu_giadatdiaban";
                        ViewData["MenuLv3"] = "menu_giadatdiaban_tt";
                        return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/Create.cshtml", m_qd);
                    }
                    else if (model != null && model.Trangthai != "HT")
                    {
                        var model_ct = _db.GiaDatDiaBanCt.Where(t => t.Mahs == model.Mahs).ToList();
                        model.GiaDatDiaBanCt = model_ct;
                        ViewData["Title"] = "Thông tin hồ sơ bảng giá đất";
                        ViewData["MenuLv1"] = "menu_giadat";
                        ViewData["MenuLv2"] = "menu_giadatdiaban";
                        ViewData["MenuLv3"] = "menu_giadatdiaban_tt";
                        ViewData["Dmloaidat"] = _db.DmLoaiDat.ToList();
                        ViewData["DsXaPhuong"] = _db.DsXaPhuong.ToList();
                        ViewData["DsDiaBanHuyen"] = _db.DsDiaBan.Where(t => t.Level == "H");
                        ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                        return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/Edit.cshtml", model);
                    }
                    else
                    {
                        ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                        ViewData["dsdonvi"] = _db.DsDonVi.ToList();
                        var model_ct = _db.GiaDatDiaBanCt.Where(t => t.Mahs == model.Mahs).ToList();
                        model.GiaDatDiaBanCt = model_ct;
                        ViewData["Title"] = "Thông tin hồ sơ bảng giá đất";
                        ViewData["MenuLv1"] = "menu_giadat";
                        ViewData["MenuLv2"] = "menu_giadatdiaban";
                        ViewData["MenuLv3"] = "menu_giadatdiaban_tt";
                        ViewData["Dmloaidat"] = _db.DmLoaiDat.ToList();
                        ViewData["DsXaPhuong"] = _db.DsXaPhuong.ToList();
                        ViewData["DsDiaBanHuyen"] = _db.DsDiaBan.Where(t => t.Level == "H");
                        return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/Show.cshtml", model);
                    }
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


        [Route("GiaDatDiaBan/Store")]
        [HttpPost]
        public IActionResult Store(CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBan request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Create"))
                {
                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBan
                    {
                        Mahs = request.Mahs,
                        Madv = request.Madv,
                        Soqd = request.Soqd,
                        Madiaban = request.Madiaban,
                        Thoidiem = request.Thoidiem,
                        Ipf1 = request.Ipf1,
                        NoiDungQDTT = request.NoiDungQDTT,
                        Trangthai = "CC",
                        Congbo = "CHUACONGBO",
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                        SoQDTT = request.SoQDTT,
                        Noidung = request.Noidung,
                        GhiChu = request.GhiChu,
                    };
                    _db.GiaDatDiaBan.Add(model);
                    this.SaveDataCXD(model.Mahs);
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Thêm mới");
                    _db.SaveChanges();

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Store", "Thêm mới hồ sơ giá đất địa bàn");

                    return RedirectToAction("Index", "GiaDatDiaBan", new { request.Madv });
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
        [Route("GiaDatDiaBan/Delete")]
        [HttpPost]
        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Delete"))
                {
                    var model = _db.GiaDatDiaBan.FirstOrDefault(t => t.Id == id_delete);
                    _db.GiaDatDiaBan.Remove(model);
                    _db.SaveChanges();

                    var model_ct = _db.GiaDatDiaBanCt.Where(t => t.Mahs == model.Mahs);
                    if (model_ct.Any())
                    {
                        _db.GiaDatDiaBanCt.RemoveRange(model_ct);
                    }
                    // xóa thông tin giấy tờ chưa lưu lại
                    var model_file_cxd = _db.ThongTinGiayTo.Where(t => t.Mahs == model.Mahs);
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
                    }
                    _db.SaveChanges();

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Delete", "Xóa hồ sơ giá đất địa bàn");

                    return RedirectToAction("Index", "GiaDatDiaBan", new { model.Madiaban });
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
        [Route("GiaDatDiaBan/Edit")]
        [HttpGet]
        public IActionResult Edit(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Edit"))
                {
                    var model = _db.GiaDatDiaBan.FirstOrDefault(t => t.Mahs == Mahs);
                    var model_ct = (from dat in _db.GiaDatDiaBanCt.Where(t => t.Mahs == Mahs)
                                    join dm in _db.DmLoaiDat on dat.Maloaidat equals dm.Maloaidat
                                    select new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt
                                    {
                                        Id = dat.Id,
                                        HienThi = dat.HienThi,
                                        Maloaidat = dat.Maloaidat,
                                        Mota = dat.Mota,
                                        Diemdau = dat.Diemdau,
                                        Diemcuoi = dat.Diemcuoi,
                                        Loaiduong = dat.Loaiduong,
                                        Hesok = dat.Hesok,
                                        Giavt1 = dat.Giavt1,
                                        Giavt2 = dat.Giavt2,
                                        Giavt3 = dat.Giavt3,
                                        Giavt4 = dat.Giavt4,
                                        Giavt5 = dat.Giavt5,
                                        Giavt6 = dat.Giavt6,
                                        Giavt7 = dat.Giavt7,
                                        Giavtconlai = dat.Giavtconlai,
                                        Loaidat = dm.Loaidat,
                                        Sapxep = dat.Sapxep
                                    });
                    model.GiaDatDiaBanCt = model_ct.ToList();
                    model.NoiDungQDTT = _db.GiaDatDiaBanTt.FirstOrDefault(t => t.Soqd == model.SoQDTT)?.Mota ?? "";
                    var thongtingiayto = _db.ThongTinGiayTo.Where(x => x.Mahs == Mahs);
                    model.ThongTinGiayTo = thongtingiayto.ToList();

                    var diaban = _db.DsDiaBan.FirstOrDefault(x => x.MaDiaBan == model.Madiaban);

                    var DsXaPhuong = _IDsDiaBan.GetListDsDiaBan(model.Madiaban);

                    ViewData["TenDiaBan"] = _db.DsDiaBan.FirstOrDefault(x => x.MaDiaBan == model.Madiaban).TenDiaBan;
                    //ViewData["DsDiaBan"] = _db.DsDiaBan;
                    ViewData["DsXaPhuong"] = DsXaPhuong;
                    ViewData["Khuvuc"] = _db.GiaDatDiaBanCt.ToList();
                    ViewData["Dmloaidat"] = _db.DmLoaiDat.ToList();
                    ViewData["Madv"] = model.Madv;
                    ViewData["Title"] = "Thông tin hồ sơ bảng giá đất";
                    ViewData["MenuLv1"] = "menu_giadat";
                    ViewData["MenuLv2"] = "menu_giadatdiaban";
                    ViewData["MenuLv3"] = "menu_giadatdiaban_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/Edit.cshtml", model);

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


        [Route("GiaDatDiaBan/Show")]
        [HttpGet]
        public IActionResult Show(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Index"))
                {
                    var model = _db.GiaDatDiaBan.FirstOrDefault(t => t.Mahs == Mahs);
                    model.GiaDatDiaBanCt = _db.GiaDatDiaBanCt.Where(t => t.Mahs == model.Mahs).ToList();

                    ViewData["TenDiaBan"] = _db.DsDiaBan.FirstOrDefault(x => x.MaDiaBan == model.Madiaban).TenDiaBan;
                    ViewData["TenTinh"] = _db.DsDiaBan.FirstOrDefault(x => string.IsNullOrEmpty(x.MaDiaBanCq)).TenDiaBan;
                    //ViewData["DsDiaBanHuyen"] = _db.DsDiaBan.Where(x => x.Level == "H");
                    ViewData["DsDiaBanXa"] = _db.DsDiaBan.Where(x => x.Level == "X");
                    ViewData["Dsloaidat"] = _db.DmLoaiDat.ToList();
                    ViewData["DsDonVi"] = _db.DsDonVi.ToList();
                    ViewData["Madv"] = model.Madv;
                    ViewData["Title"] = "Thông tin hồ sơ bảng giá đất";
                    ViewData["MenuLv1"] = "menu_giadat";
                    ViewData["MenuLv2"] = "menu_giadatdiaban";
                    ViewData["MenuLv3"] = "menu_giadatdiaban_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/Show.cshtml", model);

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

        [Route("GiaDatDiaBan/Update")]
        [HttpPost]
        public IActionResult Update(CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBan request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Edit"))
                {

                    var model = _db.GiaDatDiaBan.FirstOrDefault(t => t.Mahs == request.Mahs);

                    model.Soqd = request.Soqd;
                    model.Thoidiem = request.Thoidiem;
                    model.Madv = request.Madv;
                    model.Updated_at = DateTime.Now;
                    model.Noidung = request.Noidung;
                    model.GhiChu = request.GhiChu;
                    _db.GiaDatDiaBan.Update(model);
                    this.SaveDataCXD(model.Mahs);
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Cập nhật");
                    _db.SaveChanges();

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Update", " Update hồ sơ giá đất địa bàn");


                    return RedirectToAction("Index", "GiaDatDiaBan", new { request.Madv });
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
        //Tìm ra cơ quan chủ quản và lưu vào
        //Trạng thái thái từ CHT thành HT
        //Madv_ad --> mã đơn vị chuyển hồ sơ lên
        [Route("GiaDatDiaBan/Complete")]
        [HttpPost]
        public IActionResult Complete(string mahs_complete, string trangthai_complete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.thongtin", "Approve"))
                {
                    var model = _db.GiaDatDiaBan.FirstOrDefault(p => p.Mahs == mahs_complete);
                    model.Trangthai = trangthai_complete;
                    model.Updated_at = DateTime.Now;

                    _db.GiaDatDiaBan.Update(model);
                    _db.SaveChanges();
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), trangthai_complete);
                    return RedirectToAction("Index", "GiaDatDiaBan", new { Madv = model.Madv, Nam = model.Thoidiem.Year });
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

        [Route("GiaDatDiaBan/Search")]
        [HttpGet]
        public IActionResult Search(DateTime? NgayTu, DateTime? NgayDen, string Mahs, double DonGiaTu, double DonGiaDen, string Maloaidat, string MaDiaBan = "all")
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.timkiem", "Index"))
                {
                    DateTime nowDate = DateTime.Now;
                    DateTime firstDayCurrentYear = new DateTime(nowDate.Year, 1, 1);
                    DateTime lastDayCurrentYear = new DateTime(nowDate.Year, 12, 31);
                    NgayTu = NgayTu.HasValue ? NgayTu : firstDayCurrentYear;
                    NgayDen = NgayDen.HasValue ? NgayDen : lastDayCurrentYear;
                    Mahs = string.IsNullOrEmpty(Mahs) ? "all" : Mahs;
                    DonGiaTu = DonGiaTu == 0 ? 0 : DonGiaTu;
                    DonGiaDen = DonGiaDen == 0 ? 0 : DonGiaDen;
                    Maloaidat = string.IsNullOrEmpty(Maloaidat) ? "all" : Maloaidat;

                    var model = from dgct in _db.GiaDatDiaBanCt
                                join dg in _db.GiaDatDiaBan on dgct.Mahs equals dg.Mahs
                                join dm in _db.DmLoaiDat on dgct.Maloaidat equals dm.Maloaidat
                                join diaban in _db.DsDiaBan on dg.Madiaban equals diaban.MaDiaBan
                                select new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt
                                {
                                    Id = dg.Id,
                                    Mahs = dg.Mahs,
                                    Soqd = dg.Soqd,
                                    Loaidat = dm.Loaidat,
                                    Mota = dgct.Mota,
                                    Diemdau = dgct.Diemdau,
                                    Diemcuoi = dgct.Diemcuoi,
                                    Loaiduong = dgct.Loaiduong,
                                    Thoidiem = dg.Thoidiem,
                                    Hesok = dgct.Hesok,
                                    MaDv = dgct.MaDv,
                                    Giavt1 = dgct.Giavt1,
                                    Giavt2 = dgct.Giavt2,
                                    Giavt3 = dgct.Giavt3,
                                    Giavt4 = dgct.Giavt4,
                                    Giavt5 = dgct.Giavt5,
                                    Giavt6 = dgct.Giavt6,
                                    Giavt7 = dgct.Giavt7,
                                    Giavtconlai = dgct.Giavtconlai,
                                    Trangthai = dg.Trangthai,
                                    Madiaban = dgct.Madiaban,                                    
                                    Maloaidat = dgct.Maloaidat,
                                    TenDiaBan = diaban.TenDiaBan,
                                };
                    List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
                    model = model.Where(t => t.Thoidiem >= NgayTu && t.Thoidiem <= NgayDen && list_trangthai.Contains(t.Trangthai) && t.Giavt5 >= DonGiaTu);

                    if (Mahs != "all")
                    {
                        model = model.Where(t => t.Mahs == Mahs);
                    }
                    if (DonGiaDen > 0)
                    {
                        model = model.Where(t => t.Giavt1 <= DonGiaDen);
                    }
                    if (Maloaidat != "all")
                    {
                        model = model.Where(t => t.Maloaidat == Maloaidat);
                    }                    
                    if (MaDiaBan != "all")
                    {
                       
                        var diaban_search = _dsDonviService.GetListDonvi(MaDiaBan);
                        List<string> list_diaban_search = diaban_search.Select(t => t.MaDiaBan).ToList();
                        model = model.Where(t => list_diaban_search.Contains(t.Madiaban));

                    }
                    ViewData["DsDiaBan"] = _db.DsDiaBan;
                    ViewData["DsDiaBanXaPhuong"] = _db.DsDiaBan;
                    ViewData["MaDiaBan"] = MaDiaBan;                   
                    ViewData["NgayTu"] = NgayTu;
                    ViewData["NgayDen"] = NgayDen;
                    ViewData["Mahs"] = Mahs;
                    ViewData["DonGiaTu"] = DonGiaTu;
                    ViewData["DonGiaDen"] = DonGiaDen;
                    ViewData["Maloaidat"] = Maloaidat;
                    ViewData["DanhSachHoSo"] = _db.GiaDatDiaBan.Where(t => list_trangthai.Contains(t.Trangthai));
                    ViewData["DanhSachLoaiDat"] = _db.DmLoaiDat;

                    ViewData["Title"] = "Tìm kiếm thông tin bảng giá các loại đất";
                    ViewData["MenuLv1"] = "menu_giadat";
                    ViewData["MenuLv2"] = "menu_giadatdiaban";
                    ViewData["MenuLv3"] = "menu_giadatdiaban_tk";
                    return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/TimKiem/Search.cshtml", model);
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

        [Route("GiaDatDiaBan/PrintSearch")]
        [HttpPost]
        public IActionResult PrintSearch(DateTime? NgayTu_Search, DateTime? NgayDen_Search, string Mahs_Search,
                                    double DonGiaTu_Search, double DonGiaDen_Search, string Maloaidat_Search, string MaDiaBan = "all")
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.giadat.giadatdb.timkiem", "Index"))
                {
                    var model = from dgct in _db.GiaDatDiaBanCt
                                join dg in _db.GiaDatDiaBan on dgct.Mahs equals dg.Mahs
                                join dm in _db.DmLoaiDat on dgct.Maloaidat equals dm.Maloaidat
                                join diaban in _db.DsDiaBan on dg.Madiaban equals diaban.MaDiaBan
                                select new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt
                                {
                                    Id = dg.Id,
                                    Mahs = dg.Mahs,
                                    Soqd = dg.Soqd,
                                    Loaidat = dm.Loaidat,
                                    Mota = dgct.Mota,
                                    Diemdau = dgct.Diemdau,
                                    Diemcuoi = dgct.Diemcuoi,
                                    Loaiduong = dgct.Loaiduong,
                                    Thoidiem = dg.Thoidiem,
                                    Hesok = dgct.Hesok,
                                    MaDv = dgct.MaDv,
                                    Giavt1 = dgct.Giavt1,
                                    Giavt2 = dgct.Giavt2,
                                    Giavt3 = dgct.Giavt3,
                                    Giavt4 = dgct.Giavt4,
                                    Giavt5 = dgct.Giavt5,
                                    Giavt6 = dgct.Giavt6,
                                    Giavt7 = dgct.Giavt7,
                                    Giavtconlai = dgct.Giavtconlai,
                                    Trangthai = dg.Trangthai,
                                    Madiaban = dgct.Madiaban,                                   
                                    Maloaidat = dgct.Maloaidat,
                                    TenDiaBan = diaban.TenDiaBan,                                    
                                };
                    List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
                    model = model.Where(t => t.Thoidiem >= NgayTu_Search && t.Thoidiem <= NgayDen_Search && list_trangthai.Contains(t.Trangthai) && t.Giavt5 >= DonGiaTu_Search);
                    if (Mahs_Search != "all") { model = model.Where(t => t.Mahs == Mahs_Search); }
                    if (DonGiaDen_Search > 0) { model = model.Where(t => t.Giavt1 <= DonGiaDen_Search); }
                    if (Maloaidat_Search != "all")
                    {
                        model = model.Where(t => t.Maloaidat == Maloaidat_Search);
                    }
                    if (MaDiaBan != "all")
                    {
                       
                        var diaban_search = _dsDonviService.GetListDonvi(MaDiaBan);
                        List<string> list_diaban_search = diaban_search.Select(t => t.MaDiaBan).ToList();
                        model = model.Where(t => list_diaban_search.Contains(t.Madiaban));
                    }
                    ViewData["TenTinh"] = _db.DsDiaBan.FirstOrDefault(x => string.IsNullOrEmpty(x.MaDiaBanCq)).TenDiaBan;
                    ViewData["DsDiaBanHuyen"] = _db.DsDiaBan.Where(x => x.Level == "H");
                    ViewData["DsDiaBanXa"] = _db.DsDiaBan.Where(x => x.Level == "X");

                    ViewData["Title"] = " Tìm kiếm thông tin định giá đất địa bàn";

                    return View("Views/Admin/Manages/DinhGia/GiaDatDiaBan/TimKiem/Result.cshtml", model);
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


        [HttpGet("GiaDatDiaBan/ExportToExcel")]
        public IActionResult ExportToExcel(DateTime? NgayTu_Search, DateTime? NgayDen_Search, string Mahs_Search,
                                    double DonGiaTu_Search, double DonGiaDen_Search, string Maloaidat_Search, string MaDiaBan = "all")
        {
            var model = from dgct in _db.GiaDatDiaBanCt
                        join dg in _db.GiaDatDiaBan on dgct.Mahs equals dg.Mahs
                        join dm in _db.DmLoaiDat on dgct.Maloaidat equals dm.Maloaidat
                        join diaban in _db.DsDiaBan on dg.Madiaban equals diaban.MaDiaBan
                        select new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt
                        {
                            Id = dg.Id,
                            Mahs = dg.Mahs,
                            Soqd = dg.Soqd,
                            Loaidat = dm.Loaidat,
                            Mota = dgct.Mota,
                            Diemdau = dgct.Diemdau,
                            Diemcuoi = dgct.Diemcuoi,
                            Loaiduong = dgct.Loaiduong,
                            Thoidiem = dg.Thoidiem,
                            Hesok = dgct.Hesok,
                            MaDv = dgct.MaDv,
                            Giavt1 = dgct.Giavt1,
                            Giavt2 = dgct.Giavt2,
                            Giavt3 = dgct.Giavt3,
                            Giavt4 = dgct.Giavt4,
                            Giavt5 = dgct.Giavt5,
                            Giavt6 = dgct.Giavt6,
                            Giavt7 = dgct.Giavt7,
                            Giavtconlai = dgct.Giavtconlai,
                            Trangthai = dg.Trangthai,
                            Madiaban = dgct.Madiaban,
                            Maloaidat = dgct.Maloaidat,
                            TenDiaBan = diaban.TenDiaBan,
                        };
            List<string> list_trangthai = new List<string> { "HT", "DD", "CB" };
            model = model.Where(t => t.Thoidiem >= NgayTu_Search && t.Thoidiem <= NgayDen_Search && list_trangthai.Contains(t.Trangthai) && t.Giavt5 >= DonGiaTu_Search);
            if (Mahs_Search != "all") { model = model.Where(t => t.Mahs == Mahs_Search); }
            if (DonGiaDen_Search > 0) { model = model.Where(t => t.Giavt1 <= DonGiaDen_Search); }
            if (Maloaidat_Search != "all")
            {
                model = model.Where(t => t.Maloaidat == Maloaidat_Search);
            }
            if (MaDiaBan != "all")
            {

                var diaban_search = _dsDonviService.GetListDonvi(MaDiaBan);
                List<string> list_diaban_search = diaban_search.Select(t => t.MaDiaBan).ToList();
                model = model.Where(t => list_diaban_search.Contains(t.Madiaban));
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

                worksheet.Cells["A1"].Value = "Thông tin tìm kiếm ";
                using (var r = worksheet.Cells[1, 1, 1, 13])
                {
                    r.Merge = true;
                    r.Style.Font.Color.SetColor(Color.Green);
                    r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.Lavender);
                }

                worksheet.Cells["A2"].Value = "STT";
                worksheet.Cells["B2"].Value = "Quyết định";
                worksheet.Cells["C2"].Value = "Ngày áp dụng";
                worksheet.Cells["D2"].Value = "Địa bàn";
                worksheet.Cells["E2"].Value = "Loại đất";
                worksheet.Cells["F2"].Value = "Tên đường phố";
                worksheet.Cells["G2"].Value = "Loại đường";
                worksheet.Cells["H2"].Value = "Hệ số";
                worksheet.Cells["I2"].Value = "VT1";
                worksheet.Cells["J2"].Value = "VT2";
                worksheet.Cells["K2"].Value = "VT3";
                worksheet.Cells["L2"].Value = "VT4";
                worksheet.Cells["M2"].Value = "VT5";
             

                worksheet.Cells[2, 1, 2, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[2, 1, 2, 13].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                row = 3;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = record_id++;
                    worksheet.Cells[row, 2].Value = item.Soqd;
                    worksheet.Cells[row, 3].Value = Helpers.ConvertDateToStr(item.Thoidiem);
                    worksheet.Cells[row, 4].Value = item.TenDiaBan;
                    worksheet.Cells[row, 5].Value = item.Loaidat;
                    worksheet.Cells[row, 6].Value = item.Mota;
                    worksheet.Cells[row, 7].Value = item.Loaiduong;
                    worksheet.Cells[row, 8].Value = Helpers.ConvertDbToStr(item.Hesok);
                    worksheet.Cells[row, 9].Value = Helpers.ConvertDbToStr(item.Giavt1);
                    worksheet.Cells[row, 10].Value = Helpers.ConvertDbToStr(item.Giavt2);
                    worksheet.Cells[row, 11].Value = Helpers.ConvertDbToStr(item.Giavt3);
                    worksheet.Cells[row, 12].Value = Helpers.ConvertDbToStr(item.Giavt4);
                    worksheet.Cells[row, 13].Value = Helpers.ConvertDbToStr(item.Giavt5);

                    row++;
                }

                // Define border styles
                var borderStyle = ExcelBorderStyle.Thin;
                var borderColor = Color.Black;

                // Apply border to header cells
                using (var headerRange = worksheet.Cells["A2:M2"])
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
                    var dataRange = worksheet.Cells[i, 1, i, 13];
                    dataRange.Style.Border.Top.Style = borderStyle;
                    dataRange.Style.Border.Bottom.Style = borderStyle;
                    dataRange.Style.Border.Left.Style = borderStyle;
                    dataRange.Style.Border.Right.Style = borderStyle;
                    dataRange.Style.Border.Top.Color.SetColor(borderColor);
                    dataRange.Style.Border.Bottom.Color.SetColor(borderColor);
                    dataRange.Style.Border.Left.Color.SetColor(borderColor);
                    dataRange.Style.Border.Right.Color.SetColor(borderColor);
                }

                xlPackage.Workbook.Properties.Title = "Thông tin tìm kiếm ";



                var helpers = new Helpers(worksheet);

                // Căn chỉnh, độ rộng ô,  màu sắc
                helpers.CanChinhExCel(1, ExcelHorizontalAlignment.Center, 10, Color.Black);
                helpers.CanChinhExCel(2, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(3, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(4, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(5, ExcelHorizontalAlignment.Left, 20, Color.Black);
                helpers.CanChinhExCel(6, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(7, ExcelHorizontalAlignment.Center, 20, Color.Black);
                helpers.CanChinhExCel(8, ExcelHorizontalAlignment.Right, 50, Color.Black);
                helpers.CanChinhExCel(9, ExcelHorizontalAlignment.Right, 20, Color.Black);
                helpers.CanChinhExCel(10, ExcelHorizontalAlignment.Right, 20, Color.Black);
                helpers.CanChinhExCel(11, ExcelHorizontalAlignment.Right, 20, Color.Black);
                helpers.CanChinhExCel(12, ExcelHorizontalAlignment.Right, 20, Color.Black);
                helpers.CanChinhExCel(13, ExcelHorizontalAlignment.Right, 20, Color.Black);


                xlPackage.Save();
            }

            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Thông tin tìm kiếm .xlsx");
        }

        [HttpPost("GiaDatDiaBan/GetListHoSo")]
        public JsonResult GetListHoSo(DateTime ngaytu, DateTime ngayden)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                var model = _db.GiaDatDiaBan.Where(t => t.Thoidiem >= ngaytu && t.Thoidiem <= ngayden && t.Trangthai == "HT");
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
        private void RemoveDataCXD(string madv)
        {
            //xóa giá đất địa bàn chi tiết chưa xác định
            var giadatdiabanchitiet = _db.GiaDatDiaBanCt.Where(x => x.MaDv == madv && x.Trangthai == "CXD");
            if (giadatdiabanchitiet.Any())
            {
                _db.GiaDatDiaBanCt.RemoveRange(giadatdiabanchitiet);
            }
            // xóa giấy tờ đính kèm chưa xác định                    
            var model_file_cxd = _db.ThongTinGiayTo.Where(x => x.Madv == madv && x.Status == "CXD");
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
            }
            _db.SaveChanges();
        }
        private void SaveDataCXD(string mahs)
        {
            // Lưu lại giá đất địa bàn chưa xác định
            var modelct = _db.GiaDatDiaBanCt.Where(t => t.Mahs == mahs && t.Trangthai == "CXD").ToList();
            if (modelct.Any())
            {
                modelct.ForEach(x => x.Trangthai = "XD");
            }
            // Lưu lại thông tin giấy tờ chưa xác định
            var giayto = _db.ThongTinGiayTo.Where(t => t.Mahs == mahs).ToList();
            if (giayto.Any())
            {
                giayto.ForEach(x => x.Status = "XD");
            }
            _db.SaveChanges();
        }
    }
}
