using System;
using System.IO;
using System.Linq;
using Nethereum.CircomWitnessCalc;
using Xunit;
using Xunit.Abstractions;

namespace CircomWitnessCalc.Tests;

public class WitnessCalcTests
{
    private readonly ITestOutputHelper _output;

    public WitnessCalcTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void NativeLibrary_Loads_And_CalculatesWitness()
    {
        var graphPath = Path.Combine(AppContext.BaseDirectory, "TestData", "multiplier.graph.bin");
        Assert.True(File.Exists(graphPath), $"Test graph not found at {graphPath}");

        var graphData = File.ReadAllBytes(graphPath);
        var inputsJson = "{\"a\":\"3\",\"b\":\"5\"}";

        var witnessBytes = WitnessCalculator.CalculateWitness(graphData, inputsJson);

        Assert.NotNull(witnessBytes);
        Assert.True(witnessBytes.Length > 0, "Witness output should not be empty");
        _output.WriteLine($"Witness calculated: {witnessBytes.Length} bytes");
    }

    [Fact]
    public void CalculateWitness_DifferentInputs_ProducesDifferentOutputs()
    {
        var graphPath = Path.Combine(AppContext.BaseDirectory, "TestData", "multiplier.graph.bin");
        var graphData = File.ReadAllBytes(graphPath);

        var witness1 = WitnessCalculator.CalculateWitness(graphData, "{\"a\":\"3\",\"b\":\"5\"}");
        var witness2 = WitnessCalculator.CalculateWitness(graphData, "{\"a\":\"7\",\"b\":\"11\"}");

        Assert.True(witness1.Length > 0);
        Assert.True(witness2.Length > 0);
        Assert.Equal(witness1.Length, witness2.Length);
        Assert.False(witness1.SequenceEqual(witness2), "Different inputs should produce different witnesses");

        _output.WriteLine($"Witness 1: {witness1.Length} bytes, Witness 2: {witness2.Length} bytes");
    }

    [Fact]
    public void CalculateWitness_InvalidInputs_Throws()
    {
        var graphPath = Path.Combine(AppContext.BaseDirectory, "TestData", "multiplier.graph.bin");
        var graphData = File.ReadAllBytes(graphPath);

        Assert.Throws<ArgumentException>(() =>
            WitnessCalculator.CalculateWitness(Array.Empty<byte>(), "{\"a\":\"1\"}"));

        Assert.Throws<ArgumentException>(() =>
            WitnessCalculator.CalculateWitness(graphData, ""));
    }

    [Fact]
    public void CalculateWitness_SameInputs_ProducesIdenticalOutput()
    {
        var graphPath = Path.Combine(AppContext.BaseDirectory, "TestData", "multiplier.graph.bin");
        var graphData = File.ReadAllBytes(graphPath);

        var witness1 = WitnessCalculator.CalculateWitness(graphData, "{\"a\":\"3\",\"b\":\"5\"}");
        var witness2 = WitnessCalculator.CalculateWitness(graphData, "{\"a\":\"3\",\"b\":\"5\"}");

        Assert.True(witness1.SequenceEqual(witness2), "Same inputs should produce identical witnesses");
        _output.WriteLine("Deterministic witness generation confirmed");
    }
}
