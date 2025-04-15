using DgLandCrawler.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DgLandCrawler.Data.Configurtion
{
    public class KeywordConfiguration : IEntityTypeConfiguration<Keyword>
    {
        public void Configure(EntityTypeBuilder<Keyword> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x=>x.DGProduct).WithMany(x=>x.Keywords).HasForeignKey(x=>x.DGProductId).IsRequired();
        }
    }
}
