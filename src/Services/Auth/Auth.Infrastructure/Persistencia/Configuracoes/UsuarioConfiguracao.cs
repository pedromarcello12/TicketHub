using Auth.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistencia.Configuracoes;

public class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.NomeUsuario)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.NomeUsuario)
            .IsUnique();

        builder.Property(u => u.SenhaHash)
            .IsRequired();

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Papel)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(u => u.CriadoEm)
            .IsRequired();
    }
}
