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
    public partial class Menu : Form
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Menu()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnGetCustomer_Click(object sender, EventArgs e)
        {
            ExportCustomer exportCustomer = new ExportCustomer();
            exportCustomer.ShowDialog();
        }

        private void btnGetContract_Click(object sender, EventArgs e)
        {
            GetContractData getContractData = new GetContractData();
            getContractData.ShowDialog();
        }

        private void btnExportCustomer_Click(object sender, EventArgs e)
        {
            ExportCustomerData exportCustomerData = new ExportCustomerData();
            exportCustomerData.ShowDialog();
        }

        private void btnExportContract_Click(object sender, EventArgs e)
        {
            ExportConstractData exportConstractData = new ExportConstractData();
            exportConstractData.ShowDialog();
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            //log.Error("abc");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                PruDataEntities db = new PruDataEntities();
                // Fill VanPhong
                List<Contract> contracts = db.Contracts.Where(a => a.DataComplete == "Y" && !string.IsNullOrEmpty(a.VanPhong)).ToList();
                foreach(var constract in contracts)
                {
                    //NMBH
                    Customer customer = db.Customers.FirstOrDefault(a => a.Code == constract.NMBHCode);
                    if (customer != null)
                    {
                        bool isUpdate = false;
                        if (!string.IsNullOrEmpty(customer.VanPhong))
                        {
                            string[] vs = customer.VanPhong.Split(',');
                            bool isExist = false;
                            foreach(string item in vs)
                            {
                                if (item == constract.VanPhong)
                                {
                                    isExist = true;
                                }
                            }

                            if (isExist == false)
                            {
                                customer.VanPhong += "," + constract.VanPhong;
                                isUpdate = true;
                            }
                        }
                        else
                        {
                            customer.VanPhong = constract.VanPhong;
                            isUpdate = true;
                        }

                        if (isUpdate == true)
                        {
                            db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    //Nguoi Duoc Bao Hiem
                    List<InsuredPerson> insuredPeople = db.InsuredPersons.Where(a => a.ContractID == constract.ID).ToList();
                    foreach(var item in insuredPeople)
                    {
                        if (item.CustomerID != customer.ID)
                        {
                            Customer customer1 = db.Customers.FirstOrDefault(a => a.ID == item.CustomerID);
                            if (customer1 != null)
                            {
                                bool isUpdate = false;
                                if (!string.IsNullOrEmpty(customer1.VanPhong))
                                {
                                    string[] vs = customer1.VanPhong.Split(',');
                                    bool isExist = false;
                                    foreach (string v in vs)
                                    {
                                        if (v == constract.VanPhong)
                                        {
                                            isExist = true;
                                        }
                                    }

                                    if (isExist == false)
                                    {
                                        customer1.VanPhong += "," + constract.VanPhong;
                                        isUpdate = true;
                                    }
                                }
                                else
                                {
                                    customer1.VanPhong = constract.VanPhong;
                                    isUpdate = true;
                                }

                                if (isUpdate == true)
                                {
                                    db.Entry(customer1).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            MessageBox.Show("OK");
            Cursor.Current = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                PruDataEntities db = new PruDataEntities();
                // Fill VanPhong
                List<Contract> contracts = db.Contracts.Where(a => a.DataComplete == "Y").ToList();
                foreach (var constract in contracts)
                {
                    //NMBH
                    Customer customer = db.Customers.FirstOrDefault(a => a.Code == constract.NMBHCode);
                    if (customer != null)
                    {
                        Contract contractUpdate = db.Contracts.FirstOrDefault(a => a.ID == constract.ID);
                        if (contractUpdate != null)
                        {
                            contractUpdate.CustomerCMND = customer.CMND;
                            contractUpdate.CustomerName = customer.Name;
                            if (customer.DOB.HasValue)
                            {
                                contractUpdate.DOB = customer.DOB.Value;
                            }

                            contractUpdate.District = customer.District;
                            contractUpdate.City = customer.City;

                            db.Entry(contractUpdate).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }                        
                    }                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show("OK");
            Cursor.Current = Cursors.Default;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ConvertDataToConstract convertDataToConstract = new ConvertDataToConstract();
            convertDataToConstract.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                PruDataEntities db = new PruDataEntities();

                List<Beneficiary> beneficiaries = db.Beneficiaries.ToList();
                foreach(var item in beneficiaries)
                {
                    Contract person = new Contract();
                    person.Code = item.ConstractCode;
                    person.ParentID = item.ConstractID;
                    person.DataComplete = "C";
                    person.NTH = "Y";
                    person.CustomerCode = item.CustomerCode;
                    person.Relation = item.Relation;

                    Customer customerEx = db.Customers.Find(item.CustomerID);
                    if (customerEx != null)
                    {
                        person.CustomerCMND = customerEx.CMND;
                        person.CustomerName = customerEx.Name;
                        person.DOB = customerEx.DOB;
                        person.TelCompany = customerEx.TelCompany;
                        person.TelHome = customerEx.TelHome;
                        person.TelMobile = customerEx.Phone;
                        person.VanPhong = customerEx.VanPhong;
                    }

                    db.Contracts.Add(person);
                    db.SaveChanges();                    
                }

                MessageBox.Show("Chuyển đổi OK");
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);
            }

            Cursor.Current = Cursors.Default;
        }
    }
}
