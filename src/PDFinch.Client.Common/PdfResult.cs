using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// A wrapper class for calls into the business layer, so controllers don't have to do their own error handling.
    /// 
    /// Contains data when successful.
    /// </summary>
    public class PdfResult<T>
    {
        /// <summary>
        /// Indicates whether the call was successful.
        /// When true, <see cref="Data"/> is not null.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Data))]
        public bool Success { get; }

        /// <summary>
        /// The data of the result. Not null when <see cref="Success"/> is true.
        /// </summary>
        public T? Data { get; }

        /// <summary>
        /// Contains an error message when <see cref="Success"/> is <see langword="false"/>.
        /// </summary>
        public string? StatusMessage { get; }

        /// <summary>
        /// Indicates that the call failed due to insufficient funds.
        /// </summary>
        public bool IsOutOfCredits { get; }

        /// <summary>
        /// Indicates that an unknown error occurred.
        /// </summary>
        public bool OtherError { get; }

        /// <summary>
        /// A successful result with data.
        /// </summary>
        /// <param name="data"></param>
        public PdfResult(T data)
        {
            Data = data;
            Success = true;
        }

        /// <summary>
        /// A failed result without data.
        /// </summary>
        public PdfResult(bool outOfCredits = false, bool otherError = false, string? statusMessage = null)
        {
            IsOutOfCredits = outOfCredits;
            OtherError = otherError;
            StatusMessage = statusMessage;
        }

        /// <summary>
        /// Generate an out-of-credits response.
        /// </summary>
        public static PdfResult<T> OutOfCredits(string apiKey) => new(outOfCredits: true, statusMessage: JsonStatus($"No credit left for organization owning API key '{apiKey}'"));

        /// <summary>
        /// Generates a machine-readable StatusMessage JSON from an error string.
        /// </summary>
        public static string JsonStatus(string errorMessage)
        {
            return $"{{\"message\":\"{JsonSerializer.Serialize(errorMessage)}\"}}";
        }
    }
}
