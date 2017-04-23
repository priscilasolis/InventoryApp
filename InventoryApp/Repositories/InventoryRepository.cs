﻿using InventoryApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace InventoryApp.Repositories
{
    public class InventoryRepository
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public Item CreateItem(ItemViewModel itemViewModel, HttpPostedFileBase imageFile)
        {
            Item item = new Item
            {
                Name = itemViewModel.Name,
                Price = itemViewModel.Price,
                Description = itemViewModel.Description,
                Quantity = itemViewModel.Quantity,
                Threshold = itemViewModel.Threshold
            };

            if (imageFile != null && imageFile.IsImage())
            {
                var pictureStream = imageFile.InputStream;
                byte[] picture = new byte[pictureStream.Length];
                pictureStream.Read(picture, 0, picture.Length);

                item.Picture = picture;
                item.MimeType = imageFile.ContentType;
            }

            db.Inventory.Add(item);
            db.SaveChanges();

            return item;
        }

        public List<Item> GetInventory()
        {
            return db.Inventory.ToList();
        }

        public Item GetItem(int id)
        {
            Item item = db.Inventory.Find(id);
            return item;
        }


        public Item EditItem(ItemViewModel itemViewModel, HttpPostedFileBase imageFile)
        {
            Item item = new Item
            {
                Name = itemViewModel.Name,
                Price = itemViewModel.Price,
                Description = itemViewModel.Description,
                Quantity = itemViewModel.Quantity,
                Threshold = itemViewModel.Threshold
            };

            if (imageFile != null && imageFile.IsImage())
            {
                var pictureStream = imageFile.InputStream;
                byte[] picture = new byte[pictureStream.Length];
                pictureStream.Read(picture, 0, picture.Length);

                item.Picture = picture;
                item.MimeType = imageFile.ContentType;
            }

            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();

            return item;
        }

        public void EditItem(int id, ItemViewModel itemViewModel)
        {
            Item item = db.Inventory.Find();
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
        }

        public Item MakePurchase(int id, int quantity)
        {
            Item item = db.Inventory.Find(id);
            item.Quantity += quantity;
            return item;
        }

        public bool CreateSale(int itemId, int quantity, ApplicationUser user)
        {
            Item item = db.Inventory.Find(itemId);
            
            if (item == null)
            {
                return false;
            }
            else if (item.Quantity - quantity < 0 || quantity == 0)
            {
                return false;
            }

            Order order = new Order
            {
                ItemId = item.Id,
                Date = DateTime.UtcNow,
                ApplicationUserId = user.Id,
                Type = Order.OrderType.Sale,
                Quantity = quantity,
                IsActive = true
            };

            item.Quantity -= quantity;

            db.Entry(item).State = EntityState.Modified;
            db.Orders.Add(order);
            db.SaveChanges();

            return true;
        }

        public List<Item> FindItems(string query)
        {
            query = query.ToLower().RemoveDiacritics();
            /*var list = db.Inventory.Select(i => new { Name = i.Name, Id = i.Id }).ToList();

            var ids = list.Where(i => i.Name.ToLower().RemoveDiacritics().Contains(query));

            if (!ids.Any())
            {
                return new List<Item>();
            }*/
            return db.Inventory.ToList().Where(i => i.Name.ToLower().RemoveDiacritics().Contains(query)).ToList();
        }

        public bool IsSalePossible(int itemId, int quantity)
        {
            Item item = db.Inventory.Find(itemId);

            if (item == null)
            {
                return false;
            }
            else if (item.Quantity - quantity < 0 || quantity == 0)
            {
                return false;
            }
            return true;
        }

        //public Results CreateSales(List<>)

        public bool CancelSale(int orderId)
        {
            Order order = db.Orders.Find(orderId);
            Item item = db.Inventory.Find(order.ItemId);

            if (item == null || order == null)
            {
                return false;
            }
            else if (order.Type != Order.OrderType.Sale || !order.IsActive)
            {
                return false;
            }

            order.IsActive = false;
            item.Quantity += order.Quantity;

            db.Entry(item).State = EntityState.Modified;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();

            return true;
        }

        public void RemoveItem(int id)
        {
            Item item = db.Inventory.Find(id);
            db.Inventory.Remove(item);
            db.SaveChanges();
        }

        //public void MakeSale(int id, )

    }
}