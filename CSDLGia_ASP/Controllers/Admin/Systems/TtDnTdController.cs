using Aspose.Words;
using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
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

namespace CSDLGia_ASP.Controllers.Admin.Systems
{
    public class TtDnTdController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDsDonviService _dsDonviService;

        public TtDnTdController(CSDLGiaDBContext db, IWebHostEnvironment hostEnvironment, IDsDonviService dsDonviService)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
            _dsDonviService = dsDonviService;
        }

        [Route("DoanhNghiep/DanhSach")]
        [HttpGet]
        public IActionResult Index(string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.thongtindonvi", "Index"))
                {
                    Madv = string.IsNullOrEmpty(Madv) ? "" : Madv;
                    IQueryable<Company> model_dn = _db.Company;
                    if (Helpers.GetSsAdmin(HttpContext.Session, "Level") == "DN")
                    {
                        Madv = Helpers.GetSsAdmin(HttpContext.Session, "Madv");
                        model_dn = model_dn.Where(t => t.Madv == Madv);
                    }
                    //else
                    //{
                    //    var modelcqcq = _dsDonviService.GetListDonvi(Helpers.GetSsAdmin(HttpContext.Session, "Madv"));
                    //    List<string> list_dvcq = modelcqcq.Select(t=>t.MaDv).ToList();
                    //    model_dn = model_dn.Where(t => list_dvcq.Contains(t.Macqcq));
                    //    var data = model_dn.First();
                    //    Madv = data.Madv;
                    //}

                    var model = _db.Company.FirstOrDefault(t => t.Madv == Madv);

                    var comct_join = (from comct in _db.CompanyLvCc
                                      join nghe in _db.DmNgheKd on comct.Manghe equals nghe.Manghe
                                      select new VMCompanyLvCc
                                      {
                                          Id = comct.Id,
                                          Mahs = comct.Mahs,
                                          Madv = comct.Madv,
                                          Manghe = comct.Manghe,
                                          Tennghe = nghe.Tennghe,
                                          Manganh = comct.Manganh,
                                          Macqcq = comct.Macqcq,
                                          Trangthai = comct.Trangthai,
                                      }).Where(t => t.Madv == Madv && t.Trangthai == "XD").ToList();

                    var model_ttdntd = _db.TtDnTd.FirstOrDefault(t => t.Madv == Madv);

                    var dnct_join = (from dnct in _db.TtDnTdCt
                                     join nghe in _db.DmNgheKd on dnct.Manghe equals nghe.Manghe
                                     select new VMTtDnTdCt
                                     {
                                         Id = dnct.Id,
                                         Mahs = dnct.Mahs,
                                         Madv = dnct.Madv,
                                         Manghe = dnct.Manghe,
                                         Tennghe = nghe.Tennghe,
                                         Manganh = dnct.Manganh,
                                         Macqcq = dnct.Macqcq,
                                         Trangthai = dnct.Trangthai,
                                     }).Where(t => t.Madv == Madv).ToList();

                    ViewData["DsDoanhNghiep"] = model_dn;
                    ViewData["DsDonVi"] = _db.DsDonVi;
                    ViewData["DsDiaBan"] = _db.DsDiaBan.Where(t => t.Level != "ADMIN");
                    ViewData["CompanyLvCc"] = comct_join;
                    ViewData["TtDnTd"] = model_ttdntd;
                    ViewData["TtDnTdCt"] = dnct_join;
                    ViewData["Madv"] = Madv;
                    ViewData["Title"] = "Thông tin doanh nghiệp";
                    ViewData["MenuLv1"] = "menu_kekhaidangkygia";
                    ViewData["MenuLv2"] = "menu_kekhaidangkygia_thongtindonvi";
                    return View("Views/Admin/Systems/TtDnTd/Index.cshtml", model);
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

        [Route("DoanhNghiep/Edit")]
        [HttpGet]
        public IActionResult Edit(string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.thongtindonvi", "Edit"))
                {
                    var modelct_cxd = _db.TtDnTdCt.Where(t => t.Trangthai == "CXD" && t.Madv == Madv);
                    if (modelct_cxd.Any())
                    {
                        _db.TtDnTdCt.RemoveRange(modelct_cxd);
                        _db.SaveChanges();
                    }

                    var model = _db.Company.FirstOrDefault(t => t.Madv == Madv);
                    var model_new = new VMCompany
                    {
                        Id = model.Id,
                        Madv = model.Madv,
                        Madiaban = model.Madiaban,
                        Mahs = model.Mahs,
                        Macqcq = model.Macqcq,
                        Tendn = model.Tendn,
                        Tel = model.Tel,
                        Fax = model.Fax,
                        Email = model.Email,
                        Website = model.Website,
                        Tailieu = model.Tailieu,
                        Diachi = model.Diachi,
                        Chucdanh = model.Chucdanh,
                        Nguoiky = model.Nguoiky,
                        Diadanh = model.Diadanh,
                        Giayphepkd = model.Giayphepkd,
                        Created_at = model.Created_at,
                    };




                    /* 31/03/2024 
                    Kiểm tra trong TtDnTdCt
                    -1. Đã có bỏ qua ko thêm vào bảng TtDnTdCt
                    -2. Chưa có thì thêm từ CompanyLvCc vào TtDnTdCt
                    */
                    var ttThayDoi = _db.TtDnTdCt.Where(t => t.Madv == model_new.Madv);
                    if (!ttThayDoi.Any())
                    {
                        foreach (var item in _db.CompanyLvCc.Where(t => t.Madv == model_new.Madv))
                        {
                            var model_ttdntd_ct = new TtDnTdCt
                            {
                                Madv = item.Madv,
                                Manghe = item.Manghe,
                                Manganh = item.Manganh,
                                Macqcq = item.Macqcq,
                                Trangthai = item.Trangthai,
                                Created_at = DateTime.Now,
                            };
                            _db.TtDnTdCt.Add(model_ttdntd_ct);
                        }
                        _db.SaveChanges();
                    }
                    var model_ct = from com_ct in _db.TtDnTdCt.Where(t => t.Madv == model_new.Madv)
                                   join nghe in _db.DmNgheKd on com_ct.Manghe equals nghe.Manghe
                                   select new VMCompanyLvCc
                                   {
                                       Id = com_ct.Id,
                                       Manghe = com_ct.Manghe,
                                       Macqcq = com_ct.Macqcq,
                                       Madv = com_ct.Madv,
                                       Manganh = com_ct.Manganh,
                                       Tennghe = nghe.Tennghe,
                                       Trangthai = com_ct.Trangthai
                                   };
                    model_new.VMCompanyLvCc = model_ct.ToList();


                    ViewData["Madv"] = Madv;
                    ViewData["DsDonVi"] = _db.DsDonVi;
                    ViewData["DmNganhKd"] = _db.DmNganhKd;
                    ViewData["DmNgheKd"] = _db.DmNgheKd;
                    ViewData["Title"] = "Thông tin doanh nghiệp chỉnh sửa";
                    ViewData["MenuLv1"] = "menu_kekhaidangkygia";
                    ViewData["MenuLv2"] = "menu_kekhaidangkygia_thongtindonvi";
                    return View("Views/Admin/Systems/TtDnTd/Edit.cshtml", model_new);
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

        [Route("DoanhNghiep/Update")]
        [HttpPost]
        public async Task<IActionResult> Update(VMCompany request, IFormFile Giayphepkdupload)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.thongtindonvi", "Edit"))
                {
                    if (Giayphepkdupload != null && Giayphepkdupload.Length > 0)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string filename = Path.GetFileNameWithoutExtension(Giayphepkdupload.FileName);
                        string extension = Path.GetExtension(Giayphepkdupload.FileName);
                        filename = filename + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "/Upload/File/", filename);
                        using (var FileStream = new FileStream(path, FileMode.Create))
                        {
                            await Giayphepkdupload.CopyToAsync(FileStream);
                        }
                        request.Giayphepkd = filename;
                    }

                    var ttdntd = new TtDnTd
                    {
                        Madv = request.Madv,
                        Mahs = request.Mahs,
                        Macqcq = request.Macqcq,
                        Madiaban = request.Madiaban,
                        Tendn = request.Tendn,
                        Diachi = request.Diachi,
                        Tel = request.Tel,
                        Fax = request.Fax,
                        Email = request.Email,
                        Diadanh = request.Diadanh,
                        Chucdanh = request.Chucdanh,
                        Nguoiky = request.Nguoiky,
                        Ghichu = request.Ghichu,
                        Trangthai = "CC",
                        Tailieu = request.Tailieu,
                        Giayphepkd = request.Giayphepkd,
                        Level = request.Level,
                        Lydo = request.Lydo,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                        Ngaychuyen = DateTime.Now,
                    };
                    _db.TtDnTd.Add(ttdntd);
                    _db.SaveChanges();

                    var ttdntd_ct_new = _db.TtDnTdCt.Where(t => t.Madv == request.Madv);
                    if (ttdntd_ct_new != null)
                    {
                        foreach (var item in ttdntd_ct_new)
                        {
                            item.Mahs = ttdntd.Mahs;
                            item.Trangthai = "XD";
                        }
                    }
                    _db.TtDnTdCt.UpdateRange(ttdntd_ct_new);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "TtDnTd", new { Madv = request.Madv });
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

        public IActionResult Chuyen(string Madv)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.thongtindonvi", "Approve"))
                {
                    var model = _db.TtDnTd.FirstOrDefault(t => t.Madv == Madv);
                    model.Trangthai = "CD";
                    model.Ngaychuyen = DateTime.Now;

                    _db.TtDnTd.Update(model);
                    _db.SaveChanges();

                    return RedirectToAction("Index", "TtDnTd", new { Madv = Madv });
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

        [Route("DoanhNghiepCt/Store")]
        [HttpPost]
        public JsonResult Store(string Madv, string Manghe, string Macqcq)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.thongtindonvi", "Create"))
                {
                    var model = new TtDnTdCt
                    {
                        Madv = Madv,
                        Manghe = Manghe,
                        Macqcq = Macqcq,
                        Trangthai = "CXD",
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                    };
                    _db.TtDnTdCt.Add(model);
                    _db.SaveChanges();
                    string result = this.GetData(Madv);
                    var data = new { status = "success", message = result };
                    return Json(data);
                }
                else
                {
                    var data = new { status = "error", message = "Bạn không có quyền truy cập vào chức năng này!" };
                    return Json(data);
                }
            }
            else
            {
                var data = new { status = "error", message = "Bạn chưa đăng nhập hoặc phiên làm việc của bạn đã kết thúc" };
                return Json(data);
            }
        }

        [Route("DoanhNghiepCt/Delete")]
        [HttpPost]
        public JsonResult Delete(int Id)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.kekhaidangkygia.thongtindonvi", "Delete"))
                {
                    var model = _db.TtDnTdCt.FirstOrDefault(t => t.Id == Id);
                    _db.TtDnTdCt.Remove(model);
                    _db.SaveChanges();
                    string result = this.GetData(model.Madv);
                    var data = new { status = "success", message = result };
                    return Json(data);
                }
                else
                {
                    var data = new { status = "error", message = "Bạn không có quyền truy cập vào chức năng này!" };
                    return Json(data);
                }
            }
            else
            {
                var data = new { status = "error", message = "Bạn chưa đăng nhập hoặc phiên làm việc của bạn đã kết thúc" };
                return Json(data);
            }
        }

        //Get
        public string GetData(string Madv)
        {
            var model = _db.TtDnTdCt.Where(t => t.Madv == Madv).ToList();
            var model_join = (from cty in model
                              join dmnghe in _db.DmNgheKd on cty.Manghe equals dmnghe.Manghe
                              select new VMCompanyLvCc
                              {
                                  Id = cty.Id,
                                  Mahs = cty.Mahs,
                                  Madv = cty.Madv,
                                  Manganh = cty.Manganh,
                                  Manghe = cty.Manghe,
                                  Tennghe = dmnghe.Tennghe,
                                  Macqcq = cty.Macqcq,
                                  Trangthai = cty.Trangthai,

                              });
            var dsdonvi = _db.DsDonVi;
            int record = 1;
            string result = "<div class='card-body' id='frm_ct_data'>";
            result += "<table class='table table-striped table-bordered table-hover' id='datatable_4'>";
            result += "<thead>";
            result += "<tr style='text-align:center'>";
            result += "<th width='2%'>#</th>";
            result += "<th>Tên nghề kinh doanh</th>";
            result += "<th>Đơn vị quản lý</th>";
            result += "<th width='9%'>Thao tác</th>";
            result += "</tr>";
            result += "</thead>";
            result += "<tbody>";

            foreach (var item in model_join)
            {
                result += "<tr>";
                result += "<td style='text-align:center'>" + (record++) + "</td>";
                result += "<td style='font-weight:bold'>" + item.Tennghe + "</td>";
                if (!string.IsNullOrEmpty(item.Macqcq))
                {
                    result += "<td style='text-align:center'>" + dsdonvi.FirstOrDefault(x => x.MaDv == item.Macqcq)?.TenDv ?? "" + "</td>";
                }
                result += "<td>";
                result += "<button type='button' class='btn btn-sm btn-clean btn-icon' title='Xóa' data-toggle='modal'";
                result += " data-target='#Delete_Modal' onclick='GetDelete(`" + item.Id + "`, `" + item.Tennghe + "`)'>";
                result += "<i class='icon-lg la la-trash text-danger'></i>";
                result += "</button>";
                result += "</td>";
                result += "</tr>";
            }
            result += "</tbody>";
            result += "</table>";
            result += "</div>";

            return result;

        }
    }
}
