using System;
using Server;

class Program
{
    static void Main(string[] args)
    {
        ManageGraphics manageGraphics = new ManageGraphics();
        ManageUtils manageUtils = new ManageUtils();
        
        Bitmap b = manageGraphics.CaptureFrameBuffer();
        
        uint crc = ManageUtils.CalculateCRC(b);
        
        manageGraphics.CalculateTilesCRC();
        
        Console.WriteLine(crc);
        
    }
}