namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtebdPhoneForPersonel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Personnels", "Phone", c => c.String(maxLength: 24));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Personnels", "Phone", c => c.String(maxLength: 16));
        }
    }
}
