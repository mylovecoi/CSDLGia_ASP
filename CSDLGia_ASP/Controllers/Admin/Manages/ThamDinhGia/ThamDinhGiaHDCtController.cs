using CSDLGia_ASP.Database;
using CSDLGia_ASP.Helper;
using CSDLGia_ASP.Models.Manages.ThamDinhGia;
using CSDLGia_ASP.Models.Systems;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSDLGia_ASP.Controllers.Admin.Manages.ThamDinhGia
{
    public class ThamDinhGiaHDCtController : Controller
    {
        private readonly CSDLGiaDBContext _db;

        public ThamDinhGiaHDCtController(CSDLGiaDBContext db)
        {
            _db = db;
        }

        [Route("ThamDinhGiaHDCt/Store")]
        [HttpPost]
        public JsonResult Store(string Mahs, string MaHoiDong, int STT, string HoTen, string ChucVu, int VaiTro, int Id)
        {
            var model = _db.ThamDinhGiaHDCt.FirstOrDefault(x => x.Id == Id);
            if (model == null)
            {
                var newHDCT = new ThamDinhGiaHDCt
                {
                    MaHoiDong = MaHoiDong,
                    STT = STT,
                    HoTen = HoTen,
                    ChucVu = ChucVu,
                    VaiTro = VaiTro,
                };
                _db.ThamDinhGiaHDCt.Add(newHDCT);
            }
            else
            {
                model.MaHoiDong = MaHoiDong;
                model.STT = STT;
                model.HoTen = HoTen;
                model.ChucVu = ChucVu;
                model.VaiTro = VaiTro;
                _db.ThamDinhGiaHDCt.Update(model);
            }

            _db.SaveChanges();
            string result = GetData(Mahs, MaHoiDong);
            var data = new { status = "success", message = result };
            return Json(data);
        }

        [Route("ThamDinhGiaHDCt/Edit")]
        [HttpPost]
        public JsonResult Edit(int Id)
        {
            var model = _db.ThamDinhGiaHDCt.FirstOrDefault(p => p.Id == Id);


            string result = "<div class='modal-body' id='edit_thongtin'>";
            result += "<div class='row'>";

            result += "<div class='col-xl-12'>";
            result += "<div class='form-group fv-plugins-icon-container'>";
            result += "<label>Họ và tên </label>";
            result += "<input type='text' name='HoTen' value='" + (model?.HoTen ?? "") + "' class='form-control'/>";
            result += "</div>";
            result += "</div>";

            result += "<div class='col-xl-12'>";
            result += "<div class='form-group fv-plugins-icon-container'>";
            result += "<label>Chức vụ</label>";
            result += "<input type='text' name='ChucVu' value='" + (model?.ChucVu ?? "") + "' class='form-control'/>";
            result += "</div>";
            result += "</div>";

            result += "<div class='col-xl-12'>";
            result += "<div class='form-group fv-plugins-icon-container'>";
            result += "<label>Vai trò</label>";
            result += "<select name='VaiTro' class='form-control'>";
            result += "<option value='1'>Phó chủ tịch hội đồng</option>";
            result += "<option value='2'>Ủy viên thường trực Hội đồng</option>";
            result += "<option value='3'>Ủy viên</option>";
            result += "</select>";
            result += "</div>";
            result += "</div>";

            result += "<div class='col-xl-6'>";
            result += "<div class='form-group fv-plugins-icon-container'>";
            result += "<label>STT</label>";
            result += "<input type='number' name='STT' value='" + (model?.STT ?? 1) + "' class='form-control'/>";
            result += "</div>";
            result += "</div>";

            result += "</div>";
            result += "<input hidden type='text' name='Id' value='" + Id + "'/>";
            result += "</div>";

            var data = new { status = "success", message = result };
            return Json(data);

        }

        [Route("ThamDinhGiaHDCt/Delete")]
        [HttpPost]
        public JsonResult Delete(int Id)
        {
            var model = _db.ThamDinhGiaHDCt.FirstOrDefault(t => t.Id == Id);
            var model_HD = _db.ThamDinhGiaHD.FirstOrDefault(t => t.MaHoiDong == model.MaHoiDong);
            _db.ThamDinhGiaHDCt.Remove(model);
            _db.SaveChanges();
            string result = GetData(model_HD.Mahs, model.MaHoiDong);
            var data = new { status = "success", message = result };
            return Json(data);
        }

        public string GetData(string Mahs, string MaHoiDong)
        {
            var model = _db.ThamDinhGiaHDCt.Where(t => t.MaHoiDong == MaHoiDong).ToList();
            Dictionary<int, string> vaiTro = new Dictionary<int, string>();
            vaiTro[1] = "Phó chủ tịch hội đồng";
            vaiTro[2] = "Ủy viên thường trực Hội đồng";
            vaiTro[3] = "Ủy viên";


            string result = "<div class='card-body' id='frm_data'>";
            result += "<table class='table table-striped table-bordered table-hover table-responsive' id='datatable_4'>";
            result += "<thead>";
            result += "<tr style='text-align:center'>";
            result += "<th width='2%'>#</th>";
            result += "<th>Họ và tên</th>";
            result += "<th>Chức vụ</th>";
            result += "<th>Vai trò</th>";
            result += "<th width='10%'>Thao tác</th>";
            result += "</tr>";
            result += "</thead>";
            result += "<tbody>";

            foreach (var item in model)
            {
                string _vaiTro;
                TryGetKey(vaiTro, item.VaiTro, out _vaiTro);

                result += "<tr>";
                result += "<td style='text-align:center'>" + item.STT + "</td>";
                result += "<td>" + item.HoTen + "</td>";
                result += "<td>" + item.ChucVu + "</td>";
                result += "<td>" + _vaiTro + "</td>";
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

        public static bool TryGetKey(Dictionary<int, string> dictionary, int key, out string ketQua)
        {
            // Lặp qua từng cặp key-value trong từ điển
            foreach (var pair in dictionary)
            {
                // Nếu value của cặp key-value đúng với giá trị tìm kiếm
                if (pair.Key == key)
                {
                    // Trả về value và đánh dấu là thành công
                    ketQua = pair.Value;
                    return true;
                }
            }

            // Không tìm thấy key tương ứng với giá trị
            ketQua = "";
            return false;
        }
    }
}
