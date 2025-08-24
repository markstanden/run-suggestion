namespace RunSuggestion.TestHelpers;

public static class Any
{
    public const string String = "Any String";
    public const string FourCharString = "abcd";
    public const string FiveCharString = "abcde";
    public const string SixCharString = "abcdef";
    public const string ShortAlphanumericString = "ABC12345";
    public const string LongAlphanumericString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    public const string ShortAlphaWithSpecialCharsString = "$A^B*C(D)&1<2>3?4%";

    public const string LongAlphaWithSpecialCharsString =
        "$-A^B*C(D)E&F*G£H!I<J>K|L,M.N?O;P:Q@'R~S#T[U]V{W}X-Y_Z-+1=2`3¬4%";

    public const int Integer = 1;
}
