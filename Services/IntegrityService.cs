using System.Text.Json;
using MarbleServer.DTOs.Responses;
using MarbleServer.Exceptions;

namespace MarbleServer.Services
{
    public class IntegrityService
    {
        private readonly IWebHostEnvironment _environment;

        public IntegrityService(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<IntegrityResponse> GetHashesAsync(
            string gameVersion,
            List<string> requestedFiles)
        {
            string manifestPath = Path.Combine(
                _environment.ContentRootPath,
                "Integrity",
                gameVersion,
                "LevelIntegrityManifest.json"
            );

            if (!File.Exists(manifestPath))
            {
                throw new NotFoundException(
                    "Integrity manifest for this game version was not found.");
            }

            string json =
                await File.ReadAllTextAsync(manifestPath);

            IntegrityResponse? manifest =
                JsonSerializer.Deserialize<IntegrityResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (manifest == null)
            {
                throw new Exception(
                    "Integrity manifest is invalid.");
            }

            Dictionary<string, string> hashes =
                manifest.Files.ToDictionary(
                    x => NormalizePath(x.Path),
                    x => x.Hash,
                    StringComparer.OrdinalIgnoreCase
                );

            IntegrityResponse response =
                new IntegrityResponse();

            foreach (string requestedFile in requestedFiles)
            {
                string normalizedPath =
                    NormalizePath(requestedFile);

                if (!hashes.TryGetValue(
                        normalizedPath,
                        out string? hash))
                {
                    throw new NotFoundException(
                        $"Protected file was not found in the integrity manifest: {requestedFile}");
                }

                response.Files.Add(
                    new IntegrityFileResponse
                    {
                        Path = normalizedPath,
                        Hash = hash
                    }
                );
            }

            return response;
        }

        private static string NormalizePath(string path)
        {
            return path
                .Replace('\\', '/')
                .TrimStart('/');
        }
    }
}