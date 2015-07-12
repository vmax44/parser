namespace ParserSite
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq;

    public class Part
    {
        public int Id { get; set; }
        //public int OrderId { get; set; }
        [Required]
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        
        public virtual Order Order { get; set; }
        public virtual ICollection<ParsedData> ParsedDatas { get; set; }
    }
}