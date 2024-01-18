/* File Name: MenuMethods.cs
 Namespace: GalleryCreator
 Class: MenuMethods
 Author: Davin Chiupka
 Created on: January 5th, 2024
 Last Modified: January 17th, 2024
 Version: 0.1
 Description:
     The MenuMethods class is part of the GalleryCreator application, designed to facilitate user interaction 
     through a console-based interface. It provides a variety of functionalities such as displaying menus, 
     processing user commands, and offering options to manage and process image metadata. Key features include 
     extracting metadata from images, displaying photos with their metadata, tagging photos, saving and loading 
     image data in JSON format, and more. This class plays a crucial role in the user interface layer of the 
     application, ensuring smooth user interaction and efficient handling of photo metadata.

 Usage:
     The class is utilized to create an interactive console interface where users can choose various options 
     to manage and process image metadata. It serves as the primary interaction point for users to engage with 
     the GalleryCreator application, offering a simple and intuitive command-based interface.

 Dependencies:
     This class relies on Newtonsoft.Json for JSON serialization and deserialization. It also depends on various 
     structs such as ImageData and Resolution from the GalleryCreator.Structs namespace, which are essential for 
     handling image metadata and resolution information.
*/

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using static GalleryCreator.ResizingMethods;
using static GalleryCreator.Structs;

namespace GalleryCreator
{
    public class MetaDataMethods
    {
        /// <summary>
        /// Reads the metadata from an image and stores it in an ImageData object.
        /// This method specifically extracts camera settings and other relevant EXIF data.
        /// It also manages images without metadata by adding them to a 'NoData' list.
        /// </summary>
        /// <param name="image">The Image object to extract metadata from.</param>
        /// <param name="fileName">The name of the file associated with the image.</param>

        public static void ReadMetadata(Image image, string fileName)
        {
            if (image.Metadata.ExifProfile?.Values == null || !image.Metadata.ExifProfile.Values.Any())
            {
                Console.WriteLine($"{fileName} does not contain metadata{Environment.NewLine}");
                return;
            }

            var photo = new ImageData { FileName = fileName };

            var requiredTags = new HashSet<string> { "Model", "LensModel", "ExposureTime", "FNumber", "RecommendedExposureIndex" };

            var tagActions = new Dictionary<string, Action<IExifValue>>
            {
                ["Model"] = prop => { photo.CameraSettings.CameraModel = prop.ToString(); requiredTags.Remove("Model"); },
                ["LensModel"] = prop => { photo.CameraSettings.LensModel = prop.ToString(); requiredTags.Remove("LensModel"); },
                ["ExposureTime"] = prop => { photo.CameraSettings.ShutterSpeed = ProcessShutterSpeed(prop.ToString() ?? ""); requiredTags.Remove("ExposureTime"); },
                ["FNumber"] = prop => { photo.CameraSettings.Aperture = StringDivision(prop); requiredTags.Remove("FNumber"); },
                ["RecommendedExposureIndex"] = prop => { photo.CameraSettings.IsoValue = prop.ToString(); requiredTags.Remove("RecommendedExposureIndex"); }
            };

            foreach (var prop in image.Metadata.ExifProfile.Values)
            {
                string tag = prop.Tag.ToString();
                if (tagActions.TryGetValue(tag, out var action))
                {
                    action(prop);
                    if (requiredTags.Count == 0)
                    {
                        break;
                    }
                }
            }

            if (photo.HasData())
            {
                _ = ImageDatas?.AddLast(photo);
            }
            else
            {
                _ = NoData?.AddLast(fileName);
            }
        }

        /// <summary>
        /// Parses a string representing a fraction and returns its decimal form.
        /// Provides warnings in case of null inputs, division by zero, or parse failures.
        /// </summary>
        /// <param name="propVal">The string to parse, expected to be in a fractional format like 'numerator/denominator'.</param>
        /// <returns>The decimal representation of the fraction, or 0 in case of errors.</returns>

        public static float StringDivision(IExifValue propVal)
        {
            if (propVal == null)
            {
                Console.WriteLine("Warning: propVal is null.");
                return 0;
            }

            string? temp = propVal.ToString();
            if (string.IsNullOrEmpty(temp))
            {
                Console.WriteLine("Warning: propVal does not contain a valid string.");
                return 0;
            }

            string[] subStrings = temp.Split('/');
            if (subStrings.Length == 2)
            {
                if (float.TryParse(subStrings[0], out float numerator) && float.TryParse(subStrings[1], out float denominator))
                {
                    if (denominator == 0)
                    {
                        Console.WriteLine("Warning: Division by zero.");
                        return 0;
                    }
                    return float.Round(numerator / denominator, 2);
                }
                else
                {
                    Console.WriteLine("Warning: Unable to parse one or both values to float.");
                }
            }
            else if (float.TryParse(temp, out float fraction))
            {
                return fraction;
            }

            return 0;
        }

        /// <summary>
        /// Processes the shutter speed value from a string to a more readable format.
        /// Converts fractional representations to decimal where applicable.
        /// Handles division by zero and ensures proper string formatting.
        /// </summary>
        /// <param name="shutterSpeed">The shutter speed value as a string.</param>
        /// <returns>A processed, readable version of the shutter speed.</returns>
        public static string ProcessShutterSpeed(string shutterSpeed)
        {
            if (shutterSpeed.Contains('/'))
            {
                string[] parts = shutterSpeed.Split('/');
                if (parts.Length == 2 && int.TryParse(parts[0], out int numerator) && int.TryParse(parts[1], out int denominator))
                {
                    if (denominator == 0)
                    {
                        return "Invalid"; // Or handle division by zero appropriately
                    }

                    if (numerator >= denominator)
                    {
                        // For cases like 1/2, return as fractional. For cases like 2/1, return as whole number.
                        float result = (float)numerator / denominator;
                        return (result % 1 == 0) ? result.ToString("0") : result.ToString("0.##");
                    }
                    else
                    {
                        // Keep fractional representation for values like 1/2, 1/4 etc.
                        return $"{numerator}/{denominator}";
                    }
                }
            }
            return shutterSpeed;
        }

        /// <summary>
        /// Processes metadata for all images in a specified folder.
        /// Skips files that have already been processed or lack EXIF data.
        /// Utilizes async methods for loading and processing each image file.
        /// Displays information about files without EXIF data after processing.
        /// </summary>
        /// <param name="folderPath">The path to the folder containing image files.</param>
        public static async Task ProcessFolderMetaData(string folderPath)
        {
            while (!string.IsNullOrEmpty(folderPath))
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine("The specified folder does not exist, please enter a valid path or enter to exit to menu:");
                    Console.Write(">");
                    folderPath = Console.ReadLine() ?? "";
                    continue;
                }

                int quality = GetQualityInput();

                Console.Write("Please enter the base directory path for saving images: ");
                string baseDir = Console.ReadLine() ?? "";

                Console.Write("Would you like to add assets to the path? (yes/no): ");
                string addAssetsResponse = Console.ReadLine()?.ToLower();
                bool addAssets = addAssetsResponse == "yes";

                string[] files = Directory.GetFiles(folderPath);
                int fileCount = files.Length;
                Console.WriteLine($"\nReading {fileCount} files in {folderPath}:");

                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    string fileName = Path.GetFileName(file);

                    if (ImageDatas.Any(data => data.FileName == fileName))
                    {
                        Console.WriteLine($"Skipping file {fileName}, already read.");
                        continue;
                    }

                    if (NoData.Any(data => data == fileName))
                    {
                        Console.WriteLine($"Skipping file {fileName}, no EXIF data.");
                        continue;
                    }
                    Console.Write($"\r{new string(' ', Console.WindowWidth)}\r");
                    Console.Write($"\rProcessing file {i + 1} of {fileCount}: {fileName}");
                    await ProcessFile(file, addAssets, baseDir, quality);
                }

                DisplayNoDataFiles();
                Console.WriteLine("\nFile processing completed, press enter to continue...");
                Console.ReadLine();
                Console.Clear();
                break;
            }
        }

        /// <summary>
        /// Loads an image from a file and processes its metadata and resizing.
        /// Utilizes asynchronous operations for loading and processing.
        /// </summary>
        /// <param name="file">The path to the image file.</param>
        private static async Task ProcessFile(string file, bool addAssets, string baseDir, int quality)
        {
            try
            {
                var image = await Image.LoadAsync(file);
                ReadMetadata(image, Path.GetFileName(file));
                await ResizeImageAsync(image, Path.GetFileName(file), addAssets, baseDir, quality);
            }
            catch (ImageFormatException)
            {
                Console.WriteLine($"\nSkipping non-image file: {Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn unexpected error occurred while processing {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays a list of files that did not contain any EXIF data.
        /// This method is called after processing a batch of images to inform the user.
        /// </summary>

        private static void DisplayNoDataFiles()
        {
            if (NoData?.Count > 0)
            {
                Console.WriteLine($"\nFiles with no EXIF data: {NoData.Count}");
                foreach (var file in NoData)
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
            }
        }

        /// <summary>
        /// Allows the user to add tags to the loaded images interactively.
        /// Prompts the user to enter tags for each image and adds them to the ImageData object.
        /// Skips tagging if no images are loaded.
        /// </summary>

        public static void AddTagsToImages()
        {
            if (ImageDatas == null || ImageDatas.Count == 0)
            {
                Console.WriteLine("No images available to add tags, press enter to return to menu.");
                Console.ReadLine();
                Console.Clear();
                return;
            }

            foreach (var imageData in ImageDatas)
            {
                Console.WriteLine($"Add tags for file: {imageData.FileName}");
                Console.WriteLine("Enter tags (press Enter on an empty line to finish):");

                while (true)
                {
                    Console.Write("Enter tag: ");
                    string tag = Console.ReadLine() ?? "";

                    if (string.IsNullOrEmpty(tag))
                    {
                        break; // Exit the loop if the user enters an empty line
                    }

                    imageData.Tags.AddLast(tag);
                }
            }

            Console.WriteLine("Added tags to images. Press Enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }

        /// <summary>
        /// Prompts the user to enter a photo quality value and validates the input.
        /// </summary>
        /// <returns>
        /// An integer representing the photo quality. 
        /// The method ensures that this value is within the range of 25 to 100.
        /// </returns>
        public static int GetQualityInput()
        {
            while (true)
            {
                Console.Write("Please enter desired photo quality (25-100): ");
                if (int.TryParse(Console.ReadLine(), out int quality))
                {
                    if (quality >= 25 && quality <= 100)
                    {
                        return quality;
                    }
                    else
                    {
                        Console.WriteLine("Quality must be between 25 and 100. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number between 25 and 100.");
                }
            }
        }

    }
}
