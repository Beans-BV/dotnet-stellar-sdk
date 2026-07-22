#if TEST_SDK_NETSTANDARD21
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Exercises the <c>lib/netstandard2.0</c> System.Text.Json asset that real netstandard2.1
///     consumers (Unity, Tizen, Mono) load at runtime. This test project runs the netstandard2.1
///     SDK build on the net8.0 CLR, which resolves the package's <c>lib/net8.0</c> asset instead,
///     so without this probe the asset that actually ships to those consumers is never executed
///     in CI. The asset and its package dependency closure are resolved from the SDK's
///     <c>project.assets.json</c> restore output, loaded from the NuGet cache into an isolated
///     <see cref="AssemblyLoadContext" />, and driven via reflection because its types are
///     distinct from the host's own System.Text.Json.
/// </summary>
[TestClass]
public class SystemTextJsonNetStandard20AssetTest
{
    private const string DuplicatePropertyJson = """{"Value":1,"Value":2}""";

    // PublicationOnly so a transient load failure (e.g. project.assets.json not yet restored) is not
    // cached forever: the default Lazy mode caches the first exception and re-throws it for every later
    // access, which would mask the real first failure behind identical repeats across the test methods.
    private static readonly Lazy<NetStandard20SystemTextJson> Stj =
        new(NetStandard20SystemTextJson.Load, LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    ///     The netstandard2.1 restore must keep resolving a System.Text.Json version whose
    ///     <c>lib/netstandard2.0</c> asset carries the serializer-level
    ///     <c>AllowDuplicateProperties</c> guard, i.e. the 10.x floor introduced for the
    ///     netstandard2.1 target. This fails on any downgrade (8.x has no such option).
    /// </summary>
    [TestMethod]
    public void NetStandard21Restore_ResolvesNetStandard20Asset_WithSerializerLevelDuplicateGuard()
    {
        var stj = Stj.Value;

        StringAssert.Contains(stj.AssetPath.Replace('\\', '/'), "lib/netstandard2.0/System.Text.Json.dll");
        Assert.AreNotSame(typeof(JsonSerializerOptions).Assembly, stj.Assembly,
            "The probe must exercise the netstandard2.0 asset, not the lib/net8.0 asset the test host itself uses.");
        Assert.IsTrue(stj.PackageVersion.Major >= 10,
            $"netstandard2.1 resolved System.Text.Json {stj.PackageVersion}, but the serializer-level " +
            "AllowDuplicateProperties guard only exists from 10.x on. Unity/Tizen/Mono consumers of the " +
            "netstandard2.1 package would silently lose duplicate-property rejection.");
        Assert.IsNotNull(stj.AllowDuplicateProperties,
            "JsonSerializerOptions.AllowDuplicateProperties is missing from the netstandard2.0 asset.");
    }

    /// <summary>
    ///     With <c>AllowDuplicateProperties = false</c> the netstandard2.0 asset itself must reject
    ///     a duplicated property instead of letting the last value win. The serializer is invoked
    ///     via reflection, so the JsonException surfaces wrapped in a TargetInvocationException.
    /// </summary>
    [TestMethod]
    public void NetStandard20Asset_DuplicateProperty_ThrowsJsonException()
    {
        var ex = Assert.ThrowsException<TargetInvocationException>(() =>
            Stj.Value.Deserialize(DuplicatePropertyJson, typeof(Probe), allowDuplicateProperties: false));

        Assert.IsNotNull(ex.InnerException);
        Assert.AreEqual("System.Text.Json.JsonException", ex.InnerException!.GetType().FullName);
        Assert.AreSame(Stj.Value.Assembly, ex.InnerException.GetType().Assembly,
            "The rejection must be raised by the isolated netstandard2.0 asset itself.");
    }

    /// <summary>
    ///     Control case proving the probe harness is sensitive: the same payload deserializes
    ///     without error when duplicates are explicitly allowed, so a throw in the strict test
    ///     can only come from the duplicate-property guard.
    /// </summary>
    [TestMethod]
    public void NetStandard20Asset_DuplicatePropertyAllowed_LastValueWins()
    {
        var result = Stj.Value.Deserialize(DuplicatePropertyJson, typeof(Probe), allowDuplicateProperties: true);

        var probe = (Probe)result!;
        Assert.AreEqual(2, probe.Value);
    }

    /// <summary>
    ///     Deserialization target; public so the isolated serializer can bind to it across
    ///     load-context boundaries.
    /// </summary>
    public sealed class Probe
    {
        public int Value { get; set; }
    }

    /// <summary>
    ///     The netstandard2.0 System.Text.Json asset plus its resolved package closure, loaded
    ///     into an isolated <see cref="AssemblyLoadContext" />.
    /// </summary>
    private sealed class NetStandard20SystemTextJson
    {
        private readonly MethodInfo _deserialize;
        private readonly Type _optionsType;

        private NetStandard20SystemTextJson(Assembly assembly, string assetPath, Version packageVersion)
        {
            Assembly = assembly;
            AssetPath = assetPath;
            PackageVersion = packageVersion;
            _optionsType = assembly.GetType("System.Text.Json.JsonSerializerOptions", throwOnError: true)!;
            AllowDuplicateProperties = _optionsType.GetProperty("AllowDuplicateProperties");
            _deserialize = assembly.GetType("System.Text.Json.JsonSerializer", throwOnError: true)!
                               .GetMethod("Deserialize", new[] { typeof(string), typeof(Type), _optionsType })
                           ?? throw new MissingMethodException(
                               "System.Text.Json.JsonSerializer",
                               "Deserialize(string, Type, JsonSerializerOptions)");
        }

        public Assembly Assembly { get; }
        public string AssetPath { get; }
        public Version PackageVersion { get; }
        public PropertyInfo? AllowDuplicateProperties { get; }

        public static NetStandard20SystemTextJson Load()
        {
            var assetsFile = FindSdkAssetsFile();
            var (assemblyPaths, stjPath, stjVersion) = ReadNetStandard21RuntimeClosure(assetsFile);
            var context = new PackageAssemblyLoadContext(assemblyPaths);
            var assembly = context.LoadFromAssemblyPath(stjPath);
            return new NetStandard20SystemTextJson(assembly, stjPath, stjVersion);
        }

        public object? Deserialize(string json, Type returnType, bool allowDuplicateProperties)
        {
            if (AllowDuplicateProperties == null)
            {
                throw new InvalidOperationException(
                    $"System.Text.Json {PackageVersion} (netstandard2.0 asset) has no " +
                    "JsonSerializerOptions.AllowDuplicateProperties; the serializer-level duplicate-property " +
                    "guard requires System.Text.Json 10.x or later on the netstandard2.1 target.");
            }

            var options = Activator.CreateInstance(_optionsType);
            AllowDuplicateProperties.SetValue(options, allowDuplicateProperties);
            return _deserialize.Invoke(null, new[] { json, (object)returnType, options });
        }

        private static string FindSdkAssetsFile()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            {
                var candidate = Path.Combine(directory.FullName, "StellarDotnetSdk", "obj", "project.assets.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new FileNotFoundException(
                "Could not locate StellarDotnetSdk/obj/project.assets.json above the test output directory. " +
                "The probe needs the SDK's restore output to know which packages netstandard2.1 resolved; " +
                "run 'dotnet restore' on the solution first.");
        }

        /// <summary>
        ///     Reads the netstandard2.1 target of the SDK's assets file and maps every resolved
        ///     package runtime assembly to its NuGet-cache path — the exact closure a
        ///     netstandard2.1 package consumer restores.
        /// </summary>
        private static (Dictionary<string, string> AssemblyPaths, string StjPath, Version StjVersion)
            ReadNetStandard21RuntimeClosure(string assetsFile)
        {
            using var assets = JsonDocument.Parse(File.ReadAllText(assetsFile));

            var packageFolders = assets.RootElement.GetProperty("packageFolders")
                .EnumerateObject()
                .Select(folder => folder.Name)
                .ToList();

            var target = assets.RootElement.GetProperty("targets")
                .EnumerateObject()
                .FirstOrDefault(candidate => candidate.Name == "netstandard2.1"
                                             || candidate.Name.StartsWith(".NETStandard,Version=v2.1",
                                                 StringComparison.Ordinal));
            if (target.Value.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    $"No netstandard2.1 target found in {assetsFile}; was the SDK's netstandard2.1 " +
                    "TargetFramework removed?");
            }

            var assemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? stjPath = null;
            Version? stjVersion = null;

            foreach (var library in target.Value.EnumerateObject())
            {
                if (!library.Value.TryGetProperty("type", out var type) || type.GetString() != "package"
                    || !library.Value.TryGetProperty("runtime", out var runtime))
                {
                    continue;
                }

                var separator = library.Name.IndexOf('/');
                var id = library.Name[..separator];
                var version = library.Name[(separator + 1)..];

                foreach (var asset in runtime.EnumerateObject())
                {
                    // Skips the "_._" placeholders packages use for empty runtime groups.
                    if (!asset.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // The NuGet cache lowercases package id and version directory names.
                    var relativePath = Path.Combine(
                        id.ToLowerInvariant(),
                        version.ToLowerInvariant(),
                        asset.Name.Replace('/', Path.DirectorySeparatorChar));
                    var fullPath = packageFolders
                                       .Select(folder => Path.Combine(folder, relativePath))
                                       .FirstOrDefault(File.Exists)
                                   ?? throw new FileNotFoundException(
                                       $"Package asset {relativePath} was not found in any NuGet package folder " +
                                       $"({string.Join(", ", packageFolders)}); run 'dotnet restore' first.");

                    assemblyPaths[Path.GetFileNameWithoutExtension(asset.Name)] = fullPath;

                    if (id.Equals("System.Text.Json", StringComparison.OrdinalIgnoreCase))
                    {
                        stjPath = fullPath;
                        var prerelease = version.IndexOfAny(new[] { '-', '+' });
                        var core = prerelease < 0 ? version : version[..prerelease];
                        if (!Version.TryParse(core, out var parsedVersion))
                        {
                            throw new InvalidOperationException(
                                $"Could not parse the resolved System.Text.Json version '{version}' " +
                                $"(core '{core}') from {assetsFile}. Update this probe if the version " +
                                "string format changed.");
                        }

                        stjVersion = parsedVersion;
                    }
                }
            }

            if (stjPath == null || stjVersion == null)
            {
                throw new InvalidOperationException(
                    "The netstandard2.1 target no longer resolves a System.Text.Json package runtime asset; " +
                    "update or remove this probe.");
            }

            return (assemblyPaths, stjPath, stjVersion);
        }
    }

    /// <summary>
    ///     Resolves the recorded package closure inside the isolated context and defers everything
    ///     else (System.Private.CoreLib, the netstandard facade, ...) to the default context — the
    ///     same split a Unity/Mono runtime applies between package and framework assemblies.
    /// </summary>
    private sealed class PackageAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, string> _assemblyPaths;

        public PackageAssemblyLoadContext(IReadOnlyDictionary<string, string> assemblyPaths)
            : base(nameof(SystemTextJsonNetStandard20AssetTest))
        {
            _assemblyPaths = assemblyPaths;
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            return assemblyName.Name is { } name && _assemblyPaths.TryGetValue(name, out var path)
                ? LoadFromAssemblyPath(path)
                : null;
        }
    }
}
#endif
