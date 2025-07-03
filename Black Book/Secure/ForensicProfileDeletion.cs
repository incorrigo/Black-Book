/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
// Forensic Profile Deletion
// Securely deletes a profile in a forensic manner by corrupting its files and removing them
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlackBook.Security;

public static class ForensicProfileDeletion {

    /// <summary>
    /// Deletes a profile by corrupting its files and removing them securely
    /// </summary>
    /// <param name="profileName"></param>
    /// <param name="password"></param>
    /// <param name="confirmDelete"></param>
    /// <param name="usersRootDirectory"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static async Task DeleteProfileAsync (string profileName, string password, string confirmDelete, string usersRootDirectory, IProgress<double> progress) {
        if (confirmDelete != "DELETE")
            throw new InvalidOperationException("Delete confirmation wasn't fully entered");
        
        string profileDir = Path.Combine(usersRootDirectory, profileName);
        string[] targetFiles = { "file", "file.file" };

        // Check if the profile directory exists
        if (!Directory.Exists(profileDir))
            throw new DirectoryNotFoundException("Profile directory doesn't exist");

        try {
            // Validate the profile name and password
            await SecureProfileManager.LoadAsync(profileName, password, usersRootDirectory);
        }
        catch {
            // A successful load indicates the password is correct
            throw new UnauthorizedAccessException("Profile password validation failed");
        }

        // Deploy forensic data corruption
        const int iterations = 58; // iterations of corruption
        RandomNumberGenerator rng = RandomNumberGenerator.Create();

        foreach (var fileName in targetFiles) {
            var filePath = Path.Combine(profileDir, fileName);
            if (!File.Exists(filePath))
                continue;

            // Explicit forensic corruption
            long originalLength = new FileInfo(filePath).Length;

            // Initiate the corruption process using randomness
            for (int i = 0; i < iterations; i++) {
                int extraLength = Random.Shared.Next(256 * 1024, 1024 * 1024);
                long newLength = originalLength + extraLength;
                byte[] randomData = new byte[newLength];
                rng.GetBytes(randomData);
                await File.WriteAllBytesAsync(filePath, randomData);
                progress.Report((double)(i + 1) / (iterations * targetFiles.Length));
            }
        }

        // False hope for the forensic investigator
        using RSA rsa = RSA.Create(8192);
        using Aes aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();

        foreach (var fileName in targetFiles) {
            // Falsify hope for every file in profile directory
            var filePath = Path.Combine(profileDir, fileName);
            if (!File.Exists(filePath))
                continue;

            // Encrypt the corrupted file content
            byte[] corruptedData = await File.ReadAllBytesAsync(filePath);
            byte[] encryptedContent;

            using (var encryptor = aes.CreateEncryptor()) {
                encryptedContent = encryptor.TransformFinalBlock(corruptedData, 0, corruptedData.Length);
            }

            byte[] encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA512);

            // Save encrypted corrupted file
            await using var fs = File.Create(filePath);
            await fs.WriteAsync(encryptedKey);
            await fs.WriteAsync(aes.IV);
            await fs.WriteAsync(encryptedContent);
        }

        // Shit for brains: rename and delete
        foreach (var fileName in targetFiles) {
            var filePath = Path.Combine(profileDir, fileName);
            if (File.Exists(filePath)) {
                var misleadName = Path.Combine(profileDir, fileName + "_shit_for_brains");
                File.Move(filePath, misleadName, overwrite: true);
                File.Delete(misleadName);
            }
        }

        // Get rid of thde keys from memory
        CryptographicOperations.ZeroMemory(aes.Key);
        CryptographicOperations.ZeroMemory(aes.IV);
        rsa.Clear();
        aes.Clear();
    }
}
