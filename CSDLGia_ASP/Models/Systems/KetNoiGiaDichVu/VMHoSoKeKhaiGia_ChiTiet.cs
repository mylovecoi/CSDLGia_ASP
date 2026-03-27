using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace CSDLGia_ASP.Models.Systems.KetNoiGiaDichVu
{
    public class VMHoSoKeKhaiGia_ChiTiet
    {
        [Key]
        public int id { get; set; }
        public string macskd { get; set; }
        public string mahs { get; set; }
        public string maloaip { get; set; }
        public string loaip { get; set; }
        public string qccl { get; set; }
        public string sohieu { get; set; }
        public string ghichu { get; set; }
        public string mucgialk { get; set; }
        public string mucgiakk { get; set; }
        public string tendoituong { get; set; }
        public string apdungpublic { get; set; }

        public double mucgiakkValue
        {
            get
            {
                if (double.TryParse(mucgiakk, out double result))
                    return result;
                return 0;
            }
        }

        public double mucgialkValue
        {
            get
            {
                if (double.TryParse(mucgialk, out double result))
                    return result;
                return 0;
            }
        }
    }
}
