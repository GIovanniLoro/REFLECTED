// C#
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== SCREEN SERVER AVVIATO ===");

            
            var ip = args.Length > 0 ? IPAddress.Parse(args[0]) : IPAddress.Loopback;
            var port = args.Length > 1 && int.TryParse(args[1], out var p) ? p : 5000;
            var target = new IPEndPoint(ip, port);
            
            /*
            var ip = IPAddress.Parse("192.168.1.101"); // IP del client
            var port = 5000; // Porta del client
            var target = new IPEndPoint(ip, port);
            */

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            var sc = new ScreenCapture();
            var lastFull = DateTime.MinValue;

            using var udp = new UdpClient();

            // Header periodico: dimensioni schermo
            var bounds = Screen.PrimaryScreen.Bounds;
            var headerTask = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    using var ms = new MemoryStream();
                    using var bw = new BinaryWriter(ms);
                    bw.Write(-1); // X
                    bw.Write(-1); // Y
                    bw.Write(bounds.Width);
                    bw.Write(bounds.Height);
                    bw.Write(0); // Length=0
                    var buf = ms.ToArray();
                    udp.Send(buf, buf.Length, target);
                    await Task.Delay(1000, cts.Token);
                }
            }, cts.Token);

            // Acquisizione tile (keyframe periodico)
            Func<IReadOnlyList<(int X, int Y, int Width, int Height, byte[] Data)>> capture = () =>
            {
                if ((DateTime.UtcNow - lastFull) > TimeSpan.FromSeconds(2))
                {
                    sc.ResetCache();
                    lastFull = DateTime.UtcNow;
                }

                var list = new List<(int, int, int, int, byte[])>();
                var tiles = sc.CaptureScreenTiles();
                foreach (var t in tiles)
                    list.Add((t.X, t.Y, t.Width, t.Height, t.Data));
                return list;
            };

            // Invio tile
            Action<(int X, int Y, int Width, int Height, byte[] Data), IPEndPoint> send = (tile, ep) =>
            {
                using var ms = new MemoryStream();
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(tile.X);
                    bw.Write(tile.Y);
                    bw.Write(tile.Width);
                    bw.Write(tile.Height);
                    var len = tile.Data?.Length ?? 0;
                    bw.Write(len);
                    if (len > 0) bw.Write(tile.Data);
                }
                var buf = ms.ToArray();
                udp.Send(buf, buf.Length, ep);
            };

            var server = new ControlServer(capture, send);

            try
            {
                await server.StartScreenStreamAsync(target, cts.Token);
            }
            finally
            {
                cts.Cancel();
                try { await headerTask; } catch { /* ignored */ }
            }
        }
    }
}
