using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class ExportCustomerData : Form
    {
        private PruDataEntities db = new PruDataEntities();

        private string loai_Bo_Van_Phong = string.Empty;
        private string chi_Dinh_Van_Phong = string.Empty;

        public ExportCustomerData()
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

            List<Config> config = db.Configs.ToList(); //FirstOrDefault(a => a.Name.ToLower().Equals("loai_bo_van_phong"));
            if (config.Count > 0)
            {
                foreach(var item in config)
                {
                    if (item.Name.ToLower().Equals("loai_bo_van_phong"))
                    {
                        loai_Bo_Van_Phong = item.Value.Trim();
                    }

                    if (item.Name.ToLower().Equals("chi_dinh_van_phong"))
                    {
                        chi_Dinh_Van_Phong = item.Value.Trim();
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
            
            //dataGridView.Columns.Clear();
            
            var cusCode = new DataGridViewTextBoxColumn();
            cusCode.HeaderText = "Mã khách hàng";
            cusCode.Name = "Code";
            cusCode.Visible = true;
            cusCode.DataPropertyName = "Code";
            cusCode.Width = 100;
            cusCode.DisplayIndex = 1;
            dataGridView.Columns.Add(cusCode);

            var cmnd = new DataGridViewTextBoxColumn();
            cmnd.HeaderText = "Số CMND/GPKD";
            cmnd.Name = "CMND";
            cmnd.Visible = true;
            cmnd.DataPropertyName = "CMND";
            cmnd.Width = 100;
            cmnd.DisplayIndex = 2;
            dataGridView.Columns.Add(cmnd);

            var fullName = new DataGridViewTextBoxColumn();
            fullName.HeaderText = "Họ và Tên KH";
            fullName.Name = "Name";
            fullName.Visible = true;
            fullName.Width = 300;
            fullName.DataPropertyName = "Name";
            fullName.DisplayIndex = 3;
            dataGridView.Columns.Add(fullName);

            var dob = new DataGridViewTextBoxColumn();
            dob.HeaderText = "Ngày Sinh";
            dob.Name = "DOB";
            dob.Visible = true;
            dob.DataPropertyName = "DOB";
            dob.Width = 150;
            dob.DefaultCellStyle.Format = "dd/MM/yyyy";
            dob.DisplayIndex = 4;
            dataGridView.Columns.Add(dob);

            var gender = new DataGridViewTextBoxColumn();
            gender.HeaderText = "Giới Tính";
            gender.Name = "Gender";
            gender.Visible = true;
            gender.DataPropertyName = "Gender";
            //dob.Width = 150;
            //dob.DefaultCellStyle.Format = "dd/MM/yyyy";
            gender.DisplayIndex = 5;
            dataGridView.Columns.Add(gender);

            var address = new DataGridViewTextBoxColumn();
            address.HeaderText = "Địa chỉ";
            address.Visible = true;
            address.Name = "Address";
            address.Width = 300;
            address.DataPropertyName = "Address";
            address.DisplayIndex = 6;
            dataGridView.Columns.Add(address);

            var tp = new DataGridViewTextBoxColumn();
            tp.HeaderText = "Tỉnh/TP";
            tp.Name = "City";
            tp.Visible = true;
            tp.Width = 300;
            tp.DataPropertyName = "City";
            tp.DisplayIndex = 7;
            dataGridView.Columns.Add(tp);

            var qh = new DataGridViewTextBoxColumn();
            qh.HeaderText = "Quận/Huyện";
            qh.Name = "District";
            qh.Visible = true;
            qh.Width = 300;
            qh.DataPropertyName = "District";
            qh.DisplayIndex = 8;
            dataGridView.Columns.Add(qh);

            var phone = new DataGridViewTextBoxColumn();
            phone.HeaderText = "Điện thoại";
            phone.Name = "Phone";
            phone.Visible = true;
            phone.DataPropertyName = "Phone";
            phone.DisplayIndex = 9;
            dataGridView.Columns.Add(phone);

            var telCompany = new DataGridViewTextBoxColumn();
            telCompany.HeaderText = "ĐT Công ty";
            telCompany.Name = "TelCompany";
            telCompany.Visible = true;
            telCompany.DataPropertyName = "TelCompany";
            telCompany.DisplayIndex = 10;
            dataGridView.Columns.Add(telCompany);

            var telHome = new DataGridViewTextBoxColumn();
            telHome.HeaderText = "ĐT Nhà";
            telHome.Name = "TelHome";
            telHome.Visible = true;
            telHome.DataPropertyName = "TelHome";
            telHome.DisplayIndex = 11;
            dataGridView.Columns.Add(telHome);

            var email = new DataGridViewTextBoxColumn();
            email.HeaderText = "Email";
            email.Name = "Email";
            email.Visible = true;
            email.Width = 300;
            email.DataPropertyName = "Email";
            email.DisplayIndex = 12;
            dataGridView.Columns.Add(email);

            var vanphong = new DataGridViewTextBoxColumn();
            vanphong.HeaderText = "Văn Phòng";
            vanphong.Name = "VanPhong";
            vanphong.Visible = true;
            vanphong.Width = 300;
            vanphong.DataPropertyName = "VanPhong";
            vanphong.DisplayIndex = 13;
            dataGridView.Columns.Add(vanphong);

            var colID = new DataGridViewTextBoxColumn();
            colID.HeaderText = "ID";
            colID.Visible = false;
            colID.DataPropertyName = "ID";
            colID.DisplayIndex = 14;
            dataGridView.Columns.Add(colID);

            var createDate = new DataGridViewTextBoxColumn();
            createDate.HeaderText = "CreateDate";
            createDate.Visible = false;
            createDate.DataPropertyName = "CreateDate";
            createDate.DisplayIndex = 15;
            dataGridView.Columns.Add(createDate);

            var updateDate = new DataGridViewTextBoxColumn();
            updateDate.HeaderText = "UpdateDate";
            updateDate.Visible = false;
            updateDate.DisplayIndex = 16;
            updateDate.DataPropertyName = "UpdateDate";
            dataGridView.Columns.Add(updateDate);

            var deleteFlg = new DataGridViewTextBoxColumn();
            deleteFlg.HeaderText = "DeleteFlg";
            deleteFlg.Visible = false;
            deleteFlg.DisplayIndex = 17;
            deleteFlg.DataPropertyName = "DeleteFlg";
            dataGridView.Columns.Add(deleteFlg);

            var existFlg = new DataGridViewTextBoxColumn();
            existFlg.HeaderText = "ExistContractFlg";
            existFlg.Visible = false;
            existFlg.DisplayIndex = 18;
            existFlg.DataPropertyName = "ExistContractFlg";
            dataGridView.Columns.Add(existFlg);

            //dgv.EnableHeadersVisualStyles = false;
            //dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(224, 224, 224);

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
                }
                else
                {
                    btnExport.Enabled = false;
                }

                Cursor.Current = Cursors.Default;
            }
            catch(Exception ex)
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void BindGrid()
        {
            try
            {
                //dataGridView.cle
                //dataGridView.Columns.Clear();
                dataGridView.DataSource = GetData();
                dataGridView.Columns["Code"].DisplayIndex = 1;
                dataGridView.Columns["CMND"].DisplayIndex = 2;
                dataGridView.Columns["DOB"].DisplayIndex = 3;
                dataGridView.Columns["Gender"].DisplayIndex = 4;
                dataGridView.Columns["Address"].DisplayIndex = 5;
                dataGridView.Columns["City"].DisplayIndex = 6;
                dataGridView.Columns["District"].DisplayIndex = 7;
                dataGridView.Columns["Phone"].DisplayIndex = 8;
                dataGridView.Columns["TelCompany"].DisplayIndex = 9;
                dataGridView.Columns["TelHome"].DisplayIndex = 10;
                dataGridView.Columns["Email"].DisplayIndex = 11;
                dataGridView.Columns["VanPhong"].DisplayIndex = 12;

                //dataGridView.Columns[""].DisplayIndex = 10;
                //dataGridView.DataSource = db.Customers.ToList();
                setRowNumber(dataGridView);
            }
            catch(Exception ex)
            {

            }
        }

        private List<Customer> GetData()
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

                string cmnd = txtCMND.Text.Trim();
                string code = txtCode.Text.Trim();
                string name = txtName.Text.Trim();
                string dob = txtDOB.Text.Trim();

                string strSQL = "Select distinct * from Customer a where 1 = 1 ";
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
                    strSQL += " and a.CMND = '" + cmnd + "'";
                }

                if (!string.IsNullOrEmpty(code))
                {
                    strSQL += " and a.Code = '" + code + "'";
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    strSQL += " and a.Name like N'%" + name + "%'";
                }

                if (!string.IsNullOrEmpty(dob))
                {
                    strSQL += " and a.DOB = CONVERT(datetime, '" + dob + "', 103)";
                }

                if (radFix.Checked || radIgnore.Checked)
                {
                    strSQL += strWhereVanPhong;   
                }                

                strSQL += " and a.ExistContractFlg = 'Y' ";
                strSQL += " order by a.Address,a.DOB ";

                List<Customer> listCustomer = db.Database.SqlQuery<Customer>(strSQL).ToList();
                /*
                if (!string.IsNullOrEmpty(vanphong) && (radIgnore.Checked || radFix.Checked))
                {
                    List<Customer> listRet = new List<Customer>();

                    string[] vs = vanphong.Split(',');

                    string strWhere = "'" + vs[0].ToUpper().Trim() + "'";
                    for (int i = 1; i < vs.Length; i++)
                    {
                        strWhere += ",'" + vs[i].ToUpper().Trim() + "'";
                    }

                    string strSQLConstract = "select * from Contract where VanPhong in (" + strWhere + ")";
                    List<Contract> contracts = db.Contracts.SqlQuery(strSQLConstract).ToList();

                    if (radIgnore.Checked)
                    {
                        foreach (var item in listCustomer)
                        {
                            if (!IsExist(item, vanphong, contracts))
                            {
                                listRet.Add(item);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in listCustomer)
                        {
                            if (IsExist(item, vanphong, contracts))
                            {
                                listRet.Add(item);
                            }
                        }
                    }

                    return listRet;
                }
                */
                return listCustomer;                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool IsExist(Customer item, string loaibo, List<Contract> contracts)
        {
            try
            {                
                if (contracts.Count > 0)
                {
                    foreach(var constract in contracts)
                    {
                        List<InsuredPerson> insuredPeople = db.InsuredPersons.Where(a => a.ContractID == constract.ID).ToList();
                        if (insuredPeople.Count > 0)
                        {
                            foreach(var x in insuredPeople)
                            {
                                if (x.CustomerID == item.ID)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch(Exception ex)
            {
                return false;
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
            Cursor.Current = Cursors.WaitCursor;
            Excel.Application myExcelApp = new Excel.Application();
            try
            {
                string path = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                string fullPath = path + @"\ExportCustomer.xlsx";
                if (!File.Exists(fullPath))
                {
                    MessageBox.Show("Không tìm thấy file mẫu export", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

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

                    foreach (DataGridViewRow item in dataGridView.Rows)
                    {
                        worksheet.Cells[i, 1] = (i-1); //STT
                        worksheet.Cells[i, 2] = item.Cells["Code"].Value;// item.Code; //Code
                        worksheet.Cells[i, 3] = item.Cells["CMND"].Value;// item.CMND; //Code
                        worksheet.Cells[i, 4] = item.Cells["Name"].Value;  //item.Name; //Code
                        worksheet.Cells[i, 5] = item.Cells["DOB"].Value; //.HasValue ? item.DOB.Value.ToString("dd/MM/yyyy") : ""; //Code
                        worksheet.Cells[i, 6] = item.Cells["Gender"].Value; //.HasValue ? item.DOB.Value.ToString("dd/MM/yyyy") : ""; //Code
                        worksheet.Cells[i, 7] = item.Cells["Address"].Value; //item.District; //Code
                        worksheet.Cells[i, 8] = item.Cells["City"].Value;  //item.City; //Code
                        worksheet.Cells[i, 9] = item.Cells["District"].Value;  //item.City; //Code
                        worksheet.Cells[i, 10] = item.Cells["Phone"].Value;  //item.City; //Code
                        worksheet.Cells[i, 11] = item.Cells["TelCompany"].Value;  //item.City; //Code
                        worksheet.Cells[i, 12] = item.Cells["TelHome"].Value;  //item.City; //Code
                        worksheet.Cells[i, 13] = item.Cells["Email"].Value;  //item.City; //Code
                        worksheet.Cells[i, 14] = item.Cells["VanPhong"].Value;  //item.City; //Code

                        i++;
                    }

                    MessageBox.Show("Xuất file thành công.", "thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                //Logging.LogError(ex);
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
    }
}
