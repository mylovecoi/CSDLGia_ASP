﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using CSDLGia_ASP.Database;
using System.Security.Cryptography;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Systems;
using CSDLGia_ASP.Models.Manages.DinhGia;
using CSDLGia_ASP.ViewModels.Systems;
using CSDLGia_ASP.ViewModels.Manages.DinhGia;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaTrungThauDat
{
    public class GiaTrungThauDatCtController : Controller
    {
        private readonly CSDLGiaDBContext _db;

        public GiaTrungThauDatCtController(CSDLGiaDBContext db)
        {
            _db = db;
        }

        [Route("GiaTrungThauDatCt/Store")]
        [HttpPost]
        public JsonResult Store(string Mahs, string Solo, string Sothua, string Tobando, string Mota,
            double Dientich, string Dvt , double Giakhoidiem ,double Giadaugia)
        {
            var model = new GiaDauGiaDatCt
            {
                Mahs = Mahs,
                Solo = Solo,
                Sothua = Sothua,
                Tobanbo = Tobando,
                Mota = Mota,
                Dientich = Dientich,
                Dvt = Dvt,
                Giakhoidiem = Giakhoidiem,
                Giadaugia = Giadaugia,
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now,
            };
            _db.GiaDauGiaDatCt.Add(model);
            _db.SaveChanges();

            if (Dvt != null)
            {
                var dvt = _db.DmDvt.Where(t => t.Dvt == Dvt).ToList();
                if (dvt.Count == 0)
                {
                    var new_dvt = new DmDvt
                    {
                        Dvt = Dvt,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                    };
                    _db.DmDvt.AddRange(new_dvt);
                    _db.SaveChanges();
                }
            }
            string result = GetData(Mahs);
            var data = new { status = "success", message = result };
            return Json(data);
        }

        [Route("GiaTrungThauDatCt/Delete")]
        [HttpPost]
        public JsonResult Delete(int Id)
        {
            var model = _db.GiaDauGiaDatCt.FirstOrDefault(t => t.Id == Id);
            _db.GiaDauGiaDatCt.Remove(model);
            _db.SaveChanges();
            var result = GetData(model.Mahs);
            var data = new { status = "success", message = result };
            return Json(data);
        }

        public string GetData(string Mahs)
        {
            var model = _db.GiaDauGiaDatCt.Where(t => t.Mahs == Mahs).ToList();
            int record = 1;
            string result = "<div class='card-body' id='frm_data'>";
            result += "<table class='table table-striped table-bordered table-hover' id='datatable_4'>";
            result += "<thead>";
            result += "<tr style='text-align:center'>";
            result += "<th style='text-align:center'>STT</th>";
            result += "<th style='text-align:center'>Số lô</th>";
            result += "<th style='text-align:center'>Số thửa</th>";
            result += "<th style='text-align:center'>Tờ bản đồ</th>";
            result += "<th style='text-align:center'>Mô tả</th>";
            result += "<th style='text-align:center'>Diện tích</th>";
            result += "<th style='text-align:center'>Dvt</th>";
            result += "<th style='text-align:center'>Giá khởi điểm</th>";
            result += "<th style='text-align:center'>Giá đấu giá</th>";
            result += "<th style='text-align:center'>Thao tác</th>";
            result += "</tr></thead><tbody>";
            if (model != null)
            {
                foreach (var item in model)
                {
                    result += "<tr>";
                    result += "<td style='text-align:center'>" + (record++) + "</td>";
                    result += "<td style='text-align:center'>" + item.Solo + "</td>";
                    result += "<td style='text-align:center'>" + item.Sothua + "</td>";
                    result += "<td style='text-align:center'>" + item.Tobanbo + "</td>";
                    result += "<td style='text-align:center'>" + item.Mota + "</td>";
                    result += "<td style='text-align:center'>" + item.Dientich + "</td>";
                    result += "<td style='text-align:center'>" + item.Dvt + "</td>";
                    result += "<td style='text-align:center'>" + item.Giakhoidiem + "</td>";
                    result += "<td style='text-align:center'>" + item.Giadaugia + "</td>";
                    result += "<td>";
                    result += "<button type='button' class='btn btn-sm btn-clean btn-icon' title='Chỉnh sửa'";
                    result += " data-target='#Edit_Modal' data-toggle='modal' onclick='SetEdit(`" + item.Id + "`)'>";
                    result += "<i class='icon-lg la la-edit text-primary'></i>";
                    result += "</button>";
                    result += "<button type='button' class='btn btn-sm btn-clean btn-icon' title='Xóa'";
                    result += " data-target='#Delete_Modal' data-toggle='modal' onclick='GetDelete(`" + item.Id + "`)'>";
                    result += "<i class='icon-lg la la-trash text-danger'></i>";
                    result += "</button></td></tr>";
                }
            }

            result += "</tbody>";
            return result;
        }

        [Route("GiaTrungThauDatCt/Edit")]
        [HttpPost]
        public JsonResult Edit(int Id)
        {
            var DmDvt = _db.DmDvt.ToList();
            var model = _db.GiaDauGiaDatCt.FirstOrDefault(p => p.Id == Id);
            if (model != null)
            {
                string result = "<div class='modal-body' id='edit_thongtin'>";
                result += "<input type='Hidden' id='id_edit' name='id_edit' value='" + Id + "' class='form-control'/>";
                result += "<div class='row'>";
                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label><b>Số lô</b></label>";
                result += "<input type='text' id='Solo_edit' name='Solo_edit' value='" + @model.Solo + "' class='form-control'/>";
                result += "</div>";
                result += "</div>";
                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label><b>Số thửa</b></label>";
                result += "<input type='text' id='Sothua_edit' name='Sothua_edit' value='" + @model.Sothua + "' class='form-control'/>";
                result += "</div>";
                result += "</div>";
                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label><b>Tờ bản đồ</b></label>";
                result += "<input type='text' id='Tobando_edit' name='Tobando_edit' value='" + @model.Tobanbo + "'class='form-control'/>";
                result += "</div>";
                result += "</div>";
                result += "<div class='col-xl-12'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label><b>Mô tả</b></label>";
                result += "<textarea type='text' id='Mota_edit' name='Mota_edit' value='" + @model.Mota + "' class='form-control' />";
                result += "</div>";
                result += "</div>";
                result += "<div class='col-xl-3'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label><b>Diện tích</b></label>";
                result += "<input type='text' id='Dientich_edit' name='Dientich_edit' value='" + @model.Dientich + "' class='form-control' />";
                result += "</div>";
                result += "</div>";
                result += "<div class='col-xl-5'>";
                result += "<label class='form-control-label'><b>Đơn vị tính*</b></label>";
                result += "<select type='text' id='Dvt_edit' name='Dvt_edit' class='form-control'>";
                result += " <option value = ''> ---Select--- </ option >";
                foreach (var item in DmDvt)
                {
                    result += " <option value = '" + item.Dvt + "' " + (item.Dvt == model.Dvt ? "selected" : "") + ">" + item.Dvt + "</ option >";
                }
                result += "</select>";
                result += "</div>";
                result += " <div class='col-md-1' style='padding-left: 0px'>";
                result += " <label class='control-label'>&nbsp;&nbsp;&nbsp;</label>";
                result += " <button type ='button' class='btn btn-default' style='border:rgba(0, 0, 0, 0.1) solid 0.05px' data-target='#Dvt_edit_Modal' data-toggle='modal'>";
                result += " <i class='fa fa-plus'></i>";
                result += " </button>";
                result += "</div>";
                result += "<div class='col-xl-3'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label><b>Giá khởi điểm</b></label>";
                result += "<input type='text' id='Giakhoidiem_edit' name='Giakhoidiem_edit' value='" + @model.Giakhoidiem + "' class='form-control money text-right' style='font-weight: bold'/>";
                result += "</div>";
                result += "</div>";
                result += "<div class='col-xl-3'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label><b>Giá đấu giá</b></label>";
                result += "<input type='text' id='Giadaugia_edit' name='Giadaugia_edit' value='" + @model.Giadaugia + "' class='form-control money text-right' style='font-weight: bold'/>";
                result += "</div>";
                result += "</div>";
                result += "</div>";
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

        [Route("GiaTrungThauDatCt/Update")]
        [HttpPost]
        public JsonResult Update(int Id, string Solo, string Sothua, string Tobando, string Mota,
            double Dientich, string Dvt, double Giakhoidiem, double Giadaugia)
        {
            var model = _db.GiaDauGiaDatCt.FirstOrDefault(t => t.Id == Id);
            model.Solo = Solo;
            model.Sothua = Sothua;
            model.Tobanbo = Tobando;
            model.Mota = Mota;
            model.Dientich = Dientich;
            model.Dvt = Dvt;
            model.Giakhoidiem = Giakhoidiem;
            model.Giadaugia = Giadaugia;
            model.Updated_at = DateTime.Now;
            _db.GiaDauGiaDatCt.Update(model);
            _db.SaveChanges();

            if (Dvt != null)
            {
                var dvt = _db.DmDvt.Where(t => t.Dvt == Dvt).ToList();
                if (dvt.Count == 0)
                {
                    var new_dvt = new DmDvt
                    {
                        Dvt = Dvt,
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now,
                    };
                    _db.DmDvt.AddRange(new_dvt);
                    _db.SaveChanges();
                }
            }
            string result = GetData(model.Mahs);
            var data = new { status = "success", message = result };
            return Json(data);
        }
    }
}