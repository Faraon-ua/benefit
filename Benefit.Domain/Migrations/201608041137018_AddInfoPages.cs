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
                        Name = c.String(nullable: false, maxLength: 64),
                        Content = c.String(),
                        Order = c.Int(nullable: false),
                        LastModified = c.DateTime(nullable: false),
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
