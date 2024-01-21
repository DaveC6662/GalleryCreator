/* File Name: MenuMethods.cs
 Namespace: GalleryCreator
 Class: MenuMethods
 Author: Davin Chiupka
 Created on: January 5th 2024
 Last Modified: January 17th 2024
 Version: 0.1
 Description:
     This file contains the MenuMethods class for the GalleryCreator application.
     The MenuMethods class provides functionalities such as displaying menus, 
     processing user input, displaying photo metadata, saving and loading data 
     in JSON format, and other utility methods associated with the photo metadata 
     extraction and management process. It is a part of the user interface layer 
     of the application and interacts directly with the user.

 Usage:
     This class is used to interact with the user through the console, 
     providing options to manage and process image metadata within the GalleryCreator application.

 Dependencies:
     Requires Newtonsoft.Json for JSON serialization and deserialization.
     Depends on the ImageData and Resolution structs from the GalleryCreator.Structs namespace.
*/

using Newtonsoft.Json;
using static GalleryCreator.Structs;

namespace GalleryCreator
{
    public class MenuMethods
    {
        /// <summary>
        /// Displays the main menu of the Photo Metadata Extractor Application.
        /// The menu includes various options like processing, displaying, tagging, saving, loading images, and exiting the application.
        /// It presents these options in a formatted console output with a bordered layout.
        /// </summary>
        public static void DisplayMenu()
        {
            string border = new('*', 80);
            string emptyBorderLine = "*" + new string(' ', 78) + "*";

            Console.WriteLine(border);
            Console.WriteLine(emptyBorderLine);
            Console.WriteLine($"*\t {"Welcome to the Gallery Builder Application!",-70}*");
            Console.WriteLine(emptyBorderLine);
            Console.WriteLine($"*\t {"Please select a command by typing the corresponding option:",-70}*");
            Console.WriteLine(emptyBorderLine);
            Console.WriteLine($"*\t {"process - Extracts metadata and resizes images in a directory.",-70}*");
            Console.WriteLine($"*\t {"display - Display photos and their respective values.",-70}*");
            Console.WriteLine($"*\t {"tag     - Tag the loaded photos.",-70}*");
            Console.WriteLine($"*\t {"alt     - Add alt text to images",-70}*");
            Console.WriteLine($"*\t {"save    - Save the image data to JSON.",-70}*");
            Console.WriteLine($"*\t {"load    - Load in a JSON file.",-70}*");
            Console.WriteLine($"*\t {"exit    - Exit the application.",-70}*");
            Console.WriteLine(emptyBorderLine);
            Console.WriteLine(border + "\n");
        }

        /// <summary>
        /// Displays all photos currently loaded in the application.
        /// Each photo's metadata is displayed using the ImageData's Display method.
        /// After displaying all photos, the method waits for the user to press enter before clearing the console.
        /// </summary>
        public static void DisplayPhotos()
        {
            foreach (ImageData photo in ImageDatas)
            {
                photo.Display();
            }
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
            Console.Clear();
        }

        /// <summary>
        /// Saves the current photo metadata to a JSON file.
        /// Prompts the user to enter a file path, providing a default if none is specified.
        /// Checks for existing file and asks for overwrite confirmation if the file already exists.
        /// If the specified directory doesn't exist, it creates the directory.
        /// Finally, it converts the image data to JSON and saves it to the specified file.
        /// </summary>
        public static void SaveData()
        {
            Console.Write("Enter the path and file name to save data (default: './data.json'): ");
            string inputPath = Console.ReadLine()?.Trim() ?? "";

            string filePath = string.IsNullOrEmpty(inputPath) ? "./data.json" : inputPath;

            if (File.Exists(filePath))
            {
                Console.Write($"{filePath} already exists. Overwrite? (y/n): ");
                string overwrite = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (overwrite != "y")
                {
                    Console.WriteLine("Operation cancelled.");
                    return;
                }
            }

            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(ImageDatas, Formatting.Indented);
            try
            {
                File.WriteAllText(filePath, json);
                Console.WriteLine($"Data saved to {filePath}, press enter to continue");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message} Press enter return to menu");
            }
            Console.ReadLine();
            Console.Clear();
        }

        /// <summary>
        /// Async method to load and process a JSON file containing image data.
        /// Prompts the user to enter the path to a JSON file.
        /// Continuously prompts until a valid file path is provided or the operation is cancelled.
        /// Reads the JSON file asynchronously and processes the data using the ProcessJson method.
        /// </summary>
        public static async void GetJsonFile()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("Enter path to JSON file: ");
                string jsonPath = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrEmpty(jsonPath))
                {
                    Console.WriteLine("No path entered. Exiting...");
                    break;
                }

                if (!File.Exists(jsonPath))
                {
                    Console.WriteLine("File does not exist. Please enter a valid path.");
                    continue;
                }

                try
                {
                    string json = await File.ReadAllTextAsync(jsonPath);
                    ProcessJson(json);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file: {ex.Message}");
                    break;
                }
            }
        }

        /// <summary>
        /// Processes the JSON string containing image data.
        /// Deserializes the JSON string into a List of ImageData objects.
        /// Each ImageData object is then added to the ImageDatas LinkedList.
        /// The method handles the reconstruction of ImageData objects, including nested objects like Resolutions and CameraData.
        /// </summary>
        /// <param name="json">The JSON string to be processed.</param>

        public static void ProcessJson(string json)
        {
            var images = JsonConvert.DeserializeObject<List<ImageData>>(json);
            foreach (var data in from img in images
                                 let data = new ImageData
                                 {
                                     FileName = img.FileName,
                                     AltName = img.AltName,
                                     Type = img.Type,
                                     Alt = img.Alt,
                                     Resolutions = new Resolutions
                                     {
                                         PreviewS = new Resolution
                                         {
                                             Path = img.Resolutions.PreviewS.Path,
                                             Width = img.Resolutions.PreviewS.Width,
                                             Height = img.Resolutions.PreviewS.Height
                                         },
                                         PreviewM = new Resolution
                                         {
                                             Path = img.Resolutions.PreviewM.Path,
                                             Width = img.Resolutions.PreviewM.Width,
                                             Height = img.Resolutions.PreviewM.Height
                                         },
                                         PreviewL = new Resolution
                                         {
                                             Path = img.Resolutions.PreviewL.Path,
                                             Width = img.Resolutions.PreviewL.Width,
                                             Height = img.Resolutions.PreviewL.Height
                                         }
                                     },
                                     Tags = new LinkedList<string>(img.Tags),
                                     CameraSettings = new CameraData()
                                     {
                                         CameraModel = img.CameraSettings.CameraModel,
                                         LensModel = img.CameraSettings.LensModel,
                                         ShutterSpeed = img.CameraSettings.ShutterSpeed,
                                         Aperture = img.CameraSettings.Aperture,
                                         IsoValue = img.CameraSettings.IsoValue,
                                     }
                                 }
                                 select data)
            {
                _ = ImageDatas?.AddLast(data);
            }
        }
    }
}
