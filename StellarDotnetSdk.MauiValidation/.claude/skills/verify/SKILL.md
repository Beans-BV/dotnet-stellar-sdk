---
name: verify
description: Verify changes to the MAUI validation harness (StellarDotnetSdk.MauiValidation) at its runtime surfaces - the desktop console against Stellar Testnet, its error paths, and run-android.sh against a device.
---

# Verify the MAUI validation harness

## Desktop console: ValidationRunner.cs and StreamSearch.cs

Run from the repository root. Build first, then run the DLL itself: `dotnet run` restores packages, which fails
once the network is blocked on purpose (below).

```bash
dotnet build StellarDotnetSdk.MauiValidation/Desktop/StellarDotnetSdk.MauiValidation.Desktop.csproj -c Release
dotnet StellarDotnetSdk.MauiValidation/Desktop/bin/Release/net10.0/StellarDotnetSdk.MauiValidation.Desktop.dll > out.txt
grep -vc '^STELLAR-MAUI-VALIDATION ' out.txt   # must print 0: run-android.sh drops lines without the prefix
```

- Expect exit 0, `RESULT PASS 6/6`, two `INFO sse-latency` lines and `DONE`, in about 35 s.
- The `INFO assemblies` line names the SDK commit that ran.
- The raw `Stream.ReadAsync` reader (the StreamSearch path) is in the `horizon.submit-and-sse-stream` PASS line.
- The build warnings come from the SDK project when it is rebuilt; check `MauiValidation/*.cs` warnings only.

Error paths, with no code change: send the process through a closed proxy port.

```bash
HTTPS_PROXY=http://127.0.0.1:9 HTTP_PROXY=http://127.0.0.1:9 dotnet <the same DLL>
```

Expect exit 1 within seconds and `RESULT FAIL 2/6`: the crypto checks pass, every FAIL line is followed by prefixed
`DETAIL` lines, and the INFO lines carry the exception message, all with the prefix.

## Android: run-android.sh

- A full run builds the app first and needs the `maui-android` workload (`dotnet workload list`).
- Use a throwaway emulator, not a personal device: the script clears logcat and resizes its buffer. A read-only
  instance leaves the AVD unchanged and boots in about 40 s (`<sdk>/emulator/emulator -list-avds` lists the names):
  `<sdk>/emulator/emulator -avd <name> -read-only -no-window -no-audio -no-snapshot -no-boot-anim -port 5580`.
  Stop it with `adb -s emulator-5580 emu kill`.
- Without the workload, run the script with `DOTNET` set to a no-op script and `ANDROID_SDK_ROOT` set to a folder
  whose `platform-tools/adb` forwards to the real adb but skips `install`. Run a copy of the script, so that its
  `rm -rf` of `bin/Release` and `obj/Release` hits the copy's folder. The app is then not installed, so the launch
  check fails: monkey prints `** No activities found to run, monkey aborted.` and exits 252.
- For the success path, copy the `if ! launch=` block out of the script and run it with
  `adb=(<sdk>/platform-tools/adb -s emulator-5580)` and `app_id=com.android.settings`.

## Not reachable at a surface

- The 404 re-read in `horizon.friendbot-and-account` needs Horizon to lag. The Horizon URL is a constant, so a fake
  Horizon needs a patched copy of ValidationRunner.cs.
- The harness steps in `.github/workflows/pack_and_test.yml`: it runs on `pull_request_target`, so a PR's run uses
  the workflow from `main`. They show up only after merge.
