namespace Pagamento.Infrastructure.ServicosExternos;

public class ServicosExternosOptions
{
    public const string SectionName = "ServicosExternos";

    public string IngressosApiBaseUrl { get; set; } = string.Empty;
}
