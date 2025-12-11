namespace StellarDotnetSdk.Examples.Soroban.Helpers;

/// <summary>
///     Shared constants for Soroban WASM files.
/// </summary>
internal static class SorobanWasms
{
    public static readonly string HelloWasmPath = Path.GetFullPath("wasm/hello_world_contract.wasm");
    public static readonly string TokenWasmPath = Path.GetFullPath("wasm/token_contract.wasm");
    public static readonly string AtomicSwapWasmPath = Path.GetFullPath("wasm/atomic_swap_contract.wasm");
    public static readonly string EventsWasmPath = Path.GetFullPath("wasm/events_contract.wasm");
    public static readonly string IncrementWasmPath = Path.GetFullPath("wasm/increment_contract.wasm");
}

