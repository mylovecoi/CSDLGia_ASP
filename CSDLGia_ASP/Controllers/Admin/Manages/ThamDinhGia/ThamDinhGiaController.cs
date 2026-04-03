using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.ThamDinhGia;
using CSDLGia_ASP.Models.Systems;
using CSDLGia_ASP.Services;
using CSDLGia_ASP.ViewModels.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSDLGia_ASP.Controllers.Admin.Manages.ThamDinhGia
{
    public class ThamDinhGiaController : Controller
    {

        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDsDonviService _dsDonviService;
        private readonly ITrangThaiHoSoService _trangThaiHoSoService;

        public ThamDinhGiaController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment, IDsDonviService dsDonviService, ITrangThaiHoSoService trangThaiHoSoService)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
            _dsDonviService = dsDonviService;
            _trangThaiHoSoService = trangThaiHoSoService;
        }

        [Route("ThamDinhGia/DanhSach")]
        [HttpGet]
        public IActionResult Index(string Nam, string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Index"))
                {
                    Madv = string.IsNullOrEmpty(Madv) ? Helpers.GetSsAdmin(HttpContext.Session, "Madv") : Madv;

                    var model_donvi = _dsDonviService.GetListDonvi(Helpers.GetSsAdmin(HttpContext.Session, "Madv"));
                    List<string> list_madv = model_donvi.Select(t => t.MaDv).ToList();

                    var model = _db.ThamDinhGia.Where(t => t.Madv == Madv).ToList();
                    var model_join = (from tdg in model
                                      join dmnhomhh in _db.DmNhomHh on tdg.Tttstd equals dmnhomhh.Manhom
                                      select new CSDLGia_ASP.Models.Manages.ThamDinhGia.ThamDinhGia
                                      {
                                          Id = tdg.Id,
                                          Mahs = tdg.Mahs,
                                          Madv = tdg.Madv,
                                          Madiaban = tdg.Madiaban,
                                          Diadiem = tdg.Diadiem,
                                          Ppthamdinh = tdg.Ppthamdinh,
                                          Mucdich = tdg.Mucdich,
                                          Dvyeucau = tdg.Dvyeucau,
                                          Thoihan = tdg.Thoihan,
                                          Thoidiem = tdg.Thoidiem,
                                          Sotbkl = tdg.Sotbkl,
                                          Hosotdgia = tdg.Hosotdgia,
                                          Nguonvon = tdg.Nguonvon,
                                          Phanloai = tdg.Phanloai,
                                          Quy = tdg.Quy,
                                          Thuevat = tdg.Thuevat,
                                          Songaykq = tdg.Songaykq,
                                          Tttstd = tdg.Tttstd,
                                          Soqdpheduyet = tdg.Soqdpheduyet,
                                          Ngayqdpheduyet = tdg.Ngayqdpheduyet,
                                          Lydo = tdg.Lydo,
                                          Thongtin = tdg.Thongtin,
                                          Trangthai = tdg.Trangthai,
                                          Tennhomhh = dmnhomhh.Tennhom,
                                          Macqcq = tdg.Macqcq,
                                      });

                    if (string.IsNullOrEmpty(Nam))
                    {
                        Nam = Helpers.ConvertYearToStr(DateTime.Now.Year);
                        model_join = model_join.Where(t => Helpers.ConvertYearToStr(t.Thoidiem.Year) == Nam).ToList();
                    }
                    else
                    {
                        if (Nam != "all")
                        {
                            model_join = model_join.Where(t => Helpers.ConvertYearToStr(t.Thoidiem.Year) == Nam).ToList();
                        }
                        else
                        {
                            model_join = model_join.ToList();
                        }
                    }

                    ViewData["DsDonVi"] = model_donvi;
                    var cqcq = _db.DsDonVi.ToList();
                    //Lấy tên đơn vị chủ quản
                    foreach (var item in model_join)
                    {
                        var cq = cqcq.FirstOrDefault(x => x.MaDv == item.Macqcq);
                        if (cq != null)
                        {
                            item.Tencqcq = cq.TenDv;
                        }
                    }

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

                    ViewData["DsDiaBan"] = _db.DsDiaBan;
                    ViewData["DmNhomHh"] = _db.DmNhomHh.Where(t => t.Phanloai == "THAMDINHGIA" && t.Theodoi == "TD").ToList();
                    ViewData["Nam"] = Nam;
                    ViewData["Madv"] = Madv;
                    ViewData["Title"] = "Thông tin hồ sơ thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_tt";

                    return View("Views/Admin/Manages/ThamDinhGia/DanhSach/Index.cshtml", model_join);
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

        [Route("ThamDinhGia/DanhSach/Create")]
        [HttpGet]
        public IActionResult Create(string Madv, string Phanloai, string Tttstd)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Create"))
                {
                    var check = _db.ThamDinhGiaCt.Where(t => t.Trangthai == "CXD");
                    if (check != null)
                    {
                        _db.ThamDinhGiaCt.RemoveRange(check);
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

                    var model = new CSDLGia_ASP.Models.Manages.ThamDinhGia.ThamDinhGia
                    {
                        Mahs = Madv + "_" + DateTime.Now.ToString("yyMMddssmmHH"),
                        Madv = Madv,
                        Madiaban = _db.DsDonVi.FirstOrDefault(t => t.MaDv == Madv).MaDiaBan,
                        Phanloai = Phanloai,
                        Tttstd = Tttstd,
                        Thoidiem = DateTime.Now,
                        Ngayqdpheduyet = DateTime.Now,
                        Thoihan = DateTime.Now,
                    };

                    ViewData["Madv"] = Madv;
                    ViewData["TdgDonvi"] = _db.ThamDinhGiaDv.ToList();
                    ViewData["TdgDmHh"] = _db.ThamDinhGiaDmHh.Where(t => t.Manhom == model.Tttstd).ToList();
                    ViewData["DmNhomHh"] = _db.DmNhomHh.Where(t => t.Phanloai == "THAMDINHGIA" && t.Theodoi == "TD").ToList();
                    ViewData["Dvt"] = _db.DmDvt.ToList();
                    ViewData["Title"] = "Hồ sơ thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_tt";
                    return View("Views/Admin/Manages/ThamDinhGia/DanhSach/Create.cshtml", model);

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

        [Route("ThamDinhGia/DanhSach/Store")]
        [HttpPost]
        public async Task<IActionResult> Store(CSDLGia_ASP.Models.Manages.ThamDinhGia.ThamDinhGia request, IFormFile Ipf1upload)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Create"))
                {
                    if (Ipf1upload != null && Ipf1upload.Length > 0)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string filename = Path.GetFileNameWithoutExtension(Ipf1upload.FileName);
                        string extension = Path.GetExtension(Ipf1upload.FileName);
                        filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "/Upload/File/ThamDinhGia/", filename);
                        using (var FileStream = new FileStream(path, FileMode.Create))
                        {
                            await Ipf1upload.CopyToAsync(FileStream);
                        }
                        request.Ipf1 = filename;
                    }

                    var model = new CSDLGia_ASP.Models.Manages.ThamDinhGia.ThamDinhGia
                    {
                        Mahs = request.Mahs,
                        Madiaban = request.Madiaban,
                        Madv = request.Madv,
                        Dvyeucau = request.Dvyeucau,
                        Dvthamdinh = request.Dvthamdinh,
                        Hosotdgia = request.Hosotdgia,
                        Tttstd = request.Tttstd,
                        Thoidiem = request.Thoidiem,
                        Diadiem = request.Diadiem,
                        Sotbkl = request.Sotbkl,
                        Songaykq = request.Songaykq,
                        Thoihan = request.Thoihan,
                        Soqdpheduyet = request.Soqdpheduyet,
                        Ngayqdpheduyet = request.Ngayqdpheduyet,
                        Ghichu = request.Ghichu,
                        Phanloai = request.Phanloai,
                        Trangthai = "CHT",
                        Congbo = "CHUACONGBO",
                        Ipf1 = request.Ipf1,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                    };
                    _db.ThamDinhGia.Add(model);
                    _db.SaveChanges();

                    var modelct = _db.ThamDinhGiaCt.Where(t => t.Mahs == request.Mahs);
                    if (modelct != null)
                    {
                        foreach (var item in modelct)
                        {
                            item.Trangthai = "XD";
                        }
                    }
                    _db.ThamDinhGiaCt.UpdateRange(modelct);
                    _db.SaveChanges();

                    var modelFile = _db.ThongTinGiayTo.Where(t => t.Mahs == request.Mahs);
                    if (modelFile.Any())
                    {
                        foreach (var file in modelFile) { file.Status = "XD"; }
                    }
                    _db.ThongTinGiayTo.UpdateRange(modelFile);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "ThamDinhGia", new { request.Madv });
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

        [Route("ThamDinhGia/DanhSach/Edit")]
        [HttpGet]
        public IActionResult Edit(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Edit"))
                {
                    var model = _db.ThamDinhGia.FirstOrDefault(t => t.Mahs == Mahs);

                    var model_ct = _db.ThamDinhGiaCt.Where(t => t.Mahs == model.Mahs);
                    model.ThamDinhGiaCt = model_ct.ToList();

                    var model_file = _db.ThongTinGiayTo.Where(t => t.Mahs == model.Mahs);
                    model.ThongTinGiayTo = model_file.ToList();

                    ViewData["Mahs"] = model.Mahs;
                    ViewData["Madv"] = model.Madv;
                    ViewData["Madiaban"] = model.Madiaban;
                    ViewData["Tttstd"] = model.Tttstd;
                    ViewData["TdgDonvi"] = _db.ThamDinhGiaDv.ToList();
                    ViewData["TdgDmHh"] = _db.ThamDinhGiaDmHh.ToList();
                    ViewData["DmNhomHh"] = _db.DmNhomHh.Where(t => t.Phanloai == "THAMDINHGIA" && t.Theodoi == "TD").ToList();
                    ViewData["Dvt"] = _db.DmDvt.ToList();
                    ViewData["Title"] = "Hồ sơ thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_tt";
                    return View("Views/Admin/Manages/ThamDinhGia/DanhSach/Edit.cshtml", model);

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

        [Route("ThamDinhGia/DanhSach/EditHoiDong")]
        [HttpGet]
        public IActionResult EditHoiDong(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Edit"))
                {
                    var model_hoso = _db.ThamDinhGia.FirstOrDefault(t => t.Mahs == Mahs);
                    var model = _db.ThamDinhGiaHD.FirstOrDefault(t => t.Mahs == Mahs);
                    if (model != null)
                    {
                        var model_ct = _db.ThamDinhGiaHDCt.Where(t => t.MaHoiDong == model.MaHoiDong).ToList();


                        if (model_ct != null && model_ct.Any())
                            model.ThamDinhGiaHDCt = model_ct;
                    }
                    else
                    {
                        model = new ThamDinhGiaHD();
                        model.MaHoiDong = Mahs;
                        model.Mahs = Mahs;
                    }
                    

                    ViewData["MaDV"] = model_hoso.Madv;
                    ViewData["Title"] = "Thông tin hội đồng thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_tt";
                    return View("Views/Admin/Manages/ThamDinhGia/DanhSach/HoiDong/Edit.cshtml", model);

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

        [Route("ThamDinhGia/DanhSach/Update")]
        [HttpPost]
        public IActionResult Update(CSDLGia_ASP.Models.Manages.ThamDinhGia.ThamDinhGia request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Edit"))
                {

                    var model = _db.ThamDinhGia.FirstOrDefault(t => t.Mahs == request.Mahs);
                    model.Dvyeucau = request.Dvyeucau;
                    model.Dvthamdinh = request.Dvthamdinh;
                    model.Hosotdgia = request.Hosotdgia;
                    model.Tttstd = request.Tttstd;
                    model.Thoidiem = request.Thoidiem;
                    model.Diadiem = request.Diadiem;
                    model.Sotbkl = request.Sotbkl;
                    model.Songaykq = request.Songaykq;
                    model.Thoihan = request.Thoihan;
                    model.Soqdpheduyet = request.Soqdpheduyet;
                    model.Ngayqdpheduyet = request.Ngayqdpheduyet;
                    model.Ghichu = request.Ghichu;
                    model.Phanloai = request.Phanloai;
                    model.Updated_at = DateTime.Now;

                    var modelct = _db.ThamDinhGiaCt.Where(t => t.Mahs == request.Mahs);
                    if (modelct.Any()) { foreach (var ct in modelct) { ct.Trangthai = "XD"; } }
                    var modelfile = _db.ThongTinGiayTo.Where(t => t.Mahs == request.Mahs);
                    if (modelfile.Any()) { foreach (var file in modelfile) { file.Status = "Enable"; } }

                    _db.ThamDinhGia.Update(model);
                    _db.ThongTinGiayTo.UpdateRange(modelfile);
                    _db.ThamDinhGiaCt.UpdateRange(modelct);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "ThamDinhGia", new { request.Madv });
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

        [Route("ThamDinhGia/DanhSach/UpdateHoiDong")]
        [HttpPost]
        public IActionResult UpdateHoiDong(CSDLGia_ASP.Models.Manages.ThamDinhGia.ThamDinhGiaHD request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Edit"))
                {
                    var model_hoso = _db.ThamDinhGia.FirstOrDefault(t => t.Mahs == request.Mahs);
                    var model = _db.ThamDinhGiaHD.FirstOrDefault(t => t.Mahs == request.Mahs && t.MaHoiDong == request.MaHoiDong);

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
                        newHD.Mahs = request.Mahs;
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
                        model.Mahs = request.Mahs;
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

                    return RedirectToAction("Index", "ThamDinhGia", new { model_hoso.Madv });
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

        [Route("ThamDinhGia/DanhSach/Delete")]
        [HttpPost]
        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Delete"))
                {
                    var model = _db.ThamDinhGia.FirstOrDefault(t => t.Id == id_delete);
                    _db.ThamDinhGia.Remove(model);                  

                    var model_ct = _db.ThamDinhGiaCt.Where(t => t.Mahs == model.Mahs);
                    _db.ThamDinhGiaCt.RemoveRange(model_ct);

                    var model_hd = _db.ThamDinhGiaHD.FirstOrDefault(t => t.Mahs == model.Mahs);
                    _db.ThamDinhGiaHD.Remove(model_hd);
                    if (model_hd != null)
                    {
                        var model_hdct = _db.ThamDinhGiaHDCt.Where(t => t.MaHoiDong == model_hd.MaHoiDong);
                        _db.ThamDinhGiaHDCt.RemoveRange(model_hdct);
                    }

                    _db.SaveChanges();

                    return RedirectToAction("Index", "ThamDinhGia", new { model.Madv });
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

        [Route("ThamDinhGia/DanhSach/Show")]
        [HttpGet]
        public IActionResult Show(string Mahs)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Index"))
                {
                    var model = _db.ThamDinhGia.FirstOrDefault(t => t.Mahs == Mahs);

                    var model_ct = _db.ThamDinhGiaCt.Where(t => t.Mahs == model.Mahs);

                    model.ThamDinhGiaCt = model_ct.ToList();

                    ViewData["Mahs"] = model.Mahs;
                    ViewData["Madv"] = model.Madv;
                    ViewData["TdgDonvi"] = _db.ThamDinhGiaDv.ToList();
                    ViewData["TdgDmHh"] = _db.ThamDinhGiaDmHh.ToList();
                    ViewData["DsDiaBan"] = _db.DsDiaBan.ToList();
                    ViewData["DsDonVi"] = _db.DsDonVi.ToList();
                    ViewData["Dvt"] = _db.DmDvt.ToList();
                    ViewData["Title"] = "Hồ sơ thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_tt";
                    return View("Views/Admin/Manages/ThamDinhGia/DanhSach/Show.cshtml", model);

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

        [Route("ThamDinhGia/DanhSach/ShowFile")]
        [HttpPost]
        public JsonResult ShowFile(int Id)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                var model = _db.ThamDinhGia.FirstOrDefault(t => t.Id == Id);
                string result = "<div class='modal-body' id='frm_file'>";
                result += "<div class='row'>";
                result += "<div class='col-xl-12'>";
                result += "<div class='form -group fv-plugins-icon-container'>";
                result += "<label style='font-weight:bold'>File đính kèm</label>";

                if (model.Ipf1 != null && model.Ipf1.Length > 0)
                {
                    result += "<p>";
                    result += " - ";
                    result += "<a href='/UpLoad/File/ThamDinhGia/" + model.Ipf1 + "' target='_blank' class='btn btn-link'";
                    result += " onclick='window.open(`/UpLoad/File/ThamDinhGia/" + model.Ipf1 + "`, `mywin`, `left=20,top=20,width=500,height=500,toolbar=1,resizable=0`); return false;'>";
                    result += model.Ipf1 + "</a>";
                    result += "</p>";
                }

                result += "</div>";
                result += "</div>";

                result += "</div>";
                result += "</div>";

                var data = new { status = "success", message = result };
                return Json(data);
            }
            else
            {
                var data = new { status = "error", message = "Bạn kêt thúc phiên đăng nhập! Đăng nhập lại để tiếp tục công việc" };
                return Json(data);
            }
        }

        //[Route("ThamDinhGia/DanhSach/Complete")]
        //[HttpPost]
        //public IActionResult Complete(string mahs_chuyen, string macqcq_chuyen)
        //{
        //    if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
        //    {
        //        if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Index"))
        //        {
        //            var model = _db.ThamDinhGia.FirstOrDefault(t => t.Mahs == mahs_chuyen);

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
        //            _db.ThamDinhGia.Update(model);
        //            _db.SaveChanges();

        //            return RedirectToAction("Index", "ThamDinhGia", new { model.Madv });

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

        [Route("ThamDinhGia/DanhSach/Complete")]
        [HttpPost]
        public IActionResult Complete(string mahs_chuyen, string trangthai_complete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tt", "Index"))
                {
                    var model = _db.ThamDinhGia.FirstOrDefault(t => t.Mahs == mahs_chuyen);
                    model.Updated_at = DateTime.Now;
                    model.Trangthai = trangthai_complete;

                    _db.ThamDinhGia.Update(model);
                    _db.SaveChanges();
                    //Add Log
                    _trangThaiHoSoService.LogHoSo(model.Mahs, Helpers.GetSsAdmin(HttpContext.Session, "Name"), trangthai_complete);



                    return RedirectToAction("Index", "ThamDinhGia", new { model.Madv, Nam = model.Thoidiem.Year });

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

        [Route("ThamDinhGia/TimKiem")]
        [HttpGet]
        public IActionResult Search()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tk", "Index"))
                {

                    if (Helpers.GetSsAdmin(HttpContext.Session, "Madv") != null)
                    {
                        ViewData["Madv"] = Helpers.GetSsAdmin(HttpContext.Session, "Madv");
                    }
                    else
                    {
                        ViewData["Madv"] = "";
                    }
                    ViewData["DsDiaBan"] = _db.DsDiaBan;
                    ViewData["DsDonVi"] = _db.DsDonVi.Where(t => t.ChucNang != "QUANTRI");
                    ViewData["Title"] = "Tìm kiếm thông tin hồ sơ thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_tk";
                    return View("Views/Admin/Manages/ThamDinhGia/TimKiem/Index.cshtml");

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

        [Route("ThamDinhGia/TimKiem/KetQua")]
        [HttpPost]
        public IActionResult Result(string madv, string tenspdv, DateTime ngaynhap_tu, DateTime ngaynhap_den,
            double giatd_tu, double giatd_den, string tendvtd, string tendvyctd, double sl_tu, double sl_den)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdltdg.tdg.tk", "Index"))
                {
                    var model = (from tdgct in _db.ThamDinhGiaCt
                                 join tdg in _db.ThamDinhGia on tdgct.Mahs equals tdg.Mahs
                                 join dv in _db.DsDonVi on tdg.Madv equals dv.MaDv
                                 select new ThamDinhGiaCt
                                 {
                                     Id = tdgct.Id,
                                     Mahs = tdgct.Mahs,
                                     Tents = tdgct.Tents,
                                     Sl = tdgct.Sl,
                                     Giadenghi = tdgct.Giadenghi,
                                     Giatritstd = tdgct.Giatritstd,
                                     Madv = tdg.Madv,
                                     Thoidiem = tdg.Thoidiem,
                                     Tendv = dv.TenDv,
                                     Tttstd = tdg.Tttstd,
                                     Dvyeucau = tdg.Dvyeucau,
                                     Dvthamdinh = tdg.Dvthamdinh,
                                 });

                    if (madv != "all")
                    {
                        model = model.Where(t => t.Madv == madv);
                    }
                    //Tên hàng hoá thẩm định
                    if (!string.IsNullOrEmpty(tenspdv))
                    {
                        model = model.Where(t => t.Tents.Contains(tenspdv));
                    }

                    //Tên đơn vị thẩm định
                    if (!string.IsNullOrEmpty(tendvtd))
                    {
                        model = model.Where(t => t.Dvthamdinh.Contains(tendvtd));
                    }

                    //Tên đơn vị yêu cầu thẩm định
                    if (!string.IsNullOrEmpty(tendvtd))
                    {
                        model = model.Where(t => t.Dvyeucau.Contains(tendvyctd));
                    }

                    //Thời gian thẩm định
                    if (ngaynhap_tu.ToString("yyMMdd") != "010101")
                    {
                        model = model.Where(t => t.Thoidiem >= ngaynhap_tu);
                    }

                    if (ngaynhap_den.ToString("yyMMdd") != "010101")
                    {
                        model = model.Where(t => t.Thoidiem <= ngaynhap_den);
                    }

                    //Số lượng
                    if (sl_tu > 0)
                    {
                        model = model.Where(t => t.Sl >= sl_tu);
                    }

                    if (sl_den > 0)
                    {
                        model = model.Where(t => t.Sl <= sl_den);
                    }

                    //Giá thị thẩm định
                    if (giatd_tu > 0)
                    {
                        model = model.Where(t => t.Giatritstd >= giatd_tu);
                    }

                    if (giatd_den > 0)
                    {
                        model = model.Where(t => t.Giatritstd <= giatd_den);
                    }

                    ViewData["Title"] = "Tìm kiếm thông tin hồ sơ thẩm định giá";
                    ViewData["MenuLv1"] = "menu_tdg";
                    ViewData["MenuLv2"] = "menu_tdg_tk";
                    return View("Views/Admin/Manages/ThamDinhGia/TimKiem/Result.cshtml", model);

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

        [Route("ThamDinhGia/CongBo")]
        [HttpGet]
        public IActionResult CongBo(string Nam, string Madv)
        {

            var model = _db.ThamDinhGia.Where(t => t.Trangthai == "HT").ToList();
            var model_join = (from tdg in model
                              join dmnhomhh in _db.DmNhomHh on tdg.Tttstd equals dmnhomhh.Manhom
                              select new CSDLGia_ASP.Models.Manages.ThamDinhGia.ThamDinhGia
                              {
                                  Id = tdg.Id,
                                  Mahs = tdg.Mahs,
                                  Madv = tdg.Madv,
                                  Madiaban = tdg.Madiaban,
                                  Diadiem = tdg.Diadiem,
                                  Ppthamdinh = tdg.Ppthamdinh,
                                  Mucdich = tdg.Mucdich,
                                  Dvyeucau = tdg.Dvyeucau,
                                  Thoihan = tdg.Thoihan,
                                  Thoidiem = tdg.Thoidiem,
                                  Sotbkl = tdg.Sotbkl,
                                  Hosotdgia = tdg.Hosotdgia,
                                  Nguonvon = tdg.Nguonvon,
                                  Phanloai = tdg.Phanloai,
                                  Quy = tdg.Quy,
                                  Thuevat = tdg.Thuevat,
                                  Songaykq = tdg.Songaykq,
                                  Tttstd = tdg.Tttstd,
                                  Soqdpheduyet = tdg.Soqdpheduyet,
                                  Ngayqdpheduyet = tdg.Ngayqdpheduyet,
                                  Lydo = tdg.Lydo,
                                  Thongtin = tdg.Thongtin,
                                  Trangthai = tdg.Trangthai,
                                  Tennhomhh = dmnhomhh.Tennhom,
                              });

            if (string.IsNullOrEmpty(Nam))
            {
                Nam = Helpers.ConvertYearToStr(DateTime.Now.Year);
                model_join = model_join.Where(t => Helpers.ConvertYearToStr(t.Thoidiem.Year) == Nam).ToList();
            }
            else
            {
                if (Nam != "all")
                {
                    model_join = model_join.Where(t => Helpers.ConvertYearToStr(t.Thoidiem.Year) == Nam).ToList();
                }
                else
                {
                    model_join = model_join.ToList();
                }
            }


            ViewData["DsDonVi"] = _db.DsDonVi;
            ViewData["DsDiaBan"] = _db.DsDiaBan;
            ViewData["DmNhomHh"] = _db.DmNhomHh.Where(t => t.Phanloai == "THAMDINHGIA" && t.Theodoi == "TD").ToList();
            ViewData["Nam"] = Nam;
            ViewData["Madv"] = Madv;
            ViewBag.bSession = true;
            ViewData["Title"] = "Công bố hồ sơ thẩm định giá";

            return View("Views/Admin/Systems/CongBo/ThamDinhGia.cshtml", model_join);
        }
    }
}
