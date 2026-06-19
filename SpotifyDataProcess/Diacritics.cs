using System.Globalization;
using System.Text;

namespace SpotifyDataProcess

{
  public static class StringUtils
  {
    private static Dictionary<Tuple<string, string>, string> songTranslator = new();
    private static Dictionary<string, string> artistTranslator = new Dictionary<string, string>
    {
      {"Emily", "Emily & Justice"}
    };
    public static string? ProcessSongName(string? text, string? artist)
    {
      if (string.IsNullOrEmpty(text))
        return text;
      var normalized = text.Normalize(NormalizationForm.FormD);
      var sb = new StringBuilder();
      bool inBracket = false;
      foreach (char c in normalized)
      {
        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
        if (unicodeCategory != UnicodeCategory.NonSpacingMark)
        {
          if(c == '(')
            inBracket = true;

          if(!inBracket)
            sb.Append(char.ToLower(c));

          if(c == ')')
            inBracket = false;
        }
      }
      var result = sb.ToString().Normalize(NormalizationForm.FormC).Trim();
      var key = new Tuple<string, string>(result, artist ?? "");
      songTranslator[key] = text;
      return result;
    }

    public static string? OriginalName(string? text, string? artist)
    {
      if (string.IsNullOrEmpty(text))
        return text;
      var key = new Tuple<string, string>(text, artist ?? "");
      return songTranslator.ContainsKey(key) ? songTranslator[key] : text;
    }

    public static string? ArtistFix(string? text)
    {
      if (string.IsNullOrEmpty(text))
        return text;
      return artistTranslator.ContainsKey(text) ? artistTranslator[text] : text;
    }
  }
}