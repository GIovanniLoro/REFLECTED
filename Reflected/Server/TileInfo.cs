namespace ScreenServer
{
    public class TileInfo
    {
        public int X, Y, Width, Height;
        public byte[] Data;

        public TileInfo(int x, int y, int w, int h, byte[] data)
        {
            X = x; Y = y; Width = w; Height = h; Data = data;
        }
    }
}