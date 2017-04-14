namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddEmailToAddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Addresses", "Email", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Addresses", "Email");
        }
    }
}
