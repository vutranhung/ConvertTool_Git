using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace ConvertToolApp
{
    public partial class GetContractData : Form
    {
        private string machineNo = ConfigurationManager.AppSettings["MachineNo"];
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string UrlConstractInfo = "https://prugreat.prudential.com.vn/prugreat/ga/history/#/constractInfo";

        private PruDataEntities db = new PruDataEntities();
        private int recordFound = 0;
        //private int recordSave = 0;
        //private int recordIngore = 0;
        //private int recordUpdate = 0;

        private bool isProcessComplet = true;
        private bool isStopGetData = true;

        private DateTime dtStart;
        private DateTime dtEnd;

        private DateTime dtStartLoadWebsite;
        private DateTime dtEndLoadWebsite;

        private DateTime dtSearch;

        private string NguoiMuaBaoHiemCode = string.Empty;
        private string HDStatus = string.Empty;

        private Contract contractTAM = new Contract();

        private enum ProcessStatus
        {
            Init = 0,
            Constract_Search_Start = 1,
            Constract_Search_Processing = 2,
            
            Constract_Tab_Phi_Processing = 3,
            Constract_Tab_SanPham_Processing = 4,
            Constract_Tab_Daily_Processing = 5,
            Constract_Tab_GiaTriTienMat_Processing = 6,
            Constract_Tab_NguoiThuHuong_Processing = 7,
            Constract_Tab_DiaChiHienTai_Processing = 8,
        }

        private ProcessStatus processStatus;


        public GetContractData()
        {
            InitializeComponent();
        }

        private void btnSearchContract_Click(object sender, EventArgs e)
        {
            //So hop dong: 74198246
            try
            {
                if (webBrowser.ReadyState == WebBrowserReadyState.Complete)
                {
                    HtmlElement textbox = webBrowser.Document.GetElementById("policyNum");
                    if (textbox != null)
                    {
                        textbox.InnerText = "74198246";
                        textbox.Focus();
                    }

                    HtmlElementCollection buttons = webBrowser.Document.GetElementsByTagName("button");
                    foreach (HtmlElement button in buttons)
                    {
                        if (button.GetAttribute("ng-click") == "vm.searchByConstractNum()")
                        {
                            button.Focus();
                            button.InvokeMember("Click");

                            break;                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        private bool CheckWebsiteLoadingTab(string tab)
        {
            try
            {
                if (tab == "phi" || tab == "sanpham" || tab == "giatritienmat" || tab == "diachihientai")
                {
                    HtmlElementCollection elements = webBrowser.Document.GetElementsByTagName("input");
                    //HtmlElement item = getElementByClass("loading", elements);
                    foreach (HtmlElement input in elements)
                    {
                        if (tab == "phi")
                        {
                            string att = input.GetAttribute("ng-model");
                            string value = input.GetAttribute("value");
                            if (!string.IsNullOrEmpty(att) && att == "vm.history.frequencyString" && !string.IsNullOrEmpty(value))
                            {
                                return false;
                            }
                        }

                        if (tab == "sanpham")
                        {
                            string att = input.GetAttribute("ng-model");
                            string value = input.GetAttribute("value");
                            if (!string.IsNullOrEmpty(att) && att == "product.id.productName" && !string.IsNullOrEmpty(value))
                            {
                                return false;
                            }
                        }

                        if (tab == "giatritienmat")
                        {
                            string att = input.GetAttribute("ng-model");
                            string value = input.GetAttribute("value");
                            if (!string.IsNullOrEmpty(att) && att == "vm.history.cash.netSvAmount" && !string.IsNullOrEmpty(value))
                            {
                                return false;
                            }
                        }

                        if (tab == "diachihientai")
                        {
                            string att = input.GetAttribute("ng-model");
                            string value = input.GetAttribute("value");
                            if (!string.IsNullOrEmpty(att) && att == "vm.history.client.clientAddressProvince" && !string.IsNullOrEmpty(value))
                            {
                                return false;
                            }
                        }
                    }                    
                }

                if (tab == "nguoithuhuong" || tab == "daily")
                {
                    HtmlElementCollection elements = webBrowser.Document.GetElementsByTagName("div");
                    foreach(HtmlElement div in elements)
                    {
                        string att = div.GetAttribute("ui-view");
                        if (!string.IsNullOrEmpty(att) && att == "partial")
                        {
                            HtmlElementCollection tds = div.GetElementsByTagName("td");
                            if (tds.Count > 0)
                            {
                                return false;
                            }
                            else
                            {
                                //if (HDStatus.Contains("HĐ mất hiệu lực"))
                                //{
                                //    return false;
                                //}
                            }
                        }
                    }                   
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return true;
            }
        }

        private bool CheckWebsiteLoading()
        {
            try
            {
                HtmlElementCollection elements = webBrowser.Document.GetElementsByTagName("div");
                HtmlElement item = getElementByClass("loading", elements);
                if (item != null)
                {
                    string att = item.Style;
                    if (att.Contains("display: block"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private bool IsLoadingModal()
        {
            try
            {
                HtmlElementCollection elements = webBrowser.Document.GetElementsByTagName("body");

                if (elements.Count > 0)
                {
                    string att_class = elements[0].GetAttribute("className");
                    if (!att_class.Contains("modal-open"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private bool IsLoadedModal()
        {
            try
            {
                HtmlElementCollection elements = webBrowser.Document.GetElementsByTagName("div");

                if (elements.Count > 0)
                {
                    foreach (HtmlElement element in elements)
                    {
                        string att_class = element.Style;
                        if (!string.IsNullOrEmpty(att_class) && att_class.Contains("z-index: 1050"))
                        {
                            HtmlElementCollection inputs = webBrowser.Document.GetElementsByTagName("input");
                            if (inputs.Count > 0)
                            {
                                foreach (HtmlElement input in inputs)
                                {
                                    string t = input.GetAttribute("ng-model");
                                    if (!string.IsNullOrEmpty(t) && t.Contains("customer.clientCode"))
                                    {
                                        string value1 = input.InnerText;
                                        string value2 = input.InnerHtml;
                                        string value3 = input.GetAttribute("value");
                                        if (!string.IsNullOrEmpty(value1) || !string.IsNullOrEmpty(value2) || !string.IsNullOrEmpty(value3))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private bool IsClosingModal()
        {
            try
            {
                HtmlElementCollection elements = webBrowser.Document.GetElementsByTagName("body");

                if (elements.Count > 0)
                {
                    string att_class = elements[0].GetAttribute("className");
                    if (!string.IsNullOrEmpty(att_class) && att_class.Contains("modal-open"))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private HtmlElement getElementByClass(string cusClass, HtmlElementCollection collections)
        {
            if (collections.Count > 0)
            {
                foreach (HtmlElement item in collections)
                {
                    string c = item.GetAttribute("className");
                    if (!string.IsNullOrEmpty(c))
                    {
                        if (c.Equals(cusClass))
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (!isProcessComplet)
            {
                if (MessageBox.Show("Chương trình vẫn đang thực hiện lấy dữ liệu.\nBạn xác nhận muốn thoát chương trình?", "Thông báo", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {                
                if (!string.IsNullOrEmpty(txtTime.Text))
                {
                    if (webBrowser.ReadyState == WebBrowserReadyState.Complete)
                    {
                        string curUrl = webBrowser.Url.OriginalString;
                        if (curUrl.ToLower().Equals(UrlConstractInfo.ToLower()))
                        {
                            isProcessComplet = false;

                            txtTime.Enabled = false;
                            btnStart.Enabled = false;
                            btnStart.BackColor = Color.LightGray;
                            btnStop.Enabled = true;
                            btnStop.BackColor = Color.Red;                                                        

                            dtStart = DateTime.Now;
                            dtEnd = dtStart.AddMinutes(int.Parse(txtTime.Text));

                            //------------------------------------
                            isStopGetData = false;
                            while (isStopGetData == false)
                            {
                                //dtSearch = dtDOB.Value;
                                HDStatus = string.Empty;
                                ConstractTMP contract = db.ConstractTMPs.FirstOrDefault(a => a.DataComplete.Equals("N") && a.PCNumber.Equals(machineNo));
                                
                                if (contract != null)
                                {
                                    string constractNumber = contract.Code.Trim(); //73087959
                                    //string constractNumber = "73087959";

                                    processStatus = ProcessStatus.Constract_Search_Processing;
                                    DisplayProcessStatus();

                                    SetConstractNumber(constractNumber);

                                    StartSearch(constractNumber, contract.OriginID);

                                    contract.DataComplete = "Y";
                                    contract.UpdateDate = DateTime.Now;
                                    db.Entry(contract).State = EntityState.Modified;
                                    db.SaveChanges();

                                    processStatus = ProcessStatus.Init;
                                    DisplayProcessStatus();

                                    txtContractNumber.Text = db.ConstractTMPs.Count(a => a.DataComplete.Equals("N") && a.PCNumber.Equals(machineNo)).ToString();

                                    CheckTimeComplete();
                                }
                                else
                                {
                                    isStopGetData = true;
                                }
                            }

                            //---------------------------------
                            isStopGetData = true;
                            isProcessComplet = true;
                            txtTime.Enabled = true;
                            btnStart.Enabled = true;
                            btnStart.BackColor = Color.LightGreen;
                            btnStop.Enabled = false;
                            btnStop.BackColor = Color.LightGray;
                        }
                        else
                        {
                            MessageBox.Show("Hãy vào mục 'Thông tin HS/HD' để thực hiện", "Thông báo");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Hãy nhập SỐ PHÚT để chạy chương trình", "Thông báo");
                    txtTime.Focus();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                isProcessComplet = true;
            }
        }

        private void StartSearch(string constractNumber, int parentId)
        {
            try
            {
                doSearch();                

                tabControl1.TabPages[0].Text = "";

                //====== Get Record Found ===================                
                getDataConstract(constractNumber, parentId);                
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void getDataConstract(string constractNumber, int parentId)
        {
            try
            {                
                getDataConstractInfo(constractNumber, parentId);             
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void getDataConstractInfo(string constractNumber, int parentId)
        {
            try
            {
                WaitDisplayDataComplete();                

                Contract contract = new Contract();
                contract.Code = constractNumber;
                contract.PhiBaoHiem = 0;

                HtmlElementCollection divs = webBrowser.Document.GetElementsByTagName("div");
                foreach(HtmlElement div in divs)
                {
                    string className = div.GetAttribute("className").Trim();
                    string style = div.Style;
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(style) && className == "row" && style.Contains("margin-bottom: 10px"))
                    {
                        HtmlElementCollection arrA = div.GetElementsByTagName("a");
                        if (arrA.Count > 0)
                        {
                            foreach(HtmlElement a in arrA)
                            {
                                string att = a.GetAttribute("ng-click").Trim();
                                if (!string.IsNullOrEmpty(att) && att == "viewCustomer()")
                                {
                                    NguoiMuaBaoHiemCode = a.InnerText.Trim();
                                    contract.NMBHCode = NguoiMuaBaoHiemCode;

                                    Customer customer1 = db.Customers.AsNoTracking().FirstOrDefault(b => b.Code.Trim() == NguoiMuaBaoHiemCode);
                                    if (customer1 == null)
                                    {
                                        Customer customer2 = new Customer();                                        
                                        GetCustomerInfoTabSanPham(customer2, NguoiMuaBaoHiemCode, a);

                                        customer2.CreateDate = DateTime.Now;
                                        db.Customers.Add(customer2);
                                        db.SaveChanges();

                                        contract.CustomerCMND = customer2.CMND;
                                        contract.CustomerName = customer2.Name;
                                        contract.DOB = customer2.DOB;
                                    }
                                    else
                                    {
                                        contract.CustomerCMND = customer1.CMND;
                                        contract.CustomerName = customer1.Name;
                                        contract.DOB = customer1.DOB;
                                    }
                                }
                            }
                        }

                        HtmlElementCollection inputs = div.GetElementsByTagName("input");
                        foreach(HtmlElement input in inputs)
                        {                            
                            string name = input.GetAttribute("name").Trim();

                            if (!string.IsNullOrEmpty(name) && name == "productName")
                            {
                                contract.ProductName = input.GetAttribute("value").Trim();
                            }

                            if (!string.IsNullOrEmpty(name) && name == "statusProposal")
                            {
                                contract.Status = input.GetAttribute("value").Trim();
                                HDStatus = contract.Status;
                            }

                            if (!string.IsNullOrEmpty(name) && name == "ngayHieuLuc")
                            {
                                string date = input.GetAttribute("value").Trim();
                                if (!string.IsNullOrEmpty(date))
                                {
                                    string[] arr = date.Split('/');
                                    if (arr.Length == 3)
                                    {
                                        contract.EffectiveDate = new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0]));
                                    }
                                }                                
                            }
                        }                                                
                    }

                    if (className == "top-10")
                    {
                        //Tab Phi
                        processStatus = ProcessStatus.Constract_Tab_Phi_Processing;
                        DisplayProcessStatus();
                        WaitTimeSecond(2);
                        WebsiteLoadingTab("phi");
                        GetDataTabPhi(contract, div);

                        processStatus = ProcessStatus.Constract_Tab_NguoiThuHuong_Processing;
                        DisplayProcessStatus();
                        WaitTimeSecond(2);
                        TabClick(div, "nguoithuhuong");
                        WebsiteLoading();
                        WebsiteLoadingTab("nguoithuhuong");
                        GetDataTabNguoiThuHuong(div, constractNumber, parentId);

                        processStatus = ProcessStatus.Constract_Tab_SanPham_Processing;
                        DisplayProcessStatus();
                        WaitTimeSecond(2);
                        TabClick(div, "sanpham");
                        WebsiteLoading();
                        WebsiteLoadingTab("sanpham");
                        GetDataTabSanPham(contract, div, constractNumber, parentId);

                        processStatus = ProcessStatus.Constract_Tab_Daily_Processing;
                        DisplayProcessStatus();
                        WaitTimeSecond(2);
                        TabClick(div, "daily");
                        WebsiteLoading();
                        WebsiteLoadingTab("daily");
                        GetDataTabDaily(contract, div, constractNumber);

                        processStatus = ProcessStatus.Constract_Tab_DiaChiHienTai_Processing;
                        DisplayProcessStatus();
                        WaitTimeSecond(2);
                        TabClick(div, "diachihientai");
                        WebsiteLoading();
                        WebsiteLoadingTab("diachihientai");
                        GetDataTabDiaChiHienTai(contract, div, constractNumber);

                        processStatus = ProcessStatus.Constract_Tab_GiaTriTienMat_Processing;
                        DisplayProcessStatus();
                        WaitTimeSecond(2);
                        TabClick(div, "giatritienmat");
                        WebsiteLoading();
                        WebsiteLoadingTab("giatritienmat");
                        GetDataTabGiaTriTienMat(contract, div, constractNumber);

                        CheckForUpdateConstract(contract);                        
                    }                       
                }                
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                db = new PruDataEntities();
            }
        }

        private void CheckForUpdateConstract(Contract contract)
        {
            try
            {
                Contract contractEx = db.Contracts.FirstOrDefault(a => a.Code.Trim() == contract.Code);
                if (contractEx != null)
                {
                    if (IsConstractChanged(contract, contractEx))
                    {
                        //Contract contractEx1 = db.Contracts.AsNoTracking().FirstOrDefault(a => a.Code.Trim() == contract.Code);
                        contractEx.ProductName = contract.ProductName;
                        contractEx.InsuranceFee = contract.InsuranceFee;
                        contractEx.Status = contract.Status;
                        contractEx.EffectiveDate = contract.EffectiveDate;
                        contractEx.NextPaymentDate = contract.NextPaymentDate;
                        contractEx.LatestPaymentDate = contract.LatestPaymentDate;
                        contractEx.DueDate = contract.DueDate;
                        contractEx.ValueRefundedToAdvance = contract.ValueRefundedToAdvance;
                        contractEx.AdvancePayment = contract.AdvancePayment;
                        contractEx.RTBT = contract.RTBT;
                        contractEx.QLDK = contract.QLDK;
                        contractEx.HuyHD = contract.HuyHD;
                        contractEx.TVVCode = contract.TVVCode;
                        contractEx.TVV = contract.TVV;
                        contractEx.DiaChiHienTai = contract.DiaChiHienTai;
                        contractEx.TelCompany = ConvertMobile(contract.TelCompany);
                        contractEx.TelHome = ConvertMobile(contract.TelHome);
                        contractEx.TelMobile = ConvertMobile(contract.TelMobile);
                        contractEx.NMBHCode = contract.NMBHCode;
                        contractEx.VanPhong = contract.VanPhong;
                        contractEx.DataComplete = "Y";
                        contractEx.UpdateDate = DateTime.Now;
                        contractEx.CustomerCMND = contract.CustomerCMND;
                        contractEx.DOB = contract.DOB;
                        contractEx.CustomerName = contract.CustomerName;
                        contractEx.District = contract.District;
                        contractEx.City = contract.City;
                        contractEx.DKDP = contract.DKDP;
                        contractEx.PhiBaoHiem = contract.PhiBaoHiem;
                        contractEx.NgayDaoHan = contract.NgayDaoHan;
                        contractEx.NgayKetThucDongPhi = contract.NgayKetThucDongPhi;                        

                        db.Entry(contractEx).State = EntityState.Modified;
                        db.SaveChanges();
                    }                    

                    UpdateVanPhongCustomer(contractEx);

                    //Log
                    if (IsConstractNotChange(contractTAM, contractEx))
                    {
                        log.Error(string.Format("DATA LAP: ContractCode={0} trong bien TAM, DATA DANG GET: ContractCode={1}", contractTAM.Code, contractEx.Code));
                    }

                    contractTAM = contractEx;
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void UpdateVanPhongCustomer(Contract contractEx)
        {
            try
            {
                //Nguoi trong tab SanPham
                List<Contract> insuredPeople = db.Contracts.Where(a => a.Code == contractEx.Code
                                                                        && a.ParentID == contractEx.ID).ToList();
                if (insuredPeople.Count > 0)
                {
                    foreach(var item in insuredPeople)
                    {
                        Customer customer = db.Customers.FirstOrDefault(a => a.Code == item.CustomerCode);
                        if (customer != null)
                        {
                            bool isSave = false;
                            if (!string.IsNullOrEmpty(customer.VanPhong))
                            {
                                string[] vs = customer.VanPhong.Split(',');
                                bool isExist = false;
                                foreach(string s in vs)
                                {
                                    if (s.Equals(contractEx.VanPhong))
                                    {
                                        isExist = true;
                                    }                                    
                                }

                                if (isExist ==  false)
                                {
                                    customer.VanPhong += "," + contractEx.VanPhong;
                                    isSave = true;
                                }                                
                            }
                            else
                            {
                                customer.VanPhong = contractEx.VanPhong;
                                isSave = true;
                            }

                            if (isSave)
                            {
                                db.Entry(customer).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            if (item.VanPhong != customer.VanPhong)
                            {
                                item.VanPhong = customer.VanPhong;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                }

                //Nguoi Thu Huong
                List<Beneficiary> beneficiaries = db.Beneficiaries.Where(a => a.ConstractID == contractEx.ID).ToList();
                if (beneficiaries.Count > 0)
                {
                    foreach (var item in beneficiaries)
                    {
                        Customer customer = db.Customers.Find(item.CustomerID);
                        if (customer != null)
                        {
                            bool isSave = false;
                            if (!string.IsNullOrEmpty(customer.VanPhong))
                            {
                                string[] vs = customer.VanPhong.Split(',');
                                bool isExist = false;
                                foreach (string s in vs)
                                {
                                    if (s.Equals(contractEx.VanPhong))
                                    {
                                        isExist = true;
                                    }
                                }

                                if (isExist == false)
                                {
                                    customer.VanPhong += "," + contractEx.VanPhong;
                                    isSave = true;
                                }
                            }
                            else
                            {
                                customer.VanPhong = contractEx.VanPhong;
                                isSave = true;
                            }

                            if (isSave)
                            {
                                db.Entry(customer).State = EntityState.Modified;
                                db.SaveChanges();
                            }                            
                        }
                    }
                }

                //Nguoi Mua BH
                Customer customerNMBH = db.Customers.FirstOrDefault(a => a.Code == contractEx.NMBHCode);
                if (customerNMBH != null)
                {
                    bool isSave = false;
                    if (!string.IsNullOrEmpty(customerNMBH.VanPhong))
                    {
                        string[] vs = customerNMBH.VanPhong.Split(',');
                        bool isExist = false;
                        foreach (string s in vs)
                        {
                            if (s.Equals(contractEx.VanPhong))
                            {
                                isExist = true;
                            }
                        }

                        if (isExist == false)
                        {
                            customerNMBH.VanPhong += "," + contractEx.VanPhong;
                            isSave = true;
                        }
                    }
                    else
                    {
                        customerNMBH.VanPhong = contractEx.VanPhong;
                        isSave = true;
                    }

                    if (isSave)
                    {
                        db.Entry(customerNMBH).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private bool IsConstractChanged(Contract contract, Contract contractEx)
        {
            try
            {
                if (contract.ProductName != contractEx.ProductName)
                    return true;
                if (contract.InsuranceFee != contractEx.InsuranceFee)
                    return true;
                if (contract.Status != contractEx.Status)
                    return true;
                if (contract.EffectiveDate != contractEx.EffectiveDate)
                    return true;
                if (contract.NextPaymentDate != contractEx.NextPaymentDate)
                    return true;
                if (contract.LatestPaymentDate != contractEx.LatestPaymentDate)
                    return true;
                if (contract.DueDate != contractEx.DueDate)
                    return true;
                if (contract.ValueRefundedToAdvance != contractEx.ValueRefundedToAdvance)
                    return true;
                if (contract.AdvancePayment != contractEx.AdvancePayment)
                    return true;
                if (contract.RTBT != contractEx.RTBT)
                    return true;
                if (contract.QLDK != contractEx.QLDK)
                    return true;
                if (contract.HuyHD != contractEx.HuyHD)
                    return true;
                if (contract.TVVCode != contractEx.TVVCode)
                    return true;
                if (contract.TVV != contractEx.TVV)
                    return true;
                if (contract.DiaChiHienTai != contractEx.DiaChiHienTai)
                    return true;
                if (ConvertMobile(contract.TelCompany) != ConvertMobile(contractEx.TelCompany))
                    return true;
                if (ConvertMobile(contract.TelHome) != ConvertMobile(contractEx.TelHome))
                    return true;
                if (ConvertMobile(contract.TelMobile) != ConvertMobile(contractEx.TelMobile))
                    return true;
                if (contractEx.DataComplete == "N")
                    return true;
                if (contract.VanPhong != contractEx.VanPhong)
                    return true;
                if (contract.CustomerName != contractEx.CustomerName)
                    return true;
                if (contract.CustomerCMND != contractEx.CustomerCMND)
                    return true;
                if (contract.DOB != contractEx.DOB)
                    return true;
                if (contract.District != contractEx.District)
                    return true;
                if (contract.City != contractEx.City)
                    return true;
                if (contract.NMBHCode != contractEx.NMBHCode)
                    return true;
                if (contract.DKDP != contractEx.DKDP)
                    return true;
                if (contract.PhiBaoHiem != contractEx.PhiBaoHiem)
                    return true;
                if (contract.NgayKetThucDongPhi != contractEx.NgayKetThucDongPhi)
                    return true;
                if (contract.NgayDaoHan != contractEx.NgayDaoHan)
                    return true;

                return false;
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private bool IsConstractNotChange(Contract contract, Contract contractEx)
        {
            try
            {
                if (contract.ProductName == contractEx.ProductName
                    && contract.InsuranceFee == contractEx.InsuranceFee
                    && contract.Status == contractEx.Status
                    && contract.EffectiveDate == contractEx.EffectiveDate
                    && contract.NextPaymentDate == contractEx.NextPaymentDate
                    && contract.LatestPaymentDate == contractEx.LatestPaymentDate
                    && contract.DueDate == contractEx.DueDate
                    && contract.ValueRefundedToAdvance == contractEx.ValueRefundedToAdvance
                    && contract.AdvancePayment == contractEx.AdvancePayment
                    && contract.RTBT == contractEx.RTBT
                    && contract.QLDK == contractEx.QLDK
                    && contract.HuyHD == contractEx.HuyHD
                    && contract.TVVCode == contractEx.TVVCode
                    && contract.TVV == contractEx.TVV
                    && contract.DiaChiHienTai == contractEx.DiaChiHienTai
                    && ConvertMobile(contract.TelCompany) == ConvertMobile(contractEx.TelCompany)
                    && ConvertMobile(contract.TelHome) == ConvertMobile(contractEx.TelHome)
                    && ConvertMobile(contract.TelMobile) == ConvertMobile(contractEx.TelMobile)
                    && contract.VanPhong == contractEx.VanPhong
                    && contract.CustomerName == contractEx.CustomerName
                    && contract.CustomerCMND == contractEx.CustomerCMND
                    && contract.DOB == contractEx.DOB
                    && contract.District == contractEx.District
                    && contract.City == contractEx.City
                    && contract.NMBHCode == contractEx.NMBHCode
                    && contract.DKDP == contractEx.DKDP
                    && contract.PhiBaoHiem == contractEx.PhiBaoHiem
                    && contract.NgayKetThucDongPhi == contractEx.NgayKetThucDongPhi
                    && contract.NgayDaoHan == contractEx.NgayDaoHan
                    )
                {
                    return true;
                }
                    
                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private static string ConvertMobile(string phone)
        {
            string ret = string.Empty;
            if (!string.IsNullOrEmpty(phone))
            {
                if (phone.Length >= 11)
                {
                    //0168 xxx xxxx
                    string _7socuoi = phone.Substring(phone.Length - 7);
                    string _4sodau = phone.Substring((phone.Length - 7) - 4, 4);

                    string _changeto = string.Empty;
                    switch (_4sodau)
                    {
                        //============Viettel
                        case "0162":
                            _changeto = "032";
                            break;
                        case "0163":
                            _changeto = "033";
                            break;
                        case "0164":
                            _changeto = "034";
                            break;
                        case "0165":
                            _changeto = "035";
                            break;
                        case "0166":
                            _changeto = "036";
                            break;
                        case "0167":
                            _changeto = "037";
                            break;
                        case "0168":
                            _changeto = "038";
                            break;
                        case "0169":
                            _changeto = "039";
                            break;
                        //==========Mobile============
                        case "0120":
                            _changeto = "070";
                            break;
                        case "0121":
                            _changeto = "079";
                            break;
                        case "0122":
                            _changeto = "077";
                            break;
                        case "0126":
                            _changeto = "076";
                            break;
                        case "0128":
                            _changeto = "078";
                            break;
                        //===========Vinaphone
                        case "0123":
                            _changeto = "083";
                            break;
                        case "0124":
                            _changeto = "084";
                            break;
                        case "0125":
                            _changeto = "085";
                            break;
                        case "0127":
                            _changeto = "081";
                            break;
                        case "0129":
                            _changeto = "082";
                            break;
                        //=================Vietnamobile
                        case "0186":
                            _changeto = "056";
                            break;
                        case "0188":
                            _changeto = "058";
                            break;
                        //============= GMobile
                        case "0199":
                            _changeto = "059";
                            break;
                    }

                    if (!string.IsNullOrEmpty(_changeto))
                    {
                        return _changeto + _7socuoi;
                    }
                }
            }
            else
            {
                return string.Empty;
            }

            return phone;
        }


        private void GetDataTabGiaTriTienMat(Contract contract, HtmlElement divParent, string constractNumber)
        {
            try
            {
                HtmlElementCollection tag_divs = divParent.GetElementsByTagName("div");
                foreach (HtmlElement div in tag_divs)
                {
                    string name = div.GetAttribute("ui-view").Trim();
                    if (!string.IsNullOrEmpty(name) && name == "partial")
                    {
                        HtmlElementCollection tag_inputs = divParent.GetElementsByTagName("input");
                        if (tag_inputs.Count > 0)
                        {                            
                            foreach (HtmlElement input in tag_inputs)
                            {
                                string att = input.GetAttribute("ng-model").Trim();
                                string value = input.GetAttribute("value").Trim().Replace(",","");                                

                                if (!string.IsNullOrEmpty(att) && !string.IsNullOrEmpty(value) && att == "vm.history.cash.netSvAmount") // Gia tri hoan lai de tam ung
                                {
                                    //log.Error("ValueRefundedToAdvance:" + value);
                                    contract.ValueRefundedToAdvance = decimal.Parse(value);
                                }

                                if (!string.IsNullOrEmpty(att) && !string.IsNullOrEmpty(value) && att == "vm.history.cash.osLoanAmount")  // Cac khoan tam ung
                                {
                                    //log.Error("AdvancePayment:" + value);
                                    contract.AdvancePayment = decimal.Parse(value);
                                }

                                if (!string.IsNullOrEmpty(att) && !string.IsNullOrEmpty(value) && att == "vm.history.cash.bonusSvAmount") //Rut truoc bao tuc
                                {
                                    //log.Error("RTBT:" + value);
                                    contract.RTBT = decimal.Parse(value);
                                }

                                if (!string.IsNullOrEmpty(att) && !string.IsNullOrEmpty(value) && att == "vm.history.cash.maxPruCashAmount") //Quyen loi dinh ky
                                {
                                    //log.Error("QLDK:" + value);
                                    contract.QLDK = decimal.Parse(value);
                                }

                                if (!string.IsNullOrEmpty(att) && !string.IsNullOrEmpty(value) && att == "vm.history.cash.svAmount") //Huy hop dong
                                {
                                    //log.Error("HuyHD:" + value);
                                    contract.HuyHD = decimal.Parse(value);
                                }
                            }                            
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void GetDataTabDiaChiHienTai(Contract contract, HtmlElement divParent, string constractNumber)
        {
            try
            {
                HtmlElementCollection tag_divs = divParent.GetElementsByTagName("div");
                foreach (HtmlElement div in tag_divs)
                {
                    string name = div.GetAttribute("ui-view").Trim();
                    if (!string.IsNullOrEmpty(name) && name == "partial")
                    {
                        HtmlElementCollection tag_inputs = divParent.GetElementsByTagName("input");
                        if (tag_inputs.Count > 0)
                        {
                            string address = string.Empty;
                            foreach(HtmlElement input in tag_inputs)
                            {
                                string att = input.GetAttribute("name").Trim();
                                string value = input.GetAttribute("value").Trim();

                                if (!string.IsNullOrEmpty(att) && (att == "duongPho" || att== "phuongXa" || att == "quanHuyen" || att == "tinhThanh"))
                                {
                                    address = address + " " + value;
                                }

                                if (!string.IsNullOrEmpty(att) && att == "quanHuyen")
                                {
                                    contract.District = value;
                                }

                                if (!string.IsNullOrEmpty(att) && att == "tinhThanh")
                                {
                                    contract.City = value;
                                }

                                if (!string.IsNullOrEmpty(att) && att == "mobilePhone")
                                {
                                    contract.TelMobile = value;
                                }

                                if (!string.IsNullOrEmpty(att) && att == "companyPhone")
                                {
                                    contract.TelCompany = value;
                                }

                                if (!string.IsNullOrEmpty(att) && att == "homePhone")
                                {
                                    contract.TelHome = value;
                                }
                            }

                            if (!string.IsNullOrEmpty(address.Trim()))
                                contract.DiaChiHienTai = address.Trim();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void GetDataTabNguoiThuHuong(HtmlElement divParent, string constractNumber, int parentId)
        {
            try
            {
                HtmlElementCollection tag_divs = divParent.GetElementsByTagName("div");
                foreach (HtmlElement div in tag_divs)
                {
                    string name = div.GetAttribute("ui-view").Trim();
                    if (!string.IsNullOrEmpty(name) && name == "partial")
                    {
                        HtmlElementCollection tag_tbody = div.GetElementsByTagName("tbody");
                        if (tag_tbody.Count > 0)
                        {
                            HtmlElementCollection tag_trs = tag_tbody[0].GetElementsByTagName("tr");
                            if (tag_trs.Count > 0)
                            {
                                foreach(HtmlElement tr in tag_trs)
                                {
                                    Customer customerEx = null;
                                    HtmlElementCollection tag_as = tr.GetElementsByTagName("a");
                                    if (tag_as.Count > 0)
                                    {                                        
                                        string customerCode = tag_as[0].InnerText.Trim();
                                        customerEx = db.Customers.AsNoTracking().FirstOrDefault(a => a.Code.Trim() == customerCode);
                                        if (customerEx == null)
                                        {
                                            //Create new Customer
                                            Customer customerEx1 = new Customer();
                                            GetCustomerInfoTabSanPham(customerEx1, customerCode, tag_as[0]);
                                            customerEx1.CreateDate = DateTime.Now;
                                            db.Customers.Add(customerEx1);
                                            db.SaveChanges();

                                            customerEx = customerEx1;
                                        }                                        
                                    }

                                    if (customerEx != null)
                                    {
                                        Contract person = new Contract();
                                        person.Code = constractNumber;
                                        person.ParentID = parentId;
                                        person.DataComplete = "C";
                                        person.NTH = "Y";
                                        //insuredPerson.ContractID = contractEx.ID;                                        

                                        person.CustomerCode = customerEx.Code;
                                        person.CustomerCMND = customerEx.CMND;
                                        person.CustomerName = customerEx.Name;
                                        person.DOB = customerEx.DOB;
                                        person.TelCompany = customerEx.TelCompany;
                                        person.TelHome = customerEx.TelHome;
                                        person.TelMobile = customerEx.Phone;
                                        //person.VanPhong = customerEx.VanPhong;

                                        HtmlElementCollection tag_tds = tr.GetElementsByTagName("td");
                                        if (tag_tds.Count == 4)
                                        {
                                            if (tag_tds[3].GetElementsByTagName("span")[0].InnerText.Trim().Equals("Người được bảo hiểm"))
                                            {
                                                person.Relation = "NĐBH";
                                            }
                                            else
                                            {
                                                person.Relation = tag_tds[3].GetElementsByTagName("span")[0].InnerText.Trim();
                                            }
                                        }

                                        //Check
                                        Contract personEx = db.Contracts.FirstOrDefault(a => a.Code == person.Code
                                                                                            && a.DataComplete == "C"
                                                                                            && a.CustomerCode == person.CustomerCode
                                                                                            && a.NTH == person.NTH);

                                        if (personEx != null)
                                        {
                                            if (IsNTHChanged(personEx, person))
                                            {
                                                personEx.Relation = person.Relation;                                                
                                                personEx.UpdateDate = DateTime.Now;
                                                personEx.TelMobile = person.TelMobile;
                                                personEx.TelHome = person.TelHome;
                                                personEx.TelCompany = person.TelCompany;
                                                personEx.CustomerCMND = person.CustomerCMND;

                                                db.Entry(personEx).State = EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            person.CreateDate = DateTime.Now;
                                            db.Contracts.Add(person);
                                            db.SaveChanges();
                                        }                                        
                                    }                                    
                                }                                
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void GetDataTabDaily(Contract contract, HtmlElement divParent, string constractNumber)
        {
            try
            {                
                HtmlElementCollection tag_divs = divParent.GetElementsByTagName("div");
                foreach (HtmlElement div in tag_divs)
                {
                    string name = div.GetAttribute("ui-view").Trim();
                    if (!string.IsNullOrEmpty(name) && name == "partial")
                    {
                        HtmlElementCollection tag_tbody = div.GetElementsByTagName("tbody");
                        if (tag_tbody.Count > 0)
                        {
                            HtmlElementCollection tag_trs = tag_tbody[0].GetElementsByTagName("tr");
                            if (tag_trs.Count > 0)
                            {
                                HtmlElementCollection tag_tds = tag_trs[0].GetElementsByTagName("td");
                                
                                bool trung = false;

                                HtmlElementCollection tag_as = tag_tds[0].GetElementsByTagName("a");
                                if (tag_as.Count > 0)
                                {
                                    string att = tag_as[0].GetAttribute("ng-click");
                                    string tvvCode = tag_as[0].InnerText.Trim();
                                    //if (att == "viewCustomer()" && NguoiMuaBaoHiemCode == tvvCode)
                                    //{
                                    //    trung = true;
                                    //}

                                    string ttvMa = tag_tds[1].GetElementsByTagName("span")[0].InnerText.Trim();
                                    string ttvName = tag_tds[2].GetElementsByTagName("span")[0].InnerText.Trim();
                                    string vanphong = tag_tds[3].GetElementsByTagName("span")[0].InnerText.Trim();
                                    contract.TVV = ttvMa + " " + ttvName;
                                    contract.TVVCode = tvvCode;
                                    contract.VanPhong = vanphong;
                                }                                
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void TabClick(HtmlElement divParent, string tab)
        {
            try
            {
                HtmlElementCollection tag_as = divParent.GetElementsByTagName("a");
                foreach (HtmlElement a in tag_as)
                {
                    string name = a.GetAttribute("ng-click").Trim();
                    if (!string.IsNullOrEmpty(name) && name == "vm.changeTab(2)" && tab == "sanpham")
                    {
                        a.Focus();
                        a.InvokeMember("Click");
                        dtStartLoadWebsite = DateTime.Now;
                        dtEndLoadWebsite = dtStartLoadWebsite.AddSeconds(10);
                    }

                    if (!string.IsNullOrEmpty(name) && name == "vm.changeTab(3)" && tab == "daily")
                    {
                        a.Focus();
                        a.InvokeMember("Click");
                        dtStartLoadWebsite = DateTime.Now;
                        dtEndLoadWebsite = dtStartLoadWebsite.AddSeconds(10);
                    }

                    if (!string.IsNullOrEmpty(name) && name == "vm.changeTab(4)" && tab == "giatritienmat")
                    {
                        a.Focus();
                        a.InvokeMember("Click");
                        dtStartLoadWebsite = DateTime.Now;
                        dtEndLoadWebsite = dtStartLoadWebsite.AddSeconds(10);
                    }

                    if (!string.IsNullOrEmpty(name) && name == "vm.changeTab(5)" && tab == "nguoithuhuong")
                    {
                        a.Focus();
                        a.InvokeMember("Click");
                        dtStartLoadWebsite = DateTime.Now;
                        dtEndLoadWebsite = dtStartLoadWebsite.AddSeconds(10);
                    }

                    if (!string.IsNullOrEmpty(name) && name == "vm.changeTab(6)" && tab == "diachihientai")
                    {
                        a.Focus();
                        a.InvokeMember("Click");
                        dtStartLoadWebsite = DateTime.Now;
                        dtEndLoadWebsite = dtStartLoadWebsite.AddSeconds(10);
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void GetDataTabPhi(Contract contract, HtmlElement divParent)
        {
            try
            {
                HtmlElementCollection inputs = divParent.GetElementsByTagName("input");
                foreach (HtmlElement input in inputs)
                {
                    string name = input.GetAttribute("ng-model").Trim();
                    if (name == "vm.history.paidToDateAdvance")
                    {
                        string date = input.GetAttribute("value").Trim();
                        if (!string.IsNullOrEmpty(date))
                        {
                            string[] arr = date.Split('/');
                            if (arr.Length == 3)
                            {
                                contract.NextPaymentDate = new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0]));
                            }
                        }                        
                    } 
                    
                    if (name == "vm.history.frequencyString")
                    {
                        string value = input.GetAttribute("value").Trim();
                        contract.DKDP = value;
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void GetDataTabSanPham(Contract contract, HtmlElement divParent, string constractNum, int parentId)
        {
            try
            {
                Contract contractEx = db.Contracts.FirstOrDefault(a => a.Code.Trim() == constractNum.Trim());
                HtmlElementCollection tag_divs = divParent.GetElementsByTagName("div");
                foreach (HtmlElement div in tag_divs)
                {
                    string name = div.GetAttribute("ui-view").Trim();
                    if (name == "partial")
                    {
                        if (!contract.Status.Trim().Contains("Từ chối"))
                        {
                            HtmlElementCollection tag_subdivs = div.GetElementsByTagName("div");
                            foreach (HtmlElement subdiv in tag_subdivs)
                            {
                                string subname = subdiv.GetAttribute("className").Trim();
                                if (subname == "row")
                                {
                                    HtmlElementCollection tag_as = subdiv.GetElementsByTagName("a");
                                    if (tag_as.Count > 0)
                                    {
                                        string att = tag_as[0].GetAttribute("ng-click");
                                        if (!string.IsNullOrEmpty(att) && att == "viewCustomer()")
                                        {
                                            //Sửa tiếp từ đây
                                            //InsuredPerson insuredPerson = new InsuredPerson();
                                            Contract person = new Contract();
                                            person.Code = contract.Code;
                                            person.ParentID = parentId;
                                            person.DataComplete = "C";

                                            //insuredPerson.ContractID = contractEx.ID;

                                            string customerCode = tag_as[0].InnerText.Trim();
                                            Customer customerEx = db.Customers.AsNoTracking().FirstOrDefault(a => a.Code.Trim() == customerCode);
                                            if (customerEx == null)
                                            {
                                                //Create new Customer
                                                Customer customerEx1 = new Customer();
                                                GetCustomerInfoTabSanPham(customerEx1, customerCode, tag_as[0]);
                                                customerEx1.CreateDate = DateTime.Now;
                                                db.Customers.Add(customerEx1);
                                                db.SaveChanges();

                                                customerEx = customerEx1;
                                                //insuredPerson.CustomerID = customerEx1.ID;
                                                
                                            }
                                            else
                                            {
                                                //insuredPerson.CustomerID = customerEx.ID;
                                            }

                                            person.CustomerCode = customerEx.Code;
                                            person.CustomerCMND = customerEx.CMND;
                                            person.CustomerName = customerEx.Name;
                                            person.DOB = customerEx.DOB;
                                            person.TelCompany = customerEx.TelCompany;
                                            person.TelHome = customerEx.TelHome;
                                            person.TelMobile = customerEx.Phone;
                                            //person.VanPhong = customerEx.VanPhong;
                                            
                                            if (NguoiMuaBaoHiemCode == customerCode)
                                            {
                                                person.Relation = "NMBH";
                                            }
                                            else
                                            {
                                                //Them quan he khac vao day
                                                //Beneficiary beneficiary = db.Beneficiaries.AsNoTracking().FirstOrDefault(a => a.CustomerID == customerEx.ID);
                                                Contract beneficiary = db.Contracts.FirstOrDefault(a => a.Code == contract.Code
                                                                                                        && a.NTH == "Y"
                                                                                                        && a.CustomerCode == customerEx.Code);
                                                if (beneficiary != null)
                                                {
                                                    person.Relation = beneficiary.Relation;
                                                }
                                                else
                                                {
                                                    person.Relation = "NĐBH";
                                                }
                                            }

                                            HtmlElementCollection tag_row_inputs = subdiv.GetElementsByTagName("input");
                                            foreach (HtmlElement input in tag_row_inputs)
                                            {
                                                string name_input = input.GetAttribute("ng-model");
                                                if (name_input.Trim() == "product.id.productName")
                                                {
                                                    person.ProductName = input.GetAttribute("value").Trim();
                                                }

                                                if (name_input.Trim() == "product.id.productTypeDesc")
                                                {
                                                    person.Loai = input.GetAttribute("value").Trim();
                                                }

                                                if (name_input.Trim() == "product.id.premiumAmount")
                                                {
                                                    person.PhiBaoHiem = decimal.Parse(input.GetAttribute("value").Trim().Replace(",",""));
                                                }

                                                if (name_input.Trim() == "product.id.premiumDate")
                                                {
                                                    string date1 = input.GetAttribute("value").Trim();
                                                    if (!string.IsNullOrEmpty(date1))
                                                    {
                                                        string[] arr = date1.Split('/');
                                                        if (arr.Length == 3)
                                                        {
                                                            person.NgayKetThucDongPhi = new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0]));
                                                        }
                                                    }
                                                    //insuredPerson.DongPhiDenNgay = decimal.Parse(input.GetAttribute("value").Trim());
                                                }

                                                if (name_input.Trim() == "product.id.riskCessationDate")
                                                {
                                                    string date1 = input.GetAttribute("value").Trim();
                                                    if (!string.IsNullOrEmpty(date1))
                                                    {
                                                        string[] arr = date1.Split('/');
                                                        if (arr.Length == 3)
                                                        {
                                                            person.NgayDaoHan = new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0]));
                                                        }
                                                    }
                                                    //insuredPerson.NgayDaoHan = decimal.Parse(input.GetAttribute("value").Trim());
                                                }

                                                if (name_input.Trim() == "product.id.statusDesc")
                                                {
                                                    person.Status = input.GetAttribute("value").Trim();
                                                }
                                            }

                                            //Check
                                            Contract personEx = db.Contracts.FirstOrDefault(a => a.Code == person.Code
                                                                                                && a.DataComplete == "C"
                                                                                                && a.CustomerCode == person.CustomerCode
                                                                                                && a.ProductName == person.ProductName);

                                            if (personEx != null)
                                            {
                                                if (IsInsuredPersonChanged(personEx, person))
                                                {
                                                    personEx.Relation = person.Relation;
                                                    personEx.ProductName = person.ProductName;
                                                    personEx.Loai = person.Loai;
                                                    personEx.PhiBaoHiem = person.PhiBaoHiem;
                                                    personEx.NgayDaoHan = person.NgayDaoHan;
                                                    personEx.NgayKetThucDongPhi = person.NgayKetThucDongPhi;
                                                    personEx.Status = person.Status;
                                                    personEx.UpdateDate = DateTime.Now;
                                                    personEx.TelMobile = person.TelMobile;
                                                    personEx.TelHome = person.TelHome;
                                                    personEx.TelCompany = person.TelCompany;
                                                    personEx.CustomerCMND = person.CustomerCMND;

                                                    db.Entry(personEx).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                person.CreateDate = DateTime.Now;
                                                db.Contracts.Add(person);
                                                db.SaveChanges();
                                            }                                            
                                            
                                            if (contract.PhiBaoHiem.HasValue)
                                            {
                                                if (person.PhiBaoHiem.HasValue)
                                                {
                                                    contract.PhiBaoHiem += person.PhiBaoHiem;
                                                }                                                
                                            }
                                            else
                                            {
                                                contract.PhiBaoHiem = person.PhiBaoHiem;
                                            }

                                            if (person.Loai == "Chính")
                                            {
                                                contract.NgayDaoHan = person.NgayDaoHan;
                                                contract.NgayKetThucDongPhi = person.NgayKetThucDongPhi;
                                            }
                                            

                                            //InsuredPerson insuredPersonEx = db.InsuredPersons.FirstOrDefault(a => a.ContractID == insuredPerson.ContractID
                                            //                                                        && a.CustomerID == insuredPerson.CustomerID
                                            //                                                        && a.TenSanPham.Equals(insuredPerson.TenSanPham));
                                            //if (insuredPersonEx != null)
                                            //{
                                            //    if (IsInsuredPersonChanged(insuredPersonEx, insuredPerson))
                                            //    {
                                            //        insuredPersonEx.Relation = insuredPerson.Relation;
                                            //        insuredPersonEx.TenSanPham = insuredPerson.TenSanPham;
                                            //        insuredPersonEx.Loai = insuredPerson.Loai;
                                            //        insuredPersonEx.Fee = insuredPerson.Fee;
                                            //        insuredPersonEx.NgayDaoHan = insuredPerson.NgayDaoHan;
                                            //        insuredPersonEx.DongPhiDenNgay = insuredPerson.DongPhiDenNgay;
                                            //        insuredPersonEx.TinhTrang = insuredPerson.TinhTrang;
                                            //        insuredPersonEx.UpdateDate = DateTime.Now;

                                            //        db.Entry(insuredPersonEx).State = EntityState.Modified;
                                            //        db.SaveChanges();
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    insuredPerson.CreateDate = DateTime.Now;
                                            //    db.InsuredPersons.Add(insuredPerson);
                                            //    db.SaveChanges();
                                            //}
                                        }
                                    }
                                }
                            }

                            break;
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private bool IsInsuredPersonChanged(Contract insuredPersonEx, Contract insuredPerson)
        {
            if (insuredPersonEx.Relation != insuredPerson.Relation)
                return true;
            if (insuredPersonEx.ProductName != insuredPerson.ProductName)
                return true;
            if (insuredPersonEx.Loai != insuredPerson.Loai)
                return true;
            if (insuredPersonEx.PhiBaoHiem != insuredPerson.PhiBaoHiem)
                return true;
            if (insuredPersonEx.NgayDaoHan != insuredPerson.NgayDaoHan)
                return true;
            if (insuredPersonEx.NgayKetThucDongPhi != insuredPerson.NgayKetThucDongPhi)
                return true;
            if (insuredPersonEx.Status != insuredPerson.Status)
                return true;
            if (insuredPersonEx.TelCompany != insuredPerson.TelCompany)
                return true;
            if (insuredPersonEx.TelHome != insuredPerson.TelHome)
                return true;
            if (insuredPersonEx.TelMobile != insuredPerson.TelMobile)
                return true;
            if (insuredPersonEx.CustomerCMND != insuredPerson.CustomerCMND)
                return true;

            return false;
        }

        private bool IsNTHChanged(Contract insuredPersonEx, Contract insuredPerson)
        {
            if (insuredPersonEx.Relation != insuredPerson.Relation)
                return true;            
            if (insuredPersonEx.TelCompany != insuredPerson.TelCompany)
                return true;
            if (insuredPersonEx.TelHome != insuredPerson.TelHome)
                return true;
            if (insuredPersonEx.TelMobile != insuredPerson.TelMobile)
                return true;
            if (insuredPersonEx.CustomerCMND != insuredPerson.CustomerCMND)
                return true;

            return false;
        }

        private void GetCustomerInfoTabSanPham(Customer customerEx, string customerCode, HtmlElement htmlElementTaga)
        {
            try
            {
                //Open Modal
                htmlElementTaga.Focus();
                htmlElementTaga.InvokeMember("Click");
                WaitTimeSecond(3);
                OpenModal();
                //Wait load data
                DisplayDataModal();
                //Read Customer data
                customerEx.Code = customerCode.Trim();
                HtmlElementCollection divs = webBrowser.Document.GetElementsByTagName("div");
                if (divs.Count > 0)
                {
                    foreach (HtmlElement div in divs)
                    {
                        string att_active = div.GetAttribute("uib-modal-window");
                        if (att_active == "modal-window")
                        {
                            //Get Customer Info
                            HtmlElementCollection inputs = div.GetElementsByTagName("input");
                            string address = string.Empty;
                            foreach (HtmlElement input in inputs)
                            {
                                string att = input.GetAttribute("ng-model");

                                if (att.Contains("customer.idNum"))
                                {
                                    customerEx.CMND = input.GetAttribute("value").Trim();
                                }

                                if (att.Contains("customer.fullName"))
                                {
                                    customerEx.Name = input.GetAttribute("value").Trim();
                                }

                                //DOB
                                if (att.Contains("customer.birthDate"))
                                {
                                    //cus.DOB = input.InnerText;
                                    //string dobinner = input.InnerText;
                                    //string dob = input.InnerHtml;
                                    string dob1 = input.GetAttribute("value");
                                    if (!string.IsNullOrEmpty(dob1))
                                    {
                                        string[] arr = dob1.Split('/');
                                        if (arr.Length == 3)
                                        {
                                            customerEx.DOB = new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0]));
                                        }
                                    }
                                }

                                //Address
                                if (att.Contains("customer.address") || att.Contains("customer.wardDesc") || att.Contains("customer.districtDesc") || att.Contains("customer.cityDesc"))
                                {                                    
                                    if (att.Contains("customer.districtDesc"))
                                    {
                                        customerEx.District = input.GetAttribute("value").Trim();
                                    }

                                    if (att.Contains("customer.cityDesc"))
                                    {
                                        customerEx.City = input.GetAttribute("value").Trim();
                                    }

                                    if (!string.IsNullOrEmpty(input.GetAttribute("value")))
                                    {
                                        address += input.GetAttribute("value") + " ";
                                    }
                                }

                                //Phone
                                if (att.Contains("customer.mobilePhone"))
                                {
                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                    {
                                        customerEx.Phone = input.GetAttribute("value");
                                        customerEx.Phone = ConvertMobile(customerEx.Phone);
                                    }
                                }

                                //TelCompany
                                if (att.Contains("customer.bizPhone"))
                                {
                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                    {
                                        customerEx.TelCompany = input.GetAttribute("value");
                                        customerEx.TelCompany = ConvertMobile(customerEx.TelCompany);
                                    }
                                }

                                //TelHome
                                if (att.Contains("customer.homePhone"))
                                {
                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                    {
                                        customerEx.TelHome = input.GetAttribute("value");
                                        customerEx.TelHome = ConvertMobile(customerEx.TelHome);
                                    }
                                }

                                //Email
                                if (att.Contains("customer.email"))
                                {
                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                    {
                                        customerEx.Email = input.GetAttribute("value");
                                    }
                                }

                                //Gender
                                if (att.Contains("customer.gender"))
                                {
                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                    {
                                        customerEx.Gender = input.GetAttribute("value");
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(address.Trim()))
                            {
                                customerEx.Address = address.Trim();
                            }

                            //Get Contract Info
                            HtmlElementCollection tag_uls = div.GetElementsByTagName("ul");
                            if (tag_uls.Count > 0)
                            {
                                foreach (HtmlElement ul in tag_uls)
                                {
                                    string ul_class = ul.GetAttribute("className");
                                    if (!string.IsNullOrEmpty(ul_class) && ul_class.Contains("nav-tabs"))
                                    {
                                        HtmlElementCollection tag_as = ul.GetElementsByTagName("a");
                                        if (tag_as.Count > 0)
                                        {
                                            //tab Thong tin hop dong
                                            tag_as[1].Focus();
                                            tag_as[1].InvokeMember("Click");
                                            WaitTimeSecond(2);
                                            //Doc danh sach hop dong
                                            int x = GetContractNumberData(div);
                                            if (IsPagingContractNumber(div))
                                            {
                                                while (doPageContractNext(div) == true)
                                                {
                                                    GetContractNumberData(div);
                                                }
                                            }

                                            if (x > 0)
                                            {
                                                customerEx.ExistContractFlg = "Y";
                                            }
                                            else
                                            {
                                                customerEx.ExistContractFlg = "N";
                                            }
                                        }
                                    }
                                }
                            }

                            //Close Modal
                            HtmlElementCollection buttons = div.GetElementsByTagName("button");
                            if (buttons.Count > 0)
                            {
                                foreach(HtmlElement element in buttons)
                                {
                                    string att = element.GetAttribute("ng-click");
                                    if (!string.IsNullOrEmpty(att) && att.Equals("closeModal()"))
                                    {
                                        element.Focus();
                                        element.InvokeMember("Click");
                                        WaitTimeSecond(2);
                                        CloseModal();
                                    }
                                }                                
                            }

                            break;
                        }
                    }
                }

                //CloseModal();
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private bool CheckForUpdate(Customer customer, Customer cusExist)
        {
            try
            {
                if (customer.Name != cusExist.Name)
                {
                    return true;
                }

                if (ConvertMobile(customer.Phone) != ConvertMobile(cusExist.Phone))
                {
                    return true;
                }

                if (ConvertMobile(customer.TelCompany) != ConvertMobile(cusExist.TelCompany))
                {
                    return true;
                }

                if (ConvertMobile(customer.TelHome) != ConvertMobile(cusExist.TelHome))
                {
                    return true;
                }

                if (customer.Address != cusExist.Address)
                {
                    return true;
                }

                if (customer.City != cusExist.City)
                {
                    return true;
                }

                if (customer.CMND != cusExist.CMND)
                {
                    return true;
                }

                if (customer.District != cusExist.District)
                {
                    return true;
                }

                if (customer.DOB != cusExist.DOB)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private bool IsPagingContractNumber(HtmlElement divParent)
        {
            try
            {
                HtmlElementCollection divs = divParent.GetElementsByTagName("div");
                foreach (HtmlElement div in divs)
                {
                    if (div.GetAttribute("ng-table-pagination") == "params")
                    {
                        HtmlElementCollection lis = div.GetElementsByTagName("li");
                        if (lis.Count > 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private bool doPageContractNext(HtmlElement divParent)
        {
            try
            {
                HtmlElementCollection divs = divParent.GetElementsByTagName("div");
                foreach (HtmlElement div in divs)
                {
                    if (div.GetAttribute("ng-table-pagination") == "params")
                    {
                        HtmlElementCollection lis = div.GetElementsByTagName("li");
                        if (lis.Count > 0)
                        {
                            HtmlElement li_end = lis[lis.Count - 1];
                            string classtag = li_end.GetAttribute("className");
                            if (!string.IsNullOrEmpty(classtag) && classtag == "disabled")
                            {
                                return false;
                            }
                            else
                            {
                                //do click next page                                
                                HtmlElementCollection tagas = li_end.GetElementsByTagName("a");
                                if (tagas.Count > 0)
                                {
                                    HtmlElement a = tagas[0];
                                    a.Focus();
                                    a.InvokeMember("Click");
                                    WaitTimeSecond(2);
                                }

                                break;
                            }
                        }

                        break;
                    }
                }

                WebsiteLoading();

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private int GetContractNumberData(HtmlElement div)
        {
            int ret_Constract = 0;
            try
            {
                HtmlElementCollection tag_table = div.GetElementsByTagName("table");
                if (tag_table.Count > 0)
                {
                    HtmlElementCollection tag_tbody = tag_table[0].GetElementsByTagName("tbody");
                    if (tag_tbody.Count > 0)
                    {
                        HtmlElementCollection tag_trs = tag_tbody[0].GetElementsByTagName("tr");
                        foreach (HtmlElement tr in tag_trs)
                        {
                            Contract contract = new Contract();
                            HtmlElementCollection tag_tds = tr.GetElementsByTagName("td");
                            foreach (HtmlElement td in tag_tds)
                            {
                                string title = td.GetAttribute("data-title-text");
                                if (title.Equals("Số hợp đồng"))
                                {
                                    HtmlElementCollection tag_as = td.GetElementsByTagName("a");
                                    if (tag_as.Count > 0)
                                    {
                                        contract.Code = tag_as[0].InnerText.Trim();
                                        contract.DataComplete = "N";
                                        break;
                                    }
                                }
                            }

                            //check exist
                            Contract ex_contract = db.Contracts.AsNoTracking().FirstOrDefault(a => a.Code == contract.Code);
                            if (ex_contract == null)
                            {
                                contract.CreateDate = DateTime.Now;
                                db.Contracts.Add(contract);
                                db.SaveChanges();

                                ConstractTMP constractTMP = new ConstractTMP();
                                constractTMP.Code = contract.Code;
                                constractTMP.CreateDate = contract.CreateDate;
                                constractTMP.DataComplete = contract.DataComplete;
                                constractTMP.OriginID = contract.ID;
                                constractTMP.PCNumber = "01";

                                db.ConstractTMPs.Add(constractTMP);
                                db.SaveChanges();

                                ret_Constract++;
                            }
                            else
                            {
                                ret_Constract++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                db.Dispose();
                db = new PruDataEntities();
            }

            return ret_Constract;
        }

        private void WaitDisplayDataComplete()
        {
            bool ret = CheckDisplayDataComplete();
            while (ret == false)
            {
                Application.DoEvents();
                ret = CheckDisplayDataComplete();
            }
        }

        private bool CheckDisplayDataComplete()
        {
            try
            {
                HtmlElementCollection inputs = webBrowser.Document.GetElementsByTagName("input");
                if (inputs.Count > 0)
                {
                    foreach (HtmlElement input in inputs)
                    {
                        string name = input.GetAttribute("name");
                        if (!string.IsNullOrEmpty(name) && name.Equals("statusProposal"))
                        {
                            if (!string.IsNullOrEmpty(input.GetAttribute("value")))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private bool doPageNext()
        {
            try
            {
                HtmlElementCollection divs = webBrowser.Document.GetElementsByTagName("div");
                foreach (HtmlElement div in divs)
                {
                    if (div.GetAttribute("ng-table-pagination") == "params")
                    {
                        HtmlElementCollection lis = div.GetElementsByTagName("li");
                        if (lis.Count > 0)
                        {
                            HtmlElement li_end = lis[lis.Count - 1];
                            string classtag = li_end.GetAttribute("className");
                            if (!string.IsNullOrEmpty(classtag) && classtag == "disabled")
                            {
                                return false;
                            }
                            else
                            {
                                //do click next page                                
                                HtmlElementCollection tagas = li_end.GetElementsByTagName("a");
                                if (tagas.Count > 0)
                                {
                                    HtmlElement a = tagas[0];
                                    a.Focus();
                                    a.InvokeMember("Click");
                                }

                                break;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                WebsiteLoading();

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        public void doSearch()
        {
            try
            {
                HtmlElementCollection buttons = webBrowser.Document.GetElementsByTagName("button");
                foreach (HtmlElement button in buttons)
                {
                    if (button.GetAttribute("ng-click") == "vm.searchByConstractNum()")
                    {
                        button.Focus();
                        button.InvokeMember("Click");

                        break;
                    }
                }

                WebsiteLoading();
                WaitTimeSecond(3);
                if (IsClosingModal() == false)
                {
                    HtmlElementCollection elements = webBrowser.Document.GetElementsByTagName("div");

                    if (elements.Count > 0)
                    {
                        foreach(HtmlElement element in elements)
                        {
                            string att_class = element.GetAttribute("className");
                            if (!string.IsNullOrEmpty(att_class) && att_class.Contains("modal-dialog"))
                            {
                                HtmlElementCollection button1s = element.GetElementsByTagName("button");
                                if (button1s.Count > 0)
                                {
                                    foreach (HtmlElement button in button1s)
                                    {
                                        string att = button.GetAttribute("ng-click");
                                        if (!string.IsNullOrEmpty(att) && att == "updateClientDlgCtrl.cancel()")
                                        {
                                            button.Focus();
                                            button.InvokeMember("Click");

                                            CloseModal();
                                        }
                                    }
                                }
                            }
                        }                        
                    }                    
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void WaitTimeSecond(int v)
        {
            try
            {
                DateTime t_end = DateTime.Now.AddSeconds(v);
                while (t_end > DateTime.Now)
                {
                    Application.DoEvents();
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void WebsiteLoadingTab(string tab)
        {
            try
            {
                bool isloading = CheckWebsiteLoadingTab(tab);
                while (isloading == true)
                {
                    Application.DoEvents();
                    isloading = CheckWebsiteLoadingTab(tab);
                    if (dtEndLoadWebsite < DateTime.Now)
                    {
                        isloading = false;
                    }

                    if (isStopGetData)
                    {
                        isloading = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void WebsiteLoading()
        {
            try
            {
                bool isloading = CheckWebsiteLoading();
                while (isloading == true)
                {
                    Application.DoEvents();
                    isloading = CheckWebsiteLoading();
                    if (isStopGetData)
                    {
                        isloading = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void OpenModal()
        {
            try
            {
                bool isloading = IsLoadingModal();
                while (isloading == true)
                {
                    Application.DoEvents();
                    isloading = IsLoadingModal();
                    if (isStopGetData)
                    {
                        isloading = false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void DisplayDataModal()
        {
            try
            {
                bool isloading = IsLoadedModal();
                while (isloading == false)
                {
                    Application.DoEvents();
                    isloading = IsLoadedModal();
                    if (isStopGetData)
                    {
                        isloading = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void CloseModal()
        {
            try
            {
                bool isloading = IsClosingModal();
                while (isloading == false)
                {
                    Application.DoEvents();
                    isloading = IsClosingModal();
                    if (isStopGetData)
                    {
                        isloading = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void SetConstractNumber(string constract)
        {
            try
            {
                if (webBrowser.ReadyState == WebBrowserReadyState.Complete)
                {
                    HtmlElement textbox = webBrowser.Document.GetElementById("policyNum");
                    if (textbox != null)
                    {
                        textbox.Focus();
                        textbox.InnerText = constract;                        
                    }                    
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void txtTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)
                //&& (e.KeyChar != '.')
                )
            {
                e.Handled = true;
            }

            // only allow one decimal point
            //if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            //{
            //    e.Handled = true;
            //}
        }

        private void ExportCustomer_Load(object sender, EventArgs e)
        {
            btnStart.BackColor = Color.LightGray;
            btnStart.Enabled = false;

            btnStop.Enabled = false;
            btnStop.BackColor = Color.LightGray;

            lblProcess.Text = "";
            //GetContractNumber
            recordFound = GetContractNumber();
            txtContractNumber.Text = recordFound.ToString();

            processStatus = ProcessStatus.Init;
        }

        private void DisplayProcessStatus()
        {
            if (processStatus == ProcessStatus.Constract_Search_Processing)
                lblProcess.Text = "Lấy dữ liệu HĐ...";
            else if (processStatus == ProcessStatus.Constract_Tab_Phi_Processing)
                lblProcess.Text = "Lấy DL Tab Phí...";
            else if (processStatus == ProcessStatus.Constract_Tab_SanPham_Processing)
                lblProcess.Text = "Lấy DL Tab Sản phẩm...";
            else if (processStatus == ProcessStatus.Constract_Tab_Daily_Processing)
                lblProcess.Text = "Lấy DL Tab Đại lý...";
            else if (processStatus == ProcessStatus.Constract_Tab_GiaTriTienMat_Processing)
                lblProcess.Text = "Lấy DL Tab GTTM...";
            else if (processStatus == ProcessStatus.Constract_Tab_NguoiThuHuong_Processing)
                lblProcess.Text = "Lấy DL Tab NTH...";
            else if (processStatus == ProcessStatus.Constract_Tab_DiaChiHienTai_Processing)
                lblProcess.Text = "Lấy DL Tab ĐCHT...";
            else
                lblProcess.Text = "";
        }

        private int GetContractNumber()
        {
            try
            {
                int contractNumber = db.ConstractTMPs.Where(a => a.DataComplete.Equals("N") && a.PCNumber.Equals(machineNo)).ToList().Count;
                return contractNumber;
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                //if (MessageBox.Show("Chương trình đang thực hiện lấy dữ liệu.\nBạn xác nhận muốn dừng lấy dữ liệu?","Thông báo", MessageBoxButtons.YesNo) == DialogResult.Yes)
                //{
                isStopGetData = true;
                //}
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void CheckTimeComplete()
        {
            if (DateTime.Now >= dtEnd)
            {
                isStopGetData = true;
            }
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser.Url.OriginalString.ToLower().Equals(UrlConstractInfo.ToLower()))
            {
                btnStart.Enabled = true;
                btnStart.BackColor = Color.LightGreen;
            }
            else
            {
                btnStart.Enabled = false;
                btnStart.BackColor = Color.LightGray;
            }
        }
    }
}
