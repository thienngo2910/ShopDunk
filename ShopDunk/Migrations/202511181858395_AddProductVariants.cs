namespace ShopDunk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductVariants : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductVariants",
                c => new
                    {
                        VariantID = c.Int(nullable: false, identity: true),
                        ProductID = c.Int(nullable: false),
                        Color = c.String(nullable: false),
                        Storage = c.String(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Stock = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VariantID)
                .ForeignKey("dbo.Products", t => t.ProductID, cascadeDelete: true)
                .Index(t => t.ProductID);
            
            AddColumn("dbo.CartItems", "Color", c => c.String());
            AddColumn("dbo.CartItems", "Storage", c => c.String());
            AddColumn("dbo.OrderDetails", "Color", c => c.String());
            AddColumn("dbo.OrderDetails", "Storage", c => c.String());
            AlterColumn("dbo.Products", "Category", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductVariants", "ProductID", "dbo.Products");
            DropIndex("dbo.ProductVariants", new[] { "ProductID" });
            AlterColumn("dbo.Products", "Category", c => c.String(nullable: false));
            DropColumn("dbo.OrderDetails", "Storage");
            DropColumn("dbo.OrderDetails", "Color");
            DropColumn("dbo.CartItems", "Storage");
            DropColumn("dbo.CartItems", "Color");
            DropTable("dbo.ProductVariants");
        }
    }
}
