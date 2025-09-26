using CSDLGia_ASP.Database;
using CSDLGia_ASP.Models.Systems;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace CSDLGia_ASP.Services
{
    public interface IDsDonviService
    {
        List<DsDonVi> GetListDonvi(string Madv);
        List<DsDonVi> GetListDonviCapDuoi (string Madv);
        List<DsDonVi> GetListDonviCapTren (string MaCqcq);
    }
    public class DsDonviService: IDsDonviService
    {

        private readonly CSDLGiaDBContext _db;

        public DsDonviService(CSDLGiaDBContext db)
        {
            _db = db;
        }

        public List<DsDonVi> GetListDonvi(string Madv)
        {
            List<DsDonVi> listdonvi = new List<DsDonVi>();
            if (!string.IsNullOrEmpty(Madv))
            {
                var model = _db.DsDonVi.FirstOrDefault(t => t.MaDv == Madv);
                if (model != null)
                {
                    listdonvi.Add(new DsDonVi
                    {
                        MaDv = model.MaDv,
                        TenDv = model.TenDv,
                    });
                    if (!string.IsNullOrEmpty(model.MaDv))
                    {
                        this.Recursive(listdonvi, model.MaDv);
                    }
                }
            }
            return listdonvi;
        }

        private void Recursive(List<DsDonVi> ListDonVi, string Madv)
        {
            var model = _db.DsDonVi.Where(t => t.MaCqcq == Madv && t.MaDv != Madv).ToList();
            if (model.Any())
            {
                foreach (var item in model)
                {
                    ListDonVi.Add(new DsDonVi
                    {
                        MaDv = item.MaDv,
                        TenDv = item.TenDv,
                    });
                    if (!string.IsNullOrEmpty(item.MaDv))
                    {
                        Recursive(ListDonVi, item.MaDv);
                    }
                }
            }
            else
            {
                if (ListDonVi.FirstOrDefault(t => t.MaDv == Madv) == null)
                {
                    var data = _db.DsDonVi.FirstOrDefault(t => t.MaDv == Madv);
                    ListDonVi.Add(new DsDonVi
                    {
                        MaDv = data.MaDv,
                        TenDv = data.TenDv
                    });
                }
            }
        }

        public List<DsDonVi> GetListDonviCapDuoi(string Madv)
        {
            List<DsDonVi> listdonvi = new List<DsDonVi>();
            if (!string.IsNullOrEmpty(Madv))
            {
                var model = _db.DsDonVi.FirstOrDefault(t => t.MaDv == Madv);
                if (model != null)
                {                   
                    if (!string.IsNullOrEmpty(model.MaDv))
                    {
                        this.Recursive(listdonvi, model.MaDv);
                    }
                }
            }
            return listdonvi;
        }

        public List<DsDonVi> GetListDonviCapTren(string MaCqcq)
        {
            List<DsDonVi> listdonvi = new List<DsDonVi>();
            if (!string.IsNullOrEmpty(MaCqcq))
            {
                var model = _db.DsDonVi.FirstOrDefault(t => t.MaCqcq == MaCqcq);
                if (model != null)
                {
                    listdonvi.Add(new DsDonVi
                    {
                        MaDv = model.MaDv,
                        TenDv = model.TenDv,
                        MaCqcq = model.MaCqcq,
                    });
                    if (!string.IsNullOrEmpty(model.MaCqcq))
                    {
                        this.GetDonviCapTren(listdonvi, model.MaCqcq);
                    }
                }
            }
            return listdonvi;
        }

        private void GetDonviCapTren(List<DsDonVi> ListDonVi, string MaCqcq)
        {
            var model = _db.DsDonVi.FirstOrDefault(t => t.MaDv == MaCqcq);
            if (model!= null)
            {
                
                    ListDonVi.Add(new DsDonVi
                    {
                        MaDv = model.MaDv,
                        TenDv = model.TenDv,
                        MaCqcq = model.MaCqcq,
                    });
                    if (!string.IsNullOrEmpty(model.MaCqcq))
                    {
                        GetDonviCapTren(ListDonVi, model.MaCqcq);
                    }
            }   
        }
    }
}
