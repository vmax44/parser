namespace ParserSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderNumber = c.String(maxLength: 10),
                        ClientName = c.String(maxLength: 30),
                        ClientCar = c.String(maxLength: 30),
                        OrderDate = c.DateTime(storeType: "date"),
                        DTPDate = c.DateTime(storeType: "date"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ParsedData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParseDate = c.DateTime(storeType: "date"),
                        ParserType = c.String(maxLength: 20),
                        Original = c.String(maxLength: 20),
                        Firmname = c.String(maxLength: 20),
                        Description = c.String(maxLength: 300),
                        Statistic = c.String(maxLength: 10),
                        Price = c.Decimal(precision: 18, scale: 2),
                        SearchedArtikul = c.String(maxLength: 20),
                        Url = c.String(maxLength: 300),
                        Order_Id = c.Int(nullable: false),
                        Part_Id = c.Int(nullable: false),
                        Report_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.Order_Id, cascadeDelete: true)
                .ForeignKey("dbo.Parts", t => t.Part_Id)
                .ForeignKey("dbo.Reports", t => t.Report_Id)
                .Index(t => t.Order_Id)
                .Index(t => t.Part_Id)
                .Index(t => t.Report_Id);
            
            CreateTable(
                "dbo.Parts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PartNumber = c.String(),
                        PartName = c.String(),
                        Order_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.Order_Id, cascadeDelete: true)
                .Index(t => t.Order_Id);
            
            CreateTable(
                "dbo.Reports",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DateCalc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParsedData", "Report_Id", "dbo.Reports");
            DropForeignKey("dbo.Reports", "Id", "dbo.Orders");
            DropForeignKey("dbo.ParsedData", "Part_Id", "dbo.Parts");
            DropForeignKey("dbo.Parts", "Order_Id", "dbo.Orders");
            DropForeignKey("dbo.ParsedData", "Order_Id", "dbo.Orders");
            DropIndex("dbo.Reports", new[] { "Id" });
            DropIndex("dbo.Parts", new[] { "Order_Id" });
            DropIndex("dbo.ParsedData", new[] { "Report_Id" });
            DropIndex("dbo.ParsedData", new[] { "Part_Id" });
            DropIndex("dbo.ParsedData", new[] { "Order_Id" });
            DropTable("dbo.Reports");
            DropTable("dbo.Parts");
            DropTable("dbo.ParsedData");
            DropTable("dbo.Orders");
        }
    }
}
