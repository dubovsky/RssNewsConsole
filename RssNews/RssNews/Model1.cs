namespace RssNews
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<News> NewsItems { get; set; }
        public virtual DbSet<RSS_source> RSS_sources { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RSS_source>()
                .Property(e => e.URL)
                .IsUnicode(false);

            modelBuilder.Entity<RSS_source>()
                .HasMany(e => e.News)
                .WithRequired(e => e.RSS_source)
                .WillCascadeOnDelete(false);
        }
    }
}
