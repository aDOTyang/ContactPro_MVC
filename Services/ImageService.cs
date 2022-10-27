using ContactPro_MVC.Services.Interfaces;

namespace ContactPro_MVC.Services
{
    // inheritance of an interface by a class
    public class ImageService : IImageService
    {
        private readonly string defaultImage = "/img/DefaultContactImage.png";
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if (fileData == null)
            {
                return defaultImage;
            }

            // try-catch statement will allow application to run despite exceptions/errors
            try
            {
                // converts the byte array to string and outputs as variable imageBase64Data
                string imageBase64Data = Convert.ToBase64String(fileData);
                // formats the data into a string that can be read by HTML img tag
                string imageSrcString = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageSrcString;

            } catch (Exception)
            {
                throw;
            }

        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                // the using statement will clean up after itself by immediately reallocating the used memory
                // MemoryStream reads, buffers, and creates a cache memory of incoming data before feeding it to the PC for use
                using MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();
                return byteFile;

            } catch (Exception)
            {
                throw;
            }
        }
    }
}
