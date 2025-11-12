using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ScreenServer
{
    public class UdpSender
    {
        private readonly UdpClient _client = new UdpClient();

        public void SendTile(TileInfo tile, IPEndPoint target)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // Pacchetto: X,Y,Width,Height,Length,Data
            bw.Write(tile.X);
            bw.Write(tile.Y);
            bw.Write(tile.Width);
            bw.Write(tile.Height);
            bw.Write(tile.Data.Length);
            bw.Write(tile.Data);

            var buffer = ms.ToArray();
            _client.Send(buffer, buffer.Length, target);
        }
    }
}