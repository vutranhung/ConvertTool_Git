using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAL;

namespace ConvertToolApp
{
    public partial class ConvertDataToConstract : Form
    {
        private PruDataEntities db = new PruDataEntities();
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ConvertDataToConstract()
        {
            InitializeComponent();
        }

        private void ConvertDataToConstract_Load(object sender, EventArgs e)
        {
            try
            {
                lblCurrent.Text = "0";
                lblSum.Text = db.Contracts.Where(a => a.DataComplete == "Y").ToList().Count().ToString();
            }
            catch(Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                btnExit.Enabled = false;
                int count = 0;
                List<Contract> contracts = db.Contracts.Where(a => a.DataComplete == "Y").ToList();

                foreach (var contract in contracts)
                {
                    decimal phi = 0;
                    DateTime? ngaydaohan = null, ngayketthuc = null;
                    List<DAL.InsuredPerson> insuredPeople = db.InsuredPersons.Where(a => a.ContractID == contract.ID).ToList();
                    foreach (var item in insuredPeople)
                    {
                        Contract personEx = new Contract();

                        personEx.Code = contract.Code;

                        personEx.Relation = item.Relation;
                        personEx.ProductName = item.TenSanPham;
                        personEx.Loai = item.Loai;
                        personEx.PhiBaoHiem = item.Fee;
                        personEx.NgayDaoHan = item.NgayDaoHan;
                        personEx.NgayKetThucDongPhi = item.DongPhiDenNgay;
                        personEx.Status = item.TinhTrang;
                        personEx.DataComplete = "C";
                        personEx.ParentID = item.ContractID;

                        Customer customer = db.Customers.Find(item.CustomerID);
                        if (customer != null)
                        {
                            personEx.TelMobile = customer.Phone;
                            personEx.TelHome = customer.TelHome;
                            personEx.TelCompany = customer.TelCompany;
                            personEx.CustomerCMND = customer.CMND;
                            personEx.CustomerCode = customer.Code;
                            personEx.CustomerName = customer.Name;
                            personEx.VanPhong = customer.VanPhong;
                            personEx.DOB = customer.DOB;
                        }

                        db.Contracts.Add(personEx);
                        db.SaveChanges();

                        phi += personEx.PhiBaoHiem.HasValue ? personEx.PhiBaoHiem.Value : 0;

                        if (personEx.Loai == "Chính")
                        {
                            ngaydaohan = personEx.NgayDaoHan;
                            ngayketthuc = personEx.NgayKetThucDongPhi;
                        }                        
                    }

                    contract.PhiBaoHiem = phi;
                    contract.NgayDaoHan = ngaydaohan;
                    contract.NgayKetThucDongPhi = ngayketthuc;
                    db.Entry(contract).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    count++;
                    lblCurrent.Text = count.ToString();
                    lblCurrent.Update();
                }
                                
                btnExit.Enabled = true;
            }
            catch(Exception ex)
            {
                log.Error(ex.ToString());
                btnExit.Enabled = true;
            }

            Cursor.Current = Cursors.Default;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
