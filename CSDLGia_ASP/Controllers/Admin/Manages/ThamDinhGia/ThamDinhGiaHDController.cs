using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.ThamDinhGia;
using CSDLGia_ASP.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSDLGia_ASP.Controllers.Admin.Manages.ThamDinhGia
{
    public class ThamDinhGiaHDController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDsDonviService _dsDonviService;
        private readonly ITrangThaiHoSoService _trangThaiHoSoService;

        public ThamDinhGiaHDController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment, IDsDonviService dsDonviService, ITrangThaiHoSoService trangThaiHoSoService)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
            _dsDonviService = dsDonviService;
            _trangThaiHoSoService = trangThaiHoSoService;
        }

        [Route("ThamDinhGia/HoiDong")]
        [HttpGet]
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.hd", "Index"))
                {
                    var model = _db.ThamDinhGiaHD.ToList();

                    ViewData["Title"] = "Thông tin hội đồng thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_hd";

                    return View("Views/Admin/Manages/ThamDinhGia/HoiDong/Index.cshtml", model);
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

        [Route("ThamDinhGia/HoiDong/CreateOrEdit")]
        [HttpGet]
        public IActionResult CreateOrEdit(string MaHoiDong)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.hd", "Create"))
                {
                    var model = _db.ThamDinhGiaHD.FirstOrDefault(t => t.MaHoiDong == MaHoiDong);
                    if (model != null)
                    {
                        var model_ct = _db.ThamDinhGiaHDCt.Where(t => t.MaHoiDong == model.MaHoiDong).ToList();


                        if (model_ct != null && model_ct.Any())
                            model.ThamDinhGiaHDCt = model_ct;
                    }
                    else
                    {
                        model = new ThamDinhGiaHD();
                        model.MaHoiDong = DateTime.Now.ToString("yyMMddssmmHH");
                    }

                    ViewData["Title"] = "Thông tin hội đồng thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_hd";
                    return View("Views/Admin/Manages/ThamDinhGia/HoiDong/Edit.cshtml", model);

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

        [Route("ThamDinhGia/HoiDong/StoreOrUpdate")]
        [HttpPost]
        public IActionResult StoreOrUpdate(CSDLGia_ASP.Models.Manages.ThamDinhGia.ThamDinhGiaHD request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.hd", "Create"))
                {
                    var model = _db.ThamDinhGiaHD.FirstOrDefault(t => t.MaHoiDong == request.MaHoiDong);

                    //Xử lý file đính kèm
                    if (request.FileUpload != null && request.FileUpload.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            request.FileUpload.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            // Chuyển đổi dữ liệu tệp sang base64
                            request.FileQD_Base64 = Convert.ToBase64String(fileBytes);
                            request.FileQD = request.FileUpload.FileName;
                        }
                    }
                    else
                    {
                        //Giữ thông tin file cũ
                        if (model != null)
                        {
                            request.FileQD_Base64 = model.FileQD_Base64;
                            request.FileQD = model.FileQD;
                        }
                    }

                    if (model == null)
                    {
                        var newHD = new ThamDinhGiaHD();
                        newHD.Mahs = request.MaHoiDong;
                        newHD.MaHoiDong = request.MaHoiDong;
                        newHD.ToTung = request.ToTung;
                        newHD.CanCuPhapLy = request.CanCuPhapLy;
                        newHD.TheoDeNghi = request.TheoDeNghi;
                        newHD.CapHoiDong = request.CapHoiDong;
                        newHD.LoaiHoiDong = request.LoaiHoiDong;
                        newHD.SoQD = request.SoQD;
                        newHD.NgayQD = request.NgayQD;
                        newHD.CoQuanBanHanh = request.CoQuanBanHanh;
                        newHD.TenHoiDong = request.TenHoiDong;
                        newHD.ChuTichHoiDong = request.ChuTichHoiDong;
                        newHD.ChucVu = request.ChucVu;
                        newHD.NhiemVuHoiDong = request.NhiemVuHoiDong;
                        newHD.NoiDungQD = request.NoiDungQD;
                        newHD.MaTinhApDung = request.MaTinhApDung;
                        newHD.MaHuyenApDung = request.MaHuyenApDung;
                        newHD.FileQD = request.FileQD;
                        newHD.FileQD_Base64 = request.FileQD_Base64;
                        _db.ThamDinhGiaHD.Add(newHD);
                    }
                    else
                    {
                        model.Mahs = request.MaHoiDong;
                        model.MaHoiDong = request.MaHoiDong;
                        model.ToTung = request.ToTung;
                        model.CanCuPhapLy = request.CanCuPhapLy;
                        model.TheoDeNghi = request.TheoDeNghi;
                        model.CapHoiDong = request.CapHoiDong;
                        model.LoaiHoiDong = request.LoaiHoiDong;
                        model.SoQD = request.SoQD;
                        model.NgayQD = request.NgayQD;
                        model.CoQuanBanHanh = request.CoQuanBanHanh;
                        model.TenHoiDong = request.TenHoiDong;
                        model.ChuTichHoiDong = request.ChuTichHoiDong;
                        model.ChucVu = request.ChucVu;
                        model.NhiemVuHoiDong = request.NhiemVuHoiDong;
                        model.NoiDungQD = request.NoiDungQD;
                        model.MaTinhApDung = request.MaTinhApDung;
                        model.MaHuyenApDung = request.MaHuyenApDung;
                        model.FileQD = request.FileQD;
                        model.FileQD_Base64 = request.FileQD_Base64;
                        _db.ThamDinhGiaHD.Update(model);
                    }

                    _db.SaveChanges();

                    return RedirectToAction("Index", "ThamDinhGiaHD");
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

        [Route("ThamDinhGia/HoiDong/Delete")]
        [HttpPost]
        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.hd", "Delete"))
                {
                    var model_hd = _db.ThamDinhGiaHD.FirstOrDefault(t => t.Id == id_delete);
                    _db.ThamDinhGiaHD.Remove(model_hd);

                    if (model_hd != null)
                    {
                        var model_hdct = _db.ThamDinhGiaHDCt.Where(t => t.MaHoiDong == model_hd.MaHoiDong);
                        _db.ThamDinhGiaHDCt.RemoveRange(model_hdct);
                    }

                    _db.SaveChanges();

                    return RedirectToAction("Index", "ThamDinhGiaHD");
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
