﻿using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;
using System.Collections.Generic;


namespace CSDLGia_ASP.Controllers.Admin.Systems
{
    public class YKienGopYController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public YKienGopYController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;

        }
        [Route("YKienDongGop")]
        [HttpGet]
        public IActionResult Index(string Madv)
        {
            // Kiểm tra nhóm chức năng của Madv đăng nhập
            Madv = string.IsNullOrEmpty(Madv) ? Helpers.GetSsAdmin(HttpContext.Session, "Madv") : Madv;

            var maChucNangQuery = from gp in _db.GroupPermissions
                                  join u in _db.Users on gp.KeyLink equals u.Chucnang
                                  where u.Madv == Helpers.GetSsAdmin(HttpContext.Session, "Madv")
                                  select gp.ChucNang;

            var chucNang = maChucNangQuery.FirstOrDefault();

            // Lấy các bản ghi theo nhóm chức năng của mã đơn vị đăng nhập

            List<YKienGopY> danhsachykien = new List<YKienGopY>();

            if (chucNang == "QUANTRI" || chucNang == "TONGHOP" && Madv == Helpers.GetSsAdmin(HttpContext.Session, "Madv"))
            {
                danhsachykien = _db.YKienGopY.ToList();
            }

            if (Madv != Helpers.GetSsAdmin(HttpContext.Session, "Madv"))
            {
                danhsachykien = _db.YKienGopY.Where(t => t.MaDv == Madv).ToList();
            }
            if (Madv == "all")
            {
                danhsachykien = _db.YKienGopY.ToList();
            }

            if (chucNang == "NHAPLIEU")
            {
                Madv = Helpers.GetSsAdmin(HttpContext.Session, "Madv");
                danhsachykien = _db.YKienGopY.Where(t => t.MaDv == Madv).ToList();
            }

            // Xử lý phần ô select
            var danhsachdonvi = new List<object>();

            if (chucNang == "NHAPLIEU")
            {
                danhsachdonvi = (from ykgy in _db.YKienGopY
                                 where ykgy.MaDv == Helpers.GetSsAdmin(HttpContext.Session, "Madv")
                                 select new { ykgy.MaDv, ykgy.TenDangNhap }).Distinct().Cast<object>().ToList();
            }
            else
            {
                danhsachdonvi = (from ykgy in _db.YKienGopY
                                 select new { ykgy.MaDv, ykgy.TenDangNhap }).Distinct().Cast<object>().ToList();
            }

            ViewData["DonViList"] = danhsachdonvi;

            ViewData["Madv"] = Madv;

            ViewData["Title"] = "Thông tin ý kiến đóng góp";
            // Trả về view với danh sách ý kiến
            return View("Views/Admin/Systems/YKienGopY/Index.cshtml", danhsachykien);


        }

        [Route("YKienDongGop/Store")]
        [HttpPost]
        public async Task<IActionResult> Store(YKienGopY request, IFormFile Ipf1)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {

                if (Ipf1 != null && Ipf1.Length > 0)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string filename = Path.GetFileNameWithoutExtension(Ipf1.FileName);
                    string extension = Path.GetExtension(Ipf1.FileName);
                    filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = Path.Combine(wwwRootPath + "/Upload/File", filename);
                    using (var FileStream = new FileStream(path, FileMode.Create))
                    {
                        await Ipf1.CopyToAsync(FileStream);
                    }
                    request.Ipf1 = filename;
                }
                var model = new YKienGopY
                {
                    MaDv = Helpers.GetSsAdmin(HttpContext.Session, "Madv"),
                    TieuDe = request.TieuDe,
                    NoiDung = request.NoiDung,
                    TenDangNhap = Helpers.GetSsAdmin(HttpContext.Session, "Name"),
                    NgayGopY = DateTime.Now,
                    Ipf1 = request.Ipf1,
                    Created_at = DateTime.Now,
                    Updated_at = DateTime.Now,
                };

                _db.YKienGopY.Add(model);
                _db.SaveChanges();
                var danhsachykien = _db.YKienGopY.Count();
                HttpContext.Session.SetString("DanhSachYKienDongGop", danhsachykien.ToString());

                ViewData["Title"] = " Thông tin ý kiến đóng góp ";

                var danhsachdonvi = (from ykgy in _db.YKienGopY
                                     select new { ykgy.MaDv, ykgy.TenDangNhap }).Distinct();
                ViewData["DonViList"] = danhsachdonvi;

                return RedirectToAction("Index", "YKienGopY", new { Madv = Helpers.GetSsAdmin(HttpContext.Session, "Madv") });

            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }

        }

        [Route("YKienDongGop/Edit")]
        [HttpPost]
        public JsonResult Edit(int Id)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {

                var model = _db.YKienGopY.FirstOrDefault(t => t.Id == Id);

                string result = "<div class='modal-body' id='frm_edit'>";
                result += "<div class='row'>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Tiêu đề:</label>";
                result += "<input type='text' class='form-control' id='tieude_edit' name='tieude_edit'value='" + model.TieuDe + "'/>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Nội dung:</label>";
                result += "<textarea class='form-control' id='noidung_edit' name='noidung_edit' rows='5' cols='30'>" + model.NoiDung + "</textarea>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form -group fv-plugins-icon-container'>";
                if (!string.IsNullOrEmpty(model.Ipf1))
                {
                    result += "<br>";
                    result += "<a href='/UpLoad/File/" + model.Ipf1 + "' target='_blank' class='btn btn-link'";
                    result += " onclick='window.open(`/UpLoad/File/" + model.Ipf1 + "`, `mywin`, `left=20,top=20,width=500,height=500,toolbar=1,resizable=0`); return false;'>";
                    result += "<i class='icon-lg la la-eye text-success''></i>File đính kèm hiện tại</a>";
                    result += "<br>";
                }
                result += "<label>File mới</label>";
                result += "<input type='file' class='form-control' id='ipf1_edit' name='ipf1_edit'/>";
                result += "</div>";
                result += "</div>";

                result += "</div>";
                result += "<input hidden class='form-control' id='id_edit' name='id_edit' value='" + model.Id + "'/>";
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

        [HttpPost("YKienDongGop/Update")] // Chỉnh sửa đường dẫn
        [DisableRequestSizeLimit]
        public async Task<JsonResult> Update(IFormFile FileUpLoad, YKienGopY request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {

                var model = _db.YKienGopY.FirstOrDefault(t => t.Id == request.Id);

                if (model != null)
                {
                    string filename = model.Ipf1;
                    string wwwRootPath = _hostEnvironment.WebRootPath;

                    if (FileUpLoad != null)
                    {
                        string extension = Path.GetExtension(FileUpLoad.FileName);
                        if (Helpers.CheckFileType(extension)) // Sử dụng Helper.CheckFileType thay vì Helper.Helpers.CheckFileType
                        {
                            // Xóa file cũ nếu có
                            if (!string.IsNullOrEmpty(filename))
                            {
                                string path_del = Path.Combine(wwwRootPath, "UpLoad", "File", filename);
                                if (System.IO.File.Exists(path_del))
                                {
                                    System.IO.File.Delete(path_del);
                                }
                            }

                            // Tạo tên file mới
                            string name = Path.GetFileNameWithoutExtension(FileUpLoad.FileName);
                            name = Regex.Replace(name.Normalize(NormalizationForm.FormD), @"[^\p{L}\p{N}]", ""); // Xóa ký tự đặc biệt
                            filename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + name + extension; // Sử dụng định dạng ngày tháng năm đầy đủ

                            // Tạo đường dẫn lưu file mới
                            string path_up = Path.Combine(wwwRootPath, "UpLoad", "File", filename);

                            // Lưu file mới
                            using (var fileStream = new FileStream(path_up, FileMode.Create))
                            {
                                await FileUpLoad.CopyToAsync(fileStream);
                            }

                            // Cập nhật thông tin vào model
                            model.TieuDe = request.TieuDe;
                            model.NoiDung = request.NoiDung;
                            model.Ipf1 = filename;
                            model.NgayGopY = DateTime.Now;

                            // Cập nhật model và lưu thay đổi vào cơ sở dữ liệu
                            _db.YKienGopY.Update(model);
                            await _db.SaveChangesAsync();
                            ViewData["Title"] = " Thông tin ý kiến đóng góp ";
                            var data = new { status = "success" };
                            return Json(data);
                        }
                        else
                        {
                            var data = new { status = "error", message = "File tải lên không đúng định dạng (.jpg, .jpeg, .png, .doc, .docx, .xls, .xlsx, .pdf)!" };
                            return Json(data);
                        }
                    }
                    else
                    {
                        // Nếu không có file được tải lên, chỉ cập nhật dữ liệu không cần thay đổi file
                        model.TieuDe = request.TieuDe;
                        model.NoiDung = request.NoiDung;
                        model.NgayGopY = DateTime.Now;

                        // Cập nhật model và lưu thay đổi vào cơ sở dữ liệu
                        _db.YKienGopY.Update(model);
                        await _db.SaveChangesAsync();
                        ViewData["Title"] = " Thông tin ý kiến đóng góp ";
                        var data = new { status = "success" };
                        return Json(data);
                    }

                }
                else
                {
                    var data = new { status = "error", message = "Không tìm thấy bản ghi cần cập nhật!" };
                    return Json(data);
                }
            }
            else
            {
                var data = new { status = "error", message = "Bạn kêt thúc phiên đăng nhập! Đăng nhập lại để tiếp tục công việc" };
                return Json(data);
            }

        }

        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {

                var DanhSachGopY = _db.YKienGopY.Find(id_delete);
                if (DanhSachGopY != null)
                {
                    if (!string.IsNullOrEmpty(DanhSachGopY.Ipf1))
                    {
                        // Xóa tệp tin từ thư mục /Upload/File nếu tệp tin tồn tại
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "File", DanhSachGopY.Ipf1);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    ViewData["Title"] = "Thông tin ý kiến đóng góp";

                    // Xóa thông tin tài liệu khỏi cơ sở dữ liệu
                    _db.YKienGopY.Remove(DanhSachGopY);
                    _db.SaveChanges();
                }

                var danhsachdonvi = (from ykgy in _db.YKienGopY
                                     select new { ykgy.MaDv, ykgy.TenDangNhap }).Distinct();
                ViewData["DonViList"] = danhsachdonvi;

                var danhsachykien = _db.YKienGopY.Count();
                HttpContext.Session.SetString("DanhSachYKienDongGop", danhsachykien.ToString());
                return RedirectToAction("Index", "YKienGopY", new { MaDv = Helpers.GetSsAdmin(HttpContext.Session, "Madv") });

            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }
        }

        [Route("YKienDongGopPhanHoi/Edit")]
        [HttpPost]
        public JsonResult EditPhanHoi(int Id)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {

                var model = _db.YKienGopY.FirstOrDefault(t => t.Id == Id);

                string result = "<div class='modal-body' id='frm_edit_phanhoi'>";
                result += "<div class='row'>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Nội dung phản hồi:</label>";
                result += "<textarea class='form-control' id='noidung_edit_phanhoi' name='noidung_edit_phanhoi' rows='5' cols='30'>" + model.NoiDungPhanHoi + "</textarea>";
                result += "</div>";
                result += "</div>";

                result += "</div>";
                result += "<input hidden class='form-control' id='id_edit' name='id_edit' value='" + model.Id + "'/>";
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

        [Route("YKienDongGopPhanHoi/Update")]
        [HttpPost]
        public JsonResult Update(YKienGopY request)
        {
            var model = _db.YKienGopY.FirstOrDefault(t => t.Id == request.Id);
            model.NoiDungPhanHoi = request.NoiDungPhanHoi;
            model.NgayPhanHoi = DateTime.Now;

            model.DonViPhanHoi = Helpers.GetSsAdmin(HttpContext.Session, "Name");
            _db.YKienGopY.Update(model);
            _db.SaveChanges();
            var data = new { status = "success" };
            return Json(data);

        }
    }
}
