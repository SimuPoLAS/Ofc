namespace Ofc.LZMA.Common
{
    using System;
    using System.Collections;

    public class Parser
    {
        public ArrayList NonSwitchStrings = new ArrayList();
        private readonly SwitchResult[] _switches;

        public Parser(int numSwitches)
        {
            _switches = new SwitchResult[numSwitches];
            for (var i = 0; i < numSwitches; i++)
                _switches[i] = new SwitchResult();
        }

        private bool ParseString(string srcString, SwitchForm[] switchForms)
        {
            var len = srcString.Length;
            if (len == 0)
                return false;
            var pos = 0;
            if (!IsItSwitchChar(srcString[pos]))
                return false;
            while (pos < len)
            {
                if (IsItSwitchChar(srcString[pos]))
                    pos++;
                const int kNoLen = -1;
                var matchedSwitchIndex = 0;
                var maxLen = kNoLen;
                for (var switchIndex = 0; switchIndex < _switches.Length; switchIndex++)
                {
                    var switchLen = switchForms[switchIndex].IdString.Length;
                    if (switchLen <= maxLen || pos + switchLen > len)
                        continue;
                    if (string.Compare(switchForms[switchIndex].IdString, 0,
                        srcString, pos, switchLen, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        matchedSwitchIndex = switchIndex;
                        maxLen = switchLen;
                    }
                }
                if (maxLen == kNoLen)
                    throw new Exception("maxLen == kNoLen");
                var matchedSwitch = _switches[matchedSwitchIndex];
                var switchForm = switchForms[matchedSwitchIndex];
                if (!switchForm.Multi && matchedSwitch.ThereIs)
                    throw new Exception("switch must be single");
                matchedSwitch.ThereIs = true;
                pos += maxLen;
                var tailSize = len - pos;
                var type = switchForm.Type;
                switch (type)
                {
                    case SwitchType.PostMinus:
                    {
                        if (tailSize == 0)
                            matchedSwitch.WithMinus = false;
                        else
                        {
                            matchedSwitch.WithMinus = srcString[pos] == KSwitchMinus;
                            if (matchedSwitch.WithMinus)
                                pos++;
                        }
                        break;
                    }
                    case SwitchType.PostChar:
                    {
                        if (tailSize < switchForm.MinLen)
                            throw new Exception("switch is not full");
                        var charSet = switchForm.PostCharSet;
                        const int kEmptyCharValue = -1;
                        if (tailSize == 0)
                            matchedSwitch.PostCharIndex = kEmptyCharValue;
                        else
                        {
                            var index = charSet.IndexOf(srcString[pos]);
                            if (index < 0)
                                matchedSwitch.PostCharIndex = kEmptyCharValue;
                            else
                            {
                                matchedSwitch.PostCharIndex = index;
                                pos++;
                            }
                        }
                        break;
                    }
                    case SwitchType.LimitedPostString:
                    case SwitchType.UnLimitedPostString:
                    {
                        var minLen = switchForm.MinLen;
                        if (tailSize < minLen)
                            throw new Exception("switch is not full");
                        if (type == SwitchType.UnLimitedPostString)
                        {
                            matchedSwitch.PostStrings.Add(srcString.Substring(pos));
                            return true;
                        }
                        var stringSwitch = srcString.Substring(pos, minLen);
                        pos += minLen;
                        for (var i = minLen; i < switchForm.MaxLen && pos < len; i++, pos++)
                        {
                            var c = srcString[pos];
                            if (IsItSwitchChar(c))
                                break;
                            stringSwitch += c;
                        }
                        matchedSwitch.PostStrings.Add(stringSwitch);
                        break;
                    }
                }
            }
            return true;

        }

        public void ParseStrings(SwitchForm[] switchForms, string[] commandStrings)
        {
            var numCommandStrings = commandStrings.Length;
            var stopSwitch = false;
            for (var i = 0; i < numCommandStrings; i++)
            {
                var s = commandStrings[i];
                if (stopSwitch)
                    NonSwitchStrings.Add(s);
                else
                    if (s == KStopSwitchParsing)
                        stopSwitch = true;
                    else
                        if (!ParseString(s, switchForms))
                            NonSwitchStrings.Add(s);
            }
        }

        public SwitchResult this[int index] { get { return _switches[index]; } }

        public static int ParseCommand(CommandForm[] commandForms, string commandString,
            out string postString)
        {
            for (var i = 0; i < commandForms.Length; i++)
            {
                var id = commandForms[i].IdString;
                if (commandForms[i].PostStringMode)
                {
                    if (commandString.IndexOf(id) == 0)
                    {
                        postString = commandString.Substring(id.Length);
                        return i;
                    }
                }
                else
                    if (commandString == id)
                    {
                        postString = "";
                        return i;
                    }
            }
            postString = "";
            return -1;
        }

        private static bool ParseSubCharsCommand(int numForms, CommandSubCharsSet[] forms,
            string commandString, ArrayList indices)
        {
            indices.Clear();
            var numUsedChars = 0;
            for (var i = 0; i < numForms; i++)
            {
                var charsSet = forms[i];
                var currentIndex = -1;
                var len = charsSet.Chars.Length;
                for (var j = 0; j < len; j++)
                {
                    var c = charsSet.Chars[j];
                    var newIndex = commandString.IndexOf(c);
                    if (newIndex >= 0)
                    {
                        if (currentIndex >= 0)
                            return false;
                        if (commandString.IndexOf(c, newIndex + 1) >= 0)
                            return false;
                        currentIndex = j;
                        numUsedChars++;
                    }
                }
                if (currentIndex == -1 && !charsSet.EmptyAllowed)
                    return false;
                indices.Add(currentIndex);
            }
            return numUsedChars == commandString.Length;
        }

        private const char KSwitchId1 = '-';
        private const char KSwitchId2 = '/';

        private const char KSwitchMinus = '-';
        private const string KStopSwitchParsing = "--";

        private static bool IsItSwitchChar(char c)
        {
            return c == KSwitchId1 || c == KSwitchId2;
        }
    }
}