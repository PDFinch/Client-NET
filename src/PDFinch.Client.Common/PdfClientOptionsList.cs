using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PDFinch.Client.Common.Internal;

namespace PDFinch.Client.Common;

/// <summary>
/// Contains a validated list of <see cref="PdfClientOptions"/>.
/// </summary>
public class PdfClientOptionsList
{
    private readonly ConcurrentBag<PdfClientOptions> _optionsCache;

    /// <summary>
    /// Instantiates an options list with the options in <paramref name="options"/>, and validates those.
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public PdfClientOptionsList(ICollection<PdfClientOptions> options)
    {
        OptionsValidator.ValidateOptions(options);

        _optionsCache = new(options);
    }

    /// <summary>
    /// Finds a client's options.
    /// </summary>
    /// <param name="nameOrApiKey"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public PdfClientOptions GetClientOptions(string? nameOrApiKey)
    {
        if (nameOrApiKey == null && _optionsCache.Count != 1)
        {
            throw new ArgumentNullException(nameof(nameOrApiKey), $"There are {_optionsCache.Count} clients registered. Specify by name or API key which client you want to use, or register exactly one.");
        }

        var options = _optionsCache.FirstOrDefault(o => nameOrApiKey == null || o.ApiKey == nameOrApiKey || o.Name == nameOrApiKey);

        if (options == null)
        {
            throw new ArgumentException($"Client not found by name or API key '{nameOrApiKey}'");
        }

        return options;
    }

    /// <summary>
    /// Returns all registered options.
    /// </summary>
    /// <returns></returns>
    public IList<PdfClientOptions> GetAll() => _optionsCache.ToArray();
}