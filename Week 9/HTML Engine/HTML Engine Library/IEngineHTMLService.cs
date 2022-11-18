namespace HTML_Engine_Library
{
    public interface IEngineHTMLService
    {
        string GetHTML(string template, object model);

        string GetHTML(Stream template, object model);

        string GetHTML(byte[] bytes, object model);

        Stream GetHTMLInStream(string template, object model);

        Stream GetHTMLInStream(Stream template, object model);

        Stream GetHTMLInStream(byte[] bytes, object model);

        byte[] GetHTMLInByte(string template, object model);

        byte[] GetHTMLInByte(Stream template, object model);

        byte[] GetHTMLInByte(byte[] bytes, object model);

        void GenerateAndSaveInDirectory(string templatePath, string outputPath, string outputFileName, object model);

        void GenerateAndSaveInDirectory(Stream templatePath, Stream outputPath, string outputFileName, object model);

        void GenerateAndSaveInDirectory(byte[] templatePath, byte[] outputPath, string outputFileName, object model);

        Task GenerateAndSaveInDirectoryAsync(string templatePath, string outputPath, string outputFileName, object model);

        Task GenerateAndSaveInDirectoryAsync(Stream templatePath, Stream outputPath, string outputFileName, object model);

        Task GenerateAndSaveInDirectoryAsync(byte[] templatePath, byte[] outputPath, string outputFileName, object model);
    }
}