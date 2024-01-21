/* File Name: Structs.cs
 Namespace: GalleryCreator
 Class: Structs
 Author: Davin Chiupka
 Created on: January 5th, 2024
 Last Modified: January 17th, 2024
 Version: 0.1
 Description:
     The Structs class serves as a container for various custom data structures used in the GalleryCreator application. 
     This class includes definitions for CameraData, ImageData, Resolutions, and Resolution, each representing 
     different aspects of image metadata. The class also manages collections for storing image data and filenames with no metadata.

 Usage:
     The data structures in this class are utilized throughout the GalleryCreator application for managing and 
     storing metadata associated with images. This includes camera settings, image resolutions, and tags. 
     The ImageData structure, in particular, acts as a central data model for encapsulating all metadata for a single image.

 Dependencies:
     The class relies on standard .NET libraries for data management and manipulation. It is tightly integrated with 
     the rest of the GalleryCreator application, particularly in areas where image metadata processing and storage occur.
*/

namespace GalleryCreator
{
    public class Structs
    {
        // Static collections for image data and files with no metadata
        private static readonly LinkedList<ImageData> _imageDatas = new();
        private static readonly LinkedList<string> _noData = new();

        public static string fileType = ".jpg";

        public static LinkedList<ImageData> ImageDatas
        {
            get { return _imageDatas; }
        }
        ///
        public static LinkedList<string> NoData
        {
            get { return _noData; }
        }

        /// <summary>
        /// Represents camera metadata for an image.
        /// </summary>
        public class CameraData
        {
            public string? CameraModel;
            public string? LensModel;
            public string? ShutterSpeed;
            public float? Aperture;
            public string? IsoValue;
        }

        /// <summary>
        /// Represents all metadata associated with an image, including camera data, resolutions, and tags.
        /// Provides methods to display this information and check for the existence of data.
        /// </summary>
        public class ImageData
        {
            public string? FileName { get; set; }
            public string? AltName { get; set; }
            public string? Type { get; set; }
            public string? Alt { get; set; }

            public Resolutions Resolutions = new();

            public LinkedList<String> Tags = new();

            public CameraData CameraSettings = new();

            /// <summary>
            /// Displays the image metadata in a formatted manner.
            /// </summary>
            public void Display()
            {
                string border = new('*', 80);

                if (!string.IsNullOrEmpty(AltName))
                {
                    Console.WriteLine($"{"Origianl file name:",-25} {FileName}");
                    Console.WriteLine($"{"New file name:",-25} {AltName}");
                }
                else
                {
                    Console.WriteLine($"{"File name:",-25} {FileName}");
                }
                Console.WriteLine($"{"Type:",-25} {Type}");
                Console.WriteLine($"{"Alt Text:",-25} {Alt}");
                Console.WriteLine($"{"Camera model:",-25} {CameraSettings?.CameraModel}");
                Console.WriteLine($"{"Lens model:",-25} {CameraSettings?.LensModel}");
                Console.WriteLine($"{"Shutter Speed:",-25} {CameraSettings?.ShutterSpeed}");
                Console.WriteLine($"{"Aperture:",-25} {CameraSettings?.Aperture}");
                Console.WriteLine($"{"ISO:",-25} {CameraSettings?.IsoValue}\n");

                Console.WriteLine($"{"Exported sizes:",-25}");
                Console.WriteLine($"{"PreviewS",-25}");
                Resolutions?.PreviewS?.Display();
                Console.WriteLine($"{"PreviewM",-25}");
                Resolutions?.PreviewM?.Display();
                Console.WriteLine($"{"PreviewL",-25}");
                Resolutions?.PreviewL?.Display();
                Console.Write($"{"Tags:",-25}");
                foreach (var _tag in Tags)
                {
                    Console.Write($"{_tag}, ");
                }
                Console.WriteLine();
                Console.WriteLine(border);
            }

            /// <summary>
            /// Checks if the image data contains essential metadata.
            /// </summary>
            public bool HasData()
            {
                return !string.IsNullOrEmpty(CameraSettings?.CameraModel);
            }
        }

        /// <summary>
        /// Contains resolution data for an image in various sizes.
        /// </summary>
        public class Resolutions
        {
            public Resolution PreviewS { get; set; } = new();
            public Resolution PreviewM { get; set; } = new();
            public Resolution PreviewL { get; set; } = new();
        }

        /// <summary>
        /// Represents the resolution information for an image, including the file path, width, and height.
        /// </summary>
        public class Resolution
        {
            public string? Path { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            /// <summary>
            /// Displays the resolution information in a formatted manner.
            /// </summary>
            public void Display()
            {
                Console.WriteLine($"{"Path:",-25} {Path}");
                Console.WriteLine($"{"Width:",-25} {Width}");
                Console.WriteLine($"{"Height:",-25} {Height}\n");
            }
        }
    }
}
