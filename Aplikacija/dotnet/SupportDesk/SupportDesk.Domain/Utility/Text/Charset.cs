namespace SupportDesk.Domain.Utility.Text;

public static class Charset
{
    public static char[] GetNumericCharset()
    {
        var charset = new List<char>();

        for (var c = '0'; c <= '9'; c++)
        {
            charset.Add(c);
        }
        
        return charset.ToArray();
    }

    public static char[] GetLettersCharset()
    {
        
        var charset = new List<char>();

        for (var c = 'a'; c <= 'z'; c++)
        {
            charset.Add(c);
        }

        for (var c = 'A'; c <= 'Z'; c++)
        {
            charset.Add(c);
        }
        
        return charset.ToArray();
    }
    
    public static char[] GetAlphanumericCharset()
    {
        var lettersCharset = GetLettersCharset();
        var numericCharset = GetNumericCharset();

        char[] alphanumericCharset = [..numericCharset, ..lettersCharset];

        return alphanumericCharset;
    }
}