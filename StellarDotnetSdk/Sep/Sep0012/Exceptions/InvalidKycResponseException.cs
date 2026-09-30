using System;

namespace StellarDotnetSdk.Sep.Sep0012.Exceptions;

/// <summary>
///     Thrown when the KYC server answers with a success status but the body is not a valid SEP-0012 response: it
///     is empty, exceeds the response size limit, is not JSON, repeats a property, omits a required field, or carries
///     a status or field type outside the set SEP-0012 defines. It carries no anchor <c>error</c> text:
///     <see cref="KycServiceException.ErrorMessage" /> is always <c>null</c>.
/// </summary>
public class InvalidKycResponseException : KycServiceException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidKycResponseException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="innerException">The exception that caused this one, if any.</param>
    public InvalidKycResponseException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, statusCode, null, innerException)
    {
    }
}
