# Getting Started
This guide demonstrates how to get started with the "LibCZI_NET" library, including how to open a CZI file, read subblocks, attachments, metadata, and extract information.

TODO Hakan Maybe add some section about IDisposable
## Reading a CZI-file
To begin reading a '.czi' file, use "InputStream" along with reader from 'Factory.CreateReader()':

### Opening a File using Streams
The CZI reader is capable of consuming data from various types of input streams. These streams abstract the source of the .czi content, enabling reading from local files, memory buffers, HTTP endpoints, or custom I/O implementations.

Before reading any operation can occur, the Reader.Open(IInputStream) method must be called with a initialized stream. With below methods can create input streams:
```cs
    static IInputStream CreateInputStreamFromExternalStream(IExternalInputStream externalInputStream);
    static IInputStream CreateInputStreamFromFile(string filename);
    static IInputStream CreateInputStream(string className, string uri, IReadOnlyDictionary<string, object> parametersPropertyBag = null);
```

#### Open File using Local File Stream
```cs
    using LibCZI:Interface:

    var inputStream = Factory.CreateInputStreamFromFile("path/to/file.czi");
    var reader = Factory.CreateReader();
    reader.Open(inputstream);
```
Note: You have to call reader.Open() before performing any read operations.

With the Open-method, the caller has to pass in an object which implements the interface “IStream”. This object is used by the CZIReader in order to access the data in a CZI-file. With below methods can create input streams:

#### Open File using In-Memory Stream
When the CZI file is already loaded in memory (e.g., from a bye array), it can be wrapped in an external input stream.
```cs
    using var inputStream = Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.Get4TilesCzi()), true));

    using var reader = Factory.CreateReader();
    reader.Open(inputStream);
```

#### Open File using HTTP Stream
For remote file access, an input stream can be created using an HTTP stream class.
```cs
    inputStream = Factory.CreateInputStream(
                    "curl_http_inputstream",
                    "https://example.com/data/sample.czi",
                    new Dictionary<string, object>() { { StreamClassPropertyKeys.CurlHttpUserAgent, "libCZI" }, });

    using var reader = Factory.CreateReader();
    reader.Open(inputStream);
```
This HTTP stream enables lazy reading of large CZI files without downloading.

### Access a SubBlock
Once the file open, you can access individual subblocks that store image tiles or planes.

```cs
    var subBlock = reader.ReadSubBlock(0);
    var subBlockInfo = subBlock.SubBlockInfo;
```
From subBlockInfo, you can extract the following information:
- PixelType: The data type used for pixels in the subblock.
- PhysicalSize: The actual size of the subblock in pixels.
- LogicalRect: The rectangular area that this subblock occupies in the logical image space: X, Y, Width, and Height.
- Coordinate: The multidimensional position of the subblock.
- Mindex: Optional index associated with the subblock.
- CompressionModeRaw: Compression method used, accessible as both raw value.

### Accessing SubBlock Statistics
Get basic or extended statistical summaries about all the subblocks.

```cs
    var stats = reader.GetSimpleStatistics();
    var extendedStats = reader.GetExtendedStatistics();
```

### Accessing Metadata

Metadata in a CZI file is represented as XML and can be parsed using standart .NET tools like XDocument and XPath. 

Retrieve the metadata segment from the reader:
```cs
    var metadataSegment = reader.GetMetadataSegment();
```
From this metadata, you can retrieve structured metadata such as image size, channel count, or scene layout.

Example Metadata:
CZI Metadata is an XML:
Example:
```xml
    {
    <ImageDocument>
      <Metadata>
        <Information>
          <Document>
            <UserName>TestUser</UserName>
          </Document>
          <Image>
            <SizeX>500</SizeX>
            <SizeY>500</SizeY>
            <SizeC>1</SizeC>
            <SizeS>1</SizeS>
            <SizeM>3</SizeM>
            <Dimensions>
              <Channels>
                <Channel Id="Channel:0">
                  <PixelType>Gray8</PixelType>
                </Channel>
              </Channels>
            </Dimensions>
          </Image>
        </Information>
        <DisplaySetting>
          <Channels>
            <Channel Id="Channel:0" Name="C1">
              <BitCountRange>8</BitCountRange>
              <PixelType>Gray8</PixelType>
              <DyeName>Dye1</DyeName>
              <ColorMode>None</ColorMode>
            </Channel>
          </Channels>
        </DisplaySetting>
      </Metadata>
    </ImageDocument>
    }
```

You can use XPath queries like //Expriment/UserName or //Image/Channel/@Name to navigate and extract values.
```cs
    var xml = metadataSegment.GetMetadataAsXDocument();
    string? userName = xml.XPathSelectElement("//Document/Username").Value;

    // Optionally, extract metadataSegment.GetCziDocumentInfo();
```

### Accessing Attachments
Attatchments in a CZI file contain additional data such as thumbnails, timestamps, event logs, and preview images.

After opening a file, you can check number of available attachments:

```cs
    int count = reader.GetAttachmentsCount();
```
To retrieve details about each one:
```cs
    var attachments = reader.EnumerateAttachments(); // returns IEnumerable

    foreach (var info in attachments)
    {
        Console.WriteLine($"GUID. {info.Guid}, Type: {info.ContentFileType}, Name: {info.Name}");
    }
```
Each attachment info includes:
- Guid: Unique identifier of the attachment.
- ContentFileType: The format.
- Name: Logical name.

You can also retrieve a specific attahment by locating its index using its GUID or name:

```cs
    Guid thumbnailGuid = new Guid("{ffffffff-ffff-ffff-ffff-ffffffff}");
    int index = reader.EnumerateAttachment().Select((info, idx) => new { info, idx }).First(x => x.info.Guid == thumbnail.Guid).idx;

    using var attachment = reader.ReadAttachment(index);
```

## Writing a CZI-file
The IWriter interface in LibCZI_Net provides functionality to create and write CZI files, including image subblocks, metadata, and attachments.

### Creating an OutputStream and Initializing the Writer
To create a new file, initialize an IOutputStream and use it to open an IWriter instance:
```cs
    using var outputStream = Factory.CreateOutputStreamForFile("output.czi", overwriteExisting: true);
    using var writer = Factory.CreateWriter();
    writer.Open(outputStream);
```
For in memory creation, an external stream can be provided using a MemoryStream.

### Writing/Adding Metadata
Structured metadata can be included by writing XML document directly into the file. This metadata is stored in the header section and can be retrieved later for analysis or display.
```cs
    string metadataXml = "<Document><UserName>TestUser</UserName></Document>";
    Span<byte> metadataBytes = Encoding.UTF8.GetBytes(metadataXml);
    writer.WriteMetadata(metadataBytes);
```
This data is typically used to capture acqusition context or custom domain information.

### Writing/Adding a SubBlock
To describe image content, a subblock must be defined. This includes information about the image's size, pixel layout, and coordinate space, along with the raw pixel data.
```cs
    byte[] pixelData = [1, 2, 3, 4, 5, 6];

    AddSubBlockInfoUncompressed addSubBlockInfoUncompressed = new AddSubBlockInfoUncompressed
    {
        AddSubBlockInfo = new AddSubBlockInfo
        {
            Coordinate = new Coordinate([new DimensionAndValue(DimensionIndex.C, 0), new DimensionAndValue(DimensionIndex.T, 0)]),
            MindexValid = false,
            Mindex = 0,
            X = 0,
            Y = 0,
            LogicalWidth = 2,
            LogicalHeight = 3,
            PhysicalWidth = 2,
            PhysicalHeight = 3,
            PixelType = PixelType.Gray8,
        },
        Stride = 2,
    };

    AddSubBlockData addSubBlockData = new AddSubBlockData { BitmapData = pixelData, };

    writer.AddSubBlockUncompressed(in addSubBlockInfoUncompressed, in addSubBlockData);
    writer.Close();
```

### Writing/Adding Attachment
CZI files support the inclusion of additional attachments.
```cs
    byte[] attachmentData = [1, 2, 3, 4, 5, 6];

    var attachmentInfo = new AddAttachmentInfo
    {
        Guid = Guid:Parse(ffffffff-ffff-ffff-ffff-ffffffff),
        ContentFileType = "txt",
        Name = "Test-Attachment"
    };
    writer.AddAttachment(in attachmentInfo, attachmentData);
    writer.Close();
```

### Finalizing the File
Once all content has been written, the writer must be closed. This step completes the file structure and ensures all buffered data is flushed to the output stream.
```cs
    writer.Close();
```
