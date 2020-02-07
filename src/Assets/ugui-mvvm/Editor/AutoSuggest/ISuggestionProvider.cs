using System;
using System.Collections.Generic;

namespace AutoSuggest
{
    /// <summary>
    /// Provides suggestions to be surfaced to a user based on their current input.
    /// </summary>
    public interface ISuggestionProvider
    {
        /// <summary>
        /// Gets a list of suggestions for the current string value.
        /// </summary>
        /// <param name="currentValue">The current value for which to fetch suggestions.</param>
        /// <param name="isFocused">Flag indicating if focus is currently in the control for which the suggestions are being fetched.</param>
        /// <returns>An ordered collection of suggestions.</returns>
        IEnumerable<Suggestion> GetSuggestions(string currentValue, bool isFocused);

        /// <summary>
        /// Fired if something has invalidated the previous list of values.  The AutoSuggestField should requery to get the latest list.
        /// </summary>
        event Action SuggestionsChanged;
    }
}
