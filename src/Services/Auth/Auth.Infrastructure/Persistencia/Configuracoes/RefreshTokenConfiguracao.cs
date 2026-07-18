using Auth.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistencia.Configuracoes;

public class RefreshTokenConfiguracao : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.Property(t => t.UsuarioId)
            .IsRequired();

        builder.Property(t => t.ExpiraEm)
            .IsRequired();

        builder.Property(t => t.RevogadoEm);

        builder.Property(t => t.CriadoEm)
            .IsRequired();
    }
}
