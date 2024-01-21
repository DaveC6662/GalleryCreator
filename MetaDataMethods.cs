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
            if (!ContainsMetadata(image))
            {
                HandleNoMetadata(fileName);
                return;
            }

            _ = ExtractMetadata(image, fileName);
        }


        /// <summary>
        /// Checks if the given image contains metadata.
        /// </summary>
        /// <param name="image">The image to check for metadata.</param>
        /// <returns>True if metadata exists; otherwise, false.</returns>
        private static bool ContainsMetadata(Image image)
        {
            return image.Metadata.ExifProfile?.Values != null && image.Metadata.ExifProfile.Values.Any();
        }


        /// <summary>
        /// Handles the scenario where an image does not contain metadata.
        /// Logs a message and adds the file name to the 'NoData' collection.
        /// </summary>
        /// <param name="fileName">The name of the file that lacks metadata.</param>
        private static void HandleNoMetadata(string fileName)
        {
            Console.WriteLine($"{fileName} does not contain metadata{Environment.NewLine}");
            NoData?.AddLast(fileName);
        }


        /// <summary>
        /// Extracts metadata from the given image and stores it in an ImageData object.
        /// </summary>
        /// <param name="image">The image to extract metadata from.</param>
        /// <param name="fileName">The name of the file associated with the image.</param>
        /// <returns>An ImageData object containing the extracted metadata.</returns>
        private static ImageData ExtractMetadata(Image image, string fileName)
        {
            ImageData photo = ImageDatas.FirstOrDefault(img => img.FileName == fileName) ?? new ImageData { FileName = fileName };

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
                var tag = prop.Tag.ToString();
                if (tagActions.TryGetValue(tag, out var action))
                {
                    action(prop);
                    if (requiredTags.Count == 0)
                    {
                        break;
                    }
                }
            }

            return photo;
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
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                Console.WriteLine("Invalid or empty folder path.");
                return;
            }

            int quality = GetQualityInput();
            string baseDir = GetBaseDirectoryPath();
            bool addAssets = GetAddAssetsInput();
            fileType = GetFileType();
            bool renameResponse = GetRenameInput();

            string[] files = Directory.GetFiles(folderPath);
            var imageFiles = files.Where(file => IsImageFile(file)).ToList();

            Console.WriteLine($"\nSkipping {files.Length - imageFiles.Count} non-image files");

            Console.WriteLine($"\nReading {imageFiles.Count} photos in {folderPath}:");

            foreach (var file in imageFiles)
            {
                string fileName = Path.GetFileName(file);

                if (ImageDatas.Any(data => data.FileName == fileName) || NoData.Any(data => data == fileName))
                {
                    Console.WriteLine($"Skipping file {fileName}, already read.");
                }
                else
                {
                    string? altName = renameResponse ? GetNewFileName(Path.GetFileName(file)) : null;
                    ImageDatas.AddLast(new ImageData
                    {
                        FileName = Path.GetFileName(file),
                        AltName = altName,
                        Type = fileType,
                    });
                }
            }

            foreach (var file in imageFiles)
            {
                await ProcessFile(file, addAssets, baseDir, quality, fileType);
            }

            DisplayNoDataFiles();
            Console.WriteLine("\nFile processing completed. Press enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }

        /// <summary>
        /// Determines whether a file is an image based on its extension.
        /// </summary>
        /// <param name="filePath">The path of the file to check.</param>
        /// <returns>True if the file is an image; otherwise, false.</returns>
        private static bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
            string fileExtension = Path.GetExtension(filePath).ToLower();
            return imageExtensions.Contains(fileExtension);
        }

        /// <summary>
        /// Prompts the user to enter the base directory path for saving images.
        /// </summary>
        /// <returns>The base directory path entered by the user or the default path "./".</returns>
        private static string GetBaseDirectoryPath()
        {
            Console.Clear();
            Console.Write("Please enter the base directory path for saving images (default is \"./\"): ");
            return Console.ReadLine() ?? "";
        }

        /// <summary>
        /// Asks the user if they want to add "assets" to the path name.
        /// </summary>
        /// <returns>True if the user wants to add "assets"; otherwise, false.</returns>
        private static bool GetAddAssetsInput()
        {
            Console.Clear();
            Console.Write("Would you like to add \"assets\" to the path name? (yes/no): ");
            string response = Console.ReadLine()?.ToLower() ?? "";
            return response == "yes";
        }

        /// <summary>
        /// Asks the user if they would like to rename the images.
        /// </summary>
        /// <returns>True if the user chooses to rename images; otherwise, false.</returns>
        private static bool GetRenameInput()
        {
            Console.Clear();
            Console.Write("Would you like to rename these images? (yes/no): ");
            string response = Console.ReadLine()?.ToLower() ?? "";
            return response == "yes";
        }

        /// <summary>
        /// Prompts the user to enter a new file name for an image.
        /// </summary>
        /// <param name="originalFileName">The original file name of the image.</param>
        /// <returns>The new file name entered by the user, or the original file name if no new name is provided.</returns>
        private static string GetNewFileName(string originalFileName)
        {
            Console.Write($"Enter new file name for '{originalFileName}' (press Enter to keep original): ");
            string newName = Console.ReadLine() ?? "";
            return string.IsNullOrEmpty(newName) ? originalFileName : newName;
        }

        /// <summary>
        /// Prompts the user to select a file type for resized photos.
        /// </summary>
        /// <returns>The file type chosen by the user or the default type "jpg".</returns>
        private static string GetFileType()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("Select file type (default JPEG) for resized photos:");
                Console.WriteLine("1. PNG");
                Console.WriteLine("2. GIF");
                Console.WriteLine("3. BMP");
                Console.WriteLine("4. TIFF");
                Console.WriteLine("5. WEBP");
                Console.Write("Enter your choice (1-5) or press enter to leave as default: ");

                string? choice = Console.ReadLine();

                return choice switch
                {
                    "1" => "png",
                    "2" => "gif",
                    "3" => "bmp",
                    "4" => "tiff",
                    "5" => "webp",
                    _ => "jpg",
                };
            }
        }

        /// <summary>
        /// Loads an image from a file and processes its metadata and resizing.
        /// Utilizes asynchronous operations for loading and processing.
        /// </summary>
        /// <param name="file">The path to the image file.</param>
        private static async Task ProcessFile(string file, bool addAssets, string baseDir, int quality, string fileType)
        {
            string fileName = ImageDatas.FirstOrDefault(img => img.FileName == Path.GetFileName(file) && img.AltName != "")?.AltName ?? Path.GetFileName(file);
            try
            {
                var image = await Image.LoadAsync(file);
                Console.Write($"\r{new string(' ', Console.WindowWidth)}\r");
                Console.Write($"\rProcessing file: {fileName}");
                ReadMetadata(image, Path.GetFileName(file));
                await ResizeImageAsync(image, fileName, addAssets, baseDir, quality, fileType);
            }
            catch (ImageFormatException)
            {
                Console.WriteLine($"\nSkipping non-image file: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn unexpected error occurred while processing {fileName}: {ex.Message}");
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
                Console.WriteLine($"Add tags for image: {imageData.FileName}");
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
        /// Allows the user to add alternative text to each image in the ImageDatas collection.
        /// The method prompts the user to enter alt text for each image and updates the ImageData object.
        /// If there are no images available, it displays a message and returns to the menu.
        /// </summary>
        public static void AddAltTextToImages()
        {
            if (ImageDatas == null || ImageDatas.Count == 0)
            {
                Console.WriteLine("No images available to add alt text, press enter to return to menu.");
                Console.ReadLine();
                Console.Clear();
                return;
            }

            foreach (var imageData in ImageDatas)
            {
                Console.WriteLine($"Add alt text for image: {imageData.FileName}");
                imageData.Alt = GetAltText();
            }

            Console.WriteLine("Added tags to images. Press Enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }

        /// <summary>
        /// Prompts the user to enter alternative text for an image.
        /// </summary>
        /// <returns>The alternative text entered by the user or an empty string if no text is entered.</returns>
        private static string GetAltText()
        {
            Console.Write("Enter Alt Text for the image (press Enter to leave blank): ");
            return Console.ReadLine() ?? "";
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
                Console.Clear();
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
