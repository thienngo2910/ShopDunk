namespace ShopDunk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SliderImage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SliderImages",
                c => new
                    {
                        SliderImageID = c.Int(nullable: false, identity: true),
                        CategoryKey = c.String(nullable: false),
                        ImageUrl = c.String(),
                        Title = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SliderImageID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SliderImages");
        }
    }
}
