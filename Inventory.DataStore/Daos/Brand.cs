﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using InventoryPOS.DataStore.Daos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPOS.DataStore.Daos
{
    [Index(nameof(Value), IsUnique = true)]
    public class Brand : ProductAttribute
    {
        [Key]
        public override int Id { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public override string Value { get; set; }
        public virtual List<Product> Products{ get; set;}
    }
}
