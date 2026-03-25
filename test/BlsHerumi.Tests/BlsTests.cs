using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using mcl;
using Nethereum.Signer.Bls;
using Nethereum.Signer.Bls.Herumi;
using Xunit;
using Xunit.Abstractions;

namespace BlsHerumi.Tests;

public class BlsTests
{
    private readonly ITestOutputHelper _output;

    public BlsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task NativeLibrary_Loads_And_Initializes()
    {
        var bls = new NativeBls(new HerumiNativeBindings());
        await bls.InitializeAsync();
        _output.WriteLine("BLS native library loaded and initialized successfully");
    }

    [Fact]
    public async Task SingleSignature_RoundTrip_Verify()
    {
        var bls = new NativeBls(new HerumiNativeBindings());
        await bls.InitializeAsync();

        var secretKey = new BLS.SecretKey();
        secretKey.SetHashOf("test-key-alpha");

        var message = SHA256.HashData(Encoding.UTF8.GetBytes("hello-ethereum"));
        var signature = secretKey.Sign(message);
        var publicKey = secretKey.GetPublicKey();

        var isValid = bls.Verify(
            signature.Serialize(),
            publicKey.Serialize(),
            message);

        Assert.True(isValid, "Single BLS signature should verify");
        _output.WriteLine("Single signature verified successfully");
    }

    [Fact]
    public async Task FastAggregate_VerifiesRealSignature()
    {
        var secrets = new[]
        {
            CreateSecretKey("alpha"),
            CreateSecretKey("beta"),
            CreateSecretKey("gamma")
        };

        var message = CreateMessage("sync-committee-slot-0");
        var aggregateSignature = AggregateSignatures(secrets.Select(sk => sk.Sign(message)).ToArray());

        var publicKeys = secrets.Select(sk => sk.GetPublicKey().Serialize()).ToArray();
        var domain = CreateDomain("lc-fast-aggregate");

        var bls = new NativeBls(new HerumiNativeBindings());
        await bls.InitializeAsync();

        Assert.True(bls.VerifyAggregate(
            aggregateSignature.Serialize(),
            publicKeys,
            new[] { message },
            domain));

        var tampered = (byte[])message.Clone();
        tampered[0] ^= 0xFF;

        Assert.False(bls.VerifyAggregate(
            aggregateSignature.Serialize(),
            publicKeys,
            new[] { tampered },
            domain));

        _output.WriteLine("Fast aggregate verify passed (valid + tampered rejected)");
    }

    [Fact]
    public async Task AggregateVerify_MultiMessage_RoundTrip()
    {
        var secrets = new[]
        {
            CreateSecretKey("delta"),
            CreateSecretKey("epsilon")
        };

        var messages = new[]
        {
            CreateMessage("attestation-0"),
            CreateMessage("attestation-1")
        };

        var signatures = secrets
            .Select((sk, index) => sk.Sign(messages[index]))
            .ToArray();

        var aggregateSignature = AggregateSignatures(signatures);
        var publicKeys = secrets.Select(sk => sk.GetPublicKey().Serialize()).ToArray();
        var domain = CreateDomain("lc-aggregate");

        var bls = new NativeBls(new HerumiNativeBindings());
        await bls.InitializeAsync();

        Assert.True(bls.VerifyAggregate(
            aggregateSignature.Serialize(),
            publicKeys,
            messages,
            domain));

        _output.WriteLine("Multi-message aggregate verify passed");
    }

    private static BLS.SecretKey CreateSecretKey(string label)
    {
        var secretKey = new BLS.SecretKey();
        secretKey.SetHashOf(label);
        return secretKey;
    }

    private static byte[] CreateMessage(string label)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(label));
    }

    private static byte[] CreateDomain(string label)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes($"domain:{label}"));
    }

    private static BLS.Signature AggregateSignatures(BLS.Signature[] signatures)
    {
        var aggregate = signatures[0];
        for (var i = 1; i < signatures.Length; i++)
        {
            aggregate.Add(signatures[i]);
        }
        return aggregate;
    }
}
