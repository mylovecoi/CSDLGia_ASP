﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using CSDLGia_ASP.Database;
using System.Security.Cryptography;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.KeKhaiGia;
using CSDLGia_ASP.ViewModels.Systems;
using CSDLGia_ASP.ViewModels.Manages.KeKhaiGia;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CSDLGia_ASP.ViewModels.Manages.DinhGia;
using CSDLGia_ASP.Models.Manages.DinhGia;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaSpDvCuThe
{
    public class GiaSpDvCuTheBcController : Controller
    {
        private readonly CSDLGiaDBContext _db;

        public GiaSpDvCuTheBcController(CSDLGiaDBContext db)
        {
            _db = db;
        }

        [Route("BaoCaoDgSpDvCuThe")]
        [HttpGet]
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.spdvcuthe.baocao", "Index"))
                {
                  
                    ViewData["Nam"] = DateTime.Now.Year;
                    ViewData["Title"] = "Báo cáo tổng hợp giá sản phẩm dịch vụ cụ thể";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_spdvcuthe";
                    ViewData["MenuLv3"] = "menu_spdvcuthe_bc";
                    return View("Views/Admin/Manages/DinhGia/GiaSpDvCuThe/BaoCao/Index.cshtml");
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
        //GiaSpDvCuThe
        //GiaSpDvCuTheCt
        //GiaSpDvCuTheNhom
        //GiaSpDvCuTheDm
        [Route("BaoCaoDgSpDvCuThe/BcTH")]
        [HttpPost]
        public IActionResult BcTH(DateTime tungay, DateTime denngay)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.spdvcuthe.baocao", "Index"))
                {

                    var model = (from pl in _db.GiaSpDvCuThe.Where(t => t.Thoidiem >= tungay && t.Thoidiem <= denngay && t.Trangthai == "HT")
                                 join db in _db.DsDiaBan on pl.Madv equals db.MaDiaBan
                                 select new CSDLGia_ASP.Models.Manages.DinhGia.GiaSpDvCuThe
                                 {
                                     Id = pl.Id,
                                     Mahs = pl.Mahs,
                                     Tendiaban = db.TenDiaBan,
                                     Soqd = pl.Soqd,
                                     Thoidiem = pl.Thoidiem,
                                 });

                    ViewData["tungay"] = tungay;
                    ViewData["denngay"] = denngay;
                    ViewData["Title"] = "Báo cáo tổng hợp giá sản phẩm dịch vụ cụ thể";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_spdvcuthe";
                    ViewData["MenuLv3"] = "menu_spdvcuthe_bc";
                    return View("Views/Admin/Manages/DinhGia/GiaSpDvCuThe/BaoCao/BcTH.cshtml", model);
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

        [Route("BaoCaoDgSpDvCuThe/BcCT")]
        [HttpPost]
        public IActionResult BcCT(DateTime tungay, DateTime denngay)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.spdvcuthe.baocao", "Index"))
                {
                    var model = _db.GiaSpDvCuThe.Where(t => t.Thoidiem >= tungay && t.Thoidiem <= denngay && t.Trangthai == "HT");       
                    ViewData["tungay"] = tungay;
                    ViewData["denngay"] = denngay;
                    ViewData["ct"] = _db.GiaSpDvCuTheCt.ToList();
                    ViewData["Title"] = "Báo cáo tổng hợp giá sản phẩm dịch vụ cụ thể";
                    ViewData["MenuLv1"] = "menu_dg";
                    ViewData["MenuLv2"] = "menu_spdvcuthe";
                    ViewData["MenuLv3"] = "menu_spdvcuthe_bc";
                    return View("Views/Admin/Manages/DinhGia/GiaSpDvCuThe/BaoCao/BcCT.cshtml", model);
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