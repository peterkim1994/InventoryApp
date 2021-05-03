﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using InventoryPOS.DataStore.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPOS.DataStore.Models
{
    [Index(nameof(Value), IsUnique = true)]
    public class ItemCategory : ProductAttribute
    {
        [Key]
        public override int Id { get; set; }
        [Required]
        public override string Value { get; set; }
        public virtual List<Product> Products { get; set; }
    }
}
