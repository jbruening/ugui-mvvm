using System;
using System.Collections.Generic;

namespace AutoSuggest
{
    public interface ISuggestionProvider
    {
        // Gets a list of suggestions for the current string value.
        IEnumerable<Suggestion> GetSuggestions(string currentValue, bool isFocused);

        // Fired if something has invalidated the previous list of values.  The AutoSuggestField should requery to get the latest list.
        event Action SuggestionsChanged;
    }
}
