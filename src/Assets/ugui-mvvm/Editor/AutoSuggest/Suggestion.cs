using System.Collections.Generic;
using UnityEngine;

namespace AutoSuggest
{
    /// <summary>
    /// Class representing a suggestion to be surfaced to the user.
    /// </summary>
    public class Suggestion
    {
        private string _richDisplayText = null;

        /// <summary>
        /// The underlying value represented by this suggestion.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The text to be displayed to the user for this suggestion (often matches <see cref="Value"/> for non-error scenarios).
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// The starting index for the portion of the <see cref="DisplayText"/> that matches the user input value for which this suggestion was generated.
        /// </summary>
        public int DisplayTextMatchIndex { get; }

        /// <summary>
        /// The length of the portion of the <see cref="DisplayText"/> that matches the user input value for which this suggestion was generated.
        /// </summary>
        public int DisplayTextMatchLength { get; }

        /// <summary>
        /// Flag indicating this "suggestion" is actually an error message to surface to the user.
        /// </summary>
        public bool IsErrorMessage { get; }

        /// <summary>
        /// A formatted version of <see cref="DisplayText"/> that bolds the matching portion of text.
        /// </summary>
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

        /// <summary>
        /// Class representing a suggestion to be surfaced to the user.
        /// </summary>
        /// <param name="value">The actual value represented by this suggestion.</param>
        /// <param name="displayText">The text to be displayed to the user for this suggestion (often matches <c>value</c>).</param>
        /// <param name="displayTextMatchIndex">The starting index for the portion of the <c>displayText</c> that matches the user input value for which this suggestion was generated.</param>
        /// <param name="displayTextMatchLength">The length of the portion of the<c>displayText</c> that matches the user input value for which this suggestion was generated.</param>
        public Suggestion(string value, string displayText, int displayTextMatchIndex, int displayTextMatchLength)
        {
            Value = value;
            DisplayText = displayText;
            DisplayTextMatchIndex = displayTextMatchIndex;
            DisplayTextMatchLength = displayTextMatchLength;
        }

        /// <summary>
        /// Class representing a suggestion to be surfaced to the user.
        /// </summary>
        /// <param name="errorMessage">The error message to be surfaced to the user in a similar fashion to an actionable suggestion.</param>
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
