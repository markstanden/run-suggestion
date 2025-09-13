using RunSuggestion.Shared.Constants;

namespace RunSuggestion.TestHelpers;

public static class Any
{
    // Strings
    public const string String = "Any String";
    public const string FourCharString = "abcd";
    public const string FiveCharString = "abcde";
    public const string SixCharString = "abcdef";
    public const string ShortAlphanumericString = "ABC12345";
    public const string LongAlphanumericString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    public const string ShortAlphaWithSpecialCharsString = "$A^B*C(D)&1<2>3?4%";
    public const string LongAlphaWithSpecialCharsString = "$-A^B*C(D)&*£!<>|,.?;:@'~#[]{}-_-+1=`3¬%";
    public const string UrlString = "https://www.anydomain.com";
    public const string OtherUrlString = "https://www.anotherdomain.com";

    // Numbers
    public const int Integer = 1;

    // Run Recommendations
    public const byte EffortLevel = Runs.EffortLevel.Easy;

    // URIs
    public static readonly Uri Url = new(UrlString);
    public static readonly Uri OtherUrl = new(OtherUrlString);
}
