//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Candy.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Shipping
    {
        public int ShippingID { get; set; }
        public Nullable<int> OrderID { get; set; }
        public string ShippingMethod { get; set; }
        public Nullable<decimal> ShippingCost { get; set; }
        public Nullable<System.DateTime> EstimatedDeliveryDate { get; set; }
    
        public virtual Order Order { get; set; }
    }
}