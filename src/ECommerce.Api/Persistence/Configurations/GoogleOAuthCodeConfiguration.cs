using ECommerce.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Persistence.Configurations;

public sealed class GoogleOAuthCodeConfiguration : IEntityTypeConfiguration<GoogleOAuthCodeEntity>
{
    public void Configure(EntityTypeBuilder<GoogleOAuthCodeEntity> builder)
    {
        builder.ToTable("GoogleOAuthCodes");
        builder.HasKey(code => code.Id);
        builder.Property(code => code.CodeHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(code => code.CodeHash).IsUnique();
        builder.HasIndex(code => code.ExpiresAt);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(code => code.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
