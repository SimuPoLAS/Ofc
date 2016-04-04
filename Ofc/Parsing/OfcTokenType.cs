namespace Ofc.Parsing
{
    internal enum OfcTokenType
    {
        END_OF_STREAM = -2,
        NONE = -1,
        BRACES_OPEN = 0,
        BRACES_CLOSE = 1,
        BRACKETS_OPEN = 2,
        BRACKETS_CLOSE = 3,
        PARENTHESE_OPEN = 4,
        PARENTHESE_CLOSE = 5,
        CHEVRONS_OPEN = 6,
        CHEVRONS_CLOSE = 7,
        SEMICOLON = 8,
        HASHTAG = 9,
        KEYWORD = 10,
        NUMBER = 11,
        STRING = 12
    }
}