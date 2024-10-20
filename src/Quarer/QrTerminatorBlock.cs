﻿using System.Diagnostics;

namespace Quarer;

internal static class QrTerminatorBlock
{
    /// <summary> 
    /// QR Code terminator block, may not write anything if there is not enough space to write the terminator.
    /// </summary>
    public static void WriteTerminator(BitWriter bitWriter, QrVersion version, ErrorCorrectionLevel errorCorrectionLevel)
    {
        var (codewords, remainder) = int.DivRem(bitWriter.BitsWritten, 8);
        var dataCodewordsCapacity = version.GetDataCodewordsCapacity(errorCorrectionLevel);
        Debug.Assert(dataCodewordsCapacity >= codewords, "Data too large for version.");

        if (remainder > 0)
        {
            codewords += 1;
        }

        Debug.Assert(dataCodewordsCapacity >= codewords, "Data too large for version.");
        // No space to write the terminator
        if (dataCodewordsCapacity == codewords && remainder == 0)
        {
            return;
        }

        var remainingBits = 8 - remainder;
        if (dataCodewordsCapacity > codewords)
        {
            // we have space to write the new terminator
            bitWriter.WriteBitsBigEndian(0, 4);
        }
        else
        {
            // we might have space in the remaining unused bits in the last codeword
            // write up to 4 bits, writing less if we have less than 4 bits remaining
            bitWriter.WriteBitsBigEndian(0, int.Min(remainingBits, 4));
        }
    }
}
