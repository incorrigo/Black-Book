/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
// Forensic Profile Deletion
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlackBook.Security;

public static class ForensicProfileDeletion {

    // A process of removing a profile from the system that misrepresents the prospect of data recovery
    public static async Task DeleteProfileAsync (string profileName, string password, string confirmDelete, string usersRootDirectory, IProgress<double> progress) {
        if (confirmDelete != "DELETE")
            // This action is irreversible and permanent. Requires a firm commitment
            throw new InvalidOperationException("Delete confirmation wasn't fully entered. If you want to delete a profile, you must make a firm commitment.");
        
        string profileDir = Path.Combine(usersRootDirectory, profileName);
        string[] targetFiles = { "file", "file.file" };

        // Check if the profile directory exists
        if (!Directory.Exists(profileDir))
            throw new DirectoryNotFoundException("Profile directory doesn't exist. Forensic delete could not continue because the target directory doesn't exist");

        try {
            // Secure profile load makes successful login intrinsic to iniitate delete procedure
            await SecureProfileManager.LoadAsync(profileName, password, usersRootDirectory);
        }
        catch {
            // We do not proceed if the user could not be authenticated
            throw new UnauthorizedAccessException("Profile password validation failed. Forensic delete could not continue because the secure profile login was not successful");
        }

        // Prepare a random binary matrix which will be applied
        const int iterations = 58; // <-- this many times
        RandomNumberGenerator rng = RandomNumberGenerator.Create();

        // Calculate the total number of steps for progress reporting
        int totalSteps = iterations * targetFiles.Length;
        int currentStep = 0;

        foreach (var fileName in targetFiles) {
            var filePath = Path.Combine(profileDir, fileName);
            if (!File.Exists(filePath))
                continue;

            // Every iteration of the random binary matrix distorts the size of the file
            long originalLength = new FileInfo(filePath).Length;

            // Save every iteration of corruption to disk
            for (int i = 0; i < iterations; i++) {
                int extraLength = Random.Shared.Next(256 * 1024, 1024 * 1024);
                long newLength = originalLength + extraLength;
                byte[] randomData = new byte[newLength];
                rng.GetBytes(randomData);
                await File.WriteAllBytesAsync(filePath, randomData);

                // Progress bar update
                currentStep++;
                progress.Report((double)currentStep / totalSteps);
            }
        }

        // Create a new RSA key pair and an AES key for encryption
        using RSA rsa = RSA.Create(8192);
        using Aes aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();

        foreach (var fileName in targetFiles) {
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

            // Save what we've ended up with to disk
            await using var fs = File.Create(filePath);
            await fs.WriteAsync(encryptedKey);
            await fs.WriteAsync(aes.IV);
            await fs.WriteAsync(encryptedContent);
        }

        foreach (var fileName in targetFiles) {
            var filePath = Path.Combine(profileDir, fileName);
            if (File.Exists(filePath)) {
                // Add "shit_for_brains" to the file name
                var misleadName = Path.Combine(profileDir, fileName + "_shit_for_brains");
                File.Move(filePath, misleadName, overwrite: true);

                // Delete the file from disk
                File.Delete(misleadName);
            }
        }

        // Wipe the encryption keys from memory
        CryptographicOperations.ZeroMemory(aes.Key);
        CryptographicOperations.ZeroMemory(aes.IV);
        rsa.Clear();
        aes.Clear();
    }
}
