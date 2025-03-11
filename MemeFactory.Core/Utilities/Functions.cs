using MemeFactory.Core.Processing;

namespace MemeFactory.Core.Utilities;

public delegate ValueTask<Frame> FrameProcessor(Frame frame,
    CancellationToken cancellationToken = default);

public delegate ValueTask<Frame> FrameMerger(Frame a, Frame b,
    CancellationToken cancellationToken = default);

public delegate IAsyncEnumerable<(Frame a, Frame b)> SequenceMerger(IAsyncEnumerable<Frame> a);
