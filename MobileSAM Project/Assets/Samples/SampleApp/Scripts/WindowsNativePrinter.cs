using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using UnityEngine;

public class WindowsNativePrinter : MonoBehaviour
{
    private static WindowsNativePrinter _instance;
    public static WindowsNativePrinter Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("WindowsNativePrinter").AddComponent<WindowsNativePrinter>();
            }
            return _instance;
        }
    }

    private string _imagePath;
    private string _printerName = "Brother QL-700";  // デフォルトのプリンタ名

    public void Init(string imagePath)
    {
        _imagePath = imagePath;
    }

    public void SetPrinterName(string printerName)
    {
        _printerName = printerName;
    }

    private string GetImagePath()
    {
        #if UNITY_EDITOR
            string path = Directory.GetCurrentDirectory();  // エディター用
        #else
            string path = Application.persistentDataPath;   // ビルド後
        #endif

        // _imagePath が null の場合はエラーメッセージをログに記録
        if (string.IsNullOrEmpty(_imagePath))
        {
            Debug.LogError("画像パスが設定されていません。Initメソッドを呼び出して画像パスを設定してください。");
            return null;
        }

        return Path.Combine(path, _imagePath);  // パスを結合して返す
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
        string filePath = GetImagePath();

        // filePath が null の場合は印刷を中止
        if (filePath == null)
        {
            Debug.LogError("印刷を中止します。画像パスが無効です。");
            return;
        }

        Bitmap bitmap = LoadImage(filePath);

        if (bitmap == null)
        {
            Debug.LogError("印刷する画像が存在しません。");
            return;
        }

        PrintDocument pd = new PrintDocument();
        pd.PrinterSettings.PrinterName = _printerName;

        pd.PrintPage += (sender, e) =>
        {
            // レシート幅 62mm（DPIからピクセルに変換）
            float labelWidthInMM = 62;
            float dpiX = e.Graphics.DpiX;
            float labelWidthInPixels = labelWidthInMM * dpiX / 25.4f;  // mmからピクセルへの変換 (25.4mm = 1 inch)
            
            // 画像の比率を保持しながら、横幅がラベル幅に収まるようにスケーリング
            float scaleFactor = (labelWidthInPixels / bitmap.Width) * 1.5f;
            float scaledHeight = bitmap.Height * scaleFactor;
            float scaledWidth = (labelWidthInPixels);
            
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            

            // レシートの長さを調整 (高さに合わせる)
            // ここでカスタム用紙サイズを設定する
            PaperSize customPaperSize = new PaperSize("Custom", (int)scaledHeight, (int)scaledWidth);  // ミリ単位
            pd.DefaultPageSettings.PaperSize = customPaperSize;

            // 印刷範囲に画像を描画
            e.Graphics.DrawImage(bitmap, -250, 0, scaledWidth, scaledHeight);
        };

        try
        {
            pd.Print();
            Debug.Log("印刷が完了しました。");
        }
        catch (Exception ex)
        {
            Debug.LogError($"印刷エラー: {ex.Message}");
        }
        finally
        {
            bitmap.Dispose();  // メモリを解放
        }
    }

    private void Start()
    {
        Init("testdata.png"); // 初期画像パスを設定
        //PrintReceipt();
    }
}
