namespace ShopDunk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductReviews : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductReviews",
                c => new
                    {
                        ProductReviewID = c.Int(nullable: false, identity: true),
                        ProductID = c.Int(nullable: false),
                        UserID = c.Int(nullable: false),
                        Rating = c.Int(nullable: false),
                        Comment = c.String(),
                        ReviewDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProductReviewID)
                .ForeignKey("dbo.Products", t => t.ProductID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.ProductID)
                .Index(t => t.UserID);
            
            AddColumn("dbo.Orders", "Note", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductReviews", "UserID", "dbo.Users");
            DropForeignKey("dbo.ProductReviews", "ProductID", "dbo.Products");
            DropIndex("dbo.ProductReviews", new[] { "UserID" });
            DropIndex("dbo.ProductReviews", new[] { "ProductID" });
            DropColumn("dbo.Orders", "Note");
            DropTable("dbo.ProductReviews");
        }
    }
}
