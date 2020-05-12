namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddContactsToSeller : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "Contacts", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "Contacts");
        }
    }
}
