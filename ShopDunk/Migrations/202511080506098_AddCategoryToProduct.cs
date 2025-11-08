namespace ShopDunk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Category", c => c.String());
            AddColumn("dbo.Users", "Password", c => c.String(nullable: false));
            AddColumn("dbo.Users", "ConfirmPassword", c => c.String());
            AlterColumn("dbo.Users", "PasswordHash", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(nullable: false));
            DropColumn("dbo.Users", "ConfirmPassword");
            DropColumn("dbo.Users", "Password");
            DropColumn("dbo.Products", "Category");
        }
    }
}
