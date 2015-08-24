namespace ParserSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class datetodatetime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ParsedData", "ParseDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ParsedData", "ParseDate", c => c.DateTime(storeType: "date"));
        }
    }
}
