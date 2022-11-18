using System.Text;
using System.Text.RegularExpressions;

namespace HTML_Engine_Library
{
    public class EngineHTMLService : IEngineHTMLService
    {
        //Спсиок дисциплин(списком)
        public async Task GenerateAndSaveInDirectoryAsync(string templatePath, string outputPath, string outputFileName, object model)
        {
            if (!File.Exists(templatePath))
                throw new IOException("Invalid source");

            var template = GetHTML(await File.ReadAllTextAsync(templatePath), model);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var way = new StringBuilder(outputPath);
            if (way[^1] != '/')
                way.Append('/');
            way.Append(outputFileName);

            File.Create(way.ToString());
            await File.AppendAllTextAsync(way.ToString(), template);
        }

        public async Task GenerateAndSaveInDirectoryAsync(Stream templatePath, Stream outputPath, string outputFileName, object model)
        {
            using var temPathReader = new StreamReader(templatePath);
            var tempPath = await temPathReader.ReadToEndAsync();

            using var outPathReader = new StreamReader(outputPath);
            var outPath = await temPathReader.ReadToEndAsync();

            await GenerateAndSaveInDirectoryAsync(tempPath, outPath, outputFileName, model);
        }

        public async Task GenerateAndSaveInDirectoryAsync(byte[] templatePath, byte[] outputPath, string outputFileName, object model) =>
            await GenerateAndSaveInDirectoryAsync(Encoding.Unicode.GetString(templatePath), Encoding.Unicode.GetString(outputPath), outputFileName, model);

        public void GenerateAndSaveInDirectory(string templatePath, string outputPath, string outputFileName, object model)
        {
            if (!File.Exists(templatePath))
                throw new IOException("Invalid source");

            var template = GetHTML(File.ReadAllText(templatePath), model);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var way = new StringBuilder(outputPath);
            if (way[^1] != '/')
                way.Append('/');
            way.Append(outputFileName);

            File.Create(way.ToString());
            File.AppendAllText(way.ToString(), template);
        }

        public void GenerateAndSaveInDirectory(Stream templatePath, Stream outputPath, string outputFileName, object model)
        {
            using var temPathReader = new StreamReader(templatePath);
            var tempPath = temPathReader.ReadToEnd();

            using var outPathReader = new StreamReader(outputPath);
            var outPath = temPathReader.ReadToEnd();

            GenerateAndSaveInDirectory(tempPath, outPath, outputFileName, model);
        }



        public void GenerateAndSaveInDirectory(byte[] templatePath, byte[] outputPath, string outputFileName, object model) =>
            GenerateAndSaveInDirectory(Encoding.Unicode.GetString(templatePath), Encoding.Unicode.GetString(outputPath), outputFileName, model);

        public string GetHTML(string template, object model)
        {
            template = TemplateParser.ProcessMethods(template, new List<object>() { model });

            var j = Regex.Matches(template, @"([^\-\r\n]+(-($|[^\r\n]))*)+[\r\n]*");

            template = string.Concat(Regex.Matches(template, @"([^\-\r\n]+(-($|[^\r\n]))*)+[\r\n]*").Where(x => x.Value.Trim().Length != 0));

            var unparsed = Regex.Match(template, "{{.*}}|{{}}");
            if (unparsed.Success)
                throw new FormatException($"Unparsed value {{{{{unparsed.Value}}}}}");
            var unpairedPar = Regex.Match(template, "{{|}}");
            if (unpairedPar.Success)
                throw new FormatException($"Unpaired {unpairedPar.Value}");

            return template;
        }

        public string GetHTML(Stream template, object model)
        {
            using var strReader = new StreamReader(template);
            return GetHTML(strReader.ReadToEnd(), model);
        }

        public string GetHTML(byte[] bytes, object model) => GetHTML(Encoding.Unicode.GetString(bytes), model);

        public byte[] GetHTMLInByte(string template, object model) => Encoding.Unicode.GetBytes(GetHTML(template, model));

        public byte[] GetHTMLInByte(Stream template, object model) => Encoding.Unicode.GetBytes(GetHTML(template, model));

        public byte[] GetHTMLInByte(byte[] bytes, object model) => Encoding.Unicode.GetBytes(GetHTML(bytes, model));

        public Stream GetHTMLInStream(string template, object model) => new MemoryStream(GetHTMLInByte(template, model));

        public Stream GetHTMLInStream(Stream template, object model) => new MemoryStream(GetHTMLInByte(template, model));

        public Stream GetHTMLInStream(byte[] bytes, object model) => new MemoryStream(GetHTMLInByte(bytes, model));
    }
}
