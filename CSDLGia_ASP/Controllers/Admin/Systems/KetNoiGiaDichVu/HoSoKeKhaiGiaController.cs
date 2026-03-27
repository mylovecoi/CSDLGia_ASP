using Azure.Core;
using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Systems.KetNoiGiaDichVu;
using CSDLGia_ASP.Services;
using CSDLGia_ASP.ViewModels.Manages.KeKhaiGia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSDLGia_ASP.Controllers.Admin.Systems.KetNoiGiaDichVu
{
    public class HoSoKeKhaiGiaController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly ITrangThaiHoSoService _trangThaiHoSoService;

        public HoSoKeKhaiGiaController(CSDLGiaDBContext db, IDsDonviService dsDonviService, ITrangThaiHoSoService trangThaiHoSoService)
        {
            _db = db;
            _trangThaiHoSoService = trangThaiHoSoService;
        }

        [Route("HoSoKeKhaiGia/Show")]
        [HttpGet]
        public IActionResult Show(string Mahs)
        {
            var model = GetThongTinKk(Mahs);
            return View("~/Views/Admin/Systems/KetNoiGiaDichVu/Show.cshtml", model);
        }

        private VMHoSoKeKhaiGia GetThongTinKk(string Mahs)
        {
            var model = _db.HoSoKeKhaiGia.FirstOrDefault(t => t.mahs == Mahs);
            var hoso_kk = new VMHoSoKeKhaiGia
            {
                mahs = model.mahs,
                socv = model.socv,
                ngaynhap = model.ngaynhap,
                ngayhieuluc = model.ngayhieuluc,
                ttnguoinop = model.ttnguoinop,
                sohsnhan = model.sohsnhan,
                ngaychuyen = model.ngaychuyen,
                ngaynhan = model.ngaynhan,
            };
            var cskd = _db.CoSoKinhDoanhDVLT.FirstOrDefault(t => t.macskd == model.macskd)?.masothue ?? "";
            var doanhnghiep = _db.DoanhNghiepDVLT.FirstOrDefault(t => t.masothue == cskd)?.masothue ?? "";
            hoso_kk = GetThongTinDn(hoso_kk, doanhnghiep);
            hoso_kk = GetThongTinCt(hoso_kk, model.mahs);

            return hoso_kk;
        }

        private VMHoSoKeKhaiGia GetThongTinDn(VMHoSoKeKhaiGia hoso, string masothue)
        {
            var modeldn = _db.DoanhNghiepDVLT.FirstOrDefault(t => t.masothue == masothue);
            if (modeldn != null)
            {
                hoso.tendn = modeldn.tendn;
            }
            return hoso;
        }

        private VMHoSoKeKhaiGia GetThongTinCt(VMHoSoKeKhaiGia hoso, string Mahs)
        {
            var modelct = _db.HoSoKeKhaiGia_ChiTiet.Where(t => t.mahs == Mahs);
            if (modelct != null)
            {
                hoso.HoSoKeKhaiGia_ChiTiet = modelct.ToList();
            }
            return hoso;
        }
    }
}
