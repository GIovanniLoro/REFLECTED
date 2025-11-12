using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenServer;

public sealed class ControlServer
{
    // Delegati per acquisire e inviare tile, senza interfacce o classi aggiuntive.
    private readonly Func<IReadOnlyList<(int X, int Y, int Width, int Height, byte[] Data)>> _capture;
    private readonly Action<(int X, int Y, int Width, int Height, byte[] Data), IPEndPoint> _send;

    // Costruttore: passare funzioni/metodi che catturano e inviano i tile.
    public ControlServer(
        Func<IReadOnlyList<(int X, int Y, int Width, int Height, byte[] Data)>> capture,
        Action<(int X, int Y, int Width, int Height, byte[] Data), IPEndPoint> send)
    {
        _capture = capture ?? throw new ArgumentNullException(nameof(capture));
        _send = send ?? throw new ArgumentNullException(nameof(send));
    }

    public async Task StartScreenStreamAsync(IPEndPoint target, CancellationToken cancellationToken = default)
    {
        if (target is null) throw new ArgumentNullException(nameof(target));
        Console.WriteLine("[STREAM] Avvio invio frame a blocchi...");

        while (!cancellationToken.IsCancellationRequested)
        {
            var tiles = _capture() ?? Array.Empty<(int X, int Y, int Width, int Height, byte[] Data)>();

            foreach (var tile in tiles)
                _send(tile, target);

            var count = tiles.Count;
            if (count > 0)
                Console.WriteLine("[UDP] " + count + " tile inviati a " + target);

            await Task.Delay(100, cancellationToken).ConfigureAwait(false); // ~10 FPS
        }
    }
}