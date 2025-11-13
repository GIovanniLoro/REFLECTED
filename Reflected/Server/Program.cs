using System;
using System.Drawing.Imaging;
using Server;

class Program
{
    static void Main(string[] args)
    {
        // Inizializzo le classi
        ManageGraphics manageGraphics = new ManageGraphics();
        ManageUtils manageUtils = new ManageUtils();
        
        // Inizializzazione variabili
        string imagePath = @"C:\Users\giovanni\Documents\Scuola\Programmazione\Reflected\Dbg\";
        string imageName = "fbcur.bmp";
        string imageCRC = "fcurcrc.txt";
        manageUtils.PercorsoLogFile = @"C:\Users\giovanni\Documents\Scuola\Programmazione\Reflected\Dbg\Server.log";
        
        // Inizializzo il file di log 
        manageUtils.ClearLog();

        for (int i = 0; i < 11; i++)
        {
            // Catturo il framebuffer iniziale
            manageUtils.AppendLog("Avvio funzione cattura schermo");
            manageGraphics.CurrentCapturedScreen = manageGraphics.CaptureFrameBuffer();

            // Calcolo il CRC del framebuffer iniziale
            manageUtils.AppendLog("Calcolo il CRC della schermata acquisita e lo salvo nel file " + imagePath + imageCRC);
            manageGraphics.CalculateCapturedScreenCRC();

            //confronto il CRC del FrameBuffer corrente con quello precedente
            if (!manageGraphics.CompareFrameBufferCRC())
            {
                //Salvo il FrameBuffer sul file
                manageUtils.AppendLog("Salvo l'immagine acquisita nel file " + imagePath + imageName);
                manageGraphics.CurrentCapturedScreen.Save(imagePath + imageName, ImageFormat.Bmp);
                
                // Calcolo il CRC del framebuffer corrente
                manageUtils.AppendLog("Calcolo il CRC della schermata acquisita e lo salvo nel file " + imagePath + imageCRC);
                File.WriteAllText(imagePath + imageCRC, manageGraphics.CurrentCapturedScreenCRC);
                manageUtils.AppendLog(manageGraphics.CurrentCapturedScreenCRC);
            
                // Creo le tiles del Framebuffer corrente
                manageUtils.AppendLog("Creo le tiles della schermata principale e le salvo");
                manageGraphics.CreateTiles();
                
                // Calcolo il CRC delle tiles
                manageUtils.AppendLog("Calcolo i CRC dei tiles e li salvo nel file di testo");
                manageGraphics.CalculateTilesCRC();

                // Confronto le Tiles correnti con le precedenti
                manageUtils.AppendLog("Confronto i Tiles correnti con i precedenti");
                manageGraphics.CompareTilesCRC();

                // Scorro la lista delle Tiles modificate e salvo quelle aggiornate
                for(int j = 0 ; j < manageGraphics.ComparedTilesList.Count; j++)
                {
                    // Verifico se la Tile corrente è stata modificata
                    if (manageGraphics.ComparedTilesList[j] == false)
                    {
                        manageUtils.AppendLog("la tiles nr " + j + " è cambiata");
                        // Salvo la Tile modificata
                        string tileName = "fbtile" + j + ".bmp";
                        manageGraphics.CurrentTilesList[j].Save(imagePath + tileName, ImageFormat.Bmp);
                        
                        // Salvo il CRC della tile modificata
                        string tileNameCrc = "fbtilecrc" + j + ".txt";
                        File.WriteAllText(imagePath + tileNameCrc, manageGraphics.CurrentTilesListCRC[j]);
                    }
                }
            }
            
            //aggiorno variabili da current a previous
            manageGraphics.UpdateFrameBuffer();
            manageGraphics.UpdateFrameBufferCRC();
            manageGraphics.UpdatePreviousTilesList();
            manageGraphics.UpdatePreviousTilesListCRC();
            
            //Attendo un secondo e poi riavvio il ciclo
            Thread.Sleep(1000);
        }
        
        

        

        // Confronto i CRC delle tiles acquisite con i tile precedenti
        // Se sono tutti uguali invio CRC Generaale

        // Se sono tutti diversi invio tutte le tiles

        // Se solo alcuni sono diversi invio solo le tiles diverse e tutti i CRC

        // manageGraphics.CalculateTilesCRC();


    }
}