using System.Security.Cryptography;
using System.Text;
using LocMp.Chat.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace LocMp.Chat.Infrastructure.Services;

public sealed class AesChatEncryptionService(IOptions<EncryptionOptions> options) : IChatEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _masterKey = Convert.FromBase64String(options.Value.MasterKey);

    public string GenerateChatKey()
    {
        var perChatKey = RandomNumberGenerator.GetBytes(32);
        return EncryptBytes(perChatKey, _masterKey);
    }

    public string Encrypt(string plaintext, string encryptedChatKey)
    {
        if (string.IsNullOrEmpty(plaintext))
            return string.Empty;

        var perChatKey = DecryptBytes(encryptedChatKey, _masterKey);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        return EncryptBytes(plaintextBytes, perChatKey);
    }

    public string Decrypt(string ciphertext, string encryptedChatKey)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return string.Empty;

        var perChatKey = DecryptBytes(encryptedChatKey, _masterKey);
        var decryptedBytes = DecryptBytes(ciphertext, perChatKey);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    private static string EncryptBytes(byte[] plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var ciphertext = new byte[plaintext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var combined = new byte[NonceSize + TagSize + ciphertext.Length];
        nonce.CopyTo(combined, 0);
        tag.CopyTo(combined, NonceSize);
        ciphertext.CopyTo(combined, NonceSize + TagSize);

        return Convert.ToBase64String(combined);
    }

    private static byte[] DecryptBytes(string encoded, byte[] key)
    {
        var combined = Convert.FromBase64String(encoded);
        var nonce = combined[..NonceSize];
        var tag = combined[NonceSize..(NonceSize + TagSize)];
        var ciphertext = combined[(NonceSize + TagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }
}