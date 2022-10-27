namespace ContactPro_MVC.Services.Interfaces
{
    public interface IImageService
    {
        // <> denotes a type, i.e. Task of type Byte array or List of type T
        // public declaration not necessary as interfaces are public by default
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);
        public string ConvertByteArrayToFile(byte[] fileData, string extension);
    }
}
