/* File Name: Program.cs
 Namespace: GalleryCreator
 Class: Program
 Author: Davin Chiupka
 Created on: January 5th, 2024
 Last Modified: January 17th, 2024
 Version: 0.1
 Description:
     This file contains the Program class, which serves as the entry point for the GalleryCreator application. 
     The Program class hosts the Main method, orchestrating the overall flow of the application. It provides a 
     console-based interface for user interaction, allowing users to execute various commands related to photo 
     metadata management. These commands include processing image metadata from a folder, displaying photos, tagging 
     images, saving and loading data, and exiting the application.

 Usage:
     The Main method in this class is the starting point of the application. When the program is executed, 
     it presents a menu to the user and awaits input. Based on the user's command, it calls the appropriate 
     methods from the MenuMethods and MetaDataMethods classes, handling tasks like metadata processing, photo 
     display, and data management.

 Dependencies:
     The Program class depends on static methods from the MenuMethods and MetaDataMethods classes within the 
     GalleryCreator namespace. It utilizes these methods to perform specific operations based on user commands. 
     The class is designed to interact with the console for user input and output, making it an integral part 
     of the user interface layer of the application.

 Main Method:
     The static async Main method is the central execution point of the GalleryCreator application. It continuously 
     displays a menu, processes user commands, and invokes the corresponding functionalities. This method handles 
     the application's main loop and user interactions, ensuring a responsive and user-friendly console interface.
*/

using static GalleryCreator.MenuMethods;
using static GalleryCreator.MetaDataMethods;

namespace GalleryCreator
{
    public class Program
    {
        static string? folderPath;

        /// <summary>
        /// The entry point of the GalleryCreator application.
        /// This method runs a continuous loop, displaying a menu and processing user commands.
        /// It allows the user to choose from various options such as processing image metadata, 
        /// displaying photos, tagging images, saving and loading data, and exiting the application.
        /// </summary>
        static async Task Main()
        {
            while (true)
            {

                DisplayMenu();

                Console.Write(">");
                string command = Console.ReadLine() ?? "";


                switch (command.ToLower())
                {
                    case "process":
                        Console.Clear();
                        Console.Write("Please enter the path of the folder: ");
                        folderPath = Console.ReadLine() ?? "";
                        await ProcessFolderMetaData(folderPath);
                        break;
                    case "display":
                        Console.Clear();
                        DisplayPhotos();
                        break;
                    case "tag":
                        Console.Clear();
                        AddTagsToImages();
                        break;
                    case "alt":
                        Console.Clear();
                        AddAltTextToImages();
                        break;
                    case "save":
                        Console.Clear();
                        SaveData();
                        break;
                    case "load":
                        GetJsonFile();
                        break;
                    case "exit":
                        Console.Clear();
                        Console.WriteLine("Exiting application....");
                        return;
                    case "":
                        {
                            return;
                        }
                    default:
                        Console.Clear();
                        Console.WriteLine("Please enter a valid command, press enter to exit the program\n");
                        break;
                }
            }
        }

    }

}
