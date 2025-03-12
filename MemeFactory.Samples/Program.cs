using MemeFactory.Core.Processing;
using MemeFactory.Core.Utilities;
using SixLabors.ImageSharp;

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
    // compose the merry meme 
    .EachFrame(Composers.Draw(merry, Resizer.Auto, Layout.LeftBottom))
    // compose the punch meme sequence
    .FrameBasedZipSequence(punchSequence.LcmExpand(),
        Composers.Draw(Resizer.Auto, Layout.LeftBottom))
    // generate final image
    .AutoComposeAsync();

// output
await result.Image.SaveAsync("result." + result.Extension, result.Encoder);
