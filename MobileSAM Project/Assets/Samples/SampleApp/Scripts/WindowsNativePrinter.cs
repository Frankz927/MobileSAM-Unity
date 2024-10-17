using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using UnityEngine;
using Font = System.Drawing.Font;

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
        Bitmap bitmap = LoadImage(filePath);

        if (bitmap == null)
        {
            Debug.LogError("印刷する画像が存在しません。");
            return;
        }

        PrintDocument pd = new PrintDocument();
        pd.PrinterSettings.PrinterName = _printerName;
        pd.DefaultPageSettings.Landscape = true; // 横向きに設定

        pd.PrintPage += (sender, e) =>
        {
            // ページの描画領域とDPIを取得
            float dpiX = e.Graphics.DpiX;
            float dpiY = e.Graphics.DpiY;

            // 用紙サイズに収まるように画像をスケーリング
            float printableWidth = e.PageBounds.Width;
            float printableHeight = e.PageBounds.Height;

            // 画像を90度回転
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

            // 画像のスケーリング
            float imageAspectRatio = (float)bitmap.Height / bitmap.Width;  // 高さと幅の比率を取得
            float scaledWidth = printableHeight;  // 90度回転させたため、幅は高さに
            float scaledHeight = scaledWidth * imageAspectRatio;  // 比率を元に高さを計算

            // スケーリングが用紙に収まるか確認
            if (scaledHeight > printableWidth) // もし画像が用紙に収まらない場合
            {
                scaledHeight = printableWidth; // 高さを用紙に収まるように設定
                scaledWidth = scaledHeight / imageAspectRatio; // 幅を再計算
            }

            // テキスト描画
            string receiptText = "今回のゲームレシート";
            Font font = new Font("Arial", 24);  // フォントサイズ調整
            float textHeight = font.GetHeight(e.Graphics);
            e.Graphics.DrawString(receiptText, font, Brushes.Black, new PointF(10, 10)); // 上部に描画

            // 画像描画
            float imageXPosition = (printableWidth - scaledWidth) / 2;  // 中央に配置
            float imageYPosition = textHeight + 20; // テキストの下に配置
            e.Graphics.DrawImage(bitmap, new RectangleF(imageXPosition, imageYPosition, scaledWidth, scaledHeight));
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
        Init("testdata.png");
        PrintReceipt();  // 印刷テスト
    }
}
