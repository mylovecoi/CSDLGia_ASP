﻿using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Systems;
using CSDLGia_ASP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSDLGia_ASP.Controllers.Admin.Manages.DinhGia.GiaDatDiaBan.GiaDatDiaBanCt
{
    public class GiaDatDiaBanCtController : Controller
    {
        private readonly CSDLGiaDBContext _db;
        private readonly IDsDiaBanService _IDsDiaBan;

        public GiaDatDiaBanCtController(CSDLGiaDBContext db, IDsDiaBanService IDsDiaBan)
        {
            _db = db;
            _IDsDiaBan = IDsDiaBan;
        }

        [Route("GiaDatDiaBanCt/Store")]
        [HttpPost]
        public JsonResult Store(string MaDv, string MaDiaBan, string Mahs, string Maloaidat, string Mota, string Loaiduong, string Diemdau, string Diemcuoi, Double Hesok,
            Double Giavt1, Double Giavt2, Double Giavt3, Double Giavt4, Double Giavt5, Double Giavt6, Double Giavt7, Double Giavtconlai, string Hienthi, Double SapXep)
        {
            // Tạo 1 bản nghi mới trang thái CXD nếu thêm chi tiết xong quay lại
            var model = new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt
            {
                Mahs = Mahs,
                Maloaidat = Maloaidat,
                //Maxp = Maxp,
                //Khuvuc = Khuvuc,
                HienThi = Hienthi,
                Sapxep = SapXep,
                Loaiduong = Loaiduong,
                Diemdau = Diemdau,
                Diemcuoi = Diemcuoi,
                Hesok = Hesok,
                Giavt1 = Giavt1,
                Giavt2 = Giavt2,
                Giavt3 = Giavt3,
                Giavt4 = Giavt4,
                Giavt5 = Giavt5,
                Giavt6 = Giavt6,
                Giavt7 = Giavt7,
                Giavtconlai = Giavtconlai,
                Mota = Mota,
                Trangthai = "CXD",
                Created_at = DateTime.Now,
                Updated_at = DateTime.Now,
                MaDv = MaDv,
                Madiaban = MaDiaBan,               
            };
            _db.GiaDatDiaBanCt.Add(model);
            _db.SaveChanges();
            string result = GetData(Mahs);
            var data = new { status = "success", message = result };
            return Json(data);
        }

        //Xóa 1 bản ghi phần chi tiết
        [Route("GiaDatDiaBanCt/Delete")]
        [HttpPost]
        public JsonResult Delete(int Id)
        {
            var model = _db.GiaDatDiaBanCt.FirstOrDefault(t => t.Id == Id);
            _db.GiaDatDiaBanCt.Remove(model);
            _db.SaveChanges();
            string result = GetData(model.Mahs);
            var data = new { status = "success", message = result };
            return Json(data);
        }

        //Xóa tất cả phần chi tiết
        [Route("GiaDatDiaBanCt/Delete2")]
        [HttpPost]
        public JsonResult Delete2(string Mahs)
        {
            var model2 = _db.GiaDatDiaBanCt.Where(t => t.Mahs == Mahs).ToList();
            _db.GiaDatDiaBanCt.RemoveRange(model2);
            _db.SaveChanges();
            string result = GetData(Mahs);
            var data = new { status = "success", message = result };
            return Json(data);
        }

        [Route("GiaDatDiaBanCt/Edit")]
        [HttpPost]
        public JsonResult Edit(int Id,string MaDiaBanGiaDatDiaBan)
        {
            var model = _db.GiaDatDiaBanCt.FirstOrDefault(p => p.Id == Id);                        

            var DsXaPhuong = _IDsDiaBan.GetListDsDiaBan(MaDiaBanGiaDatDiaBan);
            

            var tendiaban = _db.DsDiaBan.FirstOrDefault(x => x.MaDiaBan == MaDiaBanGiaDatDiaBan).TenDiaBan;

            if (model != null)
            {

                string result = "<div class='modal-body' id='edit_thongtin'>";

                result += "<div class='row'>";
                
                result += "<div class='col-xl-12'>";
                result += "<div class='form-group' style='width:100%'>";
                result += "<label>Địa bàn</label>";
                result += "<select id='MaXaPhuong_edit' name='MaXaPhuong_edit' class='form-control select2basic' style='width:100%'>";                                
                foreach (var item in DsXaPhuong)
                {
                    result += "<option value='" + item.MaDiaBan + "'"+(model.Madiaban == item.MaDiaBan ? "selected":"")+">" + item.TenDiaBan + "</option>";
                }
                result += "</select>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form-group'style='width:100%'>";
                result += "<label>Loại đất</label>";
                result += "<select id='maloaidat_edit' name='maloaidat_edit' class='form-control select2basic' tabindex='-1' title='' style='width:100%'>";
                var dsloaidat = _db.DmLoaiDat.ToList();
                foreach (var item in dsloaidat)
                {
                    result += "<option value ='" + @item.Maloaidat + "' " + ((string)model.Maloaidat == item.Maloaidat ? "selected" : "") + ">" + @item.Loaidat + "</ option >";
                }
                result += "</select>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-12'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Tên đường phố</label>";
                result += "<input type='text' id='mota_edit' name='mota_edit' value='" + model.Mota + "' class='form-control'>";
                result += "</div>";
                result += "</div>";


                result += "<div class='col-xl-6'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Điểm đầu</label>";
                result += "<input type='text' id='diemdau_edit' name='diemdau_edit' value='" + model.Diemdau + "' class='form-control'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-6'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Điểm cuối</label>";
                result += "<input type='text' id='diemcuoi_edit' name='diemcuoi_edit' value='" + model.Diemcuoi + "' class='form-control'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Loại đường</label>";
                result += "<input type='text' id='loaiduong_edit' name='loaiduong_edit' value='" + model.Loaiduong + "' class='form-control'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Hệ số</label>";
                result += "<input type='text' id='hesok_edit' name='hesok_edit' value='" + model.Hesok + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá vị trí I</label>";
                result += "<input type='text' id='giavt1_edit' name='giavt1_edit' value='" + model.Giavt1 + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá vị trí II</label>";
                result += "<input type='text' id='giavt2_edit' name='giavt2_edit' value='" + model.Giavt2 + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá vị trí III</label>";
                result += "<input type='text' id='giavt3_edit' name='giavt3_edit' value='" + model.Giavt3 + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá vị trí IV</label>";
                result += "<input type='text' id='giavt4_edit' name='giavt4_edit' value='" + model.Giavt4 + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá vị trí V</label>";
                result += "<input type='text' id='giavt5_edit' name='giavt5_edit' value='" + model.Giavt5 + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá vị trí VI</label>";
                result += "<input type='text' id='giavt6_edit' name='giavt6_edit' value='" + model.Giavt6 + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá vị trí VII</label>";
                result += "<input type='text' id='giavt7_edit' name='giavt7_edit' value='" + model.Giavt7 + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='col-xl-4'>";
                result += "<div class='form-group fv-plugins-icon-container'>";
                result += "<label>Giá vị trí còn lại</label>";
                result += "<input type='text' id='giavtconlai_edit' name='giavtconlai_edit' value='" + model.Giavtconlai + "' class='form-control money-decimal-mask'>";
                result += "</div>";
                result += "</div>";

                result += "<div class='row'>";
                result += "<div class='col-xl-4'>";
                result += "<div class='form -group fv-plugins-icon-container'>";
                result += "<label>STT báo cáo</label>";
                result += "<input type='text' id='hienthi_create' name='hienthi_edit' value='" + model.HienThi + "' class='form-control'/>";
                result += "</div>";
                result += "</div>";
                result += "<div class='col-xl-4'>";
                result += "<div class='form -group fv-plugins-icon-container'>";
                result += "<label>Sắp xếp</label>";
                result += "<input type='text' id='sapxep_create' name='sapxep_edit' value='" + model.Sapxep + "' class='form-control'/>";
                result += "</div></div></div>";

                result += "<input hidden type='text' id='id_edit' name='id_edit' value='" + model.Id + "'/>";
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

        [Route("GiaDatDiaBanCt/Update")]
        [HttpPost]
        public JsonResult Update(int Id,string MaXaPhuong, string Loaiduong, string Maloaidat, string Mota, string Diemdau, string Diemcuoi, double Hesok,
                                    double Giavt1, double Giavt2, double Giavt3, double Giavt4, double Giavt5, double Giavt6, double Giavt7, double Giavtconlai, string Hienthi, double SapXep)
        {
            var model = _db.GiaDatDiaBanCt.FirstOrDefault(t => t.Id == Id);
            
            model.Madiaban= MaXaPhuong;
            model.Maloaidat = Maloaidat;
            model.HienThi = Hienthi;
            model.Sapxep = SapXep;
            model.Mota = Mota;
            model.Loaiduong = Loaiduong;
            model.Diemdau = Diemdau;
            model.Diemcuoi = Diemcuoi;
            model.Hesok = Hesok;
            model.Giavt1 = Giavt1;
            model.Giavt2 = Giavt2;
            model.Giavt3 = Giavt3;
            model.Giavt4 = Giavt4;
            model.Giavt5 = Giavt5;
            model.Giavt6 = Giavt6;
            model.Giavt7 = Giavt7;
            model.Giavtconlai = Giavtconlai;
            model.Updated_at = DateTime.Now;
            _db.GiaDatDiaBanCt.Update(model);
            _db.SaveChanges();

            string result = GetData(model.Mahs);
            var data = new { status = "success", message = result };
            return Json(data);

        }


        [Route("GiaDatDiaBanCt/Excel")]
        [HttpPost]
        public async Task<JsonResult> Excel(string Maloaidat, string Maxp, string Mahs, string Khuvuc, string Diemdau, string Diemcuoi, double Giavt1, double Giavt2, double Giavt3, double Giavt4, double Hesok,
          int Sheet, int LineStart, int LineStop, IFormFile FormFile)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SsAdmin")))
            {
                if (Helpers.CheckPermission(HttpContext.Session, "csdlmucgiahhdv.dinhgia.giadatdb.danhmuc", "Edit"))
                {
                    LineStart = LineStart == 0 ? 1 : LineStart;
                    var list_add = new List<CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt>();
                    int sheet = Sheet == 0 ? 0 : (Sheet - 1);
                    using (var stream = new MemoryStream())
                    {
                        await FormFile.CopyToAsync(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[sheet];
                            var rowcount = worksheet.Dimension.Rows;
                            LineStop = LineStop > rowcount ? rowcount : LineStop;

                            for (int row = LineStart; row <= LineStop; row++)
                            {
                                list_add.Add(new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt
                                {

                                    Mahs = Mahs,
                                    Trangthai = "CXD",
                                    Created_at = DateTime.Now,
                                    Updated_at = DateTime.Now,


                                    //Maloaidat = worksheet.Cells[row, Int16.Parse(Maloaidat)].Value.ToString() != null ?
                                    //            worksheet.Cells[row, Int16.Parse(Maloaidat)].Value.ToString().Trim() : "",

                                    //Maxp = worksheet.Cells[row, Int16.Parse(Maxp)].Value.ToString() != null ?
                                    //            worksheet.Cells[row, Int16.Parse(Maxp)].Value.ToString().Trim() : "",

                                    Khuvuc = worksheet.Cells[row, Int16.Parse(Khuvuc)].Value.ToString() != null ?
                                                worksheet.Cells[row, Int16.Parse(Khuvuc)].Value.ToString().Trim() : "",

                                    Diemdau = worksheet.Cells[row, Int16.Parse(Diemdau)].Value != null ?
                                                worksheet.Cells[row, Int16.Parse(Diemdau)].Value.ToString().Trim() : "",

                                    Diemcuoi = worksheet.Cells[row, Int16.Parse(Diemcuoi)].Value != null ?
                                                worksheet.Cells[row, Int16.Parse(Diemcuoi)].Value.ToString().Trim() : "",

                                    Giavt1 = Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Giavt1.ToString())].Value) != 0 ?
                                                Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Giavt1.ToString())].Value) : 0,

                                    Giavt2 = Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Giavt2.ToString())].Value) != 0 ?
                                                Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Giavt2.ToString())].Value) : 0,

                                    Giavt3 = Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Giavt3.ToString())].Value) != 0 ?
                                                Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Giavt3.ToString())].Value) : 0,

                                    Giavt4 = Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Giavt4.ToString())].Value) != 0 ?
                                                Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Giavt4.ToString())].Value) : 0,

                                    Hesok = Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Hesok.ToString())].Value) != 0 ?
                                                Convert.ToDouble(worksheet.Cells[row, Int16.Parse(Hesok.ToString())].Value) : 0,


                                });
                            }

                        }
                    }
                    _db.GiaDatDiaBanCt.AddRange(list_add);
                    _db.SaveChanges();
                    string result = GetData(Mahs);
                    var data = new { status = "success", message = result };
                    return Json(data);

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

        public string GetData(string Mahs)
        {
            var model = (from dat in _db.GiaDatDiaBanCt.Where(t => t.Mahs == Mahs).ToList().OrderBy(x => x.Sapxep)
                         join dm in _db.DmLoaiDat on dat.Maloaidat equals dm.Maloaidat
                         select new CSDLGia_ASP.Models.Manages.DinhGia.GiaDatDiaBanCt
                         {
                             Id = dat.Id,
                             HienThi = dat.HienThi,
                             Maloaidat = dat.Maloaidat,
                             Mota = dat.Mota,
                             Diemdau = dat.Diemdau,
                             Diemcuoi = dat.Diemcuoi,
                             Loaiduong = dat.Loaiduong,
                             Hesok = dat.Hesok,
                             Giavt1 = dat.Giavt1,
                             Giavt2 = dat.Giavt2,
                             Giavt3 = dat.Giavt3,
                             Giavt4 = dat.Giavt4,
                             Giavt5 = dat.Giavt5,
                             Giavt6 = dat.Giavt6,
                             Giavt7 = dat.Giavt7,
                             Giavtconlai = dat.Giavtconlai,
                             Loaidat = dm.Loaidat,
                             Madiaban = dat.Madiaban,
                             Maxp = dat.Maxp,

                         });
            string result = "<div class='card-body' id='frm_data'>";
            result += "<table class='table table-striped table-bordered table-hover table-responsive' id='datatable_4'>";
            result += "<thead>";
            result += "<tr style='text-align:center'>";

            result += "<th width='2%'>STT</th>";
            result += "<th>Loại đất</th>";
            result += "<th>Tên đường phố</th>";
            result += "<th>Loại đường</th>";
            result += "<th>Điểm đầu</th>";
            result += "<th>Điểm cuối</th>";
            result += "<th>Hệ số</th>";
            result += "<th>VT1</th>";
            result += "<th>VT2</th>";
            result += "<th>VT3</th>";
            result += "<th>VT4</th>";
            result += "<th>VT5</th>";
            result += "<th>VT6</th>";
            result += "<th>VT7</th>";
            result += "<th>VT Còn lại</th>";
            result += "<th>Thao tác</th>";
            result += "</tr>";
            result += "</thead>";
            result += "<tbody>";
            foreach (var item in model)
            {
                result += "<tr>";

                result += "<td style='text-align:center'>" + item.HienThi + "</td>";
                result += "<td style='text-align:center'>" + item.Loaidat + "</td>";
                result += "<td style='text-align:center'>" + item.Mota + "</td>";
                result += "<td style='text-align:center'>" + item.Loaiduong + "</td>";
                result += "<td style='text-align:center'>" + item.Diemdau + "</td>";
                result += "<td style='text-align:center'>" + item.Diemcuoi + "</td>";
                result += "<td style='text-align:center'>" + Helpers.ConvertDbToStr(item.Hesok) + "</td>";
                result += "<td style='text-align:right'>" + Helpers.ConvertDbToStr(item.Giavt1) + "</td>";
                result += "<td style='text-align:right'>" + Helpers.ConvertDbToStr(item.Giavt2) + "</td>";
                result += "<td style='text-align:right'>" + Helpers.ConvertDbToStr(item.Giavt3) + "</td>";
                result += "<td style='text-align:right'>" + Helpers.ConvertDbToStr(item.Giavt4) + "</td>";
                result += "<td style='text-align:right'>" + Helpers.ConvertDbToStr(item.Giavt5) + "</td>";
                result += "<td style='text-align:right'>" + Helpers.ConvertDbToStr(item.Giavt6) + "</td>";
                result += "<td style='text-align:right'>" + Helpers.ConvertDbToStr(item.Giavt7) + "</td>";
                result += "<td style='text-align:right'>" + Helpers.ConvertDbToStr(item.Giavtconlai) + "</td>";

                result += "<td>";
                result += "<button type='button' class='btn btn-sm btn-clean btn-icon' title='Chỉnh sửa'";
                result += " data-target='#Edit_Modal' data-toggle='modal' onclick='SetEdit(`" + item.Id + "`)'>";
                result += "<i class='icon-lg la la-edit text-primary'></i>";
                result += "</button>";
                result += "<button type='button' class='btn btn-sm btn-clean btn-icon' title='Xóa'";
                result += " data-target='#Delete_Modal' data-toggle='modal' onclick='GetDelete(`" + item.Id + "`)'>";
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
