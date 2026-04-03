using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Collections.Generic;
using CSDLGia_ASP.Services;
using CSDLGia_ASP.ViewModels.Manages.DinhGia;

namespace CSDLGia_ASP.Controllers.Admin.Manages.KeKhaiDangKyGia
{
    public class KeKhaiDangKyGiaXetDuyetController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IDsDonviService _dsDonviService;
        private readonly ITrangThaiHoSoService _trangThaiHoSoService;

        public KeKhaiDangKyGiaXetDuyetController(CSDLGiaDBContext db, IDsDonviService dsDonviService, ITrangThaiHoSoService trangThaiHoSoService)
        {
            _db = db;
            _dsDonviService = dsDonviService;
            _trangThaiHoSoService = trangThaiHoSoService;
        }

        [HttpGet("KeKhaiDangKyGiaXetDuyet")]
        public IActionResult Index(int Nam, string MaCqCq, string MaNghe)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.xetduyet", "Index"))
                {
                    MaCqCq = string.IsNullOrEmpty(MaCqCq) ? Helpers.GetSsAdmin(HttpContext.Session, "Madv") : MaCqCq;
                    MaNghe = string.IsNullOrEmpty(MaNghe) ? "all" : MaNghe;
                    List<string> list_trangthai = new List<string> { "CD", "DD", "CB" };
                    var model_donvicq = _dsDonviService.GetListDonvi(Helpers.GetSsAdmin(HttpContext.Session, "Madv"));
                    List<string> list_madvcq = model_donvicq.Select(t => t.MaDv).ToList();
                    var model_nghe = _db.DmNgheKd.Where(t => t.Theodoi == "TD").ToList();
                    model_nghe = model_nghe.Where(x => list_madvcq.Any(v => x.Madv.Split(',').Contains(v))).ToList();
                    List<string> list_manghe = model_nghe.Select(t => t.Manghe).ToList();

                    var model_hoso = _db.KeKhaiDangKyGia.Where(t => list_madvcq.Contains(t.MaCqCq) && list_manghe.Contains(t.MaNghe)
                                        && list_trangthai.Contains(t.TrangThai));
                    var model = from hoso in model_hoso
                                join cskd in _db.KeKhaiDangKyGiaCSKD on hoso.MaCsKd equals cskd.MaCsKd
                                join dn in _db.Company on cskd.MaDv equals dn.Madv
                                join donvi in _db.DsDonVi on hoso.MaCqCq equals donvi.MaDv
                                join dmnghe in _db.DmNgheKd on hoso.MaNghe equals dmnghe.Manghe
                                select new CSDLGia_ASP.Models.Manages.KeKhaiDangKyGia.KeKhaiDangKyGia
                                {
                                    Id = hoso.Id,
                                    Mahs = hoso.Mahs,
                                    TenCsKd = cskd.TenCsKd,
                                    TenDv = dn.Tendn,
                                    SoQD = hoso.SoQD,
                                    NgayQD = hoso.NgayQD,
                                    NgayChuyen = hoso.NgayChuyen,
                                    ThongTinNguoiChuyen = hoso.ThongTinNguoiChuyen,
                                    LyDo = hoso.LyDo,
                                    TrangThai = hoso.TrangThai,
                                    NgayDuyet = hoso.NgayDuyet,
                                    TenCqCq = donvi.TenDv,
                                    MaCqCq = hoso.MaCqCq,
                                    MaNghe = hoso.MaNghe,
                                    TenNghe = dmnghe.Phanloai + " " + dmnghe.Tennghe
                                };
                    if (Nam != 0)
                    {
                        model = model.Where(t => t.NgayQD.Year == Nam);
                    }
                    if (MaCqCq != "all")
                    {
                        model = model.Where(t => t.MaCqCq == MaCqCq);
                    }
                    if (MaNghe != "all")
                    {
                        model = model.Where(t => t.MaNghe == MaNghe);
                    }

                    ViewData["Nam"] = Nam;
                    ViewData["MaNghe"] = MaNghe;
                    ViewData["MaCqCq"] = MaCqCq;
                    ViewData["DsDonviCq"] = model_donvicq;
                    ViewData["DsNghe"] = model_nghe;
                    ViewData["SoHsDuyet"] = _db.KeKhaiDangKyGia.Where(t => t.MaNghe == MaNghe)?.Max(t => t.SoHsDuyet) ?? "0";
                    ViewData["maKetNoiAPI"] = "kkg";
                    ViewData["Title"] = "Thông tin xét duyệt hồ sơ kê khai đăng ký giá";
                    ViewData["MenuLv1"] = "menu_kekhaidangkygia";
                    ViewData["MenuLv2"] = "menu_kekhaidangkygia_xetduyet";
                    return View("Views/Admin/Manages/KeKhaiDangKyGia/ThongTinXetDuyet/Index.cshtml", model);
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
        public IActionResult Duyet(string mahs_duyet, string sohoso_duyet)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.xetduyet", "Approve"))
                {
                    var model = _db.KeKhaiDangKyGia.FirstOrDefault(p => p.Mahs == mahs_duyet);

                    model.NgayDuyet = DateTime.Now;
                    model.SoHsDuyet = sohoso_duyet;
                    model.TrangThai = "DD";

                    _db.KeKhaiDangKyGia.Update(model);
                    _db.SaveChanges();

                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Duyệt");
                    return RedirectToAction("Index", "KeKhaiDangKyGiaXetDuyet", new { Nam = model.NgayDuyet.Year, MaCqCq = model.MaCqCq, MaNghe = model.MaNghe });
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
        public IActionResult HuyDuyet(string mahs_huyduyet)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.xetduyet", "Approve"))
                {
                    var model = _db.KeKhaiDangKyGia.FirstOrDefault(p => p.Mahs == mahs_huyduyet);

                    model.NgayDuyet = DateTime.Now;
                    model.TrangThai = "CD";

                    _db.KeKhaiDangKyGia.Update(model);
                    _db.SaveChanges();

                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Hủy duyệt");
                    return RedirectToAction("Index", "KeKhaiDangKyGiaXetDuyet", new { Nam = model.NgayDuyet.Year, MaCqCq = model.MaCqCq, MaNghe = model.MaNghe });
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
        public IActionResult TraLai(int id_tralai, string Lydo)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.xetduyet", "Approve"))
                {
                    var model = _db.KeKhaiDangKyGia.FirstOrDefault(t => t.Id == id_tralai);
                    model.TrangThai = "BTL";
                    model.LyDo = Lydo;
                    model.NgayChuyen = DateTime.Now;

                    _db.KeKhaiDangKyGia.Update(model);
                    _db.SaveChanges();

                    // Xử lý phần lịch sử hồ sơ 

                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Trả lại");
                    //Kết thúc Xử lý phần lịch sử hồ sơ 


                    return RedirectToAction("Index", "KeKhaiDangKyGiaXetDuyet", new { Nam = model.NgayChuyen.Year, MaCqCq = model.MaCqCq, MaNghe = model.MaNghe });
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
        public IActionResult CongBo(string mahs_cb)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.xetduyet", "Public"))
                {
                    var model = _db.KeKhaiDangKyGia.FirstOrDefault(t => t.Mahs == mahs_cb);

                    model.TrangThai = "CB";

                    _db.KeKhaiDangKyGia.Update(model);
                    _db.SaveChanges();

                    // Xử lý phần lịch sử hồ sơ 


                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Công bố");

                    //Kết thúc Xử lý phần lịch sử hồ sơ 

                    return RedirectToAction("Index", "KeKhaiDangKyGiaXetDuyet", new { Nam = model.NgayDuyet.Year, MaCqCq = model.MaCqCq, MaNghe = model.MaNghe });
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
        public IActionResult HuyCongBo(string mahs_hcb)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.xetduyet", "Public"))
                {
                    var model = _db.KeKhaiDangKyGia.FirstOrDefault(t => t.Mahs == mahs_hcb);

                    model.TrangThai = "DD";

                    _db.KeKhaiDangKyGia.Update(model);
                    _db.SaveChanges();

                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), "Hủy công bố");
                    return RedirectToAction("Index", "KeKhaiDangKyGiaXetDuyet", new { Nam = model.NgayDuyet.Year, MaCqCq = model.MaCqCq, MaNghe = model.MaNghe });
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
    }
}
