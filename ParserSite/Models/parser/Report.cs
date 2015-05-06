namespace ParserSite
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Report
    {
        public int Id { get; set; }
        //public int OrderId { get; set; }
        
        public DateTime DateCalc { get; set; }

        public virtual Order Order { get; set; }

        public virtual ICollection<ParsedData> ParsedDatas { get; set; }
    }
}
