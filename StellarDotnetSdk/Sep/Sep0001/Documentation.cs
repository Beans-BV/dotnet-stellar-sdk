namespace StellarDotnetSdk.Sep.Sep0001;

/// <summary>
///     Organization Documentation. From the stellar.toml DOCUMENTATION table.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0001.md">SEP-0001</a>
/// </summary>
public sealed record Documentation
{
    /// <summary>
    ///     Legal name of the organization.
    /// </summary>
    public string? OrgName { get; init; }

    /// <summary>
    ///     (may not apply) DBA of the organization.
    /// </summary>
    public string? OrgDba { get; init; }

    /// <summary>
    ///     The organization's official URL. The stellar.toml must be hosted on the same domain.
    /// </summary>
    public string? OrgUrl { get; init; }

    /// <summary>
    ///     A URL to a PNG image of the organization's logo on a transparent background.
    /// </summary>
    public string? OrgLogo { get; init; }

    /// <summary>
    ///     Short description of the organization.
    /// </summary>
    public string? OrgDescription { get; init; }

    /// <summary>
    ///     Physical address for the organization.
    /// </summary>
    public string? OrgPhysicalAddress { get; init; }

    /// <summary>
    ///     URL on the same domain as the orgUrl that contains an image or pdf official document attesting to the physical
    ///     address.
    ///     It must list the orgName or orgDba as the party at the address. Only documents from an official third party are
    ///     acceptable.
    ///     E.g. a utility bill, mail from a financial institution, or business license.
    /// </summary>
    public string? OrgPhysicalAddressAttestation { get; init; }

    /// <summary>
    ///     The organization's phone number in E.164 format, e.g. +14155552671.
    /// </summary>
    public string? OrgPhoneNumber { get; init; }

    /// <summary>
    ///     URL on the same domain as the orgUrl that contains an image or pdf of a phone bill showing both the phone number
    ///     and the organization's name.
    /// </summary>
    public string? OrgPhoneNumberAttestation { get; init; }

    /// <summary>
    ///     A Keybase account name for the organization. Should contain proof of ownership of any public online accounts you
    ///     list here,
    ///     including the organization's domain.
    /// </summary>
    public string? OrgKeybase { get; init; }

    /// <summary>
    ///     The organization's Twitter account.
    /// </summary>
    public string? OrgTwitter { get; init; }

    /// <summary>
    ///     The organization's Github account.
    /// </summary>
    public string? OrgGithub { get; init; }

    /// <summary>
    ///     An email where clients can contact the organization. Must be hosted at the orgUrl domain.
    /// </summary>
    public string? OrgOfficialEmail { get; init; }

    /// <summary>
    ///     An email that users can use to request support regarding the organization's Stellar assets or applications.
    /// </summary>
    public string? OrgSupportEmail { get; init; }

    /// <summary>
    ///     Name of the authority or agency that licensed the organization, if applicable.
    /// </summary>
    public string? OrgLicensingAuthority { get; init; }

    /// <summary>
    ///     Type of financial or other license the organization holds, if applicable.
    /// </summary>
    public string? OrgLicenseType { get; init; }

    /// <summary>
    ///     Official license number of the organization, if applicable.
    /// </summary>
    public string? OrgLicenseNumber { get; init; }
}