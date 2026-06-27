#if TEST_SDK_NETSTANDARD21
namespace StellarDotnetSdk.Tests.Sep.Sep0009.Fixtures;

internal static class KycTestDates
{
    internal const string BirthDate = "1990-01-15";
    internal const string IdIssueDate = "2020-05-10";
    internal const string IdExpirationDate = "2030-05-10";
    internal const string RegistrationDate = "2020-01-01";
    internal const string RegistrationDateIso = "2020-01-15";
    internal const string NestedBirthDate = "1985-12-25";
    internal const string NestedRegistrationDate = "2010-06-01";
}
#else
using System;

namespace StellarDotnetSdk.Tests.Sep.Sep0009.Fixtures;

internal static class KycTestDates
{
    internal static DateOnly BirthDate => new(1990, 1, 15);
    internal static DateOnly IdIssueDate => new(2020, 5, 10);
    internal static DateOnly IdExpirationDate => new(2030, 5, 10);
    internal static DateOnly RegistrationDate => new(2020, 1, 1);
    internal static DateOnly RegistrationDateIso => new(2020, 1, 15);
    internal static DateOnly NestedBirthDate => new(1985, 12, 25);
    internal static DateOnly NestedRegistrationDate => new(2010, 6, 1);
}
#endif
