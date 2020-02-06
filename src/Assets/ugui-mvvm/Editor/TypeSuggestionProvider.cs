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
    /// <summary>
    /// Represents a suggestion to be surfaced to a user, representing a <see cref="System.Type"/>.
    /// </summary>
    public class TypeSuggestion : Suggestion
    {
        /// <summary>
        /// The underlying <see cref="System.Type"/> being represented.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Represents a suggestion to be surfaced to a user, representing a <see cref="System.Type"/>.
        /// </summary>
        /// <param name="errorMessage">The error message to be surfaced to the user in a similar fashion to an actionable suggestion.</param>
        public TypeSuggestion(string errorMessage)
            : base(errorMessage)
        { }

        /// <summary>
        /// Represents a suggestion to be surfaced to a user, representing a <see cref="System.Type"/>.
        /// </summary>
        /// <param name="type">The underlying <see cref="System.Type"/> being represented.</param>
        /// <param name="searchString">The user input string entered that maps to this suggestion.</param>
        public TypeSuggestion(Type type, string searchString)
            : base(type.FullName, type.FullName, type.FullName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase), searchString.Length)
        {
            Type = type;
        }
    }

    /// <summary>
    /// <see cref="ISuggestionProvider"/> implementation for surfacing all <see cref="Type"/>s defined in the current <see cref="AppDomain"/>.
    /// </summary>
    public class TypeSuggestionProvider : AsyncSuggestionProvider
    {
        private IEnumerable<Type> _types;

        /// <summary>
        /// The current selected <see cref="Type"/>, if a <see cref="Type"/> is selected.
        /// </summary>
        public Type SelectedType { get; private set; } = null;

        /// <summary>
        /// Flag indicating that the <see cref="SelectedType"/> property has been evaluated against the latest value.
        /// </summary>
        public bool SelectedTypeIsValid { get; protected set; } = false;

        /// <inheritdoc />
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
                    .Where((t) => Attribute.GetCustomAttribute(t, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)) == null)
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
