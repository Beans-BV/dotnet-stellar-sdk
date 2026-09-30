#!/usr/bin/env bash
# Builds the validation app in Release (trimmed), installs it on the connected Android device or emulator,
# launches it and prints the validation lines from logcat.
#
# Usage: ./run-android.sh [full|partial] [extra msbuild args...]
#   full    - TrimMode=full (default): every assembly trimmed, all IL2xxx warnings reported.
#   partial - TrimMode=partial: the out-of-the-box MAUI Release configuration.
#
# Exit code 0 only if every check passed, except checks listed in KNOWN_FAILURES that failed for their known reason,
# which are reported as "KNOWN FAIL". By default that is horizon.submit-and-sse-stream failing because the SDK's
# default SSE stream is late on Android (docs/maui-compatibility.md §3.3); any other failure of it still counts.
# Set KNOWN_FAILURES="" to make every failure fatal.
#
# Environment (defaults in brackets):
#   ANDROID_SDK_ROOT  Android SDK [$ANDROID_HOME, else ~/Library/Android/sdk on macOS, ~/Android/Sdk elsewhere]
#   JAVA_HOME         JDK 17 or 21 [taken from the environment]
#   DOTNET            dotnet executable with the maui-android workload [dotnet]
#   ADB_SERIAL        device serial when several are connected, e.g. a physical device [unset]
#   TIMEOUT_SECONDS   how long to wait for the app to finish [1200]
#   BOOT_TIMEOUT_SECONDS  how long to wait for a booted device after the build [300]
#   KNOWN_FAILURES    space-separated check names whose known failure does not fail the run; each needs a reason in
#                     known_reason below [horizon.submit-and-sse-stream]
set -euo pipefail

usage() {
    sed -n '5,7p' "$0" | sed 's/^# \{0,1\}//'
}

trim_mode="${1:-full}"
case "$trim_mode" in
    full | partial) ;;
    -h | --help)
        usage
        exit 0
        ;;
    *)
        usage >&2
        exit 2
        ;;
esac
shift || true
here="$(cd "$(dirname "$0")" && pwd)"
if [[ -n "${ANDROID_SDK_ROOT:-}" ]]; then
    sdk_root="$ANDROID_SDK_ROOT"
elif [[ -n "${ANDROID_HOME:-}" ]]; then
    sdk_root="$ANDROID_HOME"
elif [[ "$(uname -s)" == "Darwin" ]]; then
    sdk_root="$HOME/Library/Android/sdk"
else
    sdk_root="$HOME/Android/Sdk"
fi
dotnet="${DOTNET:-dotnet}"
timeout_seconds="${TIMEOUT_SECONDS:-1200}"
boot_timeout_seconds="${BOOT_TIMEOUT_SECONDS:-300}"
known_failures="${KNOWN_FAILURES-horizon.submit-and-sse-stream}"
for value in "$timeout_seconds" "$boot_timeout_seconds"; do
    if [[ ! "$value" =~ ^[0-9]{1,6}$ ]] || (( 10#$value < 1 )); then
        echo "TIMEOUT_SECONDS and BOOT_TIMEOUT_SECONDS must be whole numbers of seconds, 1 or more" >&2
        exit 2
    fi
done
# Base 10, so that a leading zero is not read as octal.
timeout_seconds=$((10#$timeout_seconds))
boot_timeout_seconds=$((10#$boot_timeout_seconds))
adb=("$sdk_root/platform-tools/adb")
[[ -n "${ADB_SERIAL:-}" ]] && adb+=(-s "$ADB_SERIAL")
app_id="io.github.beansbv.dotnetstellarsdk.mauivalidation"
log_prefix="STELLAR-MAUI-VALIDATION"
out_dir="$here/bin/Release/net10.0-android/publish"
# The failure message a known failure must show to be excused; any other failure of that check still fails the run.
known_reason() {
    case "$1" in
        horizon.submit-and-sse-stream) echo "SSE event arrived" ;;
        *) echo "" ;;
    esac
}
# Keep in sync with the checks in ValidationRunner.cs.
expected_checks="crypto.rfc8032 crypto.random-keypair horizon.friendbot-and-account horizon.submit-and-sse-stream
horizon.sse-stream-sockets-handler soroban.simulate"
# Only a check with a known reason can be excused: excusing it by name alone would hide any other failure of it.
for known in $known_failures; do
    if [[ -z "$(known_reason "$known")" ]]; then
        echo "KNOWN_FAILURES: no known failure reason is defined for '$known' (see known_reason in $0)" >&2
        exit 2
    fi
done

if [[ ! -x "${adb[0]}" ]]; then
    echo "adb not found at ${adb[0]}; set ANDROID_SDK_ROOT" >&2
    exit 2
fi

java_arg=()
[[ -n "${JAVA_HOME:-}" ]] && java_arg=("-p:JavaSdkDirectory=$JAVA_HOME")

# Start clean: switching trim modes over an incremental build can reuse stale linked assemblies, which then crash
# on startup with TypeLoadException inside MAUI itself.
rm -rf "$here/obj/Release" "$here/bin/Release"

# ValidationAndroidOnly keeps the iOS and Mac Catalyst targets out of the build on macOS, where they would otherwise
# need their workloads installed. (The ${arr[@]+...} form keeps an empty array working under bash 3.2's set -u.)
"$dotnet" publish "$here" -f net10.0-android -c Release -p:ValidationAndroidOnly=true \
    "-p:ValidationTrimMode=$trim_mode" "-p:AndroidSdkDirectory=$sdk_root" ${java_arg[@]+"${java_arg[@]}"} "$@"

# A freshly started emulator may still be booting.
boot_deadline=$((SECONDS + boot_timeout_seconds))
until [[ "$("${adb[@]}" shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" == "1" ]]; do
    if (( SECONDS > boot_deadline )); then
        echo "No booted device after ${boot_timeout_seconds} s; 'adb devices' lists what is connected" >&2
        exit 1
    fi
    sleep 2
done

"${adb[@]}" install -r "$out_dir/$app_id-Signed.apk"
"${adb[@]}" shell am force-stop "$app_id"
"${adb[@]}" logcat -c
# The verdict is read from the last full dump of the log buffer, so make it large enough that system chatter
# during a long run cannot push out the first results. Best effort: some devices refuse to resize it, and on some
# the new size stays until reboot.
"${adb[@]}" logcat -G 8M > /dev/null 2>&1 || echo "note: could not enlarge the logcat buffer" >&2
"${adb[@]}" shell monkey -p "$app_id" -c android.intent.category.LAUNCHER 1 > /dev/null 2>&1

# Wait for DONE. A crash shows up as a Java FATAL EXCEPTION ("Process: <app>, PID"), a native crash tombstone
# (">>> <app> <<<"), or ActivityManager reporting the process died.
started=$SECONDS
log=""
while true; do
    sleep 3
    if (( SECONDS - started > timeout_seconds )); then
        grep "$log_prefix" <<< "$log" | sed -E "s/^.*$log_prefix //" || true
        echo "Timed out after ${timeout_seconds} s without DONE" >&2
        exit 1
    fi
    # Tolerate a transient adb error; the timeout above still bounds the wait.
    log="$("${adb[@]}" logcat -d)" || continue
    if grep -q "$log_prefix DONE" <<< "$log"; then
        break
    fi
    if grep -qE "Process: $app_id, PID|>>> $app_id <<<|Process $app_id \(pid [0-9]+\) has died" <<< "$log"; then
        grep "$log_prefix" <<< "$log" | sed -E "s/^.*$log_prefix //" || true
        echo "App crashed:" >&2
        grep -E -A8 "FATAL EXCEPTION|Fatal signal|>>> $app_id <<<|has died" <<< "$log" >&2 || true
        exit 1
    fi
done

lines="$(grep "$log_prefix" <<< "$log" | sed -E "s/^.*$log_prefix //")"
echo "$lines"

# Verdict: every expected check reported exactly once, and only known failures failed.
failed=0
result_lines="$(grep -c '^RESULT ' <<< "$lines" || true)"
if [[ "$result_lines" != "1" ]]; then
    echo "Expected one RESULT line, found $result_lines (did the app run twice?)" >&2
    failed=1
fi
for check in $expected_checks; do
    status="$(grep -oE "^(PASS|FAIL) $check " <<< "$lines" | cut -d' ' -f1 || true)"
    is_known=0
    for known in $known_failures; do
        [[ "$known" == "$check" ]] && is_known=1
    done
    case "$status" in
        PASS)
            [[ $is_known == 1 ]] && echo "note: known failure $check passed; remove it from KNOWN_FAILURES" >&2
            ;;
        FAIL)
            reason="$(known_reason "$check")"
            fail_line="$(grep -E "^FAIL $check " <<< "$lines")"
            if [[ $is_known == 1 && "$fail_line" == *"$reason"* ]]; then
                echo "KNOWN FAIL $check (see docs/maui-compatibility.md)" >&2
            else
                [[ $is_known == 1 ]] && echo "$check failed for another reason than its known failure" >&2
                failed=1
            fi
            ;;
        *)
            echo "No single result for check $check" >&2
            failed=1
            ;;
    esac
done
# Failures outside the expected checks, such as the runner itself.
while read -r name; do
    [[ -z "$name" ]] && continue
    case " $(echo $expected_checks) " in
        *" $name "*) ;;
        *)
            echo "Unexpected failure: $name" >&2
            failed=1
            ;;
    esac
done <<< "$(grep -oE '^FAIL [^ ]+' <<< "$lines" | cut -d' ' -f2 || true)"
exit $failed
