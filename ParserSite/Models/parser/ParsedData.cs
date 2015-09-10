namespace ParserSite
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ParsedData")]
    public partial class ParsedData
    {
        public int Id { get; set; }

        //public int OrderId { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? ParseDate { get; set; }

        [StringLength(20)]
        public string ParserType { get; set; }

        [StringLength(20)]
        public string Original { get; set; }

        [StringLength(20)]
        public string Firmname { get; set; }

        //[StringLength(20)]
        //public string Artikul { get; set; }
        //public int PartId { get; set; }

        [StringLength(300)]
        public string Description { get; set; }

        [StringLength(10)]
        public string Statistic { get; set; }

        public decimal? Price { get; set; }

        [StringLength(20)]
        public string SearchedArtikul { get; set; }

        [StringLength(300)]
        public string Url { get; set; }

        public virtual Order Order { get; set; }

        public virtual Part Part { get; set; }

        public virtual Report Report { get; set; }
    }
}
