namespace nblackbox.internals
{
    static class LongToSequencenumberString
    {
        public static string ToSequenceNumber(this long sequencenumber)
        {
            return sequencenumber.ToString("000000000000");
        }
    }
}