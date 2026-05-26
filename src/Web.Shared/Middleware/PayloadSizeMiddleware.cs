using Microsoft.AspNetCore.Http;
using Web.Shared.Diagnostics;

namespace Web.Shared.Middleware;

public sealed class PayloadSizeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        Stream originalBody = context.Response.Body;
        using CountingStream countingStream = new(originalBody);
        context.Response.Body = countingStream;

        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — nothing to record
            return;
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        if (!context.RequestAborted.IsCancellationRequested)
        {
            CommerceHubDiagnostics.ResponsePayloadSize.Record(
                countingStream.BytesWritten,
                new KeyValuePair<string, object?>("http.route", context.GetEndpoint()?.DisplayName ?? "unknown"),
                new KeyValuePair<string, object?>("http.request.method", context.Request.Method));
        }
    }

    private sealed class CountingStream(Stream inner) : Stream
    {
        public long BytesWritten { get; private set; }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BytesWritten += count;
            inner.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            BytesWritten += count;
            await inner.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            BytesWritten += buffer.Length;
            await inner.WriteAsync(buffer, cancellationToken);
        }

        public override void Flush() => inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
