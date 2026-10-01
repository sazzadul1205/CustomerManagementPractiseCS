using CustomerManagementPractiseCS.Services.Interfaces;

namespace CustomerManagementPractiseCS.Services
{
    public class ImageService : IImageService
    {
        // allows the system to Work with Static Files like images 
        private readonly IWebHostEnvironment _env;

        // The only file types we allow
        private readonly string[] AllowedExtensions =[".jpg", ".jpeg", ".png", ".gif", ".webp"];

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string? SaveImage(IFormFile? file, string subFolder)
        {
            // If no file uploaded return null
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // Check And Verify the Allowed Extension with the system 
            if (!IsAllowedImage(file.FileName))
            {
                return null;
            }

            // Build the target folder: wwwroot/uploads/{subFolder}
            string folderPath = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(folderPath);

            // Unique file name so two uploads never collide
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string uniqueName = Guid.NewGuid().ToString() + extension;
            string filePath = Path.Combine(folderPath, uniqueName);

            // Write the file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Return the relative URL used in <img src="">
            return $"/uploads/{subFolder}/{uniqueName}";
        }

        public void DeleteImage(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            // Strip the leading '/' and normalize separators
            string cleanedPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(_env.WebRootPath, cleanedPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public bool IsAllowedImage(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }
    }
}