using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAL;

namespace ConvertToolApp
{
    public partial class ExportCustomer : Form
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string AddressCustomer = "https://prugreat.prudential.com.vn/prugreat/ga/history/#/customerInfo";

        private PruDataEntities db = new PruDataEntities();
        private int recordFound = 0;
        private int recordSave = 0;
        private int recordIngore = 0;
        private int recordUpdate = 0;

        private bool isProcessComplet = true;
        private bool isStopGetData = true;

        private DateTime dtStart;
        private DateTime dtEnd;

        private DateTime dtSearch;

        public ExportCustomer()
        {
            InitializeComponent();
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
            catch (Exception ex)
            {
                log.Error(ex.Message);
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
            catch(Exception ex)
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
                    foreach(HtmlElement element in elements)
                    {
                        string att_class = element.Style;
                        if (att_class.Contains("z-index: 1050"))
                        {
                            HtmlElementCollection inputs = webBrowser.Document.GetElementsByTagName("input");
                            if (inputs.Count > 0)
                            {
                                foreach(HtmlElement input in inputs)
                                {
                                    string t = input.GetAttribute("ng-model");
                                    if (t.Contains("customer.clientCode"))
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
                    if (att_class.Contains("modal-open"))
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
                foreach(HtmlElement item in collections)
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
                //isProcessComplet = false;
                if (!string.IsNullOrEmpty(txtTime.Text))
                {                    
                    if (webBrowser.ReadyState == WebBrowserReadyState.Complete)
                    {
                        string curUrl = webBrowser.Url.OriginalString;
                        if (curUrl.ToLower().Equals(AddressCustomer.ToLower()))
                        {
                            isProcessComplet = false;

                            txtTime.Enabled = false;
                            btnStart.Enabled = false;
                            btnStart.BackColor = Color.LightGray;
                            btnStop.Enabled = true;
                            btnStop.BackColor = Color.Red;

                            recordFound = 0;
                            recordSave = 0;
                            recordIngore = 0;
                            recordUpdate = 0;

                            txtFoundRecord.Text = recordFound.ToString();
                            txtIngoreRecord.Text = recordIngore.ToString();
                            txtSaveRecord.Text = recordSave.ToString();
                            txtRecordUpdate.Text = recordUpdate.ToString();

                            dtStart = DateTime.Now;
                            dtEnd = dtStart.AddMinutes(int.Parse(txtTime.Text));

                            //------------------------------------
                            isStopGetData = false;
                            while (isStopGetData == false)
                            {
                                dtSearch = dtDOB.Value;
                                string dob = dtDOB.Text;    //"16/12/1983";
                                SetDOBVaule(dob);
                                
                                StartSearch();
                                CheckTimeComplete();
                                dtDOB.Value = dtSearch.AddDays(1);
                                if (dtDOB.Value >= DateTime.Today)
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
                            MessageBox.Show("Hãy vào mục 'Thông tin KH' để thực hiện", "Thông báo");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Hãy nhập SỐ PHÚT để chạy chương trình", "Thông báo");
                    txtTime.Focus();
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
                isProcessComplet = true;
            }
        }

        private void StartSearch()
        {
            try
            {
                doSearch();
                WaitTimeSecond(2);                

                tabControl1.TabPages[0].Text = "";

                //====== Get Record Found ===================
                if (isStopGetData != true)
                {
                    HtmlElementCollection divs = webBrowser.Document.GetElementsByTagName("div");
                    foreach (HtmlElement div in divs)
                    {
                        if (div.GetAttribute("className") == "card-header")
                        {
                            HtmlElementCollection spans = div.GetElementsByTagName("span");
                            if (spans.Count > 0)
                            {
                                try
                                {
                                    int a = int.Parse(txtFoundRecord.Text);
                                    int b = 0;
                                    if (!string.IsNullOrEmpty(spans[0].InnerText.Trim()))
                                    {
                                        b = int.Parse(spans[0].InnerText.Trim());
                                    }
                                    
                                    txtFoundRecord.Text = (a + b).ToString();
                                }
                                catch(Exception ex)
                                {
                                    log.Error(ex.Message);
                                }                                
                            }

                            break;
                        }
                    }

                    getDataCustomer();
                }                
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void getDataCustomer()
        {
            try
            {                
                if (txtFoundRecord.Text != "0")
                {                    
                    getDataCustomerInfo();                 
                    while (doPageNext() == true)
                    {                        
                        getDataCustomerInfo();
                    }
                }                
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void getDataCustomerInfo()
        {
            try
            {
                WaitDisplayDataComplete();

                HtmlElementCollection table = webBrowser.Document.GetElementsByTagName("tbody");
                if (table.Count > 0)
                {
                    HtmlElementCollection elements = table[0].GetElementsByTagName("tr");
                    foreach (HtmlElement tr in elements)
                    {
                        HtmlElementCollection tds = tr.GetElementsByTagName("td");
                        if (tds.Count > 0)
                        {
                            Customer customer = new Customer();
                            bool isNew = true;
                            HtmlElement a_code = null;
                            foreach (HtmlElement td in tds)
                            {
                                string title = td.GetAttribute("data-title-text");

                                if (title == "Mã số KH")
                                {
                                    HtmlElementCollection tagas = td.GetElementsByTagName("a");
                                    if (tagas.Count > 0)
                                    {
                                        a_code = tagas[0];
                                        customer.Code = tagas[0].InnerText;                                        
                                    }
                                }

                                if (title == "Số CMND/ GPKD")
                                {
                                    customer.CMND = td.InnerText;
                                }

                                if (title == "Họ và tên KH/ Tên doanh nghiệp")
                                {
                                    customer.Name = td.InnerText;
                                }

                                if (title == "Quận/ huyện")
                                {
                                    customer.District = td.InnerText;
                                }

                                if (title == "Tỉnh/ TP")
                                {
                                    customer.City = td.InnerText;
                                }
                            }

                            if (a_code != null)
                            {
                                a_code.Focus();
                                a_code.InvokeMember("Click");
                                OpenModal();
                                WaitTimeSecond(2);
                                DisplayDataModal();

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
                                                            customer.DOB = new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0]));
                                                        }
                                                    }
                                                }

                                                //Address
                                                if (att.Contains("customer.address") || att.Contains("customer.wardDesc") || att.Contains("customer.districtDesc") || att.Contains("customer.cityDesc"))
                                                {
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
                                                        customer.Phone = input.GetAttribute("value");
                                                        customer.Phone = ConvertMobile(customer.Phone);
                                                    }
                                                }

                                                //TelCompany
                                                if (att.Contains("customer.bizPhone"))
                                                {
                                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                                    {
                                                        customer.TelCompany = input.GetAttribute("value");
                                                        customer.TelCompany = ConvertMobile(customer.TelCompany);
                                                    }
                                                }

                                                //TelHome
                                                if (att.Contains("customer.homePhone"))
                                                {
                                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                                    {
                                                        customer.TelHome = input.GetAttribute("value");
                                                        customer.TelHome = ConvertMobile(customer.TelHome);
                                                    }
                                                }

                                                //Email
                                                if (att.Contains("customer.email"))
                                                {
                                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                                    {
                                                        customer.Email = input.GetAttribute("value");                                                        
                                                    }
                                                }

                                                //Gender
                                                if (att.Contains("customer.gender"))
                                                {
                                                    if (!string.IsNullOrEmpty(input.GetAttribute("value").Trim()))
                                                    {
                                                        customer.Gender = input.GetAttribute("value");
                                                    }
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(address.Trim()))
                                            {
                                                customer.Address = address.Trim();
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
                                                            int Is_Have_constract = GetContractNumberData(div);
                                                            if (IsPagingContractNumber(div))
                                                            {
                                                                while (doPageContractNext(div) == true)
                                                                {
                                                                    GetContractNumberData(div);
                                                                }
                                                            }

                                                            if (Is_Have_constract > 0)
                                                            {
                                                                customer.ExistContractFlg = "Y";
                                                            }
                                                            else
                                                            {
                                                                customer.ExistContractFlg = "N";
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            //Close Modal
                                            HtmlElementCollection buttons = div.GetElementsByTagName("button");
                                            if (buttons.Count > 0)
                                            {
                                                buttons[0].Focus();
                                                buttons[0].InvokeMember("Click");

                                                CloseModal();
                                            }

                                            break;
                                        }
                                    }
                                }
                            }

                            //Check customer exist
                            Customer cusExist = db.Customers.AsNoTracking().FirstOrDefault(a => a.Code == customer.Code);
                            if (cusExist != null) //Update 
                            {
                                if (CheckForUpdate(customer, cusExist))
                                {
                                    cusExist = db.Customers.FirstOrDefault(a => a.ID == cusExist.ID);

                                    cusExist.Name = customer.Name;
                                    cusExist.Phone = ConvertMobile(customer.Phone);
                                    cusExist.TelCompany = ConvertMobile(customer.TelCompany);
                                    cusExist.TelHome = ConvertMobile(customer.TelHome);
                                    cusExist.Address = customer.Address;
                                    cusExist.City = customer.City;
                                    cusExist.CMND = customer.CMND;
                                    cusExist.District = customer.District;
                                    cusExist.DOB = customer.DOB;
                                    cusExist.UpdateDate = DateTime.Now;
                                    cusExist.Gender = customer.Gender;
                                    cusExist.Email = customer.Email;
                                    cusExist.ExistContractFlg = customer.ExistContractFlg;
                                    db.Entry(cusExist).State = EntityState.Modified;
                                    db.SaveChanges();

                                    recordUpdate++;
                                    txtRecordUpdate.Text = recordUpdate.ToString();
                                }
                                else
                                {
                                    recordIngore++;
                                    txtIngoreRecord.Text = recordIngore.ToString();
                                }
                            }
                            else    //New
                            {
                                customer.CreateDate = DateTime.Now;
                                db.Customers.Add(customer);
                                db.SaveChanges();

                                recordSave++;
                                txtSaveRecord.Text = recordSave.ToString();
                            }
                        }
                    }
                }                
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
                db = new PruDataEntities();
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

                if (customer.Email != cusExist.Email)
                {
                    return true;
                }

                if (customer.Gender != cusExist.Gender)
                {
                    return true;
                }

                if (customer.ExistContractFlg != cusExist.ExistContractFlg)
                {
                    return true;
                }

                return false;
            }
            catch(Exception ex)
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
                        foreach(HtmlElement tr in tag_trs)
                        {
                            Contract contract = new Contract();
                            HtmlElementCollection tag_tds = tr.GetElementsByTagName("td");
                            foreach(HtmlElement td in tag_tds)
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
                                //contract.ParentID = 0;
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
            catch(Exception ex)
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
                HtmlElementCollection table = webBrowser.Document.GetElementsByTagName("tbody");
                if (table.Count > 0)
                {
                    HtmlElementCollection elements = table[0].GetElementsByTagName("tr");
                    if (elements.Count > 0)
                    {
                        foreach (HtmlElement tr in elements)
                        {
                            HtmlElementCollection tds = tr.GetElementsByTagName("td");
                            if (tds.Count > 0)
                            {
                                foreach (HtmlElement td in tds)
                                {
                                    string title = td.GetAttribute("data-title-text");

                                    if (title == "Mã số KH")
                                    {
                                        HtmlElementCollection tagas = td.GetElementsByTagName("a");
                                        if (tagas.Count > 0)
                                        {
                                            string code = tagas[0].InnerText;
                                            if (!string.IsNullOrEmpty(code))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
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
                                    WaitTimeSecond(2);
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
            catch(Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        public void doSearch()
        {
            try
            {
                // do any background work
                HtmlElementCollection buttons = webBrowser.Document.GetElementsByTagName("button");
                foreach (HtmlElement button in buttons)
                {
                    if (button.GetAttribute("ng-click") == "vm.getCustomer()")
                    {
                        button.Focus();
                        button.InvokeMember("Click");
                        tabControl1.TabPages[0].Text = "Searching...";
                        WaitTimeSecond(2);
                        break;
                    }
                }

                WebsiteLoading();                
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void WebsiteLoading()
        {
            try
            {
                DateTime waitTime = DateTime.Now.AddSeconds(60);
                bool isloading = CheckWebsiteLoading();
                while (isloading == true)
                {
                    Application.DoEvents();
                    isloading = CheckWebsiteLoading();
                    if (waitTime < DateTime.Now)
                    {
                        isloading = false;
                    }
                }                
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void OpenModal()
        {
            try
            {
                DateTime waitTime = DateTime.Now.AddSeconds(5);
                bool isloading = IsLoadingModal();
                while (isloading == true)
                {
                    Application.DoEvents();
                    isloading = IsLoadingModal();
                    if (waitTime < DateTime.Now)
                    {
                        isloading = false;
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);                
            }
        }

        private void DisplayDataModal()
        {
            try
            {
                DateTime waitTime = DateTime.Now.AddSeconds(5);
                bool isloading = IsLoadedModal();
                while (isloading == false)
                {
                    Application.DoEvents();
                    isloading = IsLoadedModal();
                    if (waitTime < DateTime.Now)
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
                DateTime waitTime = DateTime.Now.AddSeconds(5);
                bool isloading = IsClosingModal();
                while (isloading == false)
                {
                    Application.DoEvents();
                    isloading = IsClosingModal();
                    if (waitTime < DateTime.Now)
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

        private void SetDOBVaule(string dob)
        {
            try
            {
                HtmlElementCollection textboxs = webBrowser.Document.GetElementsByTagName("input");
                foreach (HtmlElement textbox in textboxs)
                {
                    if (textbox.Name == "clientBirthDate")
                    {
                        textbox.InnerText = dob;
                        textbox.Focus();
                        break;
                    }
                }
            }
            catch(Exception ex)
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
            btnStart.BackColor = Color.LightGray; ;

            btnStart.Enabled = false;
            btnStop.Enabled = false;
            btnStop.BackColor = Color.LightGray;
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
            catch(Exception ex)
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
            if (webBrowser.Url.OriginalString.ToLower().Equals(AddressCustomer.ToLower()))
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
