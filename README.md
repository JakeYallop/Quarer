Quarer
=========
A fast and simple-to-use QR code encoding library.

<!-- TODO: Add badges here -->

By vectorizing many parts of the QR Code creation process, Quarer manages to be much faster than many other libraries. Quarer supports ECI mode encoding as well as Kanji.

![benchmark graph](./assets/timing-focused-1.png)

See the [full results](./benchmarks). Want to add get your library added to the benchmarks? Open an [issue](https://github.com/JakeYallop/Quarer/issues/new)!

## Installation
```bash
dotnet add package Quarer
```

## Usage

Create a QR Code
```csharp
using Quarer;

var qrCode = QrCode.Create("Hello, World!");

Console.WriteLine(qrCode.Version);
Console.WriteLine(qrCode.Width);
Console.WriteLine(qrCode.ErrorCorrectionLevel);
```

Output the QR code using a tool of your choice. The example below outputs directly to the console:
```csharp
Console.WriteLine("Encoding \"Hello, World!\"");

var qrCode = QrCode.Create("Hello, World");

var sb = new StringBuilder();

OutputYPadding(sb, qrCode.Width);
for (var y = 0; y < qrCode.Height; y++)
{
    for (var x = -4; x < qrCode.Width + 4; x++)
    {
        if (x < 0 || x >= qrCode.Width)
        {
            sb.Append("██");
            continue;
        }

        var v = qrCode.Data[x, y] != 0;
        var s = v ? "  " : "██";
        sb.Append(s);
    }
    sb.AppendLine();
}
OutputYPadding(sb, qrCode.Width);

Console.WriteLine(sb.ToString());

static void OutputYPadding(StringBuilder sb, int width)
{
    for (var y = 0; y < 4; y++)
    {
        for (var x = 0; x < width + 8; x++)
        {
            sb.Append('█');
            sb.Append('█');
        }
        sb.AppendLine();
    }
}
```
![QR code output in the console](./assets/qrcode%20output.png)

See more [samples](./samples).

## Roadmap
- Micro QR Codes
- rMQR Codes
- QR code Decoding
