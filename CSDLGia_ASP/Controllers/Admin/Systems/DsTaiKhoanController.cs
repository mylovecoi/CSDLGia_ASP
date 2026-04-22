using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.DinhGia;
using CSDLGia_ASP.Models.Systems;
using CSDLGia_ASP.ViewModels.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace CSDLGia_ASP.Controllers.Admin.Systems
{
    public class DsTaiKhoanController : Controller
    {
        private readonly CSDLGiaDBContext _db;

        public DsTaiKhoanController(CSDLGiaDBContext db)
        {
            _db = db;
        }

        [Route("DsTaiKhoan")]
        [HttpGet]
        public IActionResult Index(string Madv, string Phanloai)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dstaikhoan", "Index"))
                {
                    var dsdiaban = _db.DsDiaBan.ToList();
                    var dsdonvi = _db.DsDonVi.ToList();
                    Madv = string.IsNullOrEmpty(Madv) ? "all" : Madv;
                    var model = _db.Users.Where(t => !t.Sadmin && t.Status == "Kích hoạt");

                    if (Madv != "all")
                    {
                        model = model.Where(t => t.Madv == Madv);
                    }

                    if (Phanloai == "" || string.IsNullOrEmpty(Phanloai))
                    {
                        model = model.Where(t => t.Level == "" || t.Level == null);
                    }

                    if (Phanloai == "DN")
                    {
                        model = model.Where(t => t.Level == "DN");
                    }

                    ViewData["DsDonVi"] = dsdonvi;
                    ViewData["DsDiaBan"] = dsdiaban;
                    ViewData["Phanloai"] = Phanloai;
                    ViewData["Madv"] = Madv;
                    ViewData["GroupPer"] = _db.GroupPermissions;
                    ViewData["Title"] = "Danh sách tài khoản đơn vị";
                    ViewData["MenuLv1"] = "menu_qtnguoidung";
                    ViewData["MenuLv2"] = "menu_dstaikhoan";
                    return View("Views/Admin/Systems/DsTaiKhoan/Index.cshtml", model);
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

        [Route("DsTaiKhoan/Printf")]
        [HttpGet]
        public IActionResult Printf(string Madv, string Phanloai)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dstaikhoan", "Index"))
                {
                    var dsdiaban = _db.DsDiaBan.ToList();
                    var dsdonvi = _db.DsDonVi.ToList();
                    Madv = string.IsNullOrEmpty(Madv) ? "all" : Madv;
                    var model = _db.Users.Where(t => !t.Sadmin && t.Status == "Kích hoạt");

                    if (Madv != "all")
                    {
                        model = model.Where(t => t.Madv == Madv);
                    }

                    if (Phanloai == "" || string.IsNullOrEmpty(Phanloai))
                    {
                        model = model.Where(t => t.Level == "" || t.Level == null);
                    }

                    if (Phanloai == "DN")
                    {
                        model = model.Where(t => t.Level == "DN");
                    }

                    ViewData["DsDonVi"] = dsdonvi;
                    ViewData["DsDiaBan"] = dsdiaban;
                    ViewData["Phanloai"] = Phanloai;
                    ViewData["Madv"] = Madv;
                    ViewData["GroupPer"] = _db.GroupPermissions;
                    ViewData["Title"] = "In danh sách tài khoản đơn vị";
                    ViewData["MenuLv1"] = "menu_qtnguoidung";
                    ViewData["MenuLv2"] = "menu_dstaikhoan";
                    return View("Views/Admin/Systems/DsTaiKhoan/Printf.cshtml", model);
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

        [Route("DsTaiKhoan/Create")]
        [HttpGet]
        public IActionResult Create(string MaDv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dstaikhoan", "Create"))
                {
                    ViewData["ChucNang"] = _db.GroupPermissions.ToList();
                    ViewData["DsDonVi"] = _db.DsDonVi;
                    ViewData["Title"] = "Thêm mới thông tin tài khoản";

                    ViewData["MenuLv1"] = "menu_qtnguoidung";
                    ViewData["MenuLv2"] = "menu_dstaikhoan";

                    return View("Views/Admin/Systems/DsTaiKhoan/Create.cshtml");
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

        [Route("DsTaiKhoan/Store")]
        [HttpPost]
        public IActionResult Store(Users request)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dstaikhoan", "Create"))
                {
                    var check = _db.Users.FirstOrDefault(u => u.Username == request.Username);
                    if (check == null)
                    {
                        string md5_password = "";
                        using (MD5 md5Hash = MD5.Create())
                        {
                            string change = Helpers.GetMd5Hash(md5Hash, request.Password);
                            md5_password = change;
                        }

                        var checkper = _db.Permissions.Where(t => t.Status == "Disable");
                        if (checkper != null)
                        {
                            _db.Permissions.RemoveRange(checkper);
                            _db.SaveChanges();
                        }

                        /*string chucnang = "";
                        chucnang += !string.IsNullOrEmpty(nhaplieu) ? "NHAPLIEU;" : "";
                        chucnang += !string.IsNullOrEmpty(tonghop) ? "TONGHOP;" : "";
                        chucnang += !string.IsNullOrEmpty(quantri) ? "QUANTRI;" : "";*/
                        if (string.IsNullOrEmpty(request.Chucnang))
                        {
                            var model = new Users
                            {
                                Madv = request.Madv,
                                Name = request.Name,
                                Status = request.Status,
                                Chucnang = "PerS_" + DateTime.Now.ToString("yymmssfff"),
                                Username = request.Username,
                                Password = md5_password,
                                Created_at = DateTime.Now,
                                Updated_at = DateTime.Now,
                            };
                            _db.Users.Add(model);
                            _db.SaveChanges();

                            var role = Helpers.GetRoleList().ToList();
                            var per = new List<Permissions>();

                            foreach (var item in role)
                            {
                                per.Add(new Permissions()
                                {
                                    Username = model.Chucnang,
                                    Tendangnhap = model.Username,
                                    Madv = model.Madv,
                                    Roles = item.Role,
                                    Status = "Disable",
                                });
                            }
                            _db.Permissions.AddRange(per);
                            _db.SaveChanges();

                            return RedirectToAction("Index", "DsTaiKhoan", new { MaDv = request.Madv });
                        }
                        else
                        {
                            var model = new Users
                            {
                                Madv = request.Madv,
                                Name = request.Name,
                                Status = request.Status,
                                Chucnang = request.Chucnang,
                                Username = request.Username,
                                Password = md5_password,
                                Created_at = DateTime.Now,
                                Updated_at = DateTime.Now,
                            };
                            _db.Users.Add(model);
                            _db.SaveChanges();
                            return RedirectToAction("Index", "DsTaiKhoan", new { MaDv = request.Madv });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Username", "Tài khoản truy cập này đã tồn tại");

                        /*ViewData["nhaplieu"] = nhaplieu;
                        ViewData["tonghop"] = tonghop;
                        ViewData["quantri"] = quantri;*/
                        ViewData["ChucNang"] = _db.GroupPermissions.ToList();
                        ViewData["DsDonVi"] = _db.DsDonVi;
                        ViewData["MaDv"] = request.Madv;
                        ViewData["Title"] = "Thêm mới thông tin tài khoản";

                        ViewData["MenuLv1"] = "menu_qtnguoidung";
                        ViewData["MenuLv2"] = "menu_dstaikhoan";
                        return View("Views/Admin/Systems/DsTaiKhoan/Create.cshtml");
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

        [Route("DsTaiKhoan/Edit")]
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dstaikhoan", "Edit"))
                {
                    var model = _db.Users.FirstOrDefault(t => t.Id == Id);

                    ViewData["ChucNang"] = _db.GroupPermissions.ToList();
                    ViewData["DsDonVi"] = _db.DsDonVi.Where(t => t.MaDv == model.Madv).ToList();
                    ViewData["MaDv"] = model.Madv;
                    /*ViewData["nhaplieu"] = (model.Chucnang == "NHAPLIEU;" || model.Chucnang == "NHAPLIEU;TONGHOP;") ? "NHAPLIEU;" : "";
                    ViewData["tonghop"] = (model.Chucnang == "TONGHOP;" || model.Chucnang == "NHAPLIEU;TONGHOP;") ? "TONGHOP;" : "";
                    ViewData["quantri"] = (model.Chucnang == "QUANTRI;") ? "QUANTRI;" : "";*/
                    ViewData["DsDonVi"] = _db.DsDonVi;
                    ViewData["Title"] = "Chỉnh sửa thông tin tài khoản";

                    ViewData["MenuLv1"] = "menu_qtnguoidung";
                    ViewData["MenuLv2"] = "menu_dstaikhoan";

                    return View("Views/Admin/Systems/DsTaiKhoan/Edit.cshtml", model);
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

        [Route("DsTaiKhoan/Update")]
        [HttpPost]
        public IActionResult Update(Users request, string NewPassword)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dstaikhoan", "Edit"))
                {
                    var check = _db.Users.FirstOrDefault(u => u.Username == request.Username && u.Id != request.Id);
                    if (check == null)
                    {
                        var model = _db.Users.FirstOrDefault(t => t.Id == request.Id);

                        /*string chucnang = "";
                        chucnang += !string.IsNullOrEmpty(nhaplieu) ? "NHAPLIEU;" : "";
                        chucnang += !string.IsNullOrEmpty(tonghop) ? "TONGHOP;" : "";
                        chucnang += !string.IsNullOrEmpty(quantri) ? "QUANTRI;" : "";*/
                        model.Madv = request.Madv;
                        model.Name = request.Name;
                        model.Status = request.Status;
                        model.Chucnang = request.Chucnang;
                        model.Username = request.Username;
                        model.Updated_at = DateTime.Now;
                        if (!string.IsNullOrEmpty(NewPassword))
                        {
                            string md5_password = "";
                            using (MD5 md5Hash = MD5.Create())
                            {
                                string change = Helpers.GetMd5Hash(md5Hash, NewPassword);
                                md5_password = change;
                            }
                            model.Password = md5_password;
                        }
                        _db.Users.Update(model);
                        _db.SaveChanges();
                        return RedirectToAction("Index", "DsTaiKhoan", new { MaDv = request.Madv });
                    }
                    else
                    {
                        ModelState.AddModelError("Username", "Tài khoản truy cập này đã tồn tại");

                        /*ViewData["nhaplieu"] = nhaplieu;
                        ViewData["tonghop"] = tonghop;
                        ViewData["quantri"] = quantri;*/
                        ViewData["DsDonVi"] = _db.DsDonVi;
                        ViewData["MaDv"] = request.Madv;
                        ViewData["Title"] = "Chỉnh sửa thông tin tài khoản";

                        ViewData["MenuLv1"] = "menu_qtnguoidung";
                        ViewData["MenuLv2"] = "menu_dstaikhoan";
                        return View("Views/Admin/Systems/DsTaiKhoan/Edit.cshtml");
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

        [Route("DsTaiKhoan/Delete")]
        [HttpPost]
        public IActionResult Delete(int id_delete)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dstaikhoan", "Delete"))
                {
                    var model = _db.Users.FirstOrDefault(t => t.Id == id_delete);
                    /*return Ok(model);*/
                    _db.Users.Remove(model);
                    _db.SaveChanges();
                    return RedirectToAction("Index", "DsTaiKhoan", new { MaDv = model.Madv });
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

        [Route("DsTaiKhoan/Permission")]
        [HttpGet]
        public IActionResult Permissions(string Username, string Madv, string Chucnang)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dstaikhoan", "Index"))
                {
                    var model_del = _db.Permissions.Where(t => t.Status == "Disable");
                    _db.Permissions.RemoveRange(model_del);
                    _db.SaveChanges();

                    var model = _db.Permissions.Where(t => t.Username == Chucnang).ToList();

                    var data_rolelist = _db.RoleList.Where(t => t.TrangThai == "Active").ToList();

                    var per = new List<Permissions>();

                    if (model.Any())
                    {
                        ViewData["Username"] = Username;
                        ViewData["Madv"] = Madv;
                        ViewData["Chucnang"] = Chucnang;
                        ViewData["RoleList"] = data_rolelist;
                        ViewData["Title"] = "Thông tin truy cập";
                        ViewData["MenuLv1"] = "menu_qtnguoidung";
                        ViewData["MenuLv2"] = "menu_dstaikhoan";
                        return View("Views/Admin/Systems/DsTaiKhoan/PermissionCustom.cshtml", model);
                    }
                    else
                    {
                        foreach (var item in data_rolelist)
                        {
                            per.Add(new Permissions
                            {
                                Username = Chucnang,
                                Roles = item.Role,
                                Name = item.Name,
                                Index = true,
                                Create = false,
                                Edit = false,
                                Delete = false,
                                Approve = false,
                                Public = false,
                                Status = "Disable",
                                Magoc = item.MaGoc,
                                Level = item.Level,
                                Sttsx = item.STTSapXep,
                                Phanloai = item.PhanLoai,
                            });
                        }
                        _db.Permissions.AddRange(per);
                        _db.SaveChanges();

                        var permission = per.Where(t => t.Username == Chucnang);

                        ViewData["Username"] = Username;
                        ViewData["Madv"] = Madv;
                        ViewData["Chucnang"] = Chucnang;
                        ViewData["RoleList"] = data_rolelist;
                        ViewData["Title"] = "Thông tin truy cập";
                        ViewData["MenuLv1"] = "menu_qtnguoidung";
                        ViewData["MenuLv2"] = "menu_dstaikhoan";
                        return View("Views/Admin/Systems/DsTaiKhoan/PermissionCustom.cshtml", permission);
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

        [Route("DsTaiKhoan/Permission/Update")]
        [HttpPost]
        public IActionResult UpdatePermissions(string KeyLink)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "hethong.nguoidung.dsnhomtaikhoan", "Edit"))
                {
                    var model = _db.Permissions.Where(t => t.Username == KeyLink);
                    foreach (var item in model)
                    {
                        item.Status = "Enable";
                    }
                    _db.Permissions.UpdateRange(model);
                    _db.SaveChanges();

                    ViewData["Title"] = "Thông tin quyền truy cập";
                    ViewData["MenuLv1"] = "menu_qtnguoidung";
                    ViewData["MenuLv2"] = "menu_dstaikhoan";
                    return RedirectToAction("Index", "DsTaiKhoan");
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
