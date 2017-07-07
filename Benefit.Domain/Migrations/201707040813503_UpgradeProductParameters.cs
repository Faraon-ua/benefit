namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpgradeProductParameters : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters");
            DropIndex("dbo.ProductParameters", new[] { "ParentProductParameterId" });
            DropColumn("dbo.ProductParameters", "ParentProductParameterId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProductParameters", "ParentProductParameterId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ProductParameters", "ParentProductParameterId");
            AddForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters", "Id");
        }
    }
}
