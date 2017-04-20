using InventoryApp.Models;
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

        public void EditItem(Item item)
        {
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
        }

        public Item MakePurchase(int id, int quantity)
        {
            Item item = db.Inventory.Find(id);
            item.Quantity += quantity;
            return item;
        }

        internal void RemoveItem(int id)
        {
            Item item = db.Inventory.Find(id);
            db.Inventory.Remove(item);
            db.SaveChanges();
        }

        //public void MakeSale(int id, )

    }
}