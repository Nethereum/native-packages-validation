# Nethereum Native Packages Validation

Cross-platform validation tests for Nethereum native NuGet packages. These tests consume the packages as a real NuGet consumer would, verifying that native binaries load and work correctly on all supported platforms.

## Packages Under Test

| Package | Native Library | Platforms |
|---------|---------------|-----------|
| `Nethereum.Signer.Bls.Herumi` | BLS12-381 signatures via Herumi | win-x64, linux-x64, linux-arm64, osx-x64, osx-arm64 |
| `Nethereum.ZkProofs.RapidSnark` | Groth16 proof generation via rapidsnark | win-x64, linux-x64, linux-arm64, osx-x64, osx-arm64 |
| `Nethereum.CircomWitnessCalc` | Circom witness generation via circom-witnesscalc | win-x64, linux-x64, linux-arm64, osx-x64, osx-arm64 |

## CI Matrix

| Runner | OS | Architecture |
|--------|----|-------------|
| `ubuntu-latest` | Linux | x64 |
| `ubuntu-24.04-arm` | Linux | ARM64 |
| `windows-latest` | Windows | x64 |
| `macos-15` | macOS | ARM64 (Apple Silicon) |
| `macos-13` | macOS | x64 (Intel) |

## Running Locally

```bash
dotnet test test/BlsHerumi.Tests/
dotnet test test/RapidSnark.Tests/
dotnet test test/CircomWitnessCalc.Tests/
```

## Updating Packages

1. Copy new `.nupkg` files into `nativeartifacts/`
2. Update `Version` in each test `.csproj` if the version changed
3. Push — CI will validate across all platforms

## Test Data

- **RapidSnark**: `test/RapidSnark.Tests/TestData/` contains a small multiplier circuit zkey, witness, and verification key
- **CircomWitnessCalc**: `test/CircomWitnessCalc.Tests/TestData/` contains a compiled multiplier circuit graph

Source circuit: `c <== a * b` (minimal Groth16 circuit for validation)
