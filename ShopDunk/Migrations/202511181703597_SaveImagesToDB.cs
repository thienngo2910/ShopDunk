namespace ShopDunk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SaveImagesToDB : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "ImageData", c => c.Binary());
            AddColumn("dbo.SliderImages", "ImageData", c => c.Binary());
            DropColumn("dbo.Products", "ImageUrl");
            DropColumn("dbo.SliderImages", "ImageUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SliderImages", "ImageUrl", c => c.String());
            AddColumn("dbo.Products", "ImageUrl", c => c.String());
            DropColumn("dbo.SliderImages", "ImageData");
            DropColumn("dbo.Products", "ImageData");
        }
    }
}
