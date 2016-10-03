namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInfoPages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InfoPages",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 250),
                        UrlName = c.String(nullable: false, maxLength: 250),
                        Content = c.String(),
                        ImageUrl = c.String(maxLength: 128),
                        IsActive = c.Boolean(nullable: false),
                        IsNews = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastModified = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InfoPages");
        }
    }
}
