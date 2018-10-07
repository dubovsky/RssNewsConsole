namespace RssNews
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class News
    {
        [Key]
        [Column(Order = 0)]
        public DateTime PubDate { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SourceId { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(300)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(2083)]
        public string URL { get; set; }

        public virtual RSS_source RSS_source { get; set; }
    }
}
