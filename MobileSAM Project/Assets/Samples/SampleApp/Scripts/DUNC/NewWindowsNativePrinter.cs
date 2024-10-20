using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using UnityEngine;


public class NewWindowsNativePrinter : MonoBehaviour
{
    public static NewWindowsNativePrinter Instance;
    private string _imagePath;
    private string _printerName = "Brother QL-700";
    
    public void Init(string imagePath)
    {
        _imagePath = imagePath;
    }
    
    private void Start()
    {
        PrintReceipt();
    }
    
    private Bitmap LoadImage(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"ファイルが見つかりません: {filePath}");
            return null;
        }

        try
        {
            return new Bitmap(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"画像読み込みエラー: {ex.Message}");
            return null;
        }
    }

    public void PrintReceipt()
    {
        // PNGファイルとして保存
        #if UNITY_EDITOR
                string path = Directory.GetCurrentDirectory();
        #else
                string path = Application.persistentDataPath;
        #endif
                string[] files = Directory.GetFiles(path, "*.png");

        PrintDocument pd = new PrintDocument();
        
        pd.PrinterSettings.PrinterName = _printerName;

        pd.PrintPage += (sender, e) =>
        {
            float labelWidthInMM = 62;
            float dpiX = e.Graphics.DpiX;
            float labelWidthInPixels = labelWidthInMM * dpiX / 25.4f;

            int currentY = 0;
            foreach (string file in files)
            {
                Bitmap bitmap = LoadImage(file);
                float scaleFactor = (labelWidthInPixels / bitmap.Width);
                float scaledHeight = bitmap.Height * scaleFactor;
                float scaledWidth = (labelWidthInPixels);
                
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);

                e.Graphics.DrawImage(bitmap, new Rectangle(0,currentY,bitmap.Width / 3,bitmap.Height/3));
                currentY += bitmap.Height/3;

                bitmap.Dispose();
            }
        };

        try
        {
            //pd.Print();
            Debug.Log("印刷が完了しました。");
        }
        catch (Exception ex)
        {
            Debug.LogError($"印刷エラー: {ex.Message}");
        }
        
    }
}
