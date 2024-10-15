// See https://aka.ms/new-console-template for more information
using Quarer;
using SkiaSharp;

var qrCode = QrCode.Create("OH YEAH WRITER");

var moduleSize = 10;
using var image = new SKBitmap(moduleSize * (qrCode.Width + 8), moduleSize * (qrCode.Height + 8));
using var canvas = new SKCanvas(image);

var black = new SKPaint { Color = SKColors.Black };
var white = new SKPaint() { Color = SKColors.White };
canvas.Clear(SKColors.White);

for (var x = 4; x < qrCode.Data.Width + 4; x++)
{
    for (var y = 4; y < qrCode.Data.Height + 4; y++)
    {
        canvas.DrawRect(x * moduleSize, y * moduleSize, moduleSize, moduleSize, qrCode.Data[x - 4, y - 4] is 0 ? white : black);
    }
}

using var imageEncoded = image.Encode(SKEncodedImageFormat.Png, 100);
using var fs = new FileStream("qr.png", FileMode.Create);
imageEncoded.SaveTo(fs);
