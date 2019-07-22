using System.Collections.Generic;
using UnityEngine;

namespace AutoSuggest
{
    public class Suggestion
    {
        private string _richDisplayText = null;

        public string Value { get; }
        public string DisplayText { get; }
        public int DisplayTextMatchIndex { get; }
        public int DisplayTextMatchLength { get; }
        public bool IsErrorMessage { get; }

        public string RichDisplayText
        {
            get
            {
                if (_richDisplayText == null)
                {
                    _richDisplayText = MakeSubstringBold(DisplayText, DisplayTextMatchIndex, DisplayTextMatchLength);
                }

                return _richDisplayText;
            }
        }

        public Suggestion(string value, string displayText, int displayTextMatchIndex, int displayTextMatchLength)
        {
            Value = value;
            DisplayText = displayText;
            DisplayTextMatchIndex = displayTextMatchIndex;
            DisplayTextMatchLength = displayTextMatchLength;
        }

        public Suggestion(string errorMessage)
        {
            IsErrorMessage = true;
            DisplayText = errorMessage;
        }

        private static string MakeSubstringBold(string s, int index, int length)
        {
            int endIndex = index + length;

            Debug.Assert(length >= 0, $"length of substring must be >= 0.  Is actually {length}.");
            Debug.Assert(index >= 0, $"index must be >= 0.  Is actually {index}.");
            Debug.Assert(endIndex >= 0, $"endIndex must be >= 0.  Is actually {index} + {length} = {endIndex}.");
            Debug.Assert(index <= s.Length, $"index must be <= {s.Length}.  Is actually {index}.");
            Debug.Assert(endIndex <= s.Length, $"endIndex must be <= {s.Length}.  Is actually {index} + {length} = {endIndex}.");

            return s.Substring(0, index) + "<b>" + s.Substring(index, length) + "</b>" + s.Substring(index + length, s.Length - index - length);
        }

    }
}
