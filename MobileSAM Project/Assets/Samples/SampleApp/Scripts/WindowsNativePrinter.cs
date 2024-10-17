using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowsNativePrinter : MonoBehaviour
{
    // Windows API 関数の宣言
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr CreateDC(string driverName, string deviceName, string output, IntPtr initData);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool StartDoc(IntPtr hdc, [In] ref DOCINFO di);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool EndDoc(IntPtr hdc);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool StartPage(IntPtr hdc);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool EndPage(IntPtr hdc);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool TextOut(IntPtr hdc, int x, int y, string lpString, int nCount);

    // プリント関連の情報を保持する構造体
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DOCINFO
    {
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pDocName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pOutputFile;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pDatatype;
    }

    void Start()
    {
        PrintText("Brother QL-700", "テスト印刷のサンプル");
    }

    public void PrintText(string printerName, string text)
    {
        IntPtr hdc = CreateDC("WINSPOOL", printerName, null, IntPtr.Zero); // プリンタデバイスコンテキストを作成

        if (hdc == IntPtr.Zero)
        {
            Debug.LogError("プリンタが見つかりません");
            return;
        }

        DOCINFO di = new DOCINFO();
        di.pDocName = "Unity Print Job";  // ドキュメント名
        di.pOutputFile = null;
        di.pDatatype = null;

        // ドキュメントの開始
        if (!StartDoc(hdc, ref di))
        {
            Debug.LogError("ドキュメントの開始に失敗しました");
            DeleteDC(hdc);
            return;
        }

        // ページの開始
        if (!StartPage(hdc))
        {
            Debug.LogError("ページの開始に失敗しました");
            EndDoc(hdc);
            DeleteDC(hdc);
            return;
        }

        // テキストの描画 (座標はx=100, y=100の位置に描画)
        TextOut(hdc, 100, 100, text, text.Length);

        // ページの終了
        EndPage(hdc);

        // ドキュメントの終了
        EndDoc(hdc);

        // デバイスコンテキストの解放
        DeleteDC(hdc);
    }
}
