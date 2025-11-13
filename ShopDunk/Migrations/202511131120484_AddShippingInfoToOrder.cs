namespace ShopDunk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShippingInfoToOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "ShippingAddress", c => c.String(nullable: false));
            AddColumn("dbo.Orders", "PhoneNumber", c => c.String(nullable: false));
            AddColumn("dbo.Orders", "PaymentMethod", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "PaymentMethod");
            DropColumn("dbo.Orders", "PhoneNumber");
            DropColumn("dbo.Orders", "ShippingAddress");
        }
    }
}
