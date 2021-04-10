using System;
using System.IO;
using System.Threading.Tasks;

namespace ShpToWkt
{
    class Program
    {
        private static int ArgsIndex = 1;
        private static string[] Args = null;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("ShpToWkt Converter");
                Console.WriteLine("Example: ShpToWkt.exe <shpFilePath> [options]");
                Console.WriteLine("Options:");
                Console.WriteLine("\t-wkb\tConvert to WKB (Well-Known Binary)");
                Console.WriteLine("\t-o <path>\tOutput filename (Default: <shpFilePath>.wkt or <shpFilePath>.wkb)");
                Console.WriteLine("\t-convert toUtm|toLatLng <path|zoneLetter> <zoneNumber>\tProjection convert options");
                Console.WriteLine("\t\tpath\tPath of prj file (Default: <shpFilePath>.prj)");
                Console.WriteLine("\t\tzoneLetter\tZone letter of utm (Default: N)");
                Console.WriteLine("\t\tzoneNumber\tZone number of utm (Default: 40)");
                Console.WriteLine("\t-flatten [true|false]\tGenerated wkt use MULTIPOLYGON, MULTILINESTRING, MULTIPOINT instead of GEOMETRYCOLLECTION (Default: true)");
                return;
            }

            Args = args;

            MainAsync().Wait();
        }

        private static string GetDefaultOption(object @default, string arg = null)
        {
            if (@default == null)
            {
                return null;
            }

            if (@default is string str)
            {
                return str;
            }

            if (@default is Func<string, string> func)
            {
                return func(arg);
            }

            throw new NotSupportedException();
        }

        private static string GetOptionOrDefault(object @default = null)
        {
            if (ArgsIndex >= Args.Length)
            {
                return GetDefaultOption(@default);
            }

            var str = Args[ArgsIndex];
            if (str.StartsWith("-"))
            {
                return GetDefaultOption(@default, str);
            }

            ArgsIndex++;
            return str;
        }

        private static (char zoneLetter, int zoneNumber) ReadProjectionFile(string path)
            => ('N', 40);

        private static async Task MainAsync()
        {
            var path = Args[0];
            if (!File.Exists(path))
            {
                Console.WriteLine("File not found!");
                return;
            }

            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            var output = Path.Combine(directory, $"{fileName}.wkt");
            (ConvertTypes, char?, int?)? convert = null;
            var customOutput = false;
            var exportWkb = false;
            var flatten = false;

            while (Args.Length > ArgsIndex)
            {
                switch (Args[ArgsIndex++].ToLower())
                {
                    case "-wkb":
                        exportWkb = true;
                        output = customOutput ? output : Path.Combine(directory, $"{fileName}.wkb");
                        break;

                    case "-o":
                        output = GetOptionOrDefault(output);
                        customOutput = true;
                        break;

                    case "-convert":
                        var type = GetOptionOrDefault("toLatLng").ToLower();
                        if (type == "toutm")
                        {
                            convert = (ConvertTypes.ToUtm, null, null);
                            continue;
                        }
                        else if (type != "tolatlng")
                        {
                            throw new NotSupportedException($"Convert '{type}' is no supported");
                        }

                        var zoneLetter = GetOptionOrDefault((Func<string, string>)(x => x.Length > 3 ? Path.Combine(directory, $"{fileName}.prj") : "N"));
                        if (zoneLetter.Length > 2)
                        {
                            var prj = ReadProjectionFile(zoneLetter);
                            convert = (ConvertTypes.ToLatLng, prj.zoneLetter, prj.zoneNumber);
                            continue;
                        }

                        convert = (ConvertTypes.ToLatLng, zoneLetter[0], int.Parse(GetOptionOrDefault("40")));
                        break;

                    case "-flatten":
                        flatten = GetOptionOrDefault("true") == "true";
                        break;

                    default: break;
                }
            }

            Types.ShpFile shp;
            using (var shpFile = File.OpenRead(path))
            {
                shp = await shpFile.ReadShpFile();
            }

            Console.WriteLine($"Name: {fileName}");
            Console.WriteLine($"Version: {shp.Version}");

            if (shp.ShapeTypes.Length == 1)
            {
                Console.WriteLine($"Shape Type: {shp.ShapeTypes[0]}");
            }
            else
            {
                Console.WriteLine($"Shape Types: {string.Join(", ", shp.ShapeTypes)} (Non Standard)");
            }

            Console.WriteLine($"Records Count: {shp.Records.Length}");
            Console.WriteLine($"Converting to {(exportWkb ? "wkb" : "wkt")}...");
            await File.WriteAllTextAsync(output, shp.ToWkt(flatten, convert));
            Console.WriteLine($"Converting completed successfully.");
        }
    }
}
