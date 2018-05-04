namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInfoPageSellerRelation : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Sellers", "ShortSescription", "ShortDescription");
            AddColumn("dbo.InfoPages", "SellerId", c => c.String(maxLength: 128));
            CreateIndex("dbo.InfoPages", "SellerId");
            AddForeignKey("dbo.InfoPages", "SellerId", "dbo.Sellers", "Id");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Sellers", "ShortDescription", "ShortSescription");
            DropForeignKey("dbo.InfoPages", "SellerId", "dbo.Sellers");
            DropIndex("dbo.InfoPages", new[] { "SellerId" });
            DropColumn("dbo.InfoPages", "SellerId");
        }
    }
}
