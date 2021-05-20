﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using InventoryPOS.DataStore;
using InventoryPOS.DataStore.Models;
using Microsoft.EntityFrameworkCore;
using InventoryPOSApp.Core.Dtos;

namespace InventoryPOSApp.Core.Repositories
{
    public class SalesRepository : ISalesRepository
    {

        private readonly DBContext _context;

        public SalesRepository(DBContext context)
        {
            _context = context;
        }

        public void AddProductPromotion(ProductPromotion productPromotion)
        {
            _context.ProductPromotions.Add(productPromotion);
            _context.SaveChanges();
        }

        public void AddProductToTransaction(int productId, SaleInvoice invoice)
        {
            throw new NotImplementedException();
        }

        public void AddPromotion(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            _context.SaveChanges();
        }


        public void EditPromotion(Promotion promotion)
        {
            _context.Entry<Promotion>(promotion).State = EntityState.Modified;      
            _context.SaveChanges();
        }

        public Promotion GetPromotionByName(string promotionName)
        {
            var product = _context.Promotions.FirstOrDefault(pr => pr.PromotionName == promotionName);
            return product;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void ClearPromotionProducts(int promotionId)
        {
            var productPromos = from pp in _context.ProductPromotions
                                where pp.PromotionId == promotionId
                                select pp;
            _context.ProductPromotions.RemoveRange(productPromos);
            _context.SaveChanges();
        }

        public ProductPromotion GetProductPromotion(int productId, int promotionId)
        {
            return _context.ProductPromotions.FirstOrDefault
                   (
                       pp => pp.ProductId == productId && pp.PromotionId == promotionId
                   );
        }

        public IList<Promotion> GetPromotionsByDateRange(DateTime start, DateTime end)
        {
            var promos = from pr in _context.Promotions.Include(promo => promo.ProductPromotions)
                         where
                            pr.Start <= start &&
                            pr.End >= end
                         select pr;
            return promos.ToList();
        }


        public IList<Promotion> GetActivePromotions()
        {
            var promos = from pr in _context.Promotions.Include(promo => promo.ProductPromotions)
                         where
                            pr.Active == true &&
                            pr.Start <= DateTime.Now &&
                            pr.End >= DateTime.Now
                         select pr;
            return promos.ToList();
        }

        /// <summary>
        /// Returns a hash map/dictionary, keys are product Ids currently included in a promotion.
        /// values are a list of promotions which include that product
        /// </summary>
        /// <returns>A dictionary of products, and all promotions which include the product</returns>
        public Dictionary<int, IList<Promotion>> GetProductActivePromotions()
        {
            var promos = GetActivePromotions();
            Dictionary<int, IList<Promotion>> productPromos = new Dictionary<int, IList<Promotion>>(300);
            foreach(var promo in promos)
            {
                foreach(var prod in promo.ProductPromotions)
                {
                    int prodId = prod.ProductId;
                    if (!productPromos.ContainsKey(prodId))
                    {
                        productPromos[prodId] = new List<Promotion>();
                        productPromos[prodId].Add(promo);
                    }
                    else
                    {
                        productPromos[prodId].Add(promo);
                    }
                }
            }
            return productPromos;
        }

        public IList<Promotion> GetInactivePromotions()
        {
            var promos = from pr in _context.Promotions.Include(promo => promo.ProductPromotions)
                         where
                            pr.Start >= DateTime.Now ||
                            pr.End <= DateTime.Now
                         select pr;
            return promos.ToList();
        }

        public void RemoveProductPromotion(ProductPromotion productPromotion)
        {
            _context.ProductPromotions.Remove(productPromotion);
            SaveChanges();
        }

        public IList<Product> GetPromotionsProducts(int promotionId)
        {
            Promotion promo = _context.Promotions.Find(promotionId);
            if (promo == null)
                return null;
            var products = from
                             pp in _context.ProductPromotions//.Include(pp => pp.Product)                          
                           where
                              pp.PromotionId == promotionId
                           join prod in _context.Products
                                             .Include(pr => pr.Brand)
                                             .Include(pr => pr.Colour)
                                             .Include(pr => pr.Size)
                                             .Include(pr => pr.ItemCategory)
                           on pp.ProductId equals prod.Id
                           select prod;

            return products.ToList();
        }

        public IList<Promotion> GetPromotionsByDate(DateTime rangeStart, DateTime rangeEnd)
        {
            throw new NotImplementedException();
        }

        public Promotion GetPromotion(int promotionId)
        {
            return _context.Promotions.Include(p => p.ProductPromotions).FirstOrDefault(p => p.Id == promotionId);
        }

        public SaleInvoice GetSaleByInvoiceNumber(int invoiceNumber)
        {
            return _context.SalesInvoices.Find(invoiceNumber);
        }

        public SaleInvoice CreateNewSaleInvoice()
        {
            SaleInvoice newSale = new SaleInvoice();
            _context.SalesInvoices.Add(newSale);
            _context.SaveChanges();
            return newSale;
        }

        public ICollection<Product> GetProductsInTransaction(int saleId)
        {
            var products = from sale in _context.ProductSales
                           where sale.SalesInvoiceId == saleId
                           join p in _context.Products
                           on sale.ProductId equals p.Id
                           select p;
            return products.ToList();
        }

        public SaleInvoice GetPreviousSale()
        {
            return _context.SalesInvoices.Last();
        }

        public void ClearProductSales(int saleId)
        {
            var productSales = from ps in _context.ProductSales
                               where ps.SalesInvoiceId == saleId
                               select ps;
            if (productSales.Count() == 0)
                return;
            else{
                _context.ProductSales.RemoveRange(productSales);
                SaveChanges();
            }
        }

        public ICollection<Payment> GetSalesPayments(int saleId)
        {
            var payments = from sale in _context.SalesInvoices
                           where sale.Id == saleId
                           join payment in _context.Payments
                           on sale.Id equals payment.SaleInvoiceId
                           select payment;

            var salePayments = from sale in _context.SalesInvoices
                               from payment in _context.Payments
                               where sale.Id == saleId && sale.Id == payment.SaleInvoiceId
                               join paymentMethod in _context.PaymentMethods
                                    on payment.PaymentMethodId equals paymentMethod.Id
                               select new Payment { PaymentMethod = paymentMethod, Amount = payment.Amount };
            return salePayments.ToList();

        }

        public void AddSalePayment(Payment payment)
        {
            _context.Payments.Add(payment);
            _context.SaveChanges();
        }

        public bool RemovePayment(Payment payment)
        {
            Payment pay = _context.Payments
                                    .FirstOrDefault(p=> 
                                        p.SaleInvoiceId == payment.SaleInvoiceId && 
                                        p.PaymentMethodId == payment.PaymentMethodId &&
                                        p.Amount == payment.Amount                                       
                                    );
            if (pay == null)  return false;           

            _context.Payments.Remove(pay);
            _context.SaveChanges();
            return true;
        }


        public IList<Promotion> GetCurrentPromotions()
        {
            throw new NotImplementedException();
        }


        public bool IsInvoiceFinalised(int saleId)
        {
            var sale = _context.SalesInvoices.Find(saleId);
            return (sale.Finalised == true);
        }


        public void CompleteTransaction(int saleInvoiceId)
        {
            SaleInvoice sale = _context.SalesInvoices.Find(saleInvoiceId);
            sale.Finalised = true;
            _context.Entry(sale).State = EntityState.Modified;
            _context.SaveChanges();
        }


        public void AddProductSale(ProductSale productSale)
        {
            _context.ProductSales.Add(productSale);
            _context.SaveChanges();
        }

        public void DeleteProductSale(ProductSale productSale)
        {
            ProductSale product = _context.ProductSales.FirstOrDefault(p =>
                                                             p.SalesInvoiceId == productSale.SalesInvoiceId &&
                                                             p.ProductId == productSale.ProductId);
            if (product != null)
            {
                _context.ProductSales.Remove(product);
                _context.SaveChanges();
            }
        }

        public void DeleteSaleInvoice(int saleInvoiceId)
        {
            SaleInvoice sale = _context.SalesInvoices.Find(saleInvoiceId);
            //Delete ProductSales?
            _context.SalesInvoices.Remove(sale);
            _context.SaveChanges();
        }

        public SaleInvoice GetSale(int saleId)
        {
            return _context.SalesInvoices.Find(saleId);
        }

        public void DeleteSalePayments(int saleId)
        {
            var payments = from p in _context.Payments
                           where p.SaleInvoiceId == saleId
                           select p;
            _context.RemoveRange(payments);
            SaveChanges();
        }

        public void EditSalePayment(Payment payment)
        {
            //check this works
            var p = _context.Entry<Payment> (payment);
            _context.Remove(p);
            SaveChanges();
        }


    }
}
