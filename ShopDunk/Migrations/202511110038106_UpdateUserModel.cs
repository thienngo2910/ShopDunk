namespace ShopDunk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUserModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Username", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Role", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "Role", c => c.String());
            AlterColumn("dbo.Users", "PasswordHash", c => c.String());
            AlterColumn("dbo.Users", "Username", c => c.String(nullable: false));
        }
    }
}
