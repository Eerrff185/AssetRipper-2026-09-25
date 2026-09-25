using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;

namespace AssetRipperCLI;

internal static class Program
{
	private static void Main(string[] args)
	{
		if (args.Length != 2)
		{
			PrintUsage();
			Environment.Exit(1);
		}

		string inputPath = Path.GetFullPath(args[0]);
		string outputDir = Path.GetFullPath(args[1]);

		if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
		{
			Console.Error.WriteLine($"Error: Input path does not exist: {inputPath}");
			Environment.Exit(1);
		}

		// Setup logging similar to the main app
		Logger.Add(new ConsoleLogger());
		Logger.LogSystemInformation("AssetRipperCLI");

		Logger.Info(LogCategory.General, $"Input: {inputPath}");
		Logger.Info(LogCategory.General, $"Output directory: {outputDir}");

		try
		{
			FullConfiguration settings = new();
			settings.LogConfigurationValues();

			ExportHandler exportHandler = new(settings);

			string exportPath = outputDir;

			// Ensure clean export directory (delete if exists and non-empty)
			if (Directory.Exists(exportPath))
			{
				if (Directory.EnumerateFileSystemEntries(exportPath).Any())
				{
					Logger.Info(LogCategory.Export, "Output directory is not empty. Deleting existing contents...");
					Directory.Delete(exportPath, recursive: true);
				}
			}
			Directory.CreateDirectory(exportPath);

			Logger.Info(LogCategory.Export, $"Export target: {exportPath}");

			Logger.Info(LogCategory.General, "Loading and processing assets...");
			exportHandler.LoadProcessAndExport(new[] { inputPath }, exportPath, LocalFileSystem.Instance);

			Logger.Info(LogCategory.General, "Export completed successfully.");
			Logger.Info(LogCategory.General, $"Exported project is at: {exportPath}");
		}
		catch (Exception ex)
		{
			Logger.Error(LogCategory.General, "Export failed:");
			Logger.Error(LogCategory.General, ex.ToString());
			Environment.Exit(2);
		}
	}

	private static void PrintUsage()
	{
		Console.WriteLine("AssetRipperCLI - Command line interface for AssetRipper");
		Console.WriteLine();
		Console.WriteLine("Usage:");
		Console.WriteLine("  AssetRipperCLI <input-file-or-folder> <output-directory>");
		Console.WriteLine();
		Console.WriteLine("Description:");
		Console.WriteLine("  Loads the specified Unity asset file/folder and exports it as a Unity project");
		Console.WriteLine("  directly into <output-directory>.");
		Console.WriteLine("  If the directory exists and is not empty, its contents will be removed first.");
		Console.WriteLine();
		Console.WriteLine("Example:");
		Console.WriteLine("  AssetRipperCLI game.apk /tmp/exports");
		Console.WriteLine("    -> exports Unity project directly to /tmp/exports/");
	}
}
