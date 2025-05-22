# LibCZI_Net
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![REUSE status](https://api.reuse.software/badge/github.com/ZEISS/LibCZI_NET)](https://api.reuse.software/info/github.com/ZEISS/LibCZI_NET)
[![CodeQL](https://github.com/ZEISS/LibCZI_NET/actions/workflows/codeql.yml/badge.svg?branch=main&event=push)](https://github.com/ZEISS/LibCZI_NET/actions/workflows/codeql.yml)
[![GitHub Pages](https://github.com/ZEISS/LibCZI_NET/actions/workflows/pages.yml/badge.svg?branch=main&event=push)](https://github.com/ZEISS/LibCZI_NET/actions/workflows/pages.yml)

## What

LibCZI_Net is .NET-library, providing .NET-bindings for [libCZI](https://github.com/ZEISS/libczi).  
It aims to give a rich and easy-to-use API for reading and writing [CZI files](https://www.zeiss.com/microscopy/en/products/software/zeiss-zen/czi-image-file-format.html) files in .NET applications.

## Why

LibCZI_Net is a library intended for providing read and write access to [CZI](https://www.zeiss.com/microscopy/en/products/software/zeiss-zen/czi-image-file-format.html) featuring:

* reading subblocks and get the content as a bitmap
* reading subblocks which are compressed with JPEG-XR or zstd
* works with tiled images and pyramid images
* composing multi-channel images with tinting and applying a gradation curve
* access metadata
* writing subblocks and metadata


## Guidelines
[Code of Conduct](./CODE_OF_CONDUCT.md)  
[Contributing](./CONTRIBUTING.md)

## Disclaimer
ZEISS, ZEISS.com are registered trademarks of Carl Zeiss AG.