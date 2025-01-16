using MimeDetective;
using MimeDetective.Engine;
using MimeDetective.Storage;
using MimeDetective.Storage.Xml.v2;
using System.Collections.Immutable;
using System.IO;

namespace minitool5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("minitool5 <filename1> <filename2> ...");
                Console.WriteLine("minitool5 <directory>");
                Console.ReadKey();
                return;
            }

            var AllDefintions = new MimeDetective.Definitions.ExhaustiveBuilder()
            {
                UsageType = MimeDetective.Definitions.Licensing.UsageType.PersonalNonCommercial
            }.Build();
            var Extensions = new[] { "mp3", "png", "ogg", "webm", "bmp", "jpg", "wav", "mpg", "mpeg", "mpv", "m1v", "wmv", "avi", "ttf", "otf", "txt" }.ToImmutableHashSet(StringComparer.InvariantCultureIgnoreCase);
            var ScopedDefinitions = AllDefintions
                .ScopeExtensions(Extensions)
                .TrimMeta()
                .ToImmutableArray()
                ;
            var inspector = new ContentInspectorBuilder()
            {
                Definitions = ScopedDefinitions,
                Parallel = true
            }.Build();
            var customInspector = new CustomContentInspector();

            if (Directory.Exists(args[0]))
            {
                ProcessDirectory(args[0], inspector, customInspector.Instance);
            }
            else
            {
                foreach (var file in args)
                {
                    if (!File.Exists(file))
                    {
                        Console.WriteLine($"文件 {file} 不存在");
                        continue;
                    }
                    ProcessFile(file, inspector, customInspector.Instance);
                }
            }
            Console.ReadKey();
        }

        static void ProcessDirectory(string directoryPath, IContentInspector inspector, IContentInspector customInspector)
        {
            try
            {
                string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    ProcessFile(file, inspector, customInspector);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理文件夹时出错: {ex.Message}");
            }
        }

        static void ProcessFile(string filePath, IContentInspector inspector, IContentInspector customInspector)
        {
            try
            {
                ImmutableArray<DefinitionMatch> result, result2;
                using (var fileStream = File.OpenRead(filePath))
                {
                    result = customInspector.Inspect(fileStream);
                }

                if (result.Length == 1) //custom
                {
                    Console.WriteLine($"文件: {filePath}, 类型: {result[0].Definition.File.Extensions[0]}");
                    AddFileExt(filePath, result[0].Definition.File.Extensions[0]);
                }
                else //default
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        result2 = inspector.Inspect(fileStream);
                    }
                    if (result2.Length == 1)
                    {
                        Console.WriteLine($"文件: {filePath}, 类型: {result2[0].Definition.File.Extensions[0]}");
                        AddFileExt(filePath, result2[0]);
                    }
                    else if (result2.Length > 1)
                    {
                        int k = 0; long maxPoint = 0;
                        for (int i = 0; i < result2.Length; i++)
                        {
                            if (result2[i].Points > maxPoint)
                            {
                                maxPoint = result2[i].Points;
                                k = i;
                            }
                        }
                        Console.WriteLine($"文件: {filePath}, 类型: {result2[k].Definition.File.Extensions[0]}");
                        AddFileExt(filePath, result2[k]);
                    }
                    else
                    {
                        Console.WriteLine($"文件: {filePath}, 类型: 未知");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理文件 {filePath} 时出错: {ex.Message}");
            }
        }

        static void AddFileExt(string filePath, DefinitionMatch definitionMatch)
        {
            try
            {
                string ext = definitionMatch.Definition.File.Description == "Text - UTF-16 (LE) encoded" ? "KS" : definitionMatch.Definition.File.Extensions[0];
                File.Move(filePath, filePath + "." + ext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加文件扩展名时出错: {ex.Message}");
            }
        }

        static void AddFileExt(string filePath, string ext)
        {
            try
            {
                File.Move(filePath, filePath + "." + ext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加文件扩展名时出错: {ex.Message}");
            }
        }
    }
}
