using Eventos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventos.Infrastructure.Persistencia.Configuracoes;

public class EventoConfiguracao : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("Eventos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Local)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.DataHora)
            .IsRequired();

        builder.Property(e => e.CapacidadeTotal)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CriadoEm)
            .IsRequired();
    }
}
