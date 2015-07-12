namespace ParserSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addrequiredtopartnumber : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Parts", "PartNumber", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Parts", "PartNumber", c => c.String());
        }
    }
}
