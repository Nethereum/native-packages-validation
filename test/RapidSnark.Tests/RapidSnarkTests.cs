using System;
using System.IO;
using Nethereum.ZkProofs.RapidSnark;
using Xunit;
using Xunit.Abstractions;

namespace RapidSnark.Tests;

public class RapidSnarkTests
{
    private readonly ITestOutputHelper _output;

    public RapidSnarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void NativeLibrary_Loads_And_ProofSizeReturns()
    {
        ulong proofSize = 0;
        RapidSnarkBindings.groth16_proof_size(ref proofSize);

        Assert.True(proofSize > 0, "groth16_proof_size should return a positive value");
        _output.WriteLine($"groth16_proof_size = {proofSize}");
    }

    [Fact]
    public void GenerateProof_FromTestData_ReturnsValidJson()
    {
        var zkeyPath = Path.Combine(AppContext.BaseDirectory, "TestData", "circuit_final.zkey");
        var witnessPath = Path.Combine(AppContext.BaseDirectory, "TestData", "witness.wtns");

        Assert.True(File.Exists(zkeyPath), $"Test zkey not found at {zkeyPath}");
        Assert.True(File.Exists(witnessPath), $"Test witness not found at {witnessPath}");

        var zkeyBytes = File.ReadAllBytes(zkeyPath);
        var witnessBytes = File.ReadAllBytes(witnessPath);

        using var prover = new RapidSnarkProver();
        var (proofJson, publicSignalsJson) = prover.Prove(zkeyBytes, witnessBytes);

        Assert.False(string.IsNullOrEmpty(proofJson), "Proof JSON should not be empty");
        Assert.False(string.IsNullOrEmpty(publicSignalsJson), "Public signals JSON should not be empty");
        Assert.Contains("pi_a", proofJson);
        Assert.Contains("pi_b", proofJson);
        Assert.Contains("pi_c", proofJson);

        _output.WriteLine($"Proof generated successfully ({proofJson.Length} chars)");
        _output.WriteLine($"Public signals: {publicSignalsJson}");
    }

    [Fact]
    public void VerifyProof_FromTestData_Succeeds()
    {
        var zkeyPath = Path.Combine(AppContext.BaseDirectory, "TestData", "circuit_final.zkey");
        var witnessPath = Path.Combine(AppContext.BaseDirectory, "TestData", "witness.wtns");
        var vkPath = Path.Combine(AppContext.BaseDirectory, "TestData", "verification_key.json");

        Assert.True(File.Exists(vkPath), $"Verification key not found at {vkPath}");

        var zkeyBytes = File.ReadAllBytes(zkeyPath);
        var witnessBytes = File.ReadAllBytes(witnessPath);
        var vkJson = File.ReadAllText(vkPath);

        using var prover = new RapidSnarkProver();
        var (proofJson, publicSignalsJson) = prover.Prove(zkeyBytes, witnessBytes);

        var result = RapidSnarkVerifier.Verify(proofJson, publicSignalsJson, vkJson);
        Assert.True(result, "Proof should verify against the verification key");

        _output.WriteLine("Proof verified successfully");
    }

    [Fact]
    public void ReusableProver_MultipleProofs_AllValid()
    {
        var zkeyPath = Path.Combine(AppContext.BaseDirectory, "TestData", "circuit_final.zkey");
        var witnessPath = Path.Combine(AppContext.BaseDirectory, "TestData", "witness.wtns");

        var zkeyBytes = File.ReadAllBytes(zkeyPath);
        var witnessBytes = File.ReadAllBytes(witnessPath);

        using var prover = new RapidSnarkProver();
        prover.LoadZkey(zkeyBytes);

        for (int i = 0; i < 3; i++)
        {
            var (proofJson, publicSignalsJson) = prover.ProveWithLoadedZkey(witnessBytes);
            Assert.False(string.IsNullOrEmpty(proofJson));
            Assert.False(string.IsNullOrEmpty(publicSignalsJson));
            _output.WriteLine($"Reusable proof {i} generated successfully");
        }
    }
}
