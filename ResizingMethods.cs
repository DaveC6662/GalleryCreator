/* File Name: ResizingMethods.cs
 Namespace: GalleryCreator
 Class: ResizingMethods
 Author: Davin Chiupka
 Created on: January 5th, 2024
 Last Modified: January 17th, 2024
 Version: 0.1
 Description:
     The ResizingMethods class in the GalleryCreator application is designed to handle the resizing of images. 
     This class provides functionality to resize images to predefined sizes and manage the saving of these resized 
     images. It supports different resolution sizes like large, medium, and small, and is capable of updating image 
     data to reflect these changes. The class plays a crucial role in optimizing images for different display requirements 
     and storage efficiency.

 Usage:
     This class is used whenever there is a need to resize images as part of the photo metadata processing and management 
     workflow in the GalleryCreator application. It can be called to resize images to different predefined resolutions 
     and update the respective metadata in the application's data structures.

 Dependencies:
     ResizingMethods depends on the SixLabors.ImageSharp library for image processing capabilities. It also relies on 
     the ImageData struct from the GalleryCreator.Structs namespace for managing and updating image metadata during the 
     resizing process.
*/

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using static GalleryCreator.Structs;

namespace GalleryCreator
{
    public class ResizingMethods
    {
        private static readonly Size SizeLg = new(1920, 1080);
        private static readonly Size SizeMd = new(1024, 768);
        private static readonly Size SizeSm = new(960, 640);

        /// <summary>
        /// Asynchronously resizes an image to predefined sizes and saves the resized images.
        /// It creates resized versions for large, medium, and small sizes, specified in the SizeLg, SizeMd, and SizeSm fields.
        /// Each resized image is saved in a separate directory and its metadata is updated accordingly.
        /// </summary>
        /// <param name="image">The image to be resized.</param>
        /// <param name="fileName">The file name of the image.</param>
        public static async Task ResizeImageAsync(Image image, string fileName, bool addAssets, string baseDir, int quality, string outputFileType)
        {
            var resolutions = new Dictionary<string, Size>
        {
            { "Lg", SizeLg },
            { "Md", SizeMd },
            { "Sm", SizeSm }
        };
            string baseFileName = Path.GetFileNameWithoutExtension(fileName);

            foreach (var resolution in resolutions)
            {
                string OutputFileName = $"{baseFileName}.{outputFileType}";
                string outPath = Path.Combine(baseDir, "img", resolution.Key, OutputFileName);

                string resolutionPath = addAssets ? Path.Combine("assets", "img", resolution.Key, OutputFileName)
                                       : Path.Combine("img", resolution.Key, OutputFileName);

                EnsureDirectoryExists(outPath);
                var res = new Resolution { Path = resolutionPath, Width = resolution.Value.Width, Height = resolution.Value.Height };
                UpdateImageDatas(fileName, resolution.Key, res);

                await ResizeAndSaveAsync(image, resolution.Value, outPath, quality, outputFileType);
            }
        }

        /// <summary>
        /// Ensures that the directory for a given path exists.
        /// If the directory does not exist, it is created.
        /// </summary>
        /// <param name="path">The file path whose directory needs to be checked or created.</param>
        private static void EnsureDirectoryExists(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Updates the resolution data of an image in the ImageDatas collection.
        /// It checks if the image data already exists in the collection, and updates or adds new resolution data accordingly.
        /// </summary>
        /// <param name="fileName">The file name of the image being updated.</param>
        /// <param name="sizeKey">The size key indicating which resolution (Lg, Md, Sm) is being updated.</param>
        /// <param name="res">The resolution data to be updated or added.</param>
        private static void UpdateImageDatas(string fileName, string sizeKey, Resolution res)
        {
            var node = ImageDatas?.FirstOrDefault(data => data.FileName == fileName);

            if (node != null)
            {
                switch (sizeKey)
                {
                    case "Lg":
                        node.Resolutions.PreviewL = res;
                        break;
                    case "Md":
                        node.Resolutions.PreviewM = res;
                        break;
                    case "Sm":
                        node.Resolutions.PreviewS = res;
                        break;
                }
            }
            else
            {
                var photo = new ImageData { FileName = fileName };
                // Update photo.Resolutions based on sizeKey similar to above
                _ = ImageDatas?.AddLast(photo);
            }
        }

        /// <summary>
        /// Asynchronously resizes an image to a specified size and saves it to a given output path.
        /// The method also removes metadata like Exif, Xmp, Icc, Iptc, and Cicp profiles from the resized image.
        /// </summary>
        /// <param name="image">The image to be resized.</param>
        /// <param name="size">The target size for the resized image.</param>
        /// <param name="outputPath">The path where the resized image will be saved.</param>
        private static async Task ResizeAndSaveAsync(Image image, Size size, string outputPath, int quality, string fileType)
        {
            var resizeOptions = new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            };

            using var resizedImage = image.Clone(ctx => ctx?.Resize(resizeOptions));
            resizedImage.Metadata.ExifProfile = null;
            resizedImage.Metadata.XmpProfile = null;
            resizedImage.Metadata.IccProfile = null;
            resizedImage.Metadata.IptcProfile = null;
            resizedImage.Metadata.CicpProfile = null;

            IImageEncoder encoder = fileType switch
            {
                ".png" => new PngEncoder(),
                ".gif" => new GifEncoder(),
                ".bmp" => new BmpEncoder(),
                ".tiff" => new TiffEncoder(),
                ".webp" => new WebpEncoder { Quality = quality },
                _ => new JpegEncoder { Quality = quality },
            };

            await resizedImage.SaveAsync(outputPath, encoder);
        }
    }
}
