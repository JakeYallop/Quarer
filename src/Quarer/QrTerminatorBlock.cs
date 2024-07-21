using System.Diagnostics;

namespace Quarer;

internal static class QrTerminatorBlock
{
    /// <summary> 
    /// QR Code terminator block, may not write anything if there is not enough space to write the terminator.
    /// </summary>
    /// <param name="bitWriter"></param>
    /// <param name="version"></param>
    public static void WriteTerminator(BitWriter bitWriter, QrVersion version)
    {
        var (codewords, remainder) = int.DivRem(bitWriter.Count, 8);
        Debug.Assert(version.DataCodewordsCapacity >= codewords, "Data too large for version.");

        if (remainder > 0)
        {
            codewords += 1;
        }

        Debug.Assert(version.DataCodewordsCapacity >= codewords, "Data too large for version.");
        // No space to write the terminator
        if (version.DataCodewordsCapacity == codewords && remainder == 0)
        {
            return;
        }

        var remainingBits = 8 - remainder;
        if (version.DataCodewordsCapacity > codewords)
        {
            // we have space to write the new terminator
            bitWriter.WriteBits(0, 4);
        }
        else
        {
            // we might have space in the remaining unused bits in the last codeword
            // write up to 4 bits, writing less if we have less than 4 bits remaining
            bitWriter.WriteBits(0, int.Min(remainingBits, 4));
        }
    }
}
