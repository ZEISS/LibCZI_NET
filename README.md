# LibCZI_Net

## What
LibCZI_Net is an Open Source C# library to read and write [CZI](https://www.zeiss.com/microscopy/en/products/software/zeiss-zen/czi-image-file-format.html) by using libCZI_API that enables libCZI functionalties.

## Why
LibCZI_Net is a library intended for providing read and write access to [CZI](https://www.zeiss.com/microscopy/en/products/software/zeiss-zen/czi-image-file-format.html) featuring:

* reading subblocks and get the content as a bitmap
* reading subblocks which are compressed with JPEG-XR or zstd
* works with tiled images and pyramid images
* composing multi-channel images with tinting and applying a gradation curve
* access metadata
* writing subblocks and metadata
