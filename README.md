MemeFactory
----
A meme processing utility based on ImageSharp.

## Install
```xml
<PackageReference Include="MemeFactory.Core" Version="1.0.0-alpha.1" />
```
## Usage

```csharp
// load resources
using var baseImage = await Image.LoadAsync("resources/base.gif");
using var baseSequence = await baseImage.ExtractFrames()
    // the Sequence class can manage the disposal of all frames
    .ToSequenceAsync();

using var merry = await Image.LoadAsync("resources/merry.png");
using var punchSequence = await Frames
    .LoadFromFolderAsync("resources/punch")
    .Slow(times: 1)
    .ToSequenceAsync();

// process
using var result = await baseSequence  // layer 0: base sequence
    // layer 1: draw the merry meme into each frame in the base sequence
    .EachFrame(Composers.Draw(merry, Resizer.Auto, Layout.LeftBottom))
    // layer 2: expand the punch sequence using the LCM result
    // between the punch sequence and the base sequence.
    .FrameBasedZipSequence(punchSequence.LcmExpand(),
        Composers.Draw(Resizer.Auto, Layout.RightCenter))
    // compose all layers
    .AutoComposeAsync();

// output
await result.Image.SaveAsync(@"result." + result.Extension, result.Encoder);
```
