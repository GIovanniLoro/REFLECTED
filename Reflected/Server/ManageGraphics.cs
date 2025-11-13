using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Hashing;

namespace Server;

public class ManageGraphics
{
    #region Variabili Classe
    //definizione variabili della classe
    public Bitmap CurrentCapturedScreen;
    public String CurrentCapturedScreenCRC;
    public List<Bitmap> CurrentTilesList = new();
    public List<String> CurrentTilesListCRC = new();
    
    public Bitmap PreviousCapturedScreen;
    public String PreviousCapturedScreenCRC;
    public List<Bitmap> PreviousTilesList = new();
    public List<String> PreviousTilesListCRC = new();

    public List<bool> ComparedTilesList = new();
    #endregion

    #region Gestione Framebuffer Principale
    
    //TITLE: funzione cattura framebuffer:
    //DESCR: la funzione acquisisce l'immagine corrente del display selezionato
    //       e la memorizza in un oggetto Bitmap
    public Bitmap CaptureFrameBuffer()
    {
        Rectangle schermo = Screen.PrimaryScreen.Bounds;

        CurrentCapturedScreen = new Bitmap(schermo.Width, schermo.Height);

        using (Graphics g = Graphics.FromImage(CurrentCapturedScreen))
        {
            g.CopyFromScreen(schermo.X, schermo.Y, 0, 0, schermo.Size);
        }

        return CurrentCapturedScreen;

    }

    //funzione che aggiorna il PreviousFrameBuffer
    public void UpdateFrameBuffer()
    {
        PreviousCapturedScreen?.Dispose();
        PreviousCapturedScreen = (Bitmap)CurrentCapturedScreen.Clone();
    }

    
    //funzione che aggiorna il PreviousFrameBufferCRC
    public void UpdateFrameBufferCRC()
    {
        PreviousCapturedScreenCRC = CurrentCapturedScreenCRC;
    }
    
    
    
    //funzione che calcola il CRC del FrameBuffer
    public void CalculateCapturedScreenCRC()
    {
        CurrentCapturedScreenCRC = ManageUtils.CalculateCRC(CurrentCapturedScreen).ToString();
    }

    //funzione che confronta i CRC corrente e precedente 
    public bool CompareFrameBufferCRC()
    {
        return CurrentCapturedScreenCRC == PreviousCapturedScreenCRC;
    }
    
    
    #endregion

    #region Gestione Tiles
    
    //TITLE: funzione che divide immagine in n tiles 
    //DESCR: 
    public void CreateTiles()
    {
        // Rilascia le tile precedenti
        foreach (var tile in CurrentTilesList)
            tile?.Dispose();
        
        CurrentTilesList.Clear();
    
        int tileWidth = CurrentCapturedScreen.Width / 4;
        int tileHeight = CurrentCapturedScreen.Height / 6;

        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                Rectangle srcRect = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                Bitmap tile = CurrentCapturedScreen.Clone(srcRect, CurrentCapturedScreen.PixelFormat);
                CurrentTilesList.Add(tile);
            }
        }
    }
    
    //funzione che calcola CRC di tiles
    public void CalculateTilesCRC()
    {
        CurrentTilesListCRC.Clear();
        foreach (var tile in CurrentTilesList)
        {
            uint value = ManageUtils.CalculateCRC(tile);
            CurrentTilesListCRC.Add(value.ToString());
        }
    }
    
    //funzione confronta Tiles
    public void CompareTilesCRC()
    {
        if (ComparedTilesList.Count == 0)
        {
            PreviousTilesListCRC = new List<string>(CurrentTilesListCRC);

            foreach (var tile in CurrentTilesList)
                PreviousTilesList.Add((Bitmap)tile.Clone());

            InitializeCompareTilesList();
        }
        else
        {
            for (int i = 0; i < ComparedTilesList.Count; i++)
            {
                ComparedTilesList[i] = true;
                if (CurrentTilesListCRC[i] != PreviousTilesListCRC[i])
                {
                    ComparedTilesList[i] = false;

                    

                    //rilascia vecchia + clone
                    PreviousTilesList[i]?.Dispose();
                    PreviousTilesList[i] = (Bitmap)CurrentTilesList[i].Clone();

                    PreviousTilesListCRC[i] = CurrentTilesListCRC[i];
                }
            }
        }
    }
    
    //Inizializzo la ComparedTilesList a False
    private void InitializeCompareTilesList()
    {
        for (int i = 0; i < CurrentTilesListCRC.Count; i++)
        {
            ComparedTilesList.Add(false);
        }
    }
    
    //Aggiorno lista previous dei tiles
    public void UpdatePreviousTilesList()
    {
        // Rilascia le bitmap precedenti: le elimina per evitare che la memoria si riempia
        foreach (var b in PreviousTilesList)
            b?.Dispose();

        PreviousTilesList.Clear();

        // non copia riferimento ma copia oggetto e lo aggiunge alla lista 
        foreach (var tile in CurrentTilesList)
            PreviousTilesList.Add((Bitmap)tile.Clone());
    }

    
    //Aggiorno lista previous dei TIlesCRC
    public void UpdatePreviousTilesListCRC()
    {
        PreviousTilesListCRC = new List<string>(CurrentTilesListCRC);
    }
    #endregion
    
    
}