﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SFH.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DB_FinancialHelperEntities1 : DbContext
    {
        public DB_FinancialHelperEntities1()
            : base("name=DB_FinancialHelperEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tbl_ConstantValues> tbl_ConstantValues { get; set; }
        public virtual DbSet<tbl_Movement> tbl_Movement { get; set; }
    }
}
