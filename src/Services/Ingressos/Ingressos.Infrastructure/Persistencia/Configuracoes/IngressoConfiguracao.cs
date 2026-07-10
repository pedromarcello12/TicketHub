using Ingressos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ingressos.Infrastructure.Persistencia.Configuracoes;

public class IngressoConfiguracao : IEntityTypeConfiguration<Ingresso>
{
    public void Configure(EntityTypeBuilder<Ingresso> builder)
    {
        builder.ToTable("Ingressos");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.EventoId)
            .IsRequired();

        builder.Property(i => i.TipoIngresso)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Preco)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.CriadoEm)
            .IsRequired();
    }
}
