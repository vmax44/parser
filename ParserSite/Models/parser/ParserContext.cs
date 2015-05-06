namespace ParserSite
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ParserContext : DbContext
    {
        public ParserContext()
            : base("name=ParserModel")
        {
        }

        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<ParsedData> ParsedDatas { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Part> Parts { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Report>()
                .HasRequired(e => e.Order)
                .WithOptional(e => e.Report)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Report>()
                .HasMany(e => e.ParsedDatas)
                .WithOptional(e => e.Report)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ParsedData>()
                .HasRequired(e => e.Order)
                .WithMany(e=>e.ParsedDatas)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ParsedData>()
                .HasRequired(e => e.Part)
                .WithMany(e => e.ParsedDatas)
                .WillCascadeOnDelete(false);
                            
            modelBuilder.Entity<Part>()
                .HasRequired(e => e.Order)
                .WithMany(e => e.Parts)
                .WillCascadeOnDelete(true);
        }
        
    }
}
