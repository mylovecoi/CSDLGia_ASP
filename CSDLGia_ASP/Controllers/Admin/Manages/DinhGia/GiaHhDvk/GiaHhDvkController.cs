using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using CSDLGia_ASP.Database;
using System.Security.Cryptography;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.DinhGia;
using CSDLGia_ASP.ViewModels.Systems;
using CSDLGia_ASP.ViewModels.Manages.DinhGia;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using CSDLGia_ASP.Models.Systems;
using CSDLGia_ASP.Services;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaHhDvk
{
    public class GiaHhDvkController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDsDonviService _dsDonviService;
        private readonly ITrangThaiHoSoService _trangThaiHoSoService;

        public GiaHhDvkController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment, IDsDonviService dsDonviService, ITrangThaiHoSoService trangThaiHoSoService)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
            _dsDonviService = dsDonviService;
            _trangThaiHoSoService = trangThaiHoSoService;
        }

        [Route("GiaHhDvk/DanhSach")]
        [HttpGet]
        public IActionResult Index(int Thang, int Nam, string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Index"))
                {
                    Madv = string.IsNullOrEmpty(Madv) ? Helpers.GetSsAdmin(HttpContext.Session, "Madv") : Madv;

                    var model_donvi = _dsDonviService.GetListDonvi(Helpers.GetSsAdmin(HttpContext.Session, "Madv"));
                    List<string> list_madv = model_donvi.Select(t => t.MaDv).ToList();

                    IEnumerable<CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvk> model = _db.GiaHhDvk.Where(t => list_madv.Contains(t.Madv));

                    if (Madv != "all")
                    {
                        model = model.Where(t => t.Madv == Madv);
                    }

                    if (Thang != 0)
                    {
                        model = model.Where(t => t.Thoidiem.Month == Thang);
                    }

                    if (Nam != 0)
                    {
                        model = model.Where(t => t.Thoidiem.Year == Nam).ToList();
                    }

                    var model_join = (from kk in model
                                      join nhom in _db.GiaHhDvkNhom on kk.Matt equals nhom.Matt
                                      select new CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvk
                                      {
                                          Id = kk.Id,
                                          Mahs = kk.Mahs,
                                          Matt = kk.Matt,
                                          Madiaban = kk.Madiaban,
                                          Madv = kk.Madv,
                                          Thang = kk.Thang,
                                          Nam = kk.Nam,
                                          Soqd = kk.Soqd,
                                          Thoidiem = kk.Thoidiem,
                                          Soqdlk = kk.Soqdlk,
                                          Thoidiemlk = kk.Thoidiemlk,
                                          Ghichu = kk.Ghichu,
                                          Trangthai = kk.Trangthai,
                                          Ipf1 = kk.Ipf1,
                                          Macqcq = kk.Macqcq,
                                          Tentt = nhom.Tentt,
                                          Lydo = kk.Lydo,
                                      });

                    ViewData["DsDonVi"] = model_donvi;
                    var dsDonViTH = (from donvi in _db.DsDonVi
                                     join tk in _db.Users on donvi.MaDv equals tk.Madv
                                     join gr in _db.GroupPermissions.Where(x => x.ChucNang == "TONGHOP") on tk.Chucnang equals gr.KeyLink
                                     select new CSDLGia_ASP.Models.Systems.DsDonVi
                                     {
                                         MaDiaBan = donvi.MaDiaBan,
                                         MaDv = donvi.MaDv,
                                         TenDv = donvi.TenDv,
                                     });
                    ViewData["DsDonViTh"] = dsDonViTH;
                    ViewData["Thang"] = Thang;
                    ViewData["Nam"] = Nam;
                    ViewData["Madv"] = Madv;
                    ViewData["Hoso"] = _db.GiaHhDvkTh.Where(t => t.Thang == Thang.ToString() && t.Nam == Nam.ToString()).ToList();
                    ViewData["Nhomhhdvk"] = _db.GiaHhDvkNhom.ToList();
                    ViewData["DsDiaBan"] = _db.DsDiaBan;
                    ViewData["DsCqcq"] = _db.DsDonVi.ToList();
                    ViewData["Title"] = "Thông tin hồ sơ giá hàng hóa, dịch vụ khác";
                    ViewData["MenuLv1"] = "menu_hhdvk";
                    ViewData["MenuLv2"] = "menu_hhdvk_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/DanhSach/Index.cshtml", model_join);

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

        [Route("GiaHhDvk/Create")]
        [HttpGet]
        public IActionResult Create(string MattBc, string MadvBc, string MadiabanBc, string ThangBc, string NamBc)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Create"))
                {
                    var check = _db.GiaHhDvkCt.Where(t => t.Trangthai == "CXD");
                    if (check != null)
                    {
                        _db.GiaHhDvkCt.RemoveRange(check);
                        _db.SaveChanges();
                    }

                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvk
                    {
                        Mahs = MadvBc + "_" + DateTime.Now.ToString("yyMMddssmmHH"),
                        Matt = MattBc,
                        Madiaban = MadiabanBc,
                        Madv = MadvBc,
                        Thang = ThangBc,
                        Nam = NamBc,
                        Thoidiem = DateTime.Now,
                        Thoidiemlk = DateTime.Now,
                    };

                    //var danhmucdv = _db.GiaHhDvkDmDv.Where(t => t.Matt == MattBc && t.Madv == MadvBc).OrderBy(t => t.Mahhdv).ToList();
                    var danhmuc = _db.GiaHhDvkDm.Where(t => t.Matt == MattBc && t.Theodoi == "TD").ToList();

                    var chitiet = new List<GiaHhDvkCt>();


                    foreach (var item in danhmuc)
                    {
                        chitiet.Add(new GiaHhDvkCt()
                        {
                            Manhom = item.Manhom,
                            Mahs = model.Mahs,
                            Mahhdv = item.Mahhdv,
                            Loaigia = "Giá bán lẻ",
                            Nguontt = "Do cơ quan/đơn vị quản lý nhà nước có liên quan cung cấp/báo cáo theo quy định",
                            Trangthai = "CXD",
                            Created_at = DateTime.Now,
                            Updated_at = DateTime.Now,
                        });
                    }

                    //Tìm hồ sơ kê khai liền kề
                    string thangBcLk = int.Parse(ThangBc) == 1 ? "12" : (int.Parse(ThangBc) - 1).ToString();
                    string namBcLk = thangBcLk == "12" ? (int.Parse(NamBc) - 1).ToString() : NamBc;

                    var modellk = _db.GiaHhDvk.Where(t => t.Matt == MattBc && t.Madv == MadvBc && t.Trangthai == "HT" && t.Thang == thangBcLk && t.Nam == namBcLk).FirstOrDefault();
                    if (modellk != null)
                    {
                        model.Soqdlk = modellk.Soqd;
                        model.Thoidiemlk = modellk.Thoidiem;
                        var modellkct = _db.GiaHhDvkCt.Where(t => t.Mahs == modellk.Mahs).ToList();
                        //Gán thông tin giá vào hồ sơ chi tiết
                        foreach (var t in chitiet)
                        {
                            var lk = modellkct.FirstOrDefault(x => x.Mahhdv == t.Mahhdv);
                            if (lk != null)
                            {
                                t.Gia = lk.Gia;
                                t.Gialk = lk.Gia;
                            }
                        }
                    }
                    //Lưu thông tin vào db
                    _db.GiaHhDvkCt.AddRange(chitiet);
                    _db.SaveChanges();

                    var modelct_join = (from ct in chitiet
                                        join dm in _db.GiaHhDvkDm.Where(x => x.Matt == model.Matt && x.Theodoi == "TD") on ct.Mahhdv equals dm.Mahhdv
                                        select new GiaHhDvkCt
                                        {
                                            Id = ct.Id,
                                            Manhom = ct.Manhom,
                                            Mahhdv = ct.Mahhdv,
                                            Mahs = ct.Mahs,
                                            Gia = ct.Gia,
                                            Gialk = ct.Gialk,
                                            Loaigia = ct.Loaigia,
                                            Nguontt = ct.Nguontt,
                                            Ghichu = ct.Ghichu,
                                            Created_at = ct.Created_at,
                                            Updated_at = ct.Updated_at,
                                            Tenhhdv = dm.Tenhhdv,
                                            Dacdiemkt = dm.Dacdiemkt,
                                            Dvt = dm.Dvt,
                                        }).ToList();

                    model.GiaHhDvkCt = modelct_join.Where(t => t.Mahs == model.Mahs).ToList();

                    var donVi = _db.DsDonVi.FirstOrDefault(x => x.MaDv == model.Madv);
                    string diaBanApDung = donVi?.DiaBanApDung ?? null;
                    if (!string.IsNullOrEmpty(diaBanApDung))
                    {
                        ViewData["DsDiaBan"] = _db.DsDiaBan.Where(x => diaBanApDung.Contains(x.MaDiaBan));
                    }
                    else
                    {
                        ViewData["DsDiaBan"] = _db.DsDiaBan.Where(x => x.Level == "H");
                    }
                    ViewData["Nhomhhdvk"] = _db.GiaHhDvkNhom.ToList();
                    ViewData["DmDvt"] = _db.DmDvt.ToList();
                    //ViewData["DsDiaBan"] = _db.DsDiaBan.Where(t => t.Level != "ADMIN");
                    ViewData["Title"] = "Thông tin giá hàng hóa dịch vụ thêm mới";
                    ViewData["MenuLv1"] = "menu_hhdvk";
                    ViewData["MenuLv2"] = "menu_hhdvk_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/DanhSach/Create.cshtml", model);

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

        [Route("GiaHhDvk/Store")]
        [HttpPost]
        public IActionResult Store(CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvk request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Create"))
                {

                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvk
                    {
                        Mahs = request.Mahs,
                        Matt = request.Matt,
                        Madiaban = request.Madiaban,
                        Madv = request.Madv,
                        Thang = request.Thang,
                        Nam = request.Nam,
                        Soqd = request.Soqd,
                        Thoidiem = request.Thoidiem,
                        Soqdlk = request.Soqdlk,
                        Thoidiemlk = request.Thoidiemlk,
                        Ghichu = request.Ghichu,
                        Trangthai = "CHT",
                        Congbo = "CHUACONGBO",
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                    };
                    _db.GiaHhDvk.Add(model);
                    _db.SaveChanges();

                    var modelct = _db.GiaHhDvkCt.Where(t => t.Mahs == request.Mahs);
                    if (modelct != null)
                    {
                        foreach (var item in modelct)
                        {
                            item.Trangthai = "XD";
                        }
                    }
                    _db.GiaHhDvkCt.UpdateRange(modelct);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "GiaHhDvk", new { Nam = request.Nam, Thang = request.Thang, Madv = request.Madv });
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

        [Route("GiaHhDvk/Edit")]
        [HttpGet]
        public IActionResult Edit(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Edit"))
                {
                    var model = _db.GiaHhDvk.FirstOrDefault(t => t.Mahs == Mahs);

                    var modelct_join = (from ct in _db.GiaHhDvkCt.Where(t => t.Mahs == model.Mahs)
                                        join dm in _db.GiaHhDvkDm.Where(x => x.Matt == model.Matt && x.Theodoi == "TD") on ct.Mahhdv equals dm.Mahhdv
                                        select new GiaHhDvkCt
                                        {
                                            Id = ct.Id,
                                            Manhom = ct.Manhom,
                                            Mahhdv = ct.Mahhdv,
                                            Mahs = ct.Mahs,
                                            Gia = ct.Gia,
                                            Gialk = ct.Gialk,
                                            Loaigia = ct.Loaigia,
                                            Nguontt = ct.Nguontt,
                                            Ghichu = ct.Ghichu,
                                            Created_at = ct.Created_at,
                                            Updated_at = ct.Updated_at,
                                            Tenhhdv = dm.Tenhhdv,
                                            Dacdiemkt = dm.Dacdiemkt,
                                            Dvt = dm.Dvt,
                                        }).ToList();

                    model.GiaHhDvkCt = modelct_join.ToList();
                    model.ThongTinGiayTo = _db.ThongTinGiayTo.Where(t => t.Mahs == model.Mahs).ToList();

                    ViewData["Nhomhhdvk"] = _db.GiaHhDvkNhom.ToList();
                    ViewData["DsDiaBan"] = _db.DsDiaBan.Where(t => t.Level != "ADMIN");
                    ViewData["DsCqcq"] = _db.DsDonVi.ToList();
                    ViewData["DmDvt"] = _db.DmDvt.ToList();
                    ViewData["Title"] = "Thông tin giá hàng hóa dịch vụ thêm mới";
                    ViewData["MenuLv1"] = "menu_hhdvk";
                    ViewData["MenuLv2"] = "menu_hhdvk_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/DanhSach/Edit.cshtml", model);

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

        [Route("GiaHhDvk/Update")]
        [HttpPost]
        public IActionResult Update(CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvk request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Edit"))
                {

                    var model = _db.GiaHhDvk.FirstOrDefault(t => t.Mahs == request.Mahs);
                    model.Soqd = request.Soqd;
                    model.Thoidiem = request.Thoidiem;
                    model.Soqdlk = request.Soqdlk;
                    model.Thoidiemlk = request.Thoidiemlk;
                    model.Ghichu = request.Ghichu;
                    model.Ipf1 = request.Ipf1;
                    model.Updated_at = DateTime.Now;

                    _db.GiaHhDvk.Update(model);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "GiaHhDvk", new { request.Madv });
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

        [Route("GiaHhDvk/Delete")]
        [HttpPost]
        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Delete"))
                {
                    var model = _db.GiaHhDvk.FirstOrDefault(t => t.Id == id_delete);
                    _db.GiaHhDvk.Remove(model);
                    _db.SaveChanges();

                    var model_ct = _db.GiaHhDvkCt.Where(t => t.Mahs == model.Mahs);
                    _db.GiaHhDvkCt.RemoveRange(model_ct);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "GiaHhDvk", new { model.Madv });
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

        [Route("GiaHhDvk/Show")]
        [HttpGet]
        public IActionResult Show(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Index"))
                {
                    var nhomhh = _db.DmNhomHh.Where(t => t.Phanloai == "GIAHHDVK");
                    var model = _db.GiaHhDvk.FirstOrDefault(t => t.Mahs == Mahs);
                    model.GiaHhDvkCt = (from ct in _db.GiaHhDvkCt.Where(t => t.Mahs == model.Mahs)
                                        join dm in _db.GiaHhDvkDm.Where(x => x.Matt == model.Matt && x.Theodoi == "TD") on ct.Mahhdv equals dm.Mahhdv
                                        select new GiaHhDvkCt
                                        {
                                            Id = ct.Id,
                                            Manhom = ct.Manhom,
                                            Mahhdv = ct.Mahhdv,
                                            Mahs = ct.Mahs,
                                            Gia = ct.Gia,
                                            Gialk = ct.Gialk,
                                            Loaigia = ct.Loaigia,
                                            Nguontt = ct.Nguontt,
                                            Ghichu = ct.Ghichu,
                                            Created_at = ct.Created_at,
                                            Updated_at = ct.Updated_at,
                                            Tenhhdv = dm.Tenhhdv,
                                            Dacdiemkt = dm.Dacdiemkt,
                                            Dvt = dm.Dvt,
                                        }).ToList();

                    ViewData["Nhomhh"] = nhomhh;
                    ViewData["DmDvt"] = _db.DmDvt.ToList();
                    ViewData["DsDonVi"] = _db.DsDonVi.Where(x=>x.MaDv == model.Madv).ToList();
                    ViewData["Title"] = "Thông tin giá hàng hóa dịch vụ chi tiết";
                    ViewData["MenuLv1"] = "menu_hhdvk";
                    ViewData["MenuLv2"] = "menu_hhdvk_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/DanhSach/Show.cshtml", model);

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

        public IActionResult Chuyen(string mahs_chuyen, string trangthai_complete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Approve"))
                {
                    var model = _db.GiaHhDvk.FirstOrDefault(p => p.Mahs == mahs_chuyen);
                    model.Trangthai = trangthai_complete;
                    model.Updated_at = DateTime.Now;

                    _db.GiaHhDvk.Update(model);
                    _db.SaveChanges();

                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), trangthai_complete);

                    // Lưu vết từng tài khoản đăng nhập theo thời gian truy cập vào hệ thống 
                    LoggingHelper.LogAction(HttpContext, _db, "Chuyen", "Chuyển hồ sơ giá hàng hóa dịch vụ khác");


                    return RedirectToAction("Index", "GiaHhDvk", new { Madv = model.Madv, Nam = model.Thoidiem.Year });
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

        [Route("GiaHhDvk/TimKiem")]
        [HttpGet]
        public IActionResult Search()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tk", "Index"))
                {

                    if (Helpers.GetSsAdmin(HttpContext.Session, "Madv") != null)
                    {
                        ViewData["Madv"] = Helpers.GetSsAdmin(HttpContext.Session, "Madv");
                    }
                    else
                    {
                        ViewData["Madv"] = "all";
                    }
                    ViewData["DanhSachHoSo"] = _db.GiaHhDvk.Where(t => t.Thoidiem.Year == DateTime.Now.Year && t.Trangthai == "HT");

                    ViewData["DsDiaBan"] = _db.DsDiaBan;
                    ViewData["DsDonVi"] = _db.DsDonVi.Where(t => t.ChucNang != "QUANTRI");
                    ViewData["Nhomhh"] = _db.GiaHhDvkNhom;
                    ViewData["Title"] = "Tìm kiếm thông tin giá hàng hóa dịch vụ chi tiết";
                    ViewData["MenuLv1"] = "menu_hhdvk";
                    ViewData["MenuLv2"] = "menu_hhdvk_tk";
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/TimKiem/Index.cshtml");

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

        [Route("GiaHhDvk/TimKiem/KetQua")]
        [HttpPost]
        public IActionResult Result(string madv, string matt, DateTime ngaynhap_tu, DateTime ngaynhap_den,
                                    double gia_tu, double gia_den, string mahs, string Tenhhdv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tk", "Index"))
                {
                    var model = (from ct in _db.GiaHhDvkCt
                                 join kk in _db.GiaHhDvk on ct.Mahs equals kk.Mahs
                                 join nhom in _db.GiaHhDvkNhom on kk.Matt equals nhom.Matt
                                 //join dm in _db.GiaHhDvkDm on ct.Mahhdv equals dm.Mahhdv 
                                 join dm in _db.GiaHhDvkDm on new { ct.Mahhdv, nhom.Matt } equals new { dm.Mahhdv, dm.Matt }
                                 join dv in _db.DsDonVi on kk.Madv equals dv.MaDv
                                 select new GiaHhDvkCt
                                 {
                                     Id = ct.Id,
                                     Mahs = ct.Mahs,
                                     Mahhdv = ct.Mahhdv,
                                     Gialk = ct.Gialk,
                                     Gia = ct.Gia,
                                     Ghichu = ct.Ghichu,
                                     Manhom = ct.Manhom,
                                     Tenhhdv = dm.Tenhhdv,
                                     Dacdiemkt = dm.Dacdiemkt,
                                     Dvt = dm.Dvt,
                                     Madv = kk.Madv,
                                     Matt = kk.Matt,
                                     Thoidiem = kk.Thoidiem,
                                     Tendv = dv.TenDv,
                                     Tentt = nhom.Tentt,
                                     Trangthai = kk.Trangthai
                                 });
                    model = model.Where(t => t.Trangthai == "HT");
                    if (madv != "all")
                    {
                        model = model.Where(t => t.Madv == madv);
                    }
                    //return Ok(model);
                    if (matt != "all")
                    {
                        model = model.Where(t => t.Matt == matt);
                    }

                    if (ngaynhap_tu.ToString("yyMMdd") != "010101")
                    {
                        model = model.Where(t => t.Thoidiem >= ngaynhap_tu);
                    }

                    if (ngaynhap_den.ToString("yyMMdd") != "010101")
                    {
                        model = model.Where(t => t.Thoidiem <= ngaynhap_den);
                    }

                    model = model.Where(t => t.Gia >= gia_tu);
                    if (gia_den > 0)
                    {
                        model = model.Where(t => t.Gia <= gia_den);
                    }

                    if (mahs != "all")
                    {
                        model = model.Where(t => t.Mahs == mahs);
                    }

                    if (!string.IsNullOrEmpty(Tenhhdv))
                    {
                        model = model.Where(t => t.Tenhhdv.ToLower().Contains(Tenhhdv.ToLower()));
                    }

                    ViewData["Title"] = "Tìm kiếm thông tin giá hàng hóa dịch vụ chi tiết";
                    ViewData["MenuLv1"] = "menu_hhdvk";
                    ViewData["MenuLv2"] = "menu_hhdvk_tk";
                    ViewData["DmDvt"] = _db.DmDvt.ToList();
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/TimKiem/Result.cshtml", model);
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

        [Route("GiaHhDvkCt/Edit")]
        [HttpPost]
        public JsonResult EditCt(int Id, string Matt)
        {
            //var chiTiet = _db.GiaHhDvkCt.FirstOrDefault(t => t.Id == Id);
            //var hoSo = _db.GiaHhDvk.FirstOrDefault(t => t.Mahs == chiTiet.Mahs);

            var model = (from ct in _db.GiaHhDvkCt.Where(t => t.Id == Id)
                         join dm in _db.GiaHhDvkDm.Where(x => x.Matt == Matt && x.Theodoi == "TD") on ct.Mahhdv equals dm.Mahhdv
                         select new GiaHhDvkCt
                         {
                             Id = ct.Id,
                             Mahhdv = ct.Mahhdv,
                             Mahs = ct.Mahs,
                             Gia = ct.Gia,
                             Gialk = ct.Gialk,
                             Loaigia = ct.Loaigia,
                             Nguontt = ct.Nguontt,
                             Ghichu = ct.Ghichu,
                             Created_at = ct.Created_at,
                             Updated_at = ct.Updated_at,
                             Tenhhdv = dm.Tenhhdv,
                             Dacdiemkt = dm.Dacdiemkt,
                             Dvt = dm.Dvt,
                         }).First();

            if (model != null)
            {
                string result = "<div class='modal-body' id='edit_thongtin'>";

                result += "<div class='row text-left'>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Tên hàng hóa, dịch vụ</label>";
                result += "<select class='form-control' disabled='disabled'>";
                result += "<option>" + model.Tenhhdv + "</option>";
                result += "</select>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Loại giá</label>";
                result += "<select id='loaigia_edit' name='loaigia_edit' class='form-control'>";
                result += "<option value='Giá bán buôn' " + ((string)model.Loaigia == "Giá bán buôn" ? "selected" : "") + ">Giá bán buôn</option>";
                result += "<option value='Giá bán lẻ' " + ((string)model.Loaigia == "Giá bán lẻ" ? "selected" : "") + ">Giá bán lẻ</option>";
                result += "<option value='Giá kê khai' " + ((string)model.Loaigia == "Giá kê khai" ? "selected" : "") + ">Giá kê khai</option>";
                result += "<option value='Giá đăng ký' " + ((string)model.Loaigia == "Giá đăng ký" ? "selected" : "") + ">Giá đăng ký</option>";
                result += "</select>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-6'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá kỳ trước</label>";
                result += "<input type='text' id='gialk_edit' name='gialk_edit' value='" + Helpers.ConvertDbToStr(model.Gialk) + "' class='form-control money-decimal-mask' style='font-weight: bold'/>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-6'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá kỳ này</label>";
                result += "<input type='text' id='gia_edit' name='gia_edit' value='" + Helpers.ConvertDbToStr(model.Gia) + "' class='form-control money-decimal-mask' style='font-weight: bold'/>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Nguồn thông tin</label>";
                result += "<select id='nguontt_edit' name='nguontt_edit' class='form-control'>";
                result += "<option value='Do trực tiếp điều tra, thu thập' " + ((string)model.Nguontt == "Do trực tiếp điều tra, thu thập" ? "selected" : "") + ">Do trực tiếp điều tra, thu thập</option>";
                result += "<option value='Hợp đồng mua tin' " + ((string)model.Nguontt == "Hợp đồng mua tin" ? "selected" : "") + ">Hợp đồng mua tin</option>";
                result += "<option value='Do cơ quan/đơn vị quản lý nhà nước có liên quan cung cấp/báo cáo theo quy định' " + ((string)model.Nguontt == "Do cơ quan/đơn vị quản lý nhà nước có liên quan cung cấp/báo cáo theo quy định" ? "selected" : "") + ">Do cơ quan/đơn vị quản lý nhà nước có liên quan cung cấp/báo cáo theo quy định</option>";
                result += "<option value='Từ thống kê đăng ký giá, kê khai giá, thông báo giá của doanh nghiệp' " + ((string)model.Nguontt == "Từ thống kê đăng ký giá, kê khai giá, thông báo giá của doanh nghiệp" ? "selected" : "") + ">Từ thống kê đăng ký giá, kê khai giá, thông báo giá của doanh nghiệp</option>";
                result += "<option value='Các nguồn thông tin khác' " + ((string)model.Nguontt == "Các nguồn thông tin khác" ? "selected" : "") + ">Các nguồn thông tin khác</option>";
                result += "</select>";
                result += "</div>";
                result += "</div>";

                result += "</div>";

                result += "<input hidden type='text' id='id_edit' name='id_edit' value='" + model.Id + "'/>";
                result += "</div>";

                var data = new { status = "success", message = result };
                return Json(data);
            }
            else
            {
                var data = new { status = "error", message = "Không tìm thấy thông tin cần chỉnh sửa!!!" };
                return Json(data);
            }
        }

        [Route("GiaHhDvkCt/Update")]
        [HttpPost]
        public JsonResult UpdateCt(int Id, double Gia, double Gialk, string Nguontt, string Loaigia, string Matt)
        {
            var model = _db.GiaHhDvkCt.FirstOrDefault(t => t.Id == Id);
            model.Gia = Gia;
            model.Gialk = Gialk;
            model.Loaigia = Loaigia;
            model.Nguontt = Nguontt;
            model.Updated_at = DateTime.Now;
            _db.GiaHhDvkCt.Update(model);
            _db.SaveChanges();
            string result = GetDataCt(model.Mahs, Matt);
            var data = new { status = "success", message = result };
            return Json(data);
        }

        public string GetDataCt(string Mahs, string Matt)
        {
            //var hoSo = _db.GiaHhDvk.FirstOrDefault(t => t.Mahs == Mahs);
            var model = (from ct in _db.GiaHhDvkCt.Where(t => t.Mahs == Mahs)
                         join dm in _db.GiaHhDvkDm.Where(x => x.Matt == Matt && x.Theodoi == "TD") on ct.Mahhdv equals dm.Mahhdv
                         select new GiaHhDvkCt
                         {
                             Id = ct.Id,
                             Mahhdv = ct.Mahhdv,
                             Mahs = ct.Mahs,
                             Gia = ct.Gia,
                             Gialk = ct.Gialk,
                             Loaigia = ct.Loaigia,
                             Nguontt = ct.Nguontt,
                             Ghichu = ct.Ghichu,
                             Created_at = ct.Created_at,
                             Updated_at = ct.Updated_at,
                             Tenhhdv = dm.Tenhhdv,
                             Dacdiemkt = dm.Dacdiemkt,
                             Dvt = dm.Dvt,
                         });

            int record = 1;
            var dmDVT = _db.DmDvt;
            string result = "<div class='card-body' id='frm_data'>";
            result += "<table class='table table-striped table-bordered table-hover table-responsive' id='datatable_4'>";
            result += "<thead>";
            result += "<tr style='text-align:center'>";
            result += "<th width='2%'>STT</th>";
            result += "<th>Mã hàng hóa dịch vụ</th>";
            result += "<th>Tên hàng hóa dịch vụ</th>";
            result += "<th>Đặc điểm kỹ thuật</th>";
            result += "<th>Đơn vị tính</th>";
            result += "<th>Giá kỳ trước</th>";
            result += "<th>Giá kỳ này</th>";
            result += "<th width='9%'>Thao tác</th>";
            result += "</tr>";
            result += "</thead>";
            result += "<tbody>";

            foreach (var item in model.OrderBy(x => x.Tenhhdv))
            {
                result += "<tr>";
                result += "<td class='text-center'>" + record++ + "</td>";
                result += "<td class='text-center'>" + item.Mahhdv + "</td>";
                result += "<td class='text-left active'>" + item.Tenhhdv + "</td>";
                result += "<td class='text-left'>" + item.Dacdiemkt + "</td>";
                result += "<td class='text-center'>" + dmDVT.FirstOrDefault(x => x.Madvt == item.Dvt)?.Dvt + "</td>";
                result += "<td style='text-align:right; font-weight:bold'>" + Helpers.ConvertDbToStr(item.Gialk) + "</td>";
                result += "<td style='text-align:right; font-weight:bold'>" + Helpers.ConvertDbToStr(item.Gia) + "</td>";
                result += "<td>";
                result += "<button type='button' class='btn btn-sm btn-clean btn-icon' title='Nhập giá'";
                result += " data-target='#Edit_Modal' data-toggle='modal' onclick='SetEdit(`" + item.Id + "`)'>";
                result += "<i class='icon-lg la la-edit text-primary'></i>";
                result += "</button>";
                result += "</td>";
                result += "</tr>";
            }
            result += "</tbody>";
            result += "</table>";
            result += "</div>";

            return result;

        }

        [HttpPost("GiaHhDvkCt/GetListHoSo")]
        public JsonResult GetListHoSo(DateTime ngaytu, DateTime ngayden)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                var model = _db.GiaHhDvk.Where(t => t.Thoidiem >= ngaytu && t.Thoidiem <= ngayden && t.Trangthai == "HT");
                string result = "<select class='form-control' id='mahs' name='mahs'>";
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

        [Route("GiaHhDvk/CreateTH")]
        [HttpGet]
        public IActionResult CreateTH(string MattTh, string ThangTh, string NamTh, string MadvTh, string[] HosoTh)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Create"))
                {
                    var modelct = _db.GiaHhDvkThCt.Where(t => HosoTh.Contains(t.Mahs)).ToList();

                    var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvk
                    {
                        Mahs = MadvTh + "_" + DateTime.Now.ToString("yyMMddssmmHH"),
                        Matt = MattTh,
                        Madv = MadvTh,
                        Thang = ThangTh,
                        Nam = NamTh,
                        Thoidiem = DateTime.Now,
                        Thoidiemlk = DateTime.Now,
                        Trangthai = "CHT",
                    };

                    var dmHangHoa = _db.GiaHhDvkDm.Where(x => x.Matt == MattTh && x.Theodoi == "TD");
                    var chiTiet = new List<GiaHhDvkCt>();
                    foreach (var item in dmHangHoa)
                    {
                        //Lấy ds chi tiết
                        var ct = modelct.Where(x => x.Mahhdv == item.Mahhdv);
                        var Gia = ct.Any() ? ct.Sum(x => x.Gia) / ct.Count() : 0;
                        var Gialk = ct.Any() ? ct.Sum(x => x.Gialk) / ct.Count() : 0;

                        chiTiet.Add(new GiaHhDvkCt
                        {
                            Mahs = model.Mahs,
                            Mahhdv = item.Mahhdv,
                            Gialk = (double)Gialk,
                            Gia = (double)Gia,
                            Manhom = item.Manhom,
                            Loaigia = "Giá bán lẻ",
                            Nguontt = "Do cơ quan/đơn vị quản lý nhà nước có liên quan cung cấp/báo cáo theo quy định",
                            Trangthai = "CXD",
                            Created_at = DateTime.Now,
                            Updated_at = DateTime.Now,
                        });
                    }
                    _db.GiaHhDvk.Add(model);
                    _db.GiaHhDvkCt.AddRange(chiTiet);
                    _db.SaveChanges();

                    var modelct_join = (from ct in _db.GiaHhDvkCt.Where(x => x.Mahs == model.Mahs)
                                        join dm in _db.GiaHhDvkDm.Where(x => x.Matt == model.Matt && x.Theodoi == "TD") on ct.Mahhdv equals dm.Mahhdv
                                        select new GiaHhDvkCt
                                        {
                                            Id = ct.Id,
                                            Manhom = ct.Manhom,
                                            Mahhdv = ct.Mahhdv,
                                            Mahs = ct.Mahs,
                                            Gia = ct.Gia,
                                            Gialk = ct.Gialk,
                                            Loaigia = ct.Loaigia,
                                            Nguontt = ct.Nguontt,
                                            Ghichu = ct.Ghichu,
                                            Created_at = ct.Created_at,
                                            Updated_at = ct.Updated_at,
                                            Tenhhdv = dm.Tenhhdv,
                                            Dacdiemkt = dm.Dacdiemkt,
                                            Dvt = dm.Dvt,
                                        }).ToList();

                    model.GiaHhDvkCt = modelct_join;

                    ViewData["Nhomhhdvk"] = _db.GiaHhDvkNhom.ToList();
                    ViewData["DmDvt"] = _db.DmDvt.ToList();
                    ViewData["DsDiaBan"] = _db.DsDiaBan.Where(t => t.Level != "ADMIN");
                    ViewData["Title"] = "Thông tin giá hàng hóa dịch vụ thêm mới";
                    ViewData["MenuLv1"] = "menu_hhdvk";
                    ViewData["MenuLv2"] = "menu_hhdvk_tt";
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/DanhSach/Create.cshtml", model);

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

        //[Route("GiaHhDvk/Complete")]
        //[HttpPost]
        //public IActionResult Complete(string mahs_chuyen, string macqcq_chuyen)
        //{
        //    if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
        //    {
        //        if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.tt", "Index"))
        //        {
        //            var model = _db.GiaHhDvk.FirstOrDefault(t => t.Mahs == mahs_chuyen);

        //            var dvcq_join = from dvcq in _db.DsDonVi
        //                            join db in _db.DsDiaBan on dvcq.MaDiaBan equals db.MaDiaBan
        //                            select new VMDsDonVi
        //                            {
        //                                Id = dvcq.Id,
        //                                MaDiaBan = dvcq.MaDiaBan,
        //                                MaDv = dvcq.MaDv,
        //                                TenDv = dvcq.TenDv,
        //                                Level = db.Level,
        //                            };
        //            var chk_dvcq = dvcq_join.FirstOrDefault(t => t.MaDv == macqcq_chuyen);
        //            model.Macqcq = macqcq_chuyen;
        //            model.Trangthai = "HT";
        //            if (chk_dvcq != null && chk_dvcq.Level == "T")
        //            {
        //                model.Madv_t = macqcq_chuyen;
        //                model.Thoidiem_t = DateTime.Now;
        //                model.Trangthai_t = "CHT";
        //            }
        //            else if (chk_dvcq != null && chk_dvcq.Level == "ADMIN")
        //            {
        //                model.Madv_ad = macqcq_chuyen;
        //                model.Thoidiem_ad = DateTime.Now;
        //                model.Trangthai_ad = "CHT";
        //            }
        //            else
        //            {
        //                model.Madv_h = macqcq_chuyen;
        //                model.Thoidiem_h = DateTime.Now;
        //                model.Trangthai_h = "CHT";
        //            }
        //            _db.GiaHhDvk.Update(model);
        //            _db.SaveChanges();

        //            return RedirectToAction("Index", "GiaHhDvk", new { Nam = model.Nam, Thang = model.Thang, Madv = model.Madv });

        //        }
        //        else
        //        {
        //            ViewData["Messages"] = "Bạn không có quyền truy cập vào chức năng này!";
        //            return View("Views/Admin/Error/Page.cshtml");
        //        }
        //    }
        //    else
        //    {
        //        return View("Views/Admin/Error/SessionOut.cshtml");
        //    }
        //}
    }
}
