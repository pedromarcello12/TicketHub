using QRCoder;

namespace Notificacoes.Worker.QrCode;

public static class QrCodeService
{
    /// <summary>
    /// Gera um QR Code PNG a partir do conteúdo fornecido e retorna como base64.
    /// </summary>
    public static string GerarBase64(string conteudo)
    {
        using var gerador = new QRCodeGenerator();
        var dados = gerador.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(dados);
        var bytes = qrCode.GetGraphic(pixelsPerModule: 10);
        return Convert.ToBase64String(bytes);
    }
}
