namespace ParserSite.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ParserSite.ParserContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            
        }

        protected override void Seed(ParserSite.ParserContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            Part part1=new Part { PartNumber="10000001"};
            Part part2=new Part { PartNumber="10000002"};
            Part part3=new Part { PartNumber="10000003"};
            Part part4=new Part { PartNumber="10000004"};

            ParsedData data1 = new ParsedData
            {
                Description = "Бампер передний",
                Firmname = "BMW",
                Original = "original",
                ParseDate = new DateTime(2015, 5, 2),
                ParserType = "exist",
                Part = part1,
                SearchedArtikul = part1.PartNumber,
                Statistic = "2",
                Url = "http",
                Price = new Decimal(1235.65)
            };
            ParsedData data2 = new ParsedData
            {
                Description = "Бампер передний",
                Firmname = "BMW",
                Original = "original",
                ParseDate = new DateTime(2015, 5, 2),
                ParserType = "emex",
                Part = part1,
                SearchedArtikul = part1.PartNumber,
                Statistic = "2",
                Url = "http",
                Price = new Decimal(1535.00)
            };
            ParsedData data3 = new ParsedData
            {
                Description = "Блок-фара правая",
                Firmname = "BMW",
                Original = "original",
                ParseDate = new DateTime(2015, 5, 2),
                ParserType = "autodoc",
                Part = part2,
                SearchedArtikul = part2.PartNumber,
                Statistic = "2",
                Url = "http",
                Price = new Decimal(1635.58)
            };

            ParsedData data4 = new ParsedData
            {
                Description = "Капот",
                Firmname = "Peugeot",
                Original = "original",
                ParseDate = new DateTime(2015, 5, 3),
                ParserType = "exist",
                Part = part4,
                SearchedArtikul = part4.PartNumber,
                Statistic = "2",
                Url = "http",
                Price = new Decimal(635.58)
            };

            Order order1 = new Order
            {
                OrderNumber = "10-15",
                OrderDate = new DateTime(2015, 5, 2),
                DTPDate = new DateTime(2015, 4, 30),
                ClientCar = "BMW",
                ClientName = "Иванов Иван Иванович",
                ParsedDatas = new List<ParsedData>() { },
                Parts = new List<Part>() { part1, part2 },
                Report = null
            };
            
            Report report1 = new Report
            {
                DateCalc = DateTime.Now.Date,
                ParsedDatas = new List<ParsedData>() { data1, data2 }
            };

            Report report2 = new Report
            {
                DateCalc = DateTime.Now.Date,
                ParsedDatas = new List<ParsedData>() { data1, data3 }
            };
        
            order1.ParsedDatas.Add(data1);
            order1.ParsedDatas.Add(data2);
            order1.ParsedDatas.Add(data3);

            order1.Report = report1;
            // order1.Reports.Add(report2);

            Order order2 = new Order
            {
                OrderNumber = "12-15",
                OrderDate = new DateTime(2015, 5, 1),
                DTPDate = new DateTime(2015, 4, 15),
                ClientCar = "Peugeot",
                ClientName = "Петров Петр Петрович",
                ParsedDatas = new List<ParsedData>() { },
                Parts = new List<Part>() { part3,part4}
            };

            order2.ParsedDatas.Add(data4);

            context.Orders.AddOrUpdate(order1, order2);

        }
    }
}
