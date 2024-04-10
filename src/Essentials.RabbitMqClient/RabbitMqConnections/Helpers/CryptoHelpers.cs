using System.Net.Security;
using System.Security.Authentication;
using Essentials.RabbitMqClient.Options;
using RabbitMQ.Client;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.RabbitMqConnections.Helpers;

/// <summary>
/// Хелперы для работы с криптографией
/// </summary>
internal static class CryptoHelpers
{
    private const string PRIVATE_KEY_BEGIN_STRING = "-----BEGIN PRIVATE KEY-----";
    private const string PRIVATE_KEY_END_STRING = "-----END PRIVATE KEY-----";
    
    /// <summary>
    /// Настраивает Ssl
    /// </summary>
    /// <param name="connectionFactory"></param>
    /// <param name="sslOptions"></param>
    public static void ConfigureSsl(
        ConnectionFactory connectionFactory,
        SslOptions sslOptions)
    {
        if (!sslOptions.Enable)
            return;
        
        connectionFactory.Ssl.Enabled = true;
        connectionFactory.Ssl.ServerName = sslOptions.SslServerName;
        connectionFactory.Ssl.Version = SslProtocols.Tls12;
        connectionFactory.Ssl.CertificateValidationCallback = (_, _, _, _) => true;

        if (string.IsNullOrWhiteSpace(sslOptions.CertPath))
            return;
        
        connectionFactory.Ssl.CertPath = sslOptions.CertPath;
        connectionFactory.Ssl.AcceptablePolicyErrors |=
            SslPolicyErrors.RemoteCertificateNameMismatch |
            SslPolicyErrors.RemoteCertificateChainErrors;
                    
        if (!string.IsNullOrWhiteSpace(sslOptions.CertPassphrase))
        {
            connectionFactory.Ssl.CertPassphrase = sslOptions.CertPassphrase;
            return;
        }

        if (string.IsNullOrWhiteSpace(sslOptions.KeyFilePath))
            return;

        if (!CryptoHelpers.TryGetPrivateKey(sslOptions.KeyFilePath, out var privateKey))
        {
            connectionFactory.Ssl.CertPassphrase = privateKey;
            return;
        }

        MainLogger.Error(
            $"Не удалось получить приватный ключ для сертификата по пути: '{sslOptions.KeyFilePath}'.");
    }
    
    /// <summary>
    /// Пытается вернуть приватный ключ из файла
    /// </summary>
    /// <param name="keyFilePath">Путь до файла с приватным ключом</param>
    /// <param name="privateKey">Приватный ключ</param>
    /// <returns></returns>
    private static bool TryGetPrivateKey(string keyFilePath, out string privateKey)
    {
        privateKey = string.Empty;

        if (string.IsNullOrWhiteSpace(keyFilePath))
        {
            MainLogger.Error("Не удалось получить ключ. Путь до файла с ключом не задан.");
            return false;
        }

        if (!File.Exists(keyFilePath))
        {
            MainLogger.Error($"Не удалось получить ключ. Файл '{keyFilePath}' не найден.");
            return false;
        }

        try
        {
            var fileString = File.ReadAllText(keyFilePath);
            var from = fileString.IndexOf(PRIVATE_KEY_BEGIN_STRING, StringComparison.Ordinal) + PRIVATE_KEY_BEGIN_STRING.Length;
            var to = fileString.IndexOf(PRIVATE_KEY_END_STRING, StringComparison.Ordinal);
            privateKey = fileString[from..to];
        }
        catch(Exception ex)
        {
            MainLogger.Error(ex,
                $"Во время получения ключа произошла ошибка. Путь до файла с ключом: '{keyFilePath}'.");
            
            return false;
        }

        return true;
    }
}