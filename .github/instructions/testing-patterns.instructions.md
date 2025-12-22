---
applyTo: "**/Tests/**,**/*Test*.cs"
---
**Location**: `StellarDotnetSdk.Tests/`, mirror SDK structure. Name: `<Class>Test.cs`.

**Frameworks**: MSTest, Moq, FluentAssertions.

## Test Naming

Pattern: `[Method]_[Scenario]_[Result]`

| ❌ Avoid | ✅ Preferred |
|----------|-------------|
| `TestSign` | `Sign_ValidData_ReturnsSignature` |
| `TestVerifyFalse` | `Verify_InvalidSignature_ReturnsFalse` |
| `TestFromSecretSeed` | `FromSecretSeed_ValidSeed_ReturnsKeyPair` |

Common patterns:
- `Method_ValidInput_ReturnsExpected` (happy path)
- `Method_NullInput_ThrowsArgumentNull` (edge case)
- `Method_WhenDisposed_ThrowsObjectDisposed` (state)

**Don'ts**: No `Test` prefix, no vague names, describe behavior not implementation.

## Structure

**AAA**: `// Arrange`, `// Act`, `// Assert` sections.

**Mocking**: `new Mock<T>()`, `Setup().ReturnsAsync()`. `It.IsAny<T>()` for flexible matching.

**Assertions**: FluentAssertions (`Should().NotBeNull()`, `Should().BeTrue()`).

**Test data**: JSON in `TestData/`, load via `Utils.CreateTestServerWithJson()`.

**Run**: `dotnet test` | `dotnet test --filter "FullyQualifiedName~ClassName"`.
