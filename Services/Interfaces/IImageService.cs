using Microsoft.AspNetCore.Http;

namespace CustomerManagementPractiseCS.Services.Interfaces
{
    public interface IImageService
    {
        // Saves the file under wwwroot/uploads/{subFolder}/ and returns the relative URL.
        // Returns null if no file was provided or the extension is not allowed.
        string? SaveImage(IFormFile? file, string subFolder);

        // Deletes the file at the given relative URL.
        void DeleteImage(string? relativePath);

        // Returns true if the file extension is in the allowed list.
        bool IsAllowedImage(string fileName);
    }
}