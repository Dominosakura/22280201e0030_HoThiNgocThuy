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
    
    public partial class ShoppingCartItem
    {
        public int CartItemID { get; set; }
        public Nullable<int> CartID { get; set; }
        public Nullable<int> ProductID { get; set; }
        public int Quantity { get; set; }
    public virtual ProductImage ProductImage { get; set; }
        public virtual Product Product { get; set; }
        public virtual ShoppingCart ShoppingCart { get; set; }
    }
}