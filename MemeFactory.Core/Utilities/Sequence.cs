using System.Collections;
using MemeFactory.Core.Processing;

namespace MemeFactory.Core.Utilities;

public class Sequence(IEnumerable<Frame> frames) : IEnumerable<Frame>, IAsyncEnumerable<Frame>, IDisposable
{
    private readonly List<Frame> _frames = frames.ToList();
    public IEnumerator<Frame> GetEnumerator() => _frames.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _frames.GetEnumerator();
    
    public IAsyncEnumerator<Frame> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return this.ToAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var frame in this) using (frame) {}
    }
}