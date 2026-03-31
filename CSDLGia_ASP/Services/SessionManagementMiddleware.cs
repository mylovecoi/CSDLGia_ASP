using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using CSDLGia_ASP.Models.Systems;
using Newtonsoft.Json;
using System.Collections.Generic;
using CSDLGia_ASP.Database;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace CSDLGia_ASP.Services
{
    public class SessionManagementMiddleware
    {
        private readonly RequestDelegate _next;


        public SessionManagementMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu đã đăng nhập và có cookie xác thực
            if (context.User.Identity.IsAuthenticated)
            {
                using (var scope = context.RequestServices.CreateScope())
                {
                    var _db = scope.ServiceProvider.GetRequiredService<CSDLGiaDBContext>();
                    var _dsDonviService = scope.ServiceProvider.GetRequiredService<IDsDonviService>();
                    // Lấy thời gian hết hạn của cookie từ AuthenticationProperties
                    var authProperties = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    var expiresUtc = authProperties.Properties?.ExpiresUtc;

                    if (expiresUtc.HasValue && expiresUtc.Value > DateTimeOffset.UtcNow)
                    {
                        // Khởi tạo lại Session nếu hết hạn hoặc không có sẵn
                        if (!context.Session.IsAvailable || !context.Session.Keys.Contains("SsAdmin"))
                        {
                            var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
                            var model = _db.Users.FirstOrDefault(u => u.Username == username);
                            var danhsachykien = _db.YKienGopY.Count();
                            context.Session.SetString("DanhSachYKienDongGop", danhsachykien.ToString());

                            context.Session.SetString("SsAdmin", JsonConvert.SerializeObject(model));

                            var permissions = _db.Permissions.Where(t => t.Index);
                            if (model.Chucnang == "K")
                            {
                                permissions = permissions.Where(p => p.Username == username);
                            }
                            else
                            {
                                permissions = permissions.Where(p => p.Username == model.Chucnang);
                            }


                            List<DmNgheKd> data_nghe = _db.DmNgheKd.Where(t => t.Theodoi == "TD").ToList();
                            if (model.Level != "DN")
                            {
                                var model_donvi = _dsDonviService.GetListDonvi(model.Madv);
                                List<string> list_madv = model_donvi.Select(t => t.MaDv).ToList();

                                // Lọc dữ liệu sử dụng LINQ to Objects thay vì LINQ to Entities
                                data_nghe = data_nghe.Where(x => list_madv.Any(v => x.Madv.Split(',').Contains(v))).ToList();
                            }
                            else
                            {
                                var donvi_nghe = _db.CompanyLvCc.Where(t => t.Madv == model.Madv);
                                List<string> list_manghe = donvi_nghe.Select(t => t.Manghe).ToList();
                                data_nghe = data_nghe.Where(t => t.Theodoi == "TD" && list_manghe.Contains(t.Manghe)).ToList();
                            }
                            context.Session.SetString("Permission", JsonConvert.SerializeObject(permissions));
                            context.Session.SetString("KeKhaiDangKyGia", JsonConvert.SerializeObject(data_nghe));
                            // Ensure API connection/session settings are populated as they are set during SignIn
                            var heThong = _db.tblHeThong.FirstOrDefault();
                            if (heThong != null)
                            {
                                context.Session.SetString("LinkAPIXacthuc", heThong.LinkAPIXacthuc ?? string.Empty);
                                context.Session.SetString("TokenLGSP", heThong.TokenLGSP ?? string.Empty);
                                context.Session.SetString("MaDiaBanHanhChinh", heThong.MaDiaBanHanhChinh ?? string.Empty);
                                context.Session.SetString("MaDonViThuThap", heThong.MaDonViThuThap ?? string.Empty);
                                context.Session.SetString("TimeOut", heThong.TimeOut ?? string.Empty);
                            }

                            var dsKetNoi = _db.KetNoiAPI_DanhSach.ToList();
                            if (dsKetNoi != null && dsKetNoi.Any())
                            {
                                try
                                {
                                    var dict = dsKetNoi.ToDictionary(item => item.Maso, item => item);
                                    context.Session.SetString("LinkAPIKetNoi", JsonConvert.SerializeObject(dict));
                                }
                                catch
                                {
                                    // If conversion fails, ensure session key exists as empty to avoid null usage in views
                                    context.Session.SetString("LinkAPIKetNoi", string.Empty);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Cookie đã hết hạn - Xóa xác thực nếu cần
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        context.Response.Redirect("/DangNhap");
                        return; // Kết thúc xử lý middleware
                    }
                }
            }

            // Đảm bảo luôn gọi _next để tiếp tục middleware pipeline
            await _next(context);
        }
    }
}
