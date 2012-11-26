namespace _4sqChat.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FoursquareUserContext : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FoursquareUserModel",
                c => new
                    {
                        FoursquareUserId = c.Int(nullable: false),
                        UserGuid = c.Guid(nullable: false),
                        Token = c.String(),
                        UserName = c.String(),
                        LastVenueID = c.String(),
                        IsPremium = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.FoursquareUserId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FoursquareUserModel");
        }
    }
}
