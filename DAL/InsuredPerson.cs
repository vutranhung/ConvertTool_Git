//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class InsuredPerson
    {
        public int ID { get; set; }
        public int ContractID { get; set; }
        public int CustomerID { get; set; }
        public string Relation { get; set; }
        public string TenSanPham { get; set; }
        public string Loai { get; set; }
        public Nullable<decimal> Fee { get; set; }
        public Nullable<System.DateTime> NgayDaoHan { get; set; }
        public Nullable<System.DateTime> DongPhiDenNgay { get; set; }
        public string TinhTrang { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string DeleteFlg { get; set; }
    }
}