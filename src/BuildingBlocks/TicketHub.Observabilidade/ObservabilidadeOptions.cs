namespace TicketHub.Observabilidade;

public class ObservabilidadeOptions
{
    public const string SectionName = "Observabilidade";

    public string SeqUrl { get; set; } = "http://seq:5341";
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
    public bool HabilitarConsoleExporter { get; set; } = false;
}
