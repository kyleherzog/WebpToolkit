# WebP Toolkit
A Visual Studio Extension that enhances WebP support.

[![Build Status](https://dev.azure.com/kyleherzog/WebpToolkit/_apis/build/status/WebpToolkit?branchName=master)](https://dev.azure.com/kyleherzog/WebpToolkit/_build/latest?definitionId=12&branchName=master)


Download this extension from the [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=kherzog.WebpToolkit)
 
![Visual Studio Marketplace Release Date](https://img.shields.io/visual-studio-marketplace/release-date/kherzog.WebpToolkit)
![Visual Studio Marketplace Last Updated](https://img.shields.io/visual-studio-marketplace/last-updated/kherzog.WebpToolkit)
![Visual Studio Marketplace Version](https://img.shields.io/visual-studio-marketplace/v/kherzog.WebpToolkit)

Also available is the latest [CI Build](https://www.vsixgallery.com/extension/WebpToolkit.3c6bbdde-9aa2-4b8a-b6e8-732cf3bfac87)


---
## Features
- Generate WebP images from existing image files
- Convert HTML `img` tags to WebP formatted `picture` tags.

## Generate WebP Image Files
Select the image file in the Solution Explorer and right-click.  Then, select Generate WebP Files and either the Lossless or Lossy option.

![Generate Context Menu](art/generate-context.jpg)

Batch WebP file generation is available by selecting the same context menu options when a folder is selected.

### Configuration
Configuration of the WebP file generation can be set in the Visual Studio Tools > Options > WebP Toolkit dialog. Included are the ability to adjust the compression level for lossless conversions and whether or not to overwrite existing WebP files.

## HTML Img to Picture Tag Light Bulb
HTML `img` tags can be easily converted to `picture` tags that are configured to display the WebP version of the image.

![Img to Picture Tag](art/img-to-picture.jpg)

## Thanks
A shout out to the following for helping make this project possible.
- Mads Kristensen - A ton of inspiration for this project came from his [Image Optimizer](https://github.com/madskristensen/ImageOptimizer) project.
- Google - Their WebP utility binaries ([license](https://chromium.googlesource.com/webm/libwebp/+/refs/heads/master/COPYING)) are used to do the image conversions.

## License
[MIT](LICENSE)