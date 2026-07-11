using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EntidadePagamento = Pagamento.Domain.Entidades.Pagamento;

namespace Pagamento.Infrastructure.Persistencia.Configuracoes;

public class PagamentoConfiguracao : IEntityTypeConfiguration<EntidadePagamento>
{
    public void Configure(EntityTypeBuilder<EntidadePagamento> builder)
    {
        builder.ToTable("Pagamentos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.IngressoId)
            .IsRequired();

        builder.Property(p => p.Valor)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.Metodo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.CriadoEm)
            .IsRequired();
    }
}
