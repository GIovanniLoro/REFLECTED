using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Hashing;

namespace Server;

public class ManageGraphics
{
    //definizione variabili della classe
    private Bitmap bmp;
    private List<Bitmap> ListTiles = new();
    
    //funzione cattura framebuffer:
    public Bitmap CaptureFrameBuffer()
    {
        Rectangle schermo = Screen.PrimaryScreen.Bounds;

        bmp = new Bitmap(schermo.Width, schermo.Height);

        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(schermo.X, schermo.Y, 0, 0, schermo.Size);
        }

        return bmp;

    }
    
    //funzione che divide immagine in n tiles
    public void CreateTiles()
    {
            int tileWidth = bmp.Width / 4;
            int tileHeight = bmp.Height / 6;

            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Rectangle srcRect = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                    Bitmap tile = bmp.Clone(srcRect, bmp.PixelFormat);
                    ListTiles.Add(tile);
                }
            }

            
    }

    //funzione che calcola CRC di tiles
    public void CalculateTilesCRC()
    {
        if (bmp == null) throw new InvalidOperationException("Cattura prima il framebuffer con CaptureFrameBuffer().");

        CreateTiles();

        foreach (var tile in ListTiles)
        {
            using var ms = new MemoryStream();
            tile.Save(ms, ImageFormat.Png);
            var data = ms.ToArray();

            var crc = new Crc32();
            crc.Append(data);
            var hash = crc.GetCurrentHash(); // 4 byte big-endian

            uint value = ((uint)hash[0] << 24) | ((uint)hash[1] << 16) | ((uint)hash[2] << 8) | hash[3];
            Console.WriteLine($"CRC32 tile: 0x{value:X8}");
        }
    }
}
