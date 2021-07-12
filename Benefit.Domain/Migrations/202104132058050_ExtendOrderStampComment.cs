namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendOrderStampComment : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderStatusStamps", "Comment", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderStatusStamps", "Comment", c => c.String(maxLength: 64));
        }
    }
}
