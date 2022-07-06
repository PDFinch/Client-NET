using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PDFinch.Client.Common.Tests")]
namespace PDFinch.Client.Common.Internal
{
    /// <summary>
    /// Makes sure only one of each client name and API key are registered.
    /// </summary>
    internal static class OptionsValidator
    {
        /// <summary>
        /// Throws for invalid configuration.
        /// </summary>
        public static void ValidateOptions(IEnumerable<PdfClientOptions>? optionList)
        {
            var materialized = optionList?.ToArray() ?? Array.Empty<PdfClientOptions>();
            
            if (!materialized.Any())
            {
                throw new ArgumentException("No PDFinch API clients were registered.", nameof(optionList));
            }
            
            if (materialized.Any(option => string.IsNullOrEmpty(option.ApiKey) || string.IsNullOrEmpty(option.ApiSecret)))
            {
                throw new ArgumentNullException(nameof(optionList), $"No {nameof(PdfClientOptions.ApiKey)} and/or {nameof(PdfClientOptions.ApiSecret)} configured");
            }

            var duplicateNames = GetDuplicateNames(materialized, o => o.Name?.ToUpperInvariant());
            if (duplicateNames.Any())
            {
                var errorString = $"Multiple clients named '{string.Join(", ", duplicateNames)}' were registered. Client names must be unique.";
                throw new ArgumentException(errorString, nameof(optionList));
            }

            var duplicateKeys = GetDuplicateNames(materialized, o => o.ApiKey?.ToUpperInvariant());
            if (duplicateKeys.Any())
            {
                var errorString = $"Multiple clients with API key '{string.Join(", ", duplicateKeys)}' were registered. API keys must be unique.";
                throw new ArgumentException(errorString, nameof(optionList));
            }
        }

        private static IList<string> GetDuplicateNames(IEnumerable<PdfClientOptions> optionList, Func<PdfClientOptions, string?> keySelector)
        {
            var duplicates = optionList.GroupBy(keySelector)
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && kvp.Count() > 1)
                .Select(kvp => kvp.Key!)
                .ToList();

            return duplicates;
        }
    }
}
