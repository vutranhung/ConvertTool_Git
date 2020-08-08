using AutoMapper;
using DAL;
using DAL.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace ConvertToolApp
{
    public partial class ExportConstractData : Form
    {
        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private PruDataEntities db = new PruDataEntities();

        private string loai_Bo_Van_Phong = string.Empty;
        private string chi_Dinh_Van_Phong = string.Empty;
        private string exportDate = string.Empty;
        private string loai_bo_hop_dong = string.Empty;

        private string pathExportCsv = ConfigurationManager.AppSettings["PathExportConstract"];

        public ExportConstractData()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ExportCustomerData_Load(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            btnExport.Enabled = false;
            btnExportCsv.Enabled = false;

            List<Config> config = db.Configs.ToList(); //FirstOrDefault(a => a.Name.ToLower().Equals("loai_bo_van_phong"));
            if (config.Count > 0)
            {
                foreach (var item in config)
                {
                    if (item.Name.ToLower().Equals("loai_bo_van_phong"))
                    {
                        loai_Bo_Van_Phong = item.Value.Trim();
                    }

                    if (item.Name.ToLower().Equals("chi_dinh_van_phong"))
                    {
                        chi_Dinh_Van_Phong = item.Value.Trim();
                    }

                    if (item.Name.ToLower().Equals("exportdate"))
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            exportDate = item.Value.Trim();
                        }
                        else
                        {
                            exportDate = DateTime.Now.ToString("dd/MM/yyyy");
                        }
                    }

                    if (item.Name.ToLower().Equals("loai_bo_hop_dong"))
                    {
                        if (!string.IsNullOrEmpty(item.Value.Trim()))
                        {
                            loai_bo_hop_dong = item.Value.Trim();                            
                        }                        
                    }
                }

            }

            if (!string.IsNullOrEmpty(loai_Bo_Van_Phong))
            {
                radIgnore.Checked = true;
                txtIgnore.Enabled = true;
                txtIgnore.Text = loai_Bo_Van_Phong.ToUpper();
                txtFix.Text = chi_Dinh_Van_Phong.ToUpper();
            }
            else if (!string.IsNullOrEmpty(chi_Dinh_Van_Phong))
            {
                radFix.Checked = true;
                radFix.Enabled = true;
                txtIgnore.Text = loai_Bo_Van_Phong.ToUpper();
                txtFix.Text = chi_Dinh_Van_Phong.ToUpper();
            }
            else
            {
                radAll.Checked = true;

                txtIgnore.Enabled = false;
                txtFix.Enabled = false;
            }

            LoadDataGridView();
            Cursor.Current = Cursors.Default;
        }

        private void LoadDataGridView()
        {
            dataGridView.Columns.Clear();
            
            var MaKhachHang = new DataGridViewTextBoxColumn();
            MaKhachHang.HeaderText = "Mã khách hàng";
            MaKhachHang.Name = "MaKhachHang";
            MaKhachHang.Visible = true;
            MaKhachHang.DataPropertyName = "MaKhachHang";
            MaKhachHang.Width = 100;
            dataGridView.Columns.Add(MaKhachHang);

            var cmnd = new DataGridViewTextBoxColumn();
            cmnd.HeaderText = "Số CMND/GPKD";
            cmnd.Name = "CMND";
            cmnd.Visible = true;
            cmnd.DataPropertyName = "CMND";
            cmnd.Width = 100;
            dataGridView.Columns.Add(cmnd);

            var fullName = new DataGridViewTextBoxColumn();
            fullName.HeaderText = "Họ và Tên KH";
            fullName.Name = "TenKhachHang";
            fullName.Visible = true;
            fullName.Width = 300;
            fullName.DataPropertyName = "TenKhachHang";
            dataGridView.Columns.Add(fullName);

            var dob = new DataGridViewTextBoxColumn();
            dob.HeaderText = "Ngày Sinh";
            dob.Name = "NgaySinh";
            dob.Visible = true;
            dob.DataPropertyName = "NgaySinh";
            dob.Width = 150;
            dob.DefaultCellStyle.Format = "dd/MM/yyyy";
            dataGridView.Columns.Add(dob);

            var QuanHeVoiBMBH = new DataGridViewTextBoxColumn();
            QuanHeVoiBMBH.HeaderText = "Quan hệ với BMBH";
            QuanHeVoiBMBH.Name = "QuanHeVoiBMBH";
            QuanHeVoiBMBH.Visible = true;
            //QuanHeVoiBMBH.Width = 300;
            QuanHeVoiBMBH.DataPropertyName = "QuanHeVoiBMBH";
            dataGridView.Columns.Add(QuanHeVoiBMBH);

            var DiaChiHT = new DataGridViewTextBoxColumn();
            DiaChiHT.HeaderText = "Địa Chỉ Hiện Tại";
            DiaChiHT.Name = "DiaChiHT";
            DiaChiHT.Visible = true;
            DiaChiHT.Width = 300;
            DiaChiHT.DataPropertyName = "DiaChiHT";
            dataGridView.Columns.Add(DiaChiHT);

            var TelDiDong = new DataGridViewTextBoxColumn();
            TelDiDong.HeaderText = "Tel Di Động";
            TelDiDong.Name = "TelDiDong";
            TelDiDong.Visible = true;
            TelDiDong.DataPropertyName = "TelDiDong";            
            dataGridView.Columns.Add(TelDiDong);

            var NgayDaoHan = new DataGridViewTextBoxColumn();
            NgayDaoHan.HeaderText = "Ngày Đáo Hạn";
            NgayDaoHan.Name = "NgayDaoHan";
            NgayDaoHan.Visible = true;
            NgayDaoHan.DataPropertyName = "NgayDaoHan";
            dataGridView.Columns.Add(NgayDaoHan);

            var TelCoQuan = new DataGridViewTextBoxColumn();
            TelCoQuan.HeaderText = "Tel Cơ Quan";
            TelCoQuan.Name = "TelCoQuan";
            TelCoQuan.Visible = true;
            TelCoQuan.DataPropertyName = "TelCoQuan";
            dataGridView.Columns.Add(TelCoQuan);

            var TelNha = new DataGridViewTextBoxColumn();
            TelNha.HeaderText = "Tel Nhà";
            TelNha.Name = "TelNha";
            TelNha.Visible = true;
            TelNha.DataPropertyName = "TelNha";
            dataGridView.Columns.Add(TelNha);

            var SoHD = new DataGridViewTextBoxColumn();
            SoHD.HeaderText = "Số HĐ";
            SoHD.Name = "SoHD";
            SoHD.Visible = true;
            SoHD.DataPropertyName = "SoHD";
            dataGridView.Columns.Add(SoHD);

            var TenSanPham = new DataGridViewTextBoxColumn();
            TenSanPham.HeaderText = "Tên Sản Phẩm";
            TenSanPham.Name = "TenSanPham";
            TenSanPham.Visible = true;
            TenSanPham.DataPropertyName = "TenSanPham";
            dataGridView.Columns.Add(TenSanPham);

            var PhiBaoHiem = new DataGridViewTextBoxColumn();
            PhiBaoHiem.HeaderText = "Phí Bảo Hiểm";
            PhiBaoHiem.Name = "PhiBaoHiem";
            PhiBaoHiem.Visible = true;
            PhiBaoHiem.DataPropertyName = "PhiBaoHiem";
            dataGridView.Columns.Add(PhiBaoHiem);

            var DKDP = new DataGridViewTextBoxColumn();
            DKDP.HeaderText = "Định Kỳ Đóng Phí";
            DKDP.Name = "DKDP";
            DKDP.Visible = true;
            DKDP.DataPropertyName = "DKDP";
            dataGridView.Columns.Add(DKDP);

            var TrangThaiHD = new DataGridViewTextBoxColumn();
            TrangThaiHD.HeaderText = "Trạng Thái HĐ";
            TrangThaiHD.Name = "TrangThaiHD";
            TrangThaiHD.Visible = true;
            TrangThaiHD.DataPropertyName = "TrangThaiHD";
            dataGridView.Columns.Add(TrangThaiHD);

            var NgayHieuLuc = new DataGridViewTextBoxColumn();
            NgayHieuLuc.HeaderText = "Ngày Hiệu Lực";
            NgayHieuLuc.Name = "NgayHieuLuc";
            NgayHieuLuc.Visible = true;
            NgayHieuLuc.DataPropertyName = "NgayHieuLuc";
            dataGridView.Columns.Add(NgayHieuLuc);

            var NgayNopPhiTiep = new DataGridViewTextBoxColumn();
            NgayNopPhiTiep.HeaderText = "Ngày Nộp Phí Tiếp";
            NgayNopPhiTiep.Name = "NgayNopPhiTiep";
            NgayNopPhiTiep.Visible = true;
            NgayNopPhiTiep.DataPropertyName = "NgayNopPhiTiep";
            dataGridView.Columns.Add(NgayNopPhiTiep);

            var NgayKetThucDongPhi = new DataGridViewTextBoxColumn();
            NgayKetThucDongPhi.HeaderText = "Ngày Kết Thúc Đóng Phí";
            NgayKetThucDongPhi.Name = "NgayKetThucDongPhi";
            NgayKetThucDongPhi.Visible = true;
            NgayKetThucDongPhi.DataPropertyName = "NgayKetThucDongPhi";
            dataGridView.Columns.Add(NgayKetThucDongPhi);

            //var NgayDaoHan = new DataGridViewTextBoxColumn();
            //NgayDaoHan.HeaderText = "Ngày Đáo Hạn";
            //NgayDaoHan.Name = "NgayDaoHan";
            //NgayDaoHan.Visible = true;
            //NgayDaoHan.DataPropertyName = "NgayDaoHan";
            //dataGridView.Columns.Add(NgayDaoHan);

            var GiaTriHoanLai = new DataGridViewTextBoxColumn();
            GiaTriHoanLai.HeaderText = "Giá Trị HL Để T/Ứ";
            GiaTriHoanLai.Name = "GiaTriHoanLai";
            GiaTriHoanLai.Visible = true;
            GiaTriHoanLai.DataPropertyName = "GiaTriHoanLai";
            dataGridView.Columns.Add(GiaTriHoanLai);

            var CacKhoanTamUng = new DataGridViewTextBoxColumn();
            CacKhoanTamUng.HeaderText = "Các Khoản T/Ứ";
            CacKhoanTamUng.Name = "CacKhoanTamUng";
            CacKhoanTamUng.Visible = true;
            CacKhoanTamUng.DataPropertyName = "CacKhoanTamUng";
            dataGridView.Columns.Add(CacKhoanTamUng);

            var RutTruocBT = new DataGridViewTextBoxColumn();
            RutTruocBT.HeaderText = "Rút Trước BT";
            RutTruocBT.Name = "RutTruocBT";
            RutTruocBT.Visible = true;
            RutTruocBT.DataPropertyName = "RutTruocBT";
            dataGridView.Columns.Add(RutTruocBT);

            var QuyenLoiDK = new DataGridViewTextBoxColumn();
            QuyenLoiDK.HeaderText = "Quyền Lợi ĐK";
            QuyenLoiDK.Name = "QuyenLoiDK";
            QuyenLoiDK.Visible = true;
            QuyenLoiDK.DataPropertyName = "QuyenLoiDK";
            dataGridView.Columns.Add(QuyenLoiDK);

            var HuyHD = new DataGridViewTextBoxColumn();
            HuyHD.HeaderText = "Hủy HĐ";
            HuyHD.Name = "HuyHD";
            HuyHD.Visible = true;
            HuyHD.DataPropertyName = "HuyHD";
            dataGridView.Columns.Add(HuyHD);

            var TVV = new DataGridViewTextBoxColumn();
            TVV.HeaderText = "TVV";
            TVV.Name = "TVV";
            TVV.Visible = true;
            TVV.DataPropertyName = "TVV";
            dataGridView.Columns.Add(TVV);

            var vanphong = new DataGridViewTextBoxColumn();
            vanphong.HeaderText = "Văn Phòng";
            vanphong.Name = "VanPhong";
            vanphong.Visible = true;
            vanphong.DataPropertyName = "VanPhong";
            dataGridView.Columns.Add(vanphong);

            var NDH = new DataGridViewTextBoxColumn();
            NDH.HeaderText = "NDH";
            NDH.Name = "NDH";
            NDH.Visible = false;
            NDH.DataPropertyName = "NDH";
            dataGridView.Columns.Add(NDH);

            var ID = new DataGridViewTextBoxColumn();
            ID.HeaderText = "ID";
            ID.Name = "ID";
            ID.Visible = false;
            ID.DataPropertyName = "ID";
            dataGridView.Columns.Add(ID);

            var CustomerID = new DataGridViewTextBoxColumn();
            CustomerID.HeaderText = "CustomerID";
            CustomerID.Name = "CustomerID";
            CustomerID.Visible = false;
            CustomerID.DataPropertyName = "CustomerID";
            dataGridView.Columns.Add(CustomerID);

            var CustomerMaxDate = new DataGridViewTextBoxColumn();
            CustomerID.HeaderText = "MaxDate";
            CustomerID.Name = "MaxDate";
            CustomerID.Visible = false;
            CustomerID.DataPropertyName = "MaxDate";
            dataGridView.Columns.Add(CustomerMaxDate);

            //dgv.EnableHeadersVisualStyles = false;
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(224, 224, 224);            

            //dgv.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Regular);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dateValue;
                if (!string.IsNullOrEmpty(txtDOB.Text))
                {
                    string[] formats = { "dd/MM/yyyy" };
                    if (!DateTime.TryParseExact(txtDOB.Text, formats,
                                            new CultureInfo("en-US"),
                                            DateTimeStyles.None,
                                            out dateValue))
                    {
                        MessageBox.Show("Ngày sinh không đúng định dạng", "Thông báo");
                        txtDOB.Focus();
                        return;
                    }
                }

                Cursor.Current = Cursors.WaitCursor;

                

                BindGrid();
                lblRecord.Text = dataGridView.RowCount.ToString();
                if (dataGridView.Rows.Count > 0)
                {
                    btnExport.Enabled = true;
                    btnExportCsv.Enabled = true;
                }
                else
                {
                    btnExport.Enabled = false;
                    btnExportCsv.Enabled = false;
                }

                Cursor.Current = Cursors.Default;
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
                Cursor.Current = Cursors.Default;
            }
        }

        private void BindGrid()
        {
            try
            {
                //var watch = System.Diagnostics.Stopwatch.StartNew();
                //Lay du lieu NMBH
                //dataGridView.DataSource=GetData();
                dataGridView.DataSource = FillToGrid();
                dataGridView.Columns["PhiBaoHiem"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView.Columns["PhiBaoHiem"].DefaultCellStyle.Format = "N0";

                dataGridView.Columns["GiaTriHoanLai"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView.Columns["GiaTriHoanLai"].DefaultCellStyle.Format = "N0";

                dataGridView.Columns["CacKhoanTamUng"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView.Columns["CacKhoanTamUng"].DefaultCellStyle.Format = "N0";

                dataGridView.Columns["RutTruocBT"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView.Columns["RutTruocBT"].DefaultCellStyle.Format = "N0";

                dataGridView.Columns["QuyenLoiDK"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView.Columns["QuyenLoiDK"].DefaultCellStyle.Format = "N0";

                dataGridView.Columns["HuyHD"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView.Columns["HuyHD"].DefaultCellStyle.Format = "N0";
                setRowNumber(dataGridView);
               // watch.Stop();
                //TimeSpan time = watch.Elapsed;
                //lblTime.Text +=" - Total time: " +string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);


            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private IEnumerable<ExportConstract> FillToGrid()
        {
            try
            {

                var watchQuery = System.Diagnostics.Stopwatch.StartNew(); 
                List<ExportConstract> result = new List<ExportConstract>();
                var query = db.Contracts.Where(m => m.DataComplete == "Y");                
                string vanphong = string.Empty;
                string strWhereVanPhong = string.Empty;
                if (radIgnore.Checked)
                {
                    vanphong = txtIgnore.Text.Trim();
                }
                else if (radFix.Checked)
                {
                    vanphong = txtFix.Text.Trim();
                }

                if (!string.IsNullOrEmpty(vanphong))
                {
                    string[] vs = vanphong.Split(',');

                    if (radFix.Checked)
                    {
                        // strWhereVanPhong = string.Format(" and (a.VanPhong like '%{0}%'", vs[0].ToUpper().Trim());

                        // for (int i = 1; i < vs.Length; i++)
                        // {
                        //     strWhereVanPhong += string.Format(" or a.VanPhong like '%{0}%'", vs[i].ToUpper().Trim());

                        // }

                        //strWhereVanPhong += ")";

                        query = query.Where(m => vs.Contains(m.VanPhong));

                    }
                    else if (radIgnore.Checked)
                    {
                        //strWhereVanPhong = string.Format(" and ((a.VanPhong not like '%{0}%'", vs[0].ToUpper().Trim());
                        //for (int i = 1; i < vs.Length; i++)
                        //{
                        //    strWhereVanPhong += string.Format(" and a.VanPhong not like '%{0}%'", vs[i].ToUpper().Trim());
                        //}

                        //strWhereVanPhong += ") or a.VanPhong is null ) ";
                        query = query.Where(m => !vs.Contains(m.VanPhong) || m.VanPhong == null);
                    }
                    else
                    {

                    }
                }

                string strWhere2 = string.Empty;
                if (!string.IsNullOrEmpty(loai_bo_hop_dong))
                {
                    List<string> vs = loai_bo_hop_dong.Split(',').Select(m=>m.Trim()).ToList();
//                    List<string> lsttemp = vs.Select(m => m.Trim()).ToList();

                    //strWhere2 = "N'" + vs[0].ToUpper().Trim() + "'";
                    //for (int i = 1; i < vs.Length; i++)
                    //{
                    //    strWhere2 += ",N'" + vs[i].ToUpper().Trim() + "'";
                    //}
                    query = query.Where(m => !vs.Contains(m.Status));
                }

                string ttp = txtTTP.Text.Trim();
                string qh = txtQH.Text.Trim();
                string cmnd = txtCMND.Text.Trim();
                string code = txtCustomerCode.Text.Trim();
                string name = txtCustomerName.Text.Trim();
                string dob = txtDOB.Text.Trim();

                string constractCode = txtConstractCode.Text.Trim();
                string daily = txtDaily.Text.Trim();

                string strSQL = string.Empty;
                 //strSQL = "Select * from Contract a where DataComplete = 'Y' ";

                if (!string.IsNullOrEmpty(ttp))
                {
                    // strSQL += " and a.City like N'%" + ttp.ToUpper() + "%'";
                    query = query.Where(m => m.City.Contains(ttp.ToUpper()));

                }

                if (!string.IsNullOrEmpty(qh))
                {
                    //strSQL += " and a.District like N'%" + qh.ToUpper() + "%'";
                    query = query.Where(m => m.District.Contains(qh.ToUpper()));
                }

                if (!string.IsNullOrEmpty(cmnd))
                {
                    //strSQL += " and a.CustomerCMND = '" + cmnd + "'";
                    query = query.Where(m => m.CustomerCMND.Contains(cmnd));
                }

                if (!string.IsNullOrEmpty(code))
                {
                    //strSQL += " and a.NMBHCode = '" + code + "'";
                    query = query.Where(m => m.NMBHCode == code);
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    //strSQL += " and a.CustomerName like N'%" + name + "%'";
                    query = query.Where(m => m.CustomerName.Contains(name));
                }

                if (!string.IsNullOrEmpty(dob))
                {
                    //strSQL += " and a.DOB = CONVERT(datetime, '" + dob + "', 103)";
                    DateTime tmpDob = DateTime.ParseExact(dob, "dd/MM/yyyy",null);
                    DateTime fromDate = tmpDob.AddDays(-1);
                    DateTime toDate = tmpDob.AddDays(1);
                    query = query.Where(m => m.DOB > fromDate && m.DOB < toDate);

                    //query = query.Where(m => m.DOB == tmpDob);
                }

                if (!string.IsNullOrEmpty(constractCode))
                {
                    //strSQL += " and a.Code = '" + constractCode + "'";
                    query = query.Where(m => m.Code == constractCode);
                }

                if (!string.IsNullOrEmpty(daily))
                {
                    //strSQL += " and a.TVVCode = '" + daily + "'";
                    query = query.Where(m => m.TVVCode == daily);
                }

                query = query.Where(m => m.NgayDaoHan != null);



                //lay ngay dao han theo can tren
                if (!query.Any())
                {
                    return result;
                }

                DateTime dtNgayDaoHan;             
                DateTime.TryParseExact(exportDate, "dd/MM/yyyy",
                                        new CultureInfo("en-US"),
                                        DateTimeStyles.None,
                                        out dtNgayDaoHan);



              

                var parent = query.OrderByDescending(m => m.NgayDaoHan).ThenBy(m => m.CustomerCode);

                //lay tat ca con cua cha 
                var children = db.Contracts.Where(m => m.ParentID != null && parent.Select(x => x.ID).Contains(m.ParentID.Value));

                var group = parent.GroupJoin(children,
                    p => p.ID,
                    c => c.ParentID,
                    (p, childrentGroup) => new Hieararchy
                    {
                        parent = p,
                        children = childrentGroup
                    }
                    );


                watchQuery.Stop();
                TimeSpan time = watchQuery.Elapsed;
                lblTime.Text = " query time: " + string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);

                var watchProcess = System.Diagnostics.Stopwatch.StartNew();
                Dictionary<Contract, List<Contract>> dicTemp = new Dictionary<Contract, List<Contract>>();

                Dictionary<Contract, List<Contract>> dicContract = new Dictionary<Contract, List<Contract>>();

                Dictionary<ExportConstract, List<ExportConstract>> dicExportConstract = new Dictionary<ExportConstract, List<ExportConstract>>();

                foreach (var item in group)
                    dicTemp.Add(item.parent, item.children.ToList());

                // thuc hien de quy de lay du lieu

               // var lstData = new List<Contract>();
                foreach (var item in dicTemp)
                {
                    if (!dicContract.Keys.Contains(item.Key))
                    {
                        var lstChildOwner = new List<Contract>();
                      //  lstData.Add(item.Key);
                        HieararchyWalk(item.Value, parent.ToList(), dicTemp, ref lstChildOwner);
                        dicContract.Add(item.Key, lstChildOwner);
                    }

                }


                //lay duoc danh sach Dictionary theo cha va con
                // gia can chia ra theo ngay dao han va sau khi chia xong thi thuc hien sap xep
                // voi du lieu truoc ngay dao han thi sap xep giam dan, sau dao han thi sap xep tang dan


                //var lstData = new List<Contract>();
                //foreach (var item in dicTemp)
                //{
                //    if (!lstData.Contains(item.Key))
                //    {                        
                //        lstData.Add(item.Key);                        

                //        HieararchyWalk(item.Value, parent.ToList(), ref lstData, dicTemp);
                //    }

                //}


                //lam sao de sap xep duoc du lieu theo ngay dao han
                //truoc ngay dao han sap xep giam dan
                // sau ngay dao han sap xep tang dan

                if (!dicContract.Any())
                {
                    return result;
                }

                foreach(var itemDic in dicContract)
                {
                    ExportConstract key = ConvertContractToExportContract(itemDic.Key);
                    DateTime dateKey = itemDic.Key.NgayDaoHan.Value;
                    DateTime? dateMaxValues = itemDic.Value.Where(m=>m.NgayDaoHan!=null).Max(x => x.NgayDaoHan);
                   
                    if (dateMaxValues == null)
                        key.MaxDate = dateKey;
                    else
                        key.MaxDate = dateKey >= dateMaxValues.Value ? dateKey : dateMaxValues.Value;
                    List<ExportConstract> values = new List<ExportConstract>(); 
                    foreach(var itemValue in itemDic.Value)
                    {
                        values.Add(ConvertContractToExportContract(itemValue));
                    }
                    
                    dicExportConstract.Add(key, values);
                }


                var dicExportContractUp = dicExportConstract.Where(m => m.Key.MaxDate >= dtNgayDaoHan).OrderBy(m=>m.Key.MaxDate);
                var dicExportContractDown = dicExportConstract.Where(m => m.Key.MaxDate < dtNgayDaoHan).OrderByDescending(m => m.Key.MaxDate);

                dicExportContractUp.ToList().ForEach(m =>
                {
                    result.Add(m.Key);
                    result.AddRange(m.Value);
                });

                dicExportContractDown.ToList().ForEach(m =>
                {
                    result.Add(m.Key);
                    result.AddRange(m.Value);
                });



                Console.Write("tong so ban ghi:" + result.Count());

                watchProcess.Stop();
                TimeSpan timeProcess = watchProcess.Elapsed;
                lblTime.Text += " - process time: " + string.Format("{0:00}:{1:00}:{2:00}", timeProcess.Hours, timeProcess.Minutes, timeProcess.Seconds);

                //lay phan tren theo ngay dao han va sap xep tang dan

                //lay phan nua duoi va sap xep giam dan





                //   var lst = query.ToList();

                //   var parentDataUp =  query.Where(m => m.NgayDaoHan!=null && m.NgayDaoHan.Value >= dtNgayDaoHan).OrderByDescending(m => m.NgayDaoHan).ThenBy(m => m.CustomerCode);
                //    var parentDataDown = query.Where(m => m.NgayDaoHan != null && m.NgayDaoHan.Value < dtNgayDaoHan);                         

                //   var childrenDataUp = db.Contracts.Where(m =>m.NgayDaoHan>=dtNgayDaoHan && m.ParentID != null && parentDataUp.Select(x=>x.ID).ToList().Contains(m.ParentID.Value) && parentDataUp.Select(x=>x.Code).ToList().Contains(m.Code));                

                //   var childrenDataDown = db.Contracts.Where(m => m.NgayDaoHan < dtNgayDaoHan && m.ParentID != null && parentDataDown.Select(x => x.ID).ToList().Contains(m.ParentID.Value) && parentDataDown.Select(x => x.Code).ToList().Contains(m.Code));

                //var groupUp = parentDataUp.GroupJoin(childrenDataUp,
                //        p => p.ID,
                //        c => c.ParentID,
                //        (p, childrenGroup) => new Hieararchy
                //        {
                //            parent = p,
                //            children = childrenGroup
                //        }
                //        );

                //var groupDown = parentDataDown.GroupJoin(childrenDataDown,
                //         p => p.ID,
                //         c => c.ParentID,
                //         (p, childrenGroup) => new Hieararchy
                //         {
                //             parent = p,
                //             children = childrenGroup
                //         }
                //         );

                //Dictionary<Contract,List<Contract>> dicUp = new Dictionary<Contract, List<Contract>>();
                //Dictionary<Contract, List<Contract>> dicDown = new Dictionary<Contract, List<Contract>>();

                //foreach (var item in groupUp)
                //{
                //    dicUp.Add(item.parent, item.children.ToList());
                //}

                //foreach (var item in groupDown)
                //{
                //    dicDown.Add(item.parent, item.children.ToList());
                //}

                //var lstUp = new List<Contract>();
                //var lstDown = new List<Contract>();

                //foreach (var objDic in dicUp)
                //{
                //    if (!lstUp.Contains(objDic.Key))
                //    {
                //        lstUp.Add(objDic.Key);
                //        HieararchyWalk(objDic.Value, parentDataUp.ToList(),ref lstUp, dicUp);
                //    }

                //}

                //foreach (var objDic in dicDown)
                //{
                //    if (!lstDown.Contains(objDic.Key))
                //    {
                //        lstDown.Add(objDic.Key);
                //        HieararchyWalk(objDic.Value, parentDataDown.ToList(), ref lstDown, dicDown);
                //    }

                //}


                /*
                if (lstUp.Any())
                {
                    foreach (var item in lstUp)
                    {
                        ExportConstract exportConstract = new ExportConstract();
                        exportConstract.TelDiDong = item.TelMobile;
                        exportConstract.TelCoQuan = item.TelCompany;
                        exportConstract.TelNha = item.TelHome;
                        exportConstract.SoHD = item.Code;
                        exportConstract.TenSanPham = item.ProductName;
                        exportConstract.CMND = item.CustomerCMND;
                        exportConstract.NgaySinh = item.DOB.HasValue ? item.DOB.Value.ToString("dd/MM/yyyy") : "";
                        exportConstract.TrangThaiHD = item.Status;
                        exportConstract.VanPhong = item.VanPhong;
                        exportConstract.NgayKetThucDongPhi = item.NgayKetThucDongPhi.HasValue ? item.NgayKetThucDongPhi.Value.ToString("dd/MM/yyyy") : "";
                        exportConstract.NgayDaoHan = item.NgayDaoHan.HasValue ? item.NgayDaoHan.Value.ToString("dd/MM/yyyy") : "";
                        exportConstract.PhiBaoHiem = item.PhiBaoHiem.HasValue ? item.PhiBaoHiem.Value : 0;

                        if (item.ParentID == null)
                        {
                            exportConstract.ID = item.ID;                           
                            exportConstract.MaKhachHang = item.NMBHCode;
                            
                            if (item.NMBHCode == item.TVVCode)
                            {
                                exportConstract.TenKhachHang = item.CustomerName + " => Đại lý";
                            }
                            else
                            {
                                exportConstract.TenKhachHang = item.CustomerName;
                            }

                           
                            exportConstract.QuanHeVoiBMBH = "NMBH";
                            exportConstract.DiaChiHT = item.DiaChiHienTai;                          
                            exportConstract.DKDP = item.DKDP;                            
                            exportConstract.NgayHieuLuc = item.EffectiveDate.HasValue ? item.EffectiveDate.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract.NgayNopPhiTiep = item.NextPaymentDate.HasValue ? item.NextPaymentDate.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract.NDH = item.NgayDaoHan;
                            exportConstract.GiaTriHoanLai = item.ValueRefundedToAdvance.HasValue ? item.ValueRefundedToAdvance.Value : 0;
                            exportConstract.CacKhoanTamUng = item.AdvancePayment.HasValue ? item.AdvancePayment.Value : 0;
                            exportConstract.RutTruocBT = item.RTBT.HasValue ? item.RTBT.Value : 0;
                            exportConstract.QuyenLoiDK = item.QLDK;
                            exportConstract.HuyHD = item.HuyHD;
                            exportConstract.TVV = item.TVV;
                           
                        }
                        else
                        {
                            exportConstract.MaKhachHang = item.CustomerCode;
                            
                            if (item.NTH == "Y")
                            {
                                exportConstract.TenKhachHang = item.CustomerName + " => NTH";
                            }
                            else
                            {
                                exportConstract.TenKhachHang = item.CustomerName;
                            }                        
                            exportConstract.QuanHeVoiBMBH = item.Relation;                         
                          
                        }
                        result.Add(exportConstract);
                    }
                }
                if (lstDown.Any())
                {
                    foreach (var item in lstUp)
                    {
                        ExportConstract exportConstract = new ExportConstract();
                        exportConstract.TelDiDong = item.TelMobile;
                        exportConstract.TelCoQuan = item.TelCompany;
                        exportConstract.TelNha = item.TelHome;
                        exportConstract.SoHD = item.Code;
                        exportConstract.TenSanPham = item.ProductName;
                        exportConstract.CMND = item.CustomerCMND;
                        exportConstract.NgaySinh = item.DOB.HasValue ? item.DOB.Value.ToString("dd/MM/yyyy") : "";
                        exportConstract.TrangThaiHD = item.Status;
                        exportConstract.VanPhong = item.VanPhong;
                        exportConstract.NgayKetThucDongPhi = item.NgayKetThucDongPhi.HasValue ? item.NgayKetThucDongPhi.Value.ToString("dd/MM/yyyy") : "";
                        exportConstract.NgayDaoHan = item.NgayDaoHan.HasValue ? item.NgayDaoHan.Value.ToString("dd/MM/yyyy") : "";
                        exportConstract.PhiBaoHiem = item.PhiBaoHiem.HasValue ? item.PhiBaoHiem.Value : 0;

                        if (item.ParentID == null)
                        {
                            exportConstract.ID = item.ID;
                            exportConstract.MaKhachHang = item.NMBHCode;

                            if (item.NMBHCode == item.TVVCode)
                            {
                                exportConstract.TenKhachHang = item.CustomerName + " => Đại lý";
                            }
                            else
                            {
                                exportConstract.TenKhachHang = item.CustomerName;
                            }


                            exportConstract.QuanHeVoiBMBH = "NMBH";
                            exportConstract.DiaChiHT = item.DiaChiHienTai;
                            exportConstract.DKDP = item.DKDP;
                            exportConstract.NgayHieuLuc = item.EffectiveDate.HasValue ? item.EffectiveDate.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract.NgayNopPhiTiep = item.NextPaymentDate.HasValue ? item.NextPaymentDate.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract.NDH = item.NgayDaoHan;
                            exportConstract.GiaTriHoanLai = item.ValueRefundedToAdvance.HasValue ? item.ValueRefundedToAdvance.Value : 0;
                            exportConstract.CacKhoanTamUng = item.AdvancePayment.HasValue ? item.AdvancePayment.Value : 0;
                            exportConstract.RutTruocBT = item.RTBT.HasValue ? item.RTBT.Value : 0;
                            exportConstract.QuyenLoiDK = item.QLDK;
                            exportConstract.HuyHD = item.HuyHD;
                            exportConstract.TVV = item.TVV;

                        }
                        else
                        {
                            exportConstract.MaKhachHang = item.CustomerCode;

                            if (item.NTH == "Y")
                            {
                                exportConstract.TenKhachHang = item.CustomerName + " => NTH";
                            }
                            else
                            {
                                exportConstract.TenKhachHang = item.CustomerName;
                            }
                            exportConstract.QuanHeVoiBMBH = item.Relation;

                        }
                        result.Add(exportConstract);
                    }
                }
                */
                return result;
             
             
            }catch(Exception ex)
            {
                throw ex;                
            }
        }

        public  void HieararchyWalk(List<Contract> children, List<Contract> parent,  Dictionary<Contract, List<Contract>> dic, ref List<Contract> lstChildOwner)
        {
            if (children.Any())
            {
                foreach(var itemChildren in children)
                {
                    if (!lstChildOwner.Contains(itemChildren))
                    {
                           //lstData.AddRange(children);
                           lstChildOwner.AddRange(children); 
                    }
               
                    if (parent.Select(m => m.Code).Contains(itemChildren.CustomerCode))
                    {                                                                        
                        var child = parent.Where(m => m.Code == itemChildren.CustomerCode).ToList();
                        foreach(var item in child)
                        {
                            //thang con co trong parent                        
                            if (!lstChildOwner.Contains(item))
                            {
                             //   lstData.Add(item);
                                lstChildOwner.Add(item);
                                var childrentofparent = dic[item];
                                HieararchyWalk(childrentofparent, parent, dic, ref lstChildOwner);
                            }

                        }
                        

                    }

                }
            }

            else
                return;

        }


        public static void HieararchyWalk( List<Contract> children, List<Contract> parent,ref List<Contract> result, Dictionary<Contract,List<Contract>> dic)
        {
            if (children.Any())
            {
                result.AddRange(children);
                foreach (var itemChildren in children)
                {                    
                    if (parent.Select(m => m.Code).Contains(itemChildren.CustomerCode))
                    {
                        //thang con co trong parent                        
                        if (!result.Contains(itemChildren))
                        {
                            var child = parent.Where(m=>m.Code==itemChildren.CustomerCode).ToList();                            
                            HieararchyWalk(child, parent, ref result, dic);
                        }
                       
                    }

                }
            }                

            else
                return;
                           
        }

        private List<ExportConstract> GetData()
        {
            try
            {
                string ttp = txtTTP.Text.Trim();
                string qh = txtQH.Text.Trim();
                string vanphong = "";
                string strWhereVanPhong = string.Empty;
                if (radIgnore.Checked)
                {
                    vanphong = txtIgnore.Text.Trim();
                }
                else if (radFix.Checked)
                {
                    vanphong = txtFix.Text.Trim();
                }

                if (!string.IsNullOrEmpty(vanphong))
                {
                    string[] vs = vanphong.Split(',');

                    if (radFix.Checked)
                    {
                        strWhereVanPhong = string.Format(" and (a.VanPhong like '%{0}%'", vs[0].ToUpper().Trim());
                        for (int i = 1; i < vs.Length; i++)
                        {
                            strWhereVanPhong += string.Format(" or a.VanPhong like '%{0}%'", vs[i].ToUpper().Trim());
                        }

                        strWhereVanPhong += ")";
                    }
                    else if (radIgnore.Checked)
                    {
                        strWhereVanPhong = string.Format(" and ((a.VanPhong not like '%{0}%'", vs[0].ToUpper().Trim());
                        for (int i = 1; i < vs.Length; i++)
                        {
                            strWhereVanPhong += string.Format(" and a.VanPhong not like '%{0}%'", vs[i].ToUpper().Trim());
                        }

                        strWhereVanPhong += ") or a.VanPhong is null ) ";
                    }
                    else
                    {

                    }
                }

                string strWhere2 = string.Empty;
                if (!string.IsNullOrEmpty(loai_bo_hop_dong))
                {
                    string[] vs = loai_bo_hop_dong.Split(',');

                    strWhere2 = "N'" + vs[0].ToUpper().Trim() + "'";
                    for (int i = 1; i < vs.Length; i++)
                    {
                        strWhere2 += ",N'" + vs[i].ToUpper().Trim() + "'";
                    }
                }

                string cmnd = txtCMND.Text.Trim();
                string code = txtCustomerCode.Text.Trim();
                string name = txtCustomerName.Text.Trim();
                string dob = txtDOB.Text.Trim();

                string constractCode = txtConstractCode.Text.Trim();
                string daily = txtDaily.Text.Trim();

                string strSQL = string.Empty;
                strSQL = "Select * from Contract a where DataComplete = 'Y' ";

                if (!string.IsNullOrEmpty(ttp))
                {
                    strSQL += " and a.City like N'%" + ttp.ToUpper() + "%'";
                }

                if (!string.IsNullOrEmpty(qh))
                {
                    strSQL += " and a.District like N'%" + qh.ToUpper() + "%'";
                }

                if (!string.IsNullOrEmpty(cmnd))
                {
                    strSQL += " and a.CustomerCMND = '" + cmnd + "'";
                }

                if (!string.IsNullOrEmpty(code))
                {
                    strSQL += " and a.NMBHCode = '" + code + "'";
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    strSQL += " and a.CustomerName like N'%" + name + "%'";
                }

                if (!string.IsNullOrEmpty(dob))
                {
                    strSQL += " and a.DOB = CONVERT(datetime, '" + dob + "', 103)";
                }

                if (!string.IsNullOrEmpty(constractCode))
                {
                    strSQL += " and a.Code = '" + constractCode + "'";
                }

                if (!string.IsNullOrEmpty(daily))
                {
                    strSQL += " and a.TVVCode = '" + daily + "'";
                }

                if (radFix.Checked || radIgnore.Checked)
                {
                    strSQL += strWhereVanPhong;
                }

                if (!string.IsNullOrEmpty(loai_bo_hop_dong))
                {
                    strSQL += string.Format(" and Status not in ({0})", strWhere2);
                }                

                List<Contract> contracts = db.Contracts.SqlQuery(strSQL).ToList();

                List<ExportConstract> exportConstracts = new List<ExportConstract>();
                List<ExportConstract> exportConstracts_Ret = new List<ExportConstract>();

                foreach (var contract in contracts)
                {
                    ExportConstract exportConstract = new ExportConstract();
                    exportConstract.ID = contract.ID;
                    //exportConstract.CustomerID = customer.ID;
                    exportConstract.MaKhachHang = contract.NMBHCode;
                    exportConstract.CMND = contract.CustomerCMND;
                    if (contract.NMBHCode == contract.TVVCode)
                    {
                        exportConstract.TenKhachHang = contract.CustomerName + " => Đại lý";
                    }
                    else
                    {
                        exportConstract.TenKhachHang = contract.CustomerName;
                    }

                    exportConstract.NgaySinh = contract.DOB.HasValue ? contract.DOB.Value.ToString("dd/MM/yyyy") : "";
                    exportConstract.QuanHeVoiBMBH = "NMBH";
                    exportConstract.DiaChiHT = contract.DiaChiHienTai;
                    exportConstract.TelDiDong = contract.TelMobile;
                    exportConstract.TelCoQuan = contract.TelCompany;
                    exportConstract.TelNha = contract.TelHome;
                    exportConstract.SoHD = contract.Code;
                    exportConstract.TenSanPham = contract.ProductName;
                    //PhiBH                    
                    //List<InsuredPerson> insuredPeopleFee = db.InsuredPersons.Where(a => a.ContractID == contract.ID).ToList();

                    //decimal tong = 0;
                    //if (insuredPeopleFee != null && insuredPeopleFee.Count > 0)
                    //{
                    //    foreach (var x in insuredPeopleFee)
                    //    {
                    //        tong += x.Fee.HasValue ? x.Fee.Value : 0;
                    //    }

                    //    exportConstract.PhiBaoHiem = tong;
                    //}

                    exportConstract.PhiBaoHiem = contract.PhiBaoHiem.HasValue ? contract.PhiBaoHiem.Value : 0;

                    exportConstract.DKDP = contract.DKDP;
                    exportConstract.TrangThaiHD = contract.Status;
                    exportConstract.NgayHieuLuc = contract.EffectiveDate.HasValue ? contract.EffectiveDate.Value.ToString("dd/MM/yyyy") : "";
                    exportConstract.NgayNopPhiTiep = contract.NextPaymentDate.HasValue ? contract.NextPaymentDate.Value.ToString("dd/MM/yyyy") : "";

                    //Contract sanphamChinh = db.Contracts.FirstOrDefault(a => a.Code == contract.Code 
                    //                                                        && a.DataComplete == "C"
                    //                                                        && a.Loai.Trim().Equals("Chính"));
                    //if (sanphamChinh != null)
                    //{
                    exportConstract.NgayKetThucDongPhi = contract.NgayKetThucDongPhi.HasValue ? contract.NgayKetThucDongPhi.Value.ToString("dd/MM/yyyy") : "";
                    exportConstract.NgayDaoHan = contract.NgayDaoHan.HasValue ? contract.NgayDaoHan.Value.ToString("dd/MM/yyyy") : "";
                    exportConstract.NDH = contract.NgayDaoHan;
                    //}

                    //exportConstract.NgayKetThucDongPhi = contract.ke
                    //exportConstract.NgayDaoHan
                    exportConstract.GiaTriHoanLai = contract.ValueRefundedToAdvance.HasValue ? contract.ValueRefundedToAdvance.Value : 0;
                    exportConstract.CacKhoanTamUng = contract.AdvancePayment.HasValue ? contract.AdvancePayment.Value : 0;
                    exportConstract.RutTruocBT = contract.RTBT.HasValue ? contract.RTBT.Value : 0;
                    exportConstract.QuyenLoiDK = contract.QLDK;
                    exportConstract.HuyHD = contract.HuyHD;
                    exportConstract.TVV = contract.TVV;
                    exportConstract.VanPhong = contract.VanPhong;

                    exportConstracts.Add(exportConstract);
                }

                if (exportConstracts.Count > 0)
                {
                    DateTime dateTimeNDH;

                    string[] formats = { "dd/MM/yyyy" };
                    DateTime.TryParseExact(exportDate, formats,
                                            new CultureInfo("en-US"),
                                            DateTimeStyles.None,
                                            out dateTimeNDH);

                    List<ExportConstract> exportConstracts_Up = exportConstracts.Where(a => a.NDH.HasValue && a.NDH.Value >= dateTimeNDH)
                                                                                    .OrderBy(a => a.NDH)
                                                                                    .ThenByDescending(a => a.QuyenLoiDK)
                                                                                    .ThenByDescending(a => a.HuyHD)
                                                                                    .ToList();
                    List<ExportConstract> exportConstracts_Down = exportConstracts.Where(a => a.NDH.HasValue && a.NDH.Value < dateTimeNDH)
                                                                                    .OrderByDescending(a => a.NDH)
                                                                                    .ThenByDescending(a => a.QuyenLoiDK)
                                                                                    .ThenByDescending(a => a.HuyHD)
                                                                                    .ToList();

                    foreach (var constract in exportConstracts_Up)
                    {
                        exportConstracts_Ret.Add(constract);
                        //Lay nguoi duoc bao hiem trong hop dong
                        List<Contract> insuredPeoples = db.Contracts.Where(a => a.Code == constract.SoHD
                                                                                        && a.ParentID == constract.ID).ToList();
                        foreach (var item in insuredPeoples)
                        {
                            ExportConstract exportConstract1 = new ExportConstract();
                            //Customer customer = db.Customers.Find(item.CustomerID);
                            //if (customer != null)
                            //{
                            exportConstract1.MaKhachHang = item.CustomerCode;
                            exportConstract1.CMND = item.CustomerCMND;
                            if (item.NTH == "Y")
                            {
                                exportConstract1.TenKhachHang = item.CustomerName + " => NTH";
                            }
                            else
                            {
                                exportConstract1.TenKhachHang = item.CustomerName;
                            }
                            
                            exportConstract1.NgaySinh = item.DOB.HasValue ? item.DOB.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract1.QuanHeVoiBMBH = item.Relation;
                            //exportConstract1.DiaChiHT = contract.DiaChiHienTai;
                            exportConstract1.TelDiDong = item.TelMobile;
                            exportConstract1.TelCoQuan = item.TelCompany;
                            exportConstract1.TelNha = item.TelHome;
                            exportConstract1.SoHD = item.Code;
                            exportConstract1.TenSanPham = item.ProductName;
                            exportConstract1.TrangThaiHD = item.Status;
                            exportConstract1.PhiBaoHiem = item.PhiBaoHiem.HasValue ? item.PhiBaoHiem.Value : 0;
                            exportConstract1.NgayKetThucDongPhi = item.NgayKetThucDongPhi.HasValue ? item.NgayKetThucDongPhi.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract1.NgayDaoHan = item.NgayDaoHan.HasValue ? item.NgayDaoHan.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract1.VanPhong = item.VanPhong;

                            exportConstracts_Ret.Add(exportConstract1);
                            //}
                        }
                    }

                    foreach (var constract in exportConstracts_Down)
                    {
                        exportConstracts_Ret.Add(constract);
                        //Lay nguoi duoc bao hiem trong hop dong
                        List<Contract> insuredPeoples = db.Contracts.Where(a => a.Code == constract.SoHD
                                                                                        && a.ParentID == constract.ID).ToList();
                        foreach (var item in insuredPeoples)
                        {
                            ExportConstract exportConstract1 = new ExportConstract();
                            //Customer customer = db.Customers.Find(item.CustomerID);
                            //if (customer != null)
                            //{
                            exportConstract1.MaKhachHang = item.CustomerCode;
                            exportConstract1.CMND = item.CustomerCMND;

                            if (item.NTH == "Y")
                            {
                                exportConstract1.TenKhachHang = item.CustomerName + " => NTH";
                            }
                            else
                            {
                                exportConstract1.TenKhachHang = item.CustomerName;
                            }

                            exportConstract1.NgaySinh = item.DOB.HasValue ? item.DOB.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract1.QuanHeVoiBMBH = item.Relation;
                            //exportConstract1.DiaChiHT = contract.DiaChiHienTai;
                            exportConstract1.TelDiDong = item.TelMobile;
                            exportConstract1.TelCoQuan = item.TelCompany;
                            exportConstract1.TelNha = item.TelHome;
                            exportConstract1.SoHD = item.Code;
                            exportConstract1.TenSanPham = item.ProductName;
                            exportConstract1.TrangThaiHD = item.Status;
                            exportConstract1.PhiBaoHiem = item.PhiBaoHiem.HasValue ? item.PhiBaoHiem.Value : 0;
                            exportConstract1.NgayKetThucDongPhi = item.NgayKetThucDongPhi.HasValue ? item.NgayKetThucDongPhi.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract1.NgayDaoHan = item.NgayDaoHan.HasValue ? item.NgayDaoHan.Value.ToString("dd/MM/yyyy") : "";
                            exportConstract1.VanPhong = item.VanPhong;

                            exportConstracts_Ret.Add(exportConstract1);
                            //}
                        }
                    }
                }

                return exportConstracts_Ret;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return new List<ExportConstract>();
            }
        }

        private void setRowNumber(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = (row.Index + 1).ToString();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            
                     
                        if (dataGridView.RowCount <= 0)
                        {
                            MessageBox.Show("Không có dữ liệu.", "thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        var dialog = new System.Windows.Forms.SaveFileDialog();
                        dialog.DefaultExt = "*.xls";
                        dialog.Filter = "Excel file (*.xls)|*.xls";
                        System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            try
                            {
                                List<ExportConstract> dataGrid = dataGridView.DataSource as List<ExportConstract>;

                    List<ExportContractExcel> dataExcel =
                                    Mapper.Map<List<ExportContractExcel>>(dataGrid);

                    ExcelLibrary.DataSetHelper.CreateWorkbook(dialog.FileName, dataExcel);
                                MessageBox.Show("Export Complete");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("ERROR :" + ex.Message);
                            }
                        }

            /*
                        Cursor.Current = Cursors.WaitCursor;

                        Excel.Application myExcelApp = new Excel.Application();
                        try
                        {                
                            string path = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                            string fullPath = path + @"\ExportConstract.xlsx";
                            if (!File.Exists(fullPath))
                            {
                                MessageBox.Show("Không tìm thấy file mẫu export", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            //List<ExportConstract> listData = GetData(txtTTP.Text, txtQH.Text);

                            if (dataGridView.RowCount > 0)
                            {                    
                                Excel.Workbooks myExcelWorkbooks;
                                Excel.Workbook myExcelWorkbook;
                                Excel.Worksheet worksheet;

                                myExcelApp.Visible = true;
                                myExcelWorkbooks = myExcelApp.Workbooks;
                                myExcelWorkbook = myExcelWorkbooks.Open(fullPath, ReadOnly: true);
                                worksheet = myExcelWorkbook.ActiveSheet;

                                //END

                                //ADD data to Excel
                                int i = 2;
                                int countRow = dataGridView.Rows.Count;
                                foreach (DataGridViewRow item in dataGridView.Rows)
                                {
                                    worksheet.Cells[i, 1] = (i - 1); //STT
                                    worksheet.Cells[i, 2] = item.Cells["MaKhachHang"].Value;// .Code; //Code
                                    worksheet.Cells[i, 3] = item.Cells["CMND"].Value; //Code
                                    worksheet.Cells[i, 4] = item.Cells["TenKhachHang"].Value; //Code
                                    worksheet.Cells[i, 5] = item.Cells["NgaySinh"].Value; //Code
                                    worksheet.Cells[i, 6] = item.Cells["QuanHeVoiBMBH"].Value; //Code
                                    worksheet.Cells[i, 7] = item.Cells["DiaChiHT"].Value; //Code
                                    worksheet.Cells[i, 8] = item.Cells["TelDiDong"].Value; //Code
                                    worksheet.Cells[i, 9] = item.Cells["TelCoQuan"].Value; //Code
                                    worksheet.Cells[i, 10] = item.Cells["TelNha"].Value; //Code
                                    worksheet.Cells[i, 11] = item.Cells["SoHD"].Value; //Code
                                    worksheet.Cells[i, 12] = item.Cells["TenSanPham"].Value; //Code
                                    worksheet.Cells[i, 13] = item.Cells["PhiBaoHiem"].Value; //Code
                                    worksheet.Cells[i, 14] = item.Cells["DKDP"].Value; //Code
                                    worksheet.Cells[i, 15] = item.Cells["TrangThaiHD"].Value; //Code
                                    worksheet.Cells[i, 16] = item.Cells["NgayHieuLuc"].Value; //Code
                                    worksheet.Cells[i, 17] = item.Cells["NgayNopPhiTiep"].Value; //Code
                                    worksheet.Cells[i, 18] = item.Cells["NgayKetThucDongPhi"].Value; //Code
                                    worksheet.Cells[i, 19] = item.Cells["NgayDaoHan"].Value; //Code
                                    worksheet.Cells[i, 20] = item.Cells["GiaTriHoanLai"].Value; //Code
                                    worksheet.Cells[i, 21] = item.Cells["CacKhoanTamUng"].Value; //Code
                                    worksheet.Cells[i, 22] = item.Cells["RutTruocBT"].Value; //Code
                                    worksheet.Cells[i, 23] = item.Cells["QuyenLoiDK"].Value; //Code
                                    worksheet.Cells[i, 24] = item.Cells["HuyHD"].Value; //Code
                                    worksheet.Cells[i, 25] = item.Cells["TVV"].Value; //Code
                                    worksheet.Cells[i, 26] = item.Cells["VanPhong"].Value; //Code
                                    i++;

                                    log.Debug(string.Format("Write: {0}/{1}", i, countRow));
                                }

                                MessageBox.Show("Xuất file thành công.", "thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                try
                                {
                                    //Update ExportDate
                                    Config config = db.Configs.FirstOrDefault(a => a.Name.ToLower().Equals("exportdate"));
                                    if (config != null)
                                    {
                                        config.Value = DateTime.Now.ToString("dd/MM/yyyy");
                                        db.Entry(config).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        exportDate = config.Value;
                                    }
                                    else
                                    {
                                        config = new Config();
                                        config.Name = "ExportDate";
                                        config.Value = DateTime.Now.ToString("dd/MM/yyyy");
                                        db.Configs.Add(config);
                                        db.SaveChanges();
                                        exportDate = config.Value;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }                    
                            }
                            else
                            {
                                MessageBox.Show("Không có dữ liệu.", "thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }                

                            Cursor.Current = Cursors.Default;
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.Message);
                            myExcelApp.Quit();
                            Cursor.Current = Cursors.Default;
                            MessageBox.Show("Xuất file Lỗi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            KillExcel(myExcelApp);
                            System.Threading.Thread.Sleep(100);
                        }


                        Cursor.Current = Cursors.Default;
                        */
                       
        }

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);
        private static void KillExcel(Excel.Application theApp)
        {
            int id = 0;
            IntPtr intptr = new IntPtr(theApp.Hwnd);
            System.Diagnostics.Process p = null;
            try
            {
                GetWindowThreadProcessId(intptr, out id);
                p = System.Diagnostics.Process.GetProcessById(id);
                if (p != null)
                {
                    //p.Kill();
                    p.Dispose();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void radIgnore_CheckedChanged(object sender, EventArgs e)
        {
            if (radIgnore.Checked)
            {
                txtIgnore.Enabled = true;

                txtFix.Enabled = false;
            }
            else if (radFix.Checked)
            {
                txtFix.Enabled = true;

                txtIgnore.Enabled = false;
            }
            else
            {
                txtFix.Enabled = false;
                txtIgnore.Enabled = false;
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(pathExportCsv))
            {
                Cursor.Current = Cursors.WaitCursor;

                if (!System.IO.Directory.Exists(pathExportCsv))
                {
                    System.IO.Directory.CreateDirectory(pathExportCsv);
                }

                string fileName = string.Format("ExportConstract{0}.csv", DateTime.Now.ToString("yyyyMMddhhmmss"));
                string fullPath = System.IO.Path.Combine(pathExportCsv, fileName);

                StringBuilder strContent = new StringBuilder();
                //Head
                //strContent = "No.,Mã Số KH,Số CMND/GPKD,Họ và Tên KH,Ngày Sinh,Quan hệ với BMBH,Địa Chỉ,Hiện Tại,Tel Di Động,Tel Cơ Quan,Tel Nhà,Số Hợp Đồng,Tên Sản Phẩm,Phí Bảo Hiểm,Định Kỳ Đóng Phí,Trạng Thái HĐ,Ngày Hiệu Lực,Ngày Nộp Phí Tiếp Theo,Ngày Kết Thúc Đóng Phí,Ngày Đáo Hạn,Giá Trị HL Để T/Ứ,Các Khoản T/Ứ,Rút Trước BT,Quyền Lợi ĐK,Hủy HĐ,TVV,Văn Phòng";
                //strContent += "\r\n";
                strContent.Append("No.,Mã Số KH,Số CMND/GPKD,Họ và Tên KH,Ngày Sinh");
                strContent.Append(",Quan hệ với BMBH,Địa Chỉ Hiện Tại,Tel Di Động,Tel Cơ Quan");
                strContent.Append(",Tel Nhà,Số Hợp Đồng,Tên Sản Phẩm,Phí Bảo Hiểm,Định Kỳ Đóng Phí");
                strContent.Append(",Trạng Thái HĐ,Ngày Hiệu Lực,Ngày Nộp Phí Tiếp Theo,Ngày Kết Thúc Đóng Phí,Ngày Đáo Hạn");
                strContent.Append(",Giá Trị HL Để T/Ứ,Các Khoản T/Ứ,Rút Trước BT,Quyền Lợi ĐK,Hủy HĐ");
                strContent.Append(",TVV,Văn Phòng");
                strContent.Append("\r\n");

                //Excel.Application myExcelApp = new Excel.Application();
                try
                {
                    //string path = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                    //string fullPath = path + @"\ExportConstract.xlsx";
                    //if (!File.Exists(fullPath))
                    //{
                    //    MessageBox.Show("Không tìm thấy file mẫu export", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    return;
                    //}

                    //List<ExportConstract> listData = GetData(txtTTP.Text, txtQH.Text);

                    if (dataGridView.RowCount > 0)
                    {
                        //Excel.Workbooks myExcelWorkbooks;
                        //Excel.Workbook myExcelWorkbook;
                        //Excel.Worksheet worksheet;

                        //myExcelApp.Visible = true;
                        //myExcelWorkbooks = myExcelApp.Workbooks;
                        //myExcelWorkbook = myExcelWorkbooks.Open(fullPath, ReadOnly: true);
                        //worksheet = myExcelWorkbook.ActiveSheet;

                        //END

                        //ADD data to Excel
                        int i = 1;
                        int countRow = dataGridView.Rows.Count;
                        object tmp;
                        foreach (DataGridViewRow item in dataGridView.Rows)
                        {
                            //worksheet.Cells[i, 1] = (i - 1); //STT
                            //worksheet.Cells[i, 2] = item.Cells["MaKhachHang"].Value;// .Code; //Code
                            //worksheet.Cells[i, 3] = item.Cells["CMND"].Value; //Code
                            //worksheet.Cells[i, 4] = item.Cells["TenKhachHang"].Value; //Code
                            //worksheet.Cells[i, 5] = item.Cells["NgaySinh"].Value; //Code
                            //worksheet.Cells[i, 6] = item.Cells["QuanHeVoiBMBH"].Value; //Code
                            //worksheet.Cells[i, 7] = item.Cells["DiaChiHT"].Value; //Code
                            //worksheet.Cells[i, 8] = item.Cells["TelDiDong"].Value; //Code
                            //worksheet.Cells[i, 9] = item.Cells["TelCoQuan"].Value; //Code
                            //worksheet.Cells[i, 10] = item.Cells["TelNha"].Value; //Code
                            //worksheet.Cells[i, 11] = item.Cells["SoHD"].Value; //Code
                            //worksheet.Cells[i, 12] = item.Cells["TenSanPham"].Value; //Code
                            //worksheet.Cells[i, 13] = item.Cells["PhiBaoHiem"].Value; //Code
                            //worksheet.Cells[i, 14] = item.Cells["DKDP"].Value; //Code
                            //worksheet.Cells[i, 15] = item.Cells["TrangThaiHD"].Value; //Code
                            //worksheet.Cells[i, 16] = item.Cells["NgayHieuLuc"].Value; //Code
                            //worksheet.Cells[i, 17] = item.Cells["NgayNopPhiTiep"].Value; //Code
                            //worksheet.Cells[i, 18] = item.Cells["NgayKetThucDongPhi"].Value; //Code
                            //worksheet.Cells[i, 19] = item.Cells["NgayDaoHan"].Value; //Code
                            //worksheet.Cells[i, 20] = item.Cells["GiaTriHoanLai"].Value; //Code
                            //worksheet.Cells[i, 21] = item.Cells["CacKhoanTamUng"].Value; //Code
                            //worksheet.Cells[i, 22] = item.Cells["RutTruocBT"].Value; //Code
                            //worksheet.Cells[i, 23] = item.Cells["QuyenLoiDK"].Value; //Code
                            //worksheet.Cells[i, 24] = item.Cells["HuyHD"].Value; //Code
                            //worksheet.Cells[i, 25] = item.Cells["TVV"].Value; //Code
                            //worksheet.Cells[i, 26] = item.Cells["VanPhong"].Value; //Code                            
                            //===============================================

                            //===============================================
                            strContent.Append(i); //STT
                            strContent.Append("," + item.Cells["MaKhachHang"].Value);//Code
                            strContent.Append("," + item.Cells["CMND"].Value); //Code
                            tmp = item.Cells["TenKhachHang"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }
                            
                            strContent.Append("," + item.Cells["NgaySinh"].Value); //Code
                            strContent.Append("," + item.Cells["QuanHeVoiBMBH"].Value); //Code
                            tmp = item.Cells["DiaChiHT"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }

                            tmp = item.Cells["TelDiDong"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }
                            //strContent.Append("," + item.Cells["TelDiDong"].Value); //Code

                            tmp = item.Cells["TelCoQuan"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }
                            //strContent.Append("," + item.Cells["TelCoQuan"].Value); //Code

                            tmp = item.Cells["TelNha"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }
                            //strContent.Append("," + item.Cells["TelNha"].Value); //Code
                            strContent.Append("," + item.Cells["SoHD"].Value); //Code
                            tmp = item.Cells["TenSanPham"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }
                            
                            strContent.Append("," + item.Cells["PhiBaoHiem"].Value); //Code
                            strContent.Append("," + item.Cells["DKDP"].Value); //Code
                            tmp = item.Cells["TrangThaiHD"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }
                            
                            strContent.Append("," + item.Cells["NgayHieuLuc"].Value); //Code
                            strContent.Append("," + item.Cells["NgayNopPhiTiep"].Value); //Code
                            strContent.Append("," + item.Cells["NgayKetThucDongPhi"].Value); //Code
                            strContent.Append("," + item.Cells["NgayDaoHan"].Value); //Code
                            strContent.Append("," + item.Cells["GiaTriHoanLai"].Value); //Code
                            strContent.Append("," + item.Cells["CacKhoanTamUng"].Value); //Code
                            strContent.Append("," + item.Cells["RutTruocBT"].Value); //Code
                            strContent.Append("," + item.Cells["QuyenLoiDK"].Value); //Code
                            strContent.Append("," + item.Cells["HuyHD"].Value); //Code
                            tmp = item.Cells["TVV"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }
                            
                            tmp = item.Cells["VanPhong"].Value;
                            if (tmp != null)
                            {
                                strContent.Append("," + tmp.ToString().Replace(',', '-')); //Code
                            }
                            else
                            {
                                strContent.Append(",");
                            }
                            
                            strContent.Append("\r\n");
                            //log.Debug(string.Format("Write: {0}/{1}", i, countRow));

                            i++;                            
                        }

                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fullPath, false, Encoding.Unicode))
                        {
                            // Write the stringbuilder text to the the file.
                            sw.WriteLine(strContent);
                        }

                        MessageBox.Show("Xuất file thành công.", "thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        try
                        {
                            //Update ExportDate
                            Config config = db.Configs.FirstOrDefault(a => a.Name.ToLower().Equals("exportdate"));
                            if (config != null)
                            {
                                config.Value = DateTime.Now.ToString("dd/MM/yyyy");
                                db.Entry(config).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                exportDate = config.Value;
                            }
                            else
                            {
                                config = new Config();
                                config.Name = "ExportDate";
                                config.Value = DateTime.Now.ToString("dd/MM/yyyy");
                                db.Configs.Add(config);
                                db.SaveChanges();
                                exportDate = config.Value;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có dữ liệu.", "thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //return;
                    }

                    //Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    //myExcelApp.Quit();
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("Xuất file Lỗi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    //KillExcel(myExcelApp);
                    //System.Threading.Thread.Sleep(100);
                }

                Cursor.Current = Cursors.Default;
            }
            else
            {
                MessageBox.Show("Chưa thiết lập đường dẫn export file.", "Thông báo");
            }            
        }


        public ExportConstract ConvertContractToExportContract(Contract item)
        {
            ExportConstract exportConstract = new ExportConstract();
            exportConstract.TelDiDong = item.TelMobile;
            exportConstract.TelCoQuan = item.TelCompany;
            exportConstract.TelNha = item.TelHome;
            exportConstract.SoHD = item.Code;
            exportConstract.TenSanPham = item.ProductName;
            exportConstract.CMND = item.CustomerCMND;
            exportConstract.NgaySinh = item.DOB.HasValue ? item.DOB.Value.ToString("dd/MM/yyyy") : "";
            exportConstract.TrangThaiHD = item.Status;
            exportConstract.VanPhong = item.VanPhong;
            exportConstract.NgayKetThucDongPhi = item.NgayKetThucDongPhi.HasValue ? item.NgayKetThucDongPhi.Value.ToString("dd/MM/yyyy") : "";
            exportConstract.NgayDaoHan = item.NgayDaoHan.HasValue ? item.NgayDaoHan.Value.ToString("dd/MM/yyyy") : "";
            exportConstract.PhiBaoHiem = item.PhiBaoHiem.HasValue ? item.PhiBaoHiem.Value : 0;

            if (item.ParentID == null)
            {
                exportConstract.ID = item.ID;
                exportConstract.MaKhachHang = item.NMBHCode;

                if (item.NMBHCode == item.TVVCode)
                {
                    exportConstract.TenKhachHang = item.CustomerName + " => Đại lý";
                }
                else
                {
                    exportConstract.TenKhachHang = item.CustomerName;
                }


                exportConstract.QuanHeVoiBMBH = "NMBH";
                exportConstract.DiaChiHT = item.DiaChiHienTai;
                exportConstract.DKDP = item.DKDP;
                exportConstract.NgayHieuLuc = item.EffectiveDate.HasValue ? item.EffectiveDate.Value.ToString("dd/MM/yyyy") : "";
                exportConstract.NgayNopPhiTiep = item.NextPaymentDate.HasValue ? item.NextPaymentDate.Value.ToString("dd/MM/yyyy") : "";
                exportConstract.NDH = item.NgayDaoHan;
                exportConstract.GiaTriHoanLai = item.ValueRefundedToAdvance.HasValue ? item.ValueRefundedToAdvance.Value : 0;
                exportConstract.CacKhoanTamUng = item.AdvancePayment.HasValue ? item.AdvancePayment.Value : 0;
                exportConstract.RutTruocBT = item.RTBT.HasValue ? item.RTBT.Value : 0;
                exportConstract.QuyenLoiDK = item.QLDK;
                exportConstract.HuyHD = item.HuyHD;
                exportConstract.TVV = item.TVV;

            }
            else
            {
                exportConstract.MaKhachHang = item.CustomerCode;

                if (item.NTH == "Y")
                {
                    exportConstract.TenKhachHang = item.CustomerName + " => NTH";
                }
                else
                {
                    exportConstract.TenKhachHang = item.CustomerName;
                }
                exportConstract.QuanHeVoiBMBH = item.Relation;

            }

            return exportConstract;
        }
    }



    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SelectRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            foreach (var parent in source)
            {
                yield return parent;

                var children = selector(parent);
                foreach (var child in SelectRecursive(children, selector))
                    yield return child;
            }
        }
    }


    
}
