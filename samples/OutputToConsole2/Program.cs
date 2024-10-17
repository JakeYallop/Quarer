using System.Text;
using Quarer;

// A little more complicated, but outputs a smaller QR Code to the console.
/*
█████████████████████████████
█████████████████████████████
████ ▄▄▄▄▄ █ ▄█ ▄█ ▄▄▄▄▄ ████
████ █   █ █▀ ▀ ██ █   █ ████
████ █▄▄▄█ ██▀ ███ █▄▄▄█ ████
████▄▄▄▄▄▄▄█ ▀▄█▄█▄▄▄▄▄▄▄████
████ █  ▀▄▄ █▄ █▀▀▄██ ▀▄ ████
████  ▄  ▀▄ ▀▄▄█ ▄█▄▀▀█ ▄████
████████▄▄▄█ ▄█ █▀▄▄▀█▀█ ████
████ ▄▄▄▄▄ █ ▀▄█▀▀▄▀▄▀▀██████
████ █   █ █▀ ▀█▀ █▄▄   █████
████ █▄▄▄█ █▄ █ ▄▀  ▀█▄█▀████
████▄▄▄▄▄▄▄█▄█▄▄▄▄▄▄██▄██████
█████████████████████████████
▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
*/

Console.WriteLine("Encoding \"Hello, World!\"");

var qrCode = QrCode.Create("Hello, World");

var sb = new StringBuilder();

for (var y = 0; y < 2; y++)
{
    for (var x = 0; x < qrCode.Width + 8; x++)
    {
        sb.Append('█');
    }
    sb.AppendLine();
}

for (var y = 0; y < qrCode.Height; y += 2)
{
    for (var x = -4; x < qrCode.Width + 4; x++)
    {
        if (x < 0 || x >= qrCode.Width)
        {
            sb.Append('█');
            continue;
        }

        var upper = qrCode.Data[x, y] != 0 ? 0b10 : 0;
        var lower = y + 1 < qrCode.Height ? qrCode.Data[x, y + 1] != 0 ? 0b01 : 0 : 0;

        var c = (upper + lower) switch
        {
            0b00 => '█',
            0b01 => '▀',
            0b10 => '▄',
            0b11 => ' ',
            _ => throw new InvalidOperationException()
        };

        sb.Append(c);
    }
    sb.AppendLine();
}

for (var x = 0; x < qrCode.Width + 8; x++)
{
    sb.Append('█');
}
sb.AppendLine();

for (var x = 0; x < qrCode.Width + 8; x++)
{
    sb.Append('▀');
}
sb.AppendLine();

Console.WriteLine(sb.ToString());
