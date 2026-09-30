using StellarDotnetSdk.MauiValidation;

// ValidationRunner writes every line to the console itself. Exit code 0 only if every check passed.
return await new ValidationRunner(_ => { }).RunAllAsync() ? 0 : 1;
