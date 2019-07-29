using AutoSuggest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace uguimvvm
{
    public class TypeSuggestion : Suggestion
    {
        public Type Type { get; }

        public TypeSuggestion(string errorMessage)
            : base(errorMessage)
        { }

        public TypeSuggestion(Type type, string searchString)
            : base(type.FullName, type.FullName, type.FullName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase), searchString.Length)
        {
            Type = type;
        }
    }

    public class TypeSuggestionProvider : AsyncSuggestionProvider
    {
        private IEnumerable<Type> _types;

        public Type SelectedType { get; private set; } = null;
        public bool SelectedTypeIsValid { get; protected set; } = false;

        public override async Task<IEnumerable<Suggestion>> GetSuggestionsAsync(string currentValue, bool isFocused, CancellationToken cancellationToken)
        {
            IList<TypeSuggestion> results = null;

            // Mark selected type as invalid while loading
            SelectedTypeIsValid = false;
            SelectedType = null;

            await Task.Run(() =>
            {
                if (_types == null)
                {
                    var typeQuery = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                    {
                        try
                        {
                            return a.GetTypes();
                        }
                        catch (Exception)
                        {
                            return new Type[] { };
                        }
                    });
                    // Calling ToList forces the query to execute this one time, instead of executing every single time "types" is enumerated.
                    _types = typeQuery.ToList();
                }

                results = _types
                    .AsParallel()
                    .WithCancellation(cancellationToken)
                    .Select((t) => new TypeSuggestion(t, currentValue))
                    .Where((s) => s.DisplayTextMatchIndex >= 0)
                    .OrderBy(opt => opt.DisplayTextMatchIndex)
                    .ThenBy(opt => opt.Value.Length)
                    .ThenBy(opt => opt.Value)
                    .ToList();

                cancellationToken.ThrowIfCancellationRequested();

                if (results.Any() && results[0].Value == currentValue)
                {
                    SelectedType = results[0].Type;
                }
                else
                {
                    SelectedType = null;
                }

                // Now that loading has completed, mark selected type as valid.  It is valid even if it is null.
                SelectedTypeIsValid = true;

                if (isFocused)
                {
                    if (!results.Any())
                    {
                        // No valid results from the search string
                        results.Add(new TypeSuggestion($"Error: No type matches the search string of \"{currentValue}\""));
                    }
                }
                else
                {
                    // Not focused.  Clear the list.
                    results.Clear();

                    if (!string.IsNullOrEmpty(currentValue) && SelectedType == null)
                    {
                        // Not focused and have typed in a string but it didn't perfectly match any type.
                        results.Add(new TypeSuggestion($"Error: There is no type named \"{currentValue}\""));
                    }
                }
            }).ConfigureAwait(false);

            return results;
        }
    }
}
