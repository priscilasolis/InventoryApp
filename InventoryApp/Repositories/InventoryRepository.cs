using InventoryApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace InventoryApp.Repositories
{
    public class InventoryRepository
    {
        public ApplicationDbContext DatabaseContext { get { return db; } }
        public UserManager<ApplicationUser> UserManager { get { return userManager; } }

        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManager<ApplicationUser> userManager;

        public InventoryRepository()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

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

        internal object GetOrders()
        {
            return db.Orders.ToList();
        }

        public Order GetOrder(int? id)
        {
            return db.Orders.Find(id);
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

        public List<Order> GetSalesUser(ApplicationUser user)
        {
            return db.Orders.Where(o => o.IsActive && o.ApplicationUserId == user.Id && o.Type == Order.OrderType.Sale).ToList();
        }

        public Item EditItem(Item item, HttpPostedFileBase imageFile)
        {
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

            if (item.Quantity < item.Threshold)
            {
                sendEmailAlert(item);
            }

            return item;
        }

        public void EditItem(int id, ItemViewModel itemViewModel)
        {
            Item item = db.Inventory.Find();
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
        }

        public bool CreatePurchase(int itemId, int quantity, ApplicationUser user)
        {
            Item item = db.Inventory.Find(itemId);

            if (item == null || quantity < 0)
            {
                return false;
            }
            else if (item.Quantity < 0)
            {
                throw new IndexOutOfRangeException();
            }

            Order order = new Order
            {
                ItemId = item.Id,
                Date = DateTime.UtcNow,
                ApplicationUserId = user.Id,
                Type = Order.OrderType.Purchase,
                Quantity = quantity,
                IsActive = true
            };

            item.Quantity += quantity;

            db.Entry(item).State = EntityState.Modified;
            db.Orders.Add(order);
            db.SaveChanges();

            return true;
        }

        public bool CancelPurchase(int orderId)
        {
            Order order = db.Orders.Find(orderId);
            Item item = db.Inventory.Find(order.ItemId);

            if (item == null || order == null)
            {
                return false;
            }
            else if (order.Type != Order.OrderType.Purchase || !order.IsActive)
            {
                return false;
            }

            bool cannotCancel = db.Orders.Any(o => o.Type == Order.OrderType.Sale && o.ItemId == item.Id && o.IsActive && o.Date > order.Date);

            if (cannotCancel)
            {
                return false;
            }

            order.IsActive = false;
            item.Quantity -= order.Quantity;
        
            db.Entry(item).State = EntityState.Modified;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            
            if (item.Quantity < item.Threshold)
            {
                sendEmailAlert(item);
            }

            return true;
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

            if (item.Quantity < item.Threshold)
            {
                sendEmailAlert(item);
            }

            return true;
        }

        public bool CancelOrder(int id)
        {
            Order order = GetOrder(id);
            if (order.Type == Order.OrderType.Sale)
            {
                return CancelSale(id);
            } else if (order.Type == Order.OrderType.Purchase)
            {
                return CancelPurchase(id);
            }
            return false;
        }

        public List<Item> FindItems(string query)
        {
            query = query.ToLower().RemoveDiacritics();
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

        public List<ItemSoldViewModel> GetMostSoldWeek()
        {
            DateTime aWeekAgo = DateTime.UtcNow.AddDays(-7);
            var x = db.Orders.Where(o => o.IsActive && o.Type == Order.OrderType.Sale && o.Date >= aWeekAgo)
                .GroupBy(i => i.Item, i => i.Quantity, (item, quantity) => new ItemSoldViewModel
                {
                    Name = item.Name,
                    QuantitySold = quantity.Sum()
                }).ToList();
            return x.OrderByDescending(i => i.QuantitySold).Take(10).ToList();
        }

        public List<Item> GetItemsByThreshold()
        {
            var behindThreshold = db.Inventory.Where(i => i.Quantity < i.Threshold).OrderBy(i => i.Quantity);
            var length = behindThreshold.Count();

            if (length == 10)
            {
                return behindThreshold.ToList();
            }
            else if (length > 10)
            {
                return behindThreshold.Take(10).ToList();
            }

            var barelyAboveThreshold = db.Inventory.Except(behindThreshold).OrderBy(i => i.Quantity - i.Threshold).ToList();

            List<Item> results = behindThreshold.ToList().Concat(barelyAboveThreshold.ToList()).Take(10).ToList();

            return results;
        }

        public void RemoveItem(int id)
        {
            Item item = db.Inventory.Find(id);
            db.Inventory.Remove(item);
            db.SaveChanges();
        }

        private void sendEmailAlert(Item item)
        {
            MailMessage mail = new MailMessage();
            SmtpClient client = new SmtpClient()
            {
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.gmail.com",
                Credentials = new System.Net.NetworkCredential("erickelias881@gmail.com", "Erick2127")
            };
            List<string> superusersEmails = GetSuperUsers().Select(u => u.Email).ToList();

            foreach (string email in superusersEmails)
            {
                mail.To.Add(new MailAddress(email));
            }
            
            mail.From = new MailAddress("erickelias881@gmail.com");
            mail.Subject = "InventoryApp - Inventory alert";

            string body = "<h1>Inventory alert</h1>";

            body += "<p>The next item is below of its threshold</p><br/>";
            body += string.Format("<p>Name: {0}</p><p>Quantity available: {1}</p><p>Threshold: {2}</p>", item.Name, item.Quantity, item.Threshold);
            mail.Body = body;

            mail.IsBodyHtml = true;
            client.Send(mail);
        }

        public List<ApplicationUser> GetSuperUsers()
        {
            return db.Users.ToList().Where(u => UserManager.IsInRole(u.Id, "Administrator") || UserManager.IsInRole(u.Id, "Supervisor")).ToList();
        }
    }


}