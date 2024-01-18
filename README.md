# GalleryCreator

## Overview
GalleryCreator: A Sophisticated .NET Application for Dynamic Photo Gallery Management

GalleryCreator stands out as a comprehensive .NET solution, specifically designed to streamline the management and processing of dynamic photo galleries. This application is particularly fine-tuned for integration with Angular-based projects, offering a rich array of features for meticulous image handling. This is an ideal tool for photographers, web developers, and digital artists seeking an efficient and automated way to build a showcase for their photographic work online. By handling the technical aspects of gallery creation, it allows creatives to focus on the artistic side of their projects.

## Features
- **Metadata Processing:**  GalleryCreator adeptly extracts EXIF data from each photograph within a directory, providing valuable insights into camera settings used to take each photo.
- **Image Resizing:** The application is designed with advanced image resizing capabilities, ensuring optimal display on a variety of devices. It adeptly exports images in multiple sizes tailored specifically for desktops, tablets, and mobile devices, guaranteeing a seamless and responsive viewing experience. Additionally, when exporting images in each resolution, the application meticulously strips out metadata, enhancing privacy and security for images hosted online.
- **Tagging System:** By incorporating tagging for each image within the JSON file, GalleryCreator allows for the implementation of advanced filtering options. This feature enriches the user experience, enabling easy navigation and exploration of the photo gallery based on specific criteria or themes.
- **Efficient Data Management:** Enables saving and loading of image metadata and each resolution source path in JSON format, ensuring data portability and ease of access.
- **Optimized for Angular:** Tailored to integrate seamlessly with Angular applications, enhancing front-end interactivity and user experience.

## Menu

![Menu](https://github.com/DaveC6662/GalleryCreator/assets/141587948/f3014b12-b867-4ad5-bfef-945d0ebbb5f8)

## Process

![Process](https://github.com/DaveC6662/GalleryCreator/assets/141587948/6d312374-a040-4ae1-9be6-7d63216c178d)

## Display

![Display](https://github.com/DaveC6662/GalleryCreator/assets/141587948/ea69fec0-c25b-4ae6-a04e-4c01d8e88126)

## Tag

![Tag](https://github.com/DaveC6662/GalleryCreator/assets/141587948/410a8a9b-1d75-4693-9a10-4b3a396e9a37)

## Save

![Save](https://github.com/DaveC6662/GalleryCreator/assets/141587948/10463bde-b74a-47f2-ac40-682b1025c917)

## Exported JSON

![Exported JSON](https://github.com/DaveC6662/GalleryCreator/assets/141587948/0feca4e9-794c-42c0-b301-ae9b2263f768)

## Using JSON to filter by image tags

![Image Filter](https://github.com/DaveC6662/GalleryCreator/assets/141587948/9ad1fd08-675c-4cb0-a7ab-be84fb5cf3ae)

## Using JSON to display the image EXIF data

![EXIF Data](https://github.com/DaveC6662/GalleryCreator/assets/141587948/e73c602a-10a7-49ab-873a-8ce5a9915707)


## Requirements
- .NET 7.0
- Newtonsoft.Json for advanced JSON serialization and deserialization.
- SixLabors.ImageSharp for state-of-the-art image processing.

## Installation
To get started with GalleryCreator:

git clone https://github.com/DaveC6662/GalleryCreator.git
After cloning, navigate to the directory and build the solution using your preferred .NET CLI or IDE.

## Usage
Important: Please back up your photos before processing them with GalleryCreator.

Launch the application via .NET CLI or IDE. A user-friendly, console-based menu will guide you through various options like processing, displaying, tagging, saving, and loading image metadata.

## Contributing
Your contributions can help make GalleryCreator even better! Fork the repository and submit a pull request with your suggested changes or enhancements.

## License
GalleryCreator is open-source software, licensed under the Apache 2.0 License. For more details, see the LICENSE file.

## Author
- Davin Chiupka

## Acknowledgments
Special thanks to:

SixLabors.ImageSharp: A comprehensive .NET image processing library. Utilized in GalleryCreator for its powerful image resizing and manipulation capabilities. 
Licensed under the Apache 2.0 License. Kudos to the SixLabors team for their invaluable contribution to the open-source community.
Additional gratitude goes to the creators and maintainers of all other libraries and tools leveraged in the development of GalleryCreator.

## Contact
For any queries or further information, please contact davinchiupka@gmail.com.
