namespace Ingressos.Infrastructure.ServicosExternos;

public class ServicosExternosOptions
{
    public const string SectionName = "ServicosExternos";

    public string EventosApiBaseUrl { get; set; } = string.Empty;
}
