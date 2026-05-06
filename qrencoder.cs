using QRCoder;
using System.Text.RegularExpressions;

if (args is not [var textToEncode, ..])
{
    Console.Error.WriteLine("Usage: qrencoder 'text to encode' [optional: outputPngFile]");
    return 1;
}

using var qrCodeData = textToEncode.ToQRCodeData();

if (args.Length == 1)
    Console.WriteLine(qrCodeData.ToAsciiArt());
else
{
    var outputPngFile = args[1].IsValidFileName() ? args[1] : "qrcode.png";
    try
    {
        await qrCodeData.SaveAsPngAsync(outputPngFile);
        Console.WriteLine($"QR code saved to {outputPngFile}");
    }
    catch (IOException ex)
    {
        Console.Error.WriteLine($"Failed to save file: {ex.Message}");
        return 1;
    }
}

return 0;

static partial class Extensions
{
    [GeneratedRegex(@"[^\s]+\.png$", RegexOptions.IgnoreCase)]
    private static partial Regex PngFileNameRegex();

    public static QRCodeData ToQRCodeData(this string self)
    {
        var qrGenerator = new QRCodeGenerator();
        return qrGenerator.CreateQrCode(self, QRCodeGenerator.ECCLevel.Q);
    }

    public static string ToAsciiArt(this QRCodeData self)
    {
        var qrCode = new AsciiQRCode(self);
        return qrCode.GetGraphic(1);
    }

    public static byte[] ToPngBytes(this QRCodeData self)
    {
        var qrCode = new PngByteQRCode(self);
        return qrCode.GetGraphic(10);
    }

    public static async Task SaveAsPngAsync(this QRCodeData self, string filePath) =>
        await File.WriteAllBytesAsync(filePath, self.ToPngBytes());

    public static bool IsValidFileName(this string self) => PngFileNameRegex().IsMatch(self);
}

