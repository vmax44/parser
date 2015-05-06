namespace ParserSite
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Order
    {
        public Order()
        {
            ParsedDatas = new HashSet<ParsedData>();
            Parts = new HashSet<Part>();
        }

        public int Id { get; set; }

        [StringLength(10)]
        public string OrderNumber { get; set; }

        [StringLength(30)]
        public string ClientName { get; set; }

        [StringLength(30)]
        public string ClientCar { get; set; }

        [Column(TypeName = "date")]
        public DateTime? OrderDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DTPDate { get; set; }

        public virtual ICollection<ParsedData> ParsedDatas { get; set; }

        public virtual ICollection<Part> Parts { get; set; }

        public virtual Report Report { get; set; }
    }
}
