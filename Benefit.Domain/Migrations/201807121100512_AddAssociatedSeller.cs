namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAssociatedSeller : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "AssociatedSellerId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Sellers", "AssociatedSellerId");
            AddForeignKey("dbo.Sellers", "AssociatedSellerId", "dbo.Sellers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Sellers", "AssociatedSellerId", "dbo.Sellers");
            DropIndex("dbo.Sellers", new[] { "AssociatedSellerId" });
            DropColumn("dbo.Sellers", "AssociatedSellerId");
        }
    }
}
