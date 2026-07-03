using System.Text;
using FlipTextLayout.Models;

namespace FlipTextLayout.Services;

public sealed class RussianEnglishLayoutConverter : ILayoutConverter
{
    private static readonly IReadOnlyDictionary<char, char> EnglishToRussian = BuildEnglishToRussianMap();
    private static readonly IReadOnlyDictionary<char, char> RussianToEnglish = BuildRussianToEnglishMap();

    public LayoutConversionResult Convert(string text)
    {
        TextLayout sourceLayout = DetectSourceLayout(text);

        if (sourceLayout == TextLayout.Russian)
        {
            return ConvertWithMap(text, RussianToEnglish, TextLayout.Russian, TextLayout.English);
        }

        if (sourceLayout == TextLayout.English)
        {
            return ConvertWithMap(text, EnglishToRussian, TextLayout.English, TextLayout.Russian);
        }

        return new LayoutConversionResult(text, TextLayout.Unknown, TextLayout.Unknown, Changed: false);
    }

    private static LayoutConversionResult ConvertWithMap(
        string text,
        IReadOnlyDictionary<char, char> map,
        TextLayout sourceLayout,
        TextLayout targetLayout)
    {
        StringBuilder builder = new(text.Length);
        bool changed = false;

        foreach (char character in text)
        {
            if (map.TryGetValue(character, out char converted))
            {
                builder.Append(converted);
                changed = true;
            }
            else
            {
                builder.Append(character);
            }
        }

        return new LayoutConversionResult(builder.ToString(), sourceLayout, targetLayout, changed);
    }

    private static TextLayout DetectSourceLayout(string text)
    {
        int englishScore = 0;
        int russianScore = 0;

        foreach (char character in text)
        {
            if (EnglishToRussian.ContainsKey(character))
            {
                englishScore++;
            }

            if (RussianToEnglish.ContainsKey(character))
            {
                russianScore++;
            }
        }

        if (englishScore == 0 && russianScore == 0)
        {
            return TextLayout.Unknown;
        }

        return russianScore > englishScore
            ? TextLayout.Russian
            : TextLayout.English;
    }

    private static IReadOnlyDictionary<char, char> BuildEnglishToRussianMap()
    {
        Dictionary<char, char> map = new()
        {
            ['`'] = 'ё',
            ['q'] = 'й',
            ['w'] = 'ц',
            ['e'] = 'у',
            ['r'] = 'к',
            ['t'] = 'е',
            ['y'] = 'н',
            ['u'] = 'г',
            ['i'] = 'ш',
            ['o'] = 'щ',
            ['p'] = 'з',
            ['['] = 'х',
            [']'] = 'ъ',
            ['a'] = 'ф',
            ['s'] = 'ы',
            ['d'] = 'в',
            ['f'] = 'а',
            ['g'] = 'п',
            ['h'] = 'р',
            ['j'] = 'о',
            ['k'] = 'л',
            ['l'] = 'д',
            [';'] = 'ж',
            ['\''] = 'э',
            ['z'] = 'я',
            ['x'] = 'ч',
            ['c'] = 'с',
            ['v'] = 'м',
            ['b'] = 'и',
            ['n'] = 'т',
            ['m'] = 'ь',
            [','] = 'б',
            ['.'] = 'ю',
            ['/'] = '.',
            ['~'] = 'Ё',
            ['Q'] = 'Й',
            ['W'] = 'Ц',
            ['E'] = 'У',
            ['R'] = 'К',
            ['T'] = 'Е',
            ['Y'] = 'Н',
            ['U'] = 'Г',
            ['I'] = 'Ш',
            ['O'] = 'Щ',
            ['P'] = 'З',
            ['{'] = 'Х',
            ['}'] = 'Ъ',
            ['A'] = 'Ф',
            ['S'] = 'Ы',
            ['D'] = 'В',
            ['F'] = 'А',
            ['G'] = 'П',
            ['H'] = 'Р',
            ['J'] = 'О',
            ['K'] = 'Л',
            ['L'] = 'Д',
            [':'] = 'Ж',
            ['"'] = 'Э',
            ['Z'] = 'Я',
            ['X'] = 'Ч',
            ['C'] = 'С',
            ['V'] = 'М',
            ['B'] = 'И',
            ['N'] = 'Т',
            ['M'] = 'Ь',
            ['<'] = 'Б',
            ['>'] = 'Ю',
            ['?'] = ',',
            ['@'] = '"',
            ['#'] = '№',
            ['$'] = ';',
            ['^'] = ':',
            ['&'] = '?'
        };

        return map;
    }

    private static IReadOnlyDictionary<char, char> BuildRussianToEnglishMap()
    {
        Dictionary<char, char> map = [];

        foreach (KeyValuePair<char, char> pair in EnglishToRussian)
        {
            map[pair.Value] = pair.Key;
        }

        return map;
    }
}
