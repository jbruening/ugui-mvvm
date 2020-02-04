using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoSuggest
{
    /// <summary>
    /// Suggestion provider for wrapping a non-changing list of possible options that should be used to provide suggestions.
    /// </summary>
    public class FixedListSuggestionProvider : ISuggestionProvider
    {
        private IReadOnlyList<string> _options;
        private readonly bool _warnForUnknownValues;

        /// <inheritdoc />
        public event Action SuggestionsChanged;

        /// <summary>
        /// Suggestion provider for wrapping a non-changing list of possible options that should be used to provide suggestions.
        /// </summary>
        /// <param name="options">The complete collection of possible options.</param>
        /// <param name="warnForUnknownValues">Flag indicating if the user should be warned when their entered value cannot be mapped to any of the <c>options</c>.</param>
        public FixedListSuggestionProvider(IReadOnlyList<string> options, bool warnForUnknownValues)
        {
            _options = options;
            _warnForUnknownValues = warnForUnknownValues;
        }

        /// <inheritdoc />
        public IEnumerable<Suggestion> GetSuggestions(string currentValue, bool isFocused)
        {
            var errors = new List<Suggestion>();
            var suggestions = Enumerable.Empty<Suggestion>();

            // Note: This currently uses a very basic 'includes' based search.  If in the future this
            // is not providing helpful enough results, we could instead use the (much more computationally
            // expensive) Levenshtein distance to rank results (https://en.wikipedia.org/wiki/Levenshtein_distance)
            var optionsWithInfo = _options.Select(s => new Suggestion(s, s, s.IndexOf(currentValue, StringComparison.CurrentCultureIgnoreCase), currentValue.Length));
            optionsWithInfo = optionsWithInfo.Where(opt => opt.DisplayTextMatchIndex >= 0);
            suggestions = optionsWithInfo
                .OrderBy(opt => opt.DisplayTextMatchIndex)
                .ThenBy(opt => opt.Value.Length)
                .ThenBy(opt => opt.Value);

            if (_warnForUnknownValues)
            {
                if (isFocused)
                {
                    if (!suggestions.Any())
                    {
                        // When focused, show an error only if there are no suggestions
                        errors.Add(new Suggestion($"\"{currentValue}\" does not match any known value."));
                    }
                }
                else // not focused
                {
                    // When not focused, show error if the current value is not in the suggestions.
                    if (!suggestions.Any((s) => s.Value == currentValue))
                    {
                        errors.Add(new Suggestion($"\"{currentValue}\" does not match any known value."));
                    }
                }
            }

            if (!isFocused ||
                (suggestions.Count() == 1 && suggestions.First().Value == currentValue))
            {
                // Either the control is not focused or
                // there is only one item to display and it perfectly matches the typed string.
                // In both of those cases, don't display the suggestions.
                suggestions = Enumerable.Empty<Suggestion>();
            }

            return errors.Concat(suggestions);
        }

        /// <summary>
        /// Update the fixed set of options from which to generate suggestions.
        /// </summary>
        /// <param name="options">The complete collection of possible options.</param>
        public void SetOptions(IReadOnlyList<string> options)
        {
            if (!_options.SequenceEqual(options))
            {
                _options = options;
                SuggestionsChanged?.Invoke();
            }
        }
    }
}
