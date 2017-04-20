namespace InventoryApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItemsAndOrders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Quantity = c.Int(nullable: false),
                        Threshold = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(),
                        Picture = c.Binary(),
                        MimeType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quantity = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Type = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        ItemId = c.Int(nullable: false),
                        ApplicationUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .ForeignKey("dbo.Items", t => t.ItemId, cascadeDelete: true)
                .Index(t => t.ItemId)
                .Index(t => t.ApplicationUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "ItemId", "dbo.Items");
            DropForeignKey("dbo.Orders", "ApplicationUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Orders", new[] { "ApplicationUserId" });
            DropIndex("dbo.Orders", new[] { "ItemId" });
            DropTable("dbo.Orders");
            DropTable("dbo.Items");
        }
    }
}
