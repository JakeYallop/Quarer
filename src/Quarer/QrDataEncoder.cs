using System.Buffers;

namespace Quarer;

public static class QrDataEncoder
{
    public static readonly SearchValues<char> AlphanumericCharacters = SearchValues.Create(AlphanumericEncoder.Characters);
    public static readonly SearchValues<char> NumericCharacters = SearchValues.Create("0123456789");

    public static QrDataEncoding Analyze(ReadOnlySpan<char> data)
    {
        //For now, just use a single mode for the full set of data.

        ModeIndicator mode;
        int dataLength = data.Length;
        if (data.ContainsAnyExcept(AlphanumericCharacters))
        {
            if (KanjiEncoder.ContainsAnyExceptKanji(in data))
            {
                mode = ModeIndicator.Byte;
                //TOOD: Try convert to UTF-8 and get length
                dataLength = dataLength * 2;
            }
            else
            {
                mode = ModeIndicator.Kanji;
            }

        }
        else
        {
            mode = data.ContainsAnyExcept(NumericCharacters) ? ModeIndicator.Alphanumeric : ModeIndicator.Numeric;
        }

        return new QrDataEncoding(default!, default!);
    }
}
