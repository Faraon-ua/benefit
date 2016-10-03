namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RemoveCardUniqueness : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ApplicationUsers", new[] { "CardNumber" });
            CreateIndex("dbo.ApplicationUsers", "CardNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ApplicationUsers", new[] { "CardNumber" });
            CreateIndex("dbo.ApplicationUsers", "CardNumber", unique: true);
        }
    }
}
