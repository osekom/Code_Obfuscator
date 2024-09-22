using System.Text;

namespace Obfuscator.Extensions;

public static class StringExtensions
{
    private const int min = 4;
    private const int max = 6;
    
    public static string RandomName()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();
        int randomLength = random.Next(min, max);
        var variableName = new StringBuilder();
        
        variableName.Append(chars[random.Next(chars.Length)]);
        
        const string charsAndNumbers = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        for (int i = 1; i < randomLength; i++)
        {
            variableName.Append(charsAndNumbers[random.Next(charsAndNumbers.Length)]);
        }

        return variableName.ToString();
    }
}