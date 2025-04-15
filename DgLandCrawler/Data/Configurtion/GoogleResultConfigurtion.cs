using DgLandCrawler.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DgLandCrawler.Data.Configurtion
{
    public class GoogleResultConfigurtion : IEntityTypeConfiguration<GoogleSearchResult>
    {
        public void Configure(EntityTypeBuilder<GoogleSearchResult> builder)
        {
            builder.HasKey(x => x.GoogleId).HasName("PrimaryKey_GoogleId");
            builder.Property(x => x.CreationTime).HasDefaultValueSql("GETDATE()");
            builder.HasOne(x=>x.DGProduct).WithMany(x=> x.GoogleResult).HasForeignKey(x => x.DGProductId).IsRequired();
        }
    }
}
