﻿using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.DinhGia;
using CSDLGia_ASP.Models.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaHhDvk
{
    public class GiaHhDvkDmController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _env;

        public GiaHhDvkDmController(CSDLGiaDBContext db, IWebHostEnvironment hostingEnv)
        {
            _db = db;
            _env = hostingEnv;
        }

        [Route("GiaHhDvkDmCt")]
        [HttpGet]
        public IActionResult Index(string Matt)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.dm", "Index"))
                {
                    var model = _db.GiaHhDvkDm.Where(t => t.Matt == Matt);
                    ViewData["Matt"] = Matt;
                    ViewData["Tentt"] = _db.GiaHhDvkNhom.FirstOrDefault(t => t.Matt == Matt).Tentt;
                    ViewData["Dmnhomhh"] = _db.DmNhomHh.Where(t => t.Phanloai == "GIAHHDVKHAC").ToList();
                    var dmDvts = _db.DmDvt.ToList();
                    ViewData["Dvt"] = _db.DmDvt;
                    ViewData["Title"] = "Thông tin chi tiết hàng hóa dịch vụ";
                    ViewData["MenuLv1"] = "menu_hhdvk";
                    ViewData["MenuLv2"] = "menu_hhdvk_dm";
                    foreach (var item in model)
                    {
                        var foundItem = dmDvts.FirstOrDefault(x => x.Madvt == item.Dvt);
                        if (foundItem != null)
                        {
                            item.Dvt = foundItem.Dvt;
                        }
                    }
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/DanhMuc/ChiTiet/Index.cshtml", model);
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

        [Route("GiaHhDvkDmCt/Store")]
        [HttpPost]
        public JsonResult Store(string Matt, string Mahhdv, string Tenhhdv, string Dacdiemkt, string Dvt)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.dm", "Create"))
                {
                    var check = _db.GiaHhDvkDm.FirstOrDefault(t => t.Matt == Matt && t.Mahhdv == Mahhdv);
                    if (check == null)
                    {
                        var request = new GiaHhDvkDm
                        {
                            Matt = Matt,
                            Mahhdv = Mahhdv,
                            Tenhhdv = Tenhhdv,
                            Dacdiemkt = Dacdiemkt,
                            Dvt = Dvt,
                            Theodoi = "TD",
                            Created_at = DateTime.Now,
                            Updated_at = DateTime.Now,
                        };
                        _db.GiaHhDvkDm.Add(request);
                        _db.SaveChanges();

                        var check_dvt = _db.DmDvt.FirstOrDefault(t => t.Dvt == Dvt);

                        if (check_dvt == null)
                        {
                            var dvt = new DmDvt
                            {
                                Dvt = Dvt,
                                Created_at = DateTime.Now,
                                Updated_at = DateTime.Now,
                            };
                            _db.DmDvt.Add(dvt);
                            _db.SaveChanges();
                        }

                        var data = new { status = "success", message = "Thêm mới thành công!" };
                        return Json(data);
                    }
                    else
                    {
                        var data = new { status = "error", message = "Mã hàng hóa dịch vụ này đã tồn tại!" };
                        return Json(data);
                    }
                }
                else
                {
                    var data = new { status = "error", message = "Bạn không có quyền thực hiện chức năng này!!!" };
                    return Json(data);
                }
            }
            else
            {
                var data = new { status = "error", message = "Bạn kêt thúc phiên đăng nhập! Đăng nhập lại để tiếp tục công việc" };
                return Json(data);
            }
        }

        [Route("GiaHhDvkDmCt/Edit")]
        [HttpPost]
        public JsonResult Edit(int Id)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.dm", "Edit"))
                {
                    var model = _db.GiaHhDvkDm.FirstOrDefault(p => p.Id == Id);
                    var dmnhomhh = _db.DmNhomHh.Where(t => t.Phanloai == "GIAHHDVKHAC").ToList();
                    var dvt = _db.DmDvt;

                    if (model != null)
                    {
                        string result = "<div class='row' id='edit_thongtin'>";

                        result += "<div class='col-xl-12'>";
                        result += "<div class='form-group fv-plugins-icon-container'>";
                        result += "<label>Mã nhóm HHDV</label>";
                        result += "<select id='manhom_edit' name='manhom_edit' class='form-control kt_select2_1_modal'>";
                        foreach (var item in dmnhomhh)
                        {
                            result += "<option value='" + item.Manhom + "' " + (model.Manhom == item.Manhom ? "selected" : "") + ">" + item.Tennhom + "</option>";
                        }
                        result += "</select>";
                        result += "</div>";
                        result += "</div>";

                        result += "<div class='col-xl-12'>";
                        result += "<div class='form-group fv-plugins-icon-container'>";
                        result += "<label>Mã hàng hóa dịch vụ*</label>";
                        result += "<input type='text' id='mahhdv_edit' name='mahhdv_edit' class='form-control' value='" + model.Mahhdv + "'/>";
                        result += "</div>";
                        result += "</div>";
                        result += "<div class='col-xl-12'>";
                        result += "<div class='form-group fv-plugins-icon-container'>";
                        result += "<label>Tên hàng hóa dịch vụ*</label>";
                        result += "<input type='text' id='tenhhdv_edit' name='tenhhdv_edit' class='form-control' value='" + model.Tenhhdv + "'/>";
                        result += "</div>";
                        result += "</div>";
                        result += "<div class='col-xl-12'>";
                        result += "<div class='form-group fv-plugins-icon-container'>";
                        result += "<label>Đặc điểm kỹ thuật*</label>";
                        result += "<input type='text' id='dacdiemkt_edit' name='dacdiemkt_edit' class='form-control' value='" + model.Dacdiemkt + "'/>";
                        result += "</div>";
                        result += "</div>";

                        result += "<div class='col-xl-12'>";
                        result += "<div class='form-group fv-plugins-icon-container'>";
                        result += "<label>Đơn vị tính</label>";
                        result += "<select id='dvt_edit' name='dvt_edit' class='form-control kt_select2_1_modal'>";
                        foreach (var item in dvt)
                        {
                            result += "<option value='" + item.Dvt + "' " + ((string)model.Dvt == item.Dvt ? "selected" : "") + ">" + item.Dvt + "</option>";
                        }
                        result += "</select>";
                        result += "</div>";
                        result += "</div>";

                        result += "<div class='col-xl-12'>";
                        result += "<div class='form-group fv-plugins-icon-container'>";
                        result += "<label>Theo dõi</label>";
                        result += "<select id='theodoi_edit' name='theodoi_edit' class='form-control kt_select2_1_modal'>";
                        result += "<option value='TD' " + ((string)model.Theodoi == "TD" ? "selected" : "") + ">Theo dõi</option>";
                        result += "<option value='KTD' " + ((string)model.Theodoi == "KTD" ? "selected" : "") + ">Không theo dõi</option>";
                        result += "</select>";
                        result += "</div>";
                        result += "</div>";

                        result += "<input hidden type='text' id='id_edit' name='id_edit' value='" + model.Id + "'/>";
                        result += "<input hidden type='text' id='matt_edit' name='matt_edit' value='" + model.Matt + "'/>";
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
                else
                {
                    var data = new { status = "error", message = "Bạn không có quyền thực hiện chức năng này!!!" };
                    return Json(data);
                }
            }
            else
            {
                var data = new { status = "error", message = "Bạn kêt thúc phiên đăng nhập! Đăng nhập lại để tiếp tục công việc" };
                return Json(data);
            }
        }

        [Route("GiaHhDvkDmCt/Update")]
        [HttpPost]
        public JsonResult Update(int Id, string Manhom, string Matt, string Mahhdv, string Tenhhdv, string Dacdiemkt, string Dvt, string Theodoi)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.dm", "Edit"))
                {
                    int check = _db.GiaHhDvkDm.Where(t => t.Manhom == Manhom && t.Matt == Matt && t.Mahhdv == Mahhdv && t.Id != Id).Count();
                    if (check == 0)
                    {
                        var model = _db.GiaHhDvkDm.FirstOrDefault(t => t.Id == Id);
                        model.Manhom = Manhom;
                        model.Mahhdv = Mahhdv;
                        model.Tenhhdv = Tenhhdv;
                        model.Dacdiemkt = Dacdiemkt;
                        model.Dvt = Dvt;
                        model.Theodoi = Theodoi;
                        model.Updated_at = DateTime.Now;
                        _db.GiaHhDvkDm.Update(model);
                        _db.SaveChanges();

                        var check_dvt = _db.DmDvt.FirstOrDefault(t => t.Dvt == Dvt);

                        if (check_dvt == null)
                        {
                            var dvt = new DmDvt
                            {
                                Dvt = Dvt,
                                Created_at = DateTime.Now,
                                Updated_at = DateTime.Now,
                            };
                            _db.DmDvt.Add(dvt);
                            _db.SaveChanges();
                        }

                        var data = new { status = "success", message = "Cập nhật thành công!" };
                        return Json(data);
                    }
                    else
                    {
                        var data = new { status = "error", message = "Mã hàng hóa dịch vụ đã tồn tại!!!" };
                        return Json(data);
                    }

                }
                else
                {
                    var data = new { status = "error", message = "Bạn không có quyền thực hiện chức năng này!!!" };
                    return Json(data);
                }
            }
            else
            {
                var data = new { status = "error", message = "Bạn kêt thúc phiên đăng nhập! Đăng nhập lại để tiếp tục công việc" };
                return Json(data);
            }
        }

        [Route("GiaHhDvkDmCt/Delete")]
        [HttpPost]
        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.dm", "Delete"))
                {
                    var model = _db.GiaHhDvkDm.FirstOrDefault(p => p.Id == id_delete);
                    _db.GiaHhDvkDm.Remove(model);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "GiaHhDvkDm", new { Matt = model.Matt });
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

        [Route("GiaHhDvkDmCt/DeleteAll")]
        [HttpPost]
        public IActionResult DeleteAll(string Matt)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.hhdvk.dm", "Delete"))
                {
                    var model = _db.GiaHhDvkDm.Where(p => p.Matt == Matt);
                    _db.GiaHhDvkDm.RemoveRange(model);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "GiaHhDvkDm", new { Matt = Matt });
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

        public IActionResult NhanExcel(string Matt)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.lephi.danhmuc", "Create"))
                {
                    var model = new CSDLGia_ASP.ViewModels.VMImportExcel
                    {
                        LineStart = 4,
                        LineStop = 1000,
                        Sheet = 1,
                        Matt = Matt,
                        MaNhom = Matt,
                        TenNhom = _db.GiaHhDvkNhom.FirstOrDefault(t => t.Matt == Matt)?.Tentt ?? ""
                    };

                    ViewData["Title"] = "Danh mục chi tiết hàng hoá, dịch vụ";
                    ViewData["MenuLv1"] = "menu_spdvtoida";
                    ViewData["MenuLv2"] = "menu_spdvtoida_dm";
                    return View("Views/Admin/Manages/DinhGia/GiaHhDvk/DanhMuc/Excel.cshtml", model);

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
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                requests.LineStart = requests.LineStart == 0 ? 1 : requests.LineStart;
                requests.Matt = requests.MaNhom;
                int sheet = requests.Sheet == 0 ? 0 : (requests.Sheet - 1);

                using (var stream = new MemoryStream())
                {
                    await requests.FormFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[sheet];
                        if (worksheet != null)
                        {
                            int rowcount = worksheet.Dimension.Rows;
                            requests.LineStop = requests.LineStop > rowcount ? rowcount : requests.LineStop;
                            Regex trimmer = new Regex(@"\s\s+"); // Xóa khoảng trắng thừa trong chuỗi
                            var list_add = new List<CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvkDm>();

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

                                string maHHDV = worksheet.Cells[row, 2].Value != null ?
                                                 worksheet.Cells[row, 2].Value.ToString().Trim() : "";
                                if (string.IsNullOrEmpty(maHHDV))
                                {
                                    continue;
                                }
                                string[] a_maHHDV = maHHDV.Split('.');
                                list_add.Add(new CSDLGia_ASP.Models.Manages.DinhGia.GiaHhDvkDm
                                {
                                    Mahhdv = maHHDV,
                                    Tenhhdv = worksheet.Cells[row, 3].Value != null ?
                                                 worksheet.Cells[row, 3].Value.ToString().Trim() : "",
                                    Dacdiemkt = worksheet.Cells[row, 4].Value != null ?
                                                 worksheet.Cells[row, 4].Value.ToString().Trim() : "",
                                    Dvt = worksheet.Cells[row, 5].Value != null ?
                                                 worksheet.Cells[row, 5].Value.ToString().Trim() : "",
                                    Matt = requests.MaNhom,
                                    Theodoi = "TD",
                                    Manhom = a_maHHDV[0]
                                });

                            }
                            _db.GiaHhDvkDm.AddRange(list_add);
                            _db.SaveChanges();
                        }
                    }
                }

                return RedirectToAction("Index", "GiaHhDvkDm", new { Matt = requests.MaNhom });
            }
            else
            {
                return View("Views/Admin/Error/SessionOut.cshtml");
            }

        }
    }
}
