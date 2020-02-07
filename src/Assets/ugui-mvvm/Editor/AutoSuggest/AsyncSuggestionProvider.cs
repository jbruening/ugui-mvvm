using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoSuggest
{
    /// <summary>
    /// Helper class for suggestion providers that get their suggestions async.
    /// It ensures derived class's GetSuggestionsAsync is called only once for a given key press
    /// and caches the suggestions to show until the new ones are ready, then notifies the caller that they've changed.
    /// </summary>
    public abstract class AsyncSuggestionProvider : ISuggestionProvider
    {
        private CancellationTokenSource _cancelSource;
        private string _searchString;
        private bool _searchFocus;
        private IEnumerable<Suggestion> _results = Enumerable.Empty<Suggestion>();

        /// <inheritdoc />
        public event Action SuggestionsChanged;

        /// <inheritdoc />
        public IEnumerable<Suggestion> GetSuggestions(string currentValue, bool isFocused)
        {
            if (_searchString != currentValue
                || _searchFocus != isFocused)
            {
                _searchString = currentValue;
                _searchFocus = isFocused;

                GetSuggestionsAndUpdateResultsAsync(currentValue, isFocused);
            }

            return _results;
        }

        /// <summary>
        /// Async version of <see cref="GetSuggestions(string, bool)"/> to be implemented by inherited class.
        /// </summary>
        /// <param name="currentValue">The current value for which to fetch suggestions.</param>
        /// <param name="isFocused">Flag indicating if focus is currently in the control for which the suggestions are being fetched.</param>
        /// <param name="cancellationToken">Token to cancel the async operation if its result is no longer relevant.</param>
        /// <returns>An ordered collection of suggestions.</returns>
        abstract public Task<IEnumerable<Suggestion>> GetSuggestionsAsync(string currentValue, bool isFocused, CancellationToken cancellationToken);

        private async void GetSuggestionsAndUpdateResultsAsync(string currentValue, bool isFocused)
        {
            if (_cancelSource != null)
            {
                _cancelSource.Cancel();
            }
            _cancelSource = new CancellationTokenSource();

            try
            {
                _results = await GetSuggestionsAsync(currentValue, isFocused, _cancelSource.Token).ConfigureAwait(false);

                SuggestionsChanged?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
        }
    }
}
