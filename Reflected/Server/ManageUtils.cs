
using System.Drawing.Imaging;
using System.IO.Hashing;

namespace Server;

public class ManageUtils
{
    public string PercorsoLogFile = "";
    
    //funzione calcolo CRC di un'immagine
    public static uint CalculateCRC(Bitmap bmp)
    {
        // Blocca i dati dell'immagine in memoria per accesso diretto ai byte
        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
        try
        {
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);

            // Calcolo CRC32 (nativo e super veloce)
            return BitConverter.ToUInt32(Crc32.Hash(rgbValues));
        }
        finally
        {
            bmp.UnlockBits(bmpData);
        }

    }

    //funzione che scrive nel file di log
    public void AppendLog(string LogText)
    {
        File.AppendAllText(PercorsoLogFile, LogText + "\n");
    }

    public void ClearLog()
    {
        File.WriteAllText(PercorsoLogFile,"");
    }


}