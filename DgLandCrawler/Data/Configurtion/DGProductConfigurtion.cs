using DgLandCrawler.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DgLandCrawler.Data.Configurtion
{
    public class DGProductDataConfigurtion : IEntityTypeConfiguration<DGProductData>
    {
        public void Configure(EntityTypeBuilder<DGProductData> builder)
        {
            builder.HasKey(x => x.Id).HasName("PrimaryKey_DGProductId");
            builder.Property(x => x.SKU);
            builder.Property(x => x.CrawlDateTime);
            builder.Property(x => x.RegularPrice);
            builder.HasMany(x => x.GoogleResult).WithOne(x=>x.DGProduct).HasForeignKey(x=> x.DGProductId).IsRequired();
        }
    }
}
