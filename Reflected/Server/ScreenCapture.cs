// csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace ScreenServer
{
    public class ScreenCapture
    {
        private readonly int _tileWidth = 320;   // meno tile = meno pacchetti/UI update
        private readonly int _tileHeight = 180;
        private readonly Dictionary<(int,int), byte[]> _lastTileHashes = new();

        private static ImageCodecInfo GetJpegCodec()
        {
            var encoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i < encoders.Length; i++)
                if (encoders[i].MimeType == "image/jpeg") return encoders[i];
            throw new InvalidOperationException("JPEG encoder non trovato.");
        }

        public List<TileInfo> CaptureScreenTiles()
        {
            var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            using var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            var tiles = new List<TileInfo>();
            using var md5 = MD5.Create();

            // Prepara encoder JPEG qualità 50
            var jpeg = GetJpegCodec();
            using var encParams = new EncoderParameters(1);
            encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);

            int tilesX = (int)Math.Ceiling((double)bitmap.Width / _tileWidth);
            int tilesY = (int)Math.Ceiling((double)bitmap.Height / _tileHeight);

            for (int ty = 0; ty < tilesY; ty++)
            {
                for (int tx = 0; tx < tilesX; tx++)
                {
                    int x = tx * _tileWidth;
                    int y = ty * _tileHeight;
                    int w = Math.Min(_tileWidth, bitmap.Width - x);
                    int h = Math.Min(_tileHeight, bitmap.Height - y);

                    using var tile = bitmap.Clone(new Rectangle(x, y, w, h), bitmap.PixelFormat);
                    using var ms = new MemoryStream();
                    tile.Save(ms, jpeg, encParams); // qualità più bassa = meno banda/CPU
                    var bytes = ms.ToArray();
                    var hash = md5.ComputeHash(bytes);

                    var key = (tx, ty);
                    bool changed = !_lastTileHashes.TryGetValue(key, out var old) || !CompareHash(old, hash);
                    if (changed)
                    {
                        tiles.Add(new TileInfo(x, y, w, h, bytes));
                        _lastTileHashes[key] = hash;
                    }
                }
            }

            return tiles;
        }

        public void ResetCache() => _lastTileHashes.Clear();

        private bool CompareHash(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}
