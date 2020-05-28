namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerCustomColors : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "HeaderColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "MenuColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "BodyColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "AdverticementColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "FooterColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "CopyrightColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "CabinetColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "CatalogColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "FavoritesColor", c => c.String(maxLength: 16));
            AddColumn("dbo.Sellers", "CardColor", c => c.String(maxLength: 16));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "CardColor");
            DropColumn("dbo.Sellers", "FavoritesColor");
            DropColumn("dbo.Sellers", "CatalogColor");
            DropColumn("dbo.Sellers", "CabinetColor");
            DropColumn("dbo.Sellers", "CopyrightColor");
            DropColumn("dbo.Sellers", "FooterColor");
            DropColumn("dbo.Sellers", "AdverticementColor");
            DropColumn("dbo.Sellers", "BodyColor");
            DropColumn("dbo.Sellers", "MenuColor");
            DropColumn("dbo.Sellers", "HeaderColor");
        }
    }
}
