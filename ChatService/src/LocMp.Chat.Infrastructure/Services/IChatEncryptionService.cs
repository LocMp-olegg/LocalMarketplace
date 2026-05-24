namespace LocMp.Chat.Infrastructure.Services;

public interface IChatEncryptionService
{
    string GenerateChatKey();
    string Encrypt(string plaintext, string encryptedChatKey);
    string Decrypt(string ciphertext, string encryptedChatKey);
}