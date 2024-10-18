using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TowerDownloader : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] GameObject stage;
    [SerializeField] string screenshotFilePath = "";
    private int _originalCullingMask;
    private CameraClearFlags _originalClearFlags;
    private Color _originalBackgroundColor;
    private float _orthographicSize;
    private float _originalAspect;
    private Vector3 _mainCameraPosition;
    
    public static TowerDownloader Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _originalCullingMask = mainCamera.cullingMask;
        _originalClearFlags = mainCamera.clearFlags;
        _originalBackgroundColor = mainCamera.backgroundColor;
        _orthographicSize = mainCamera.orthographicSize;
        _mainCameraPosition = mainCamera.transform.position;
        _originalAspect = mainCamera.aspect;
    }

    // オブジェクトが積まれて全体が動かなくなったら呼び出す
    public void CaptureScreenshot()
    {
        
        // タグ "Target" が付いたオブジェクトを取得し、y座標が -4 以上のもののみを残す
        GameObject[] allTargetObjects = GameObject.FindGameObjectsWithTag("Target");
        List<GameObject> validTargetObjects = new List<GameObject>();
        foreach (GameObject obj in allTargetObjects)
        {
            if (obj.transform.position.y >= -2)
            {
                validTargetObjects.Add(obj);
            }
        }

        // 有効なターゲットオブジェクトが存在しない場合は処理を終了
        if (validTargetObjects.Count == 0 || stage == null)
        {
            return;
        }

        // スクリーンショット用レイヤーに設定
        int screenshotLayer = 8;
        foreach (GameObject obj in validTargetObjects)
        {
            obj.layer = screenshotLayer;
        }
        mainCamera.cullingMask = 1 << screenshotLayer;

        // ステージの y 座標を取得
        float stageYPosition = stage.transform.position.y;

        // 有効なターゲットオブジェクトの中で最も高い y 座標を取得
        float maxYPosition = GetHighestYPosition(validTargetObjects.ToArray());

        // ステージの y 座標と最も高い y 座標の距離をカメラの高さとする
        float newCameraHeight = (maxYPosition + 10 - stageYPosition);;

        // カメラの中央を、ステージと最高ブロックの中間点に設定
        mainCamera.transform.position = new Vector3(
            mainCamera.transform.position.x,
            stageYPosition + newCameraHeight / 2.0f,
            mainCamera.transform.position.z
        );

        // カメラの表示領域 (orthographicSize) を設定
        mainCamera.orthographicSize = newCameraHeight / 2.0f;
        
        // 横幅を DebugBar の長さに合わせる
        float stageWidth = stage.GetComponent<Renderer>().bounds.size.x + 5;
        Debug.Log(stageWidth);
        float aspectRatio = stageWidth / (newCameraHeight);
        mainCamera.aspect = aspectRatio;

        // 透明な背景の設定
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0, 0, 0, 0);  // アルファ値 0 の透明色

        // RenderTexture を画面サイズに設定　
        RenderTexture renderTexture = new RenderTexture(
            Mathf.CeilToInt(stageWidth * 100),  // 横幅を DebugBar の横幅に調整
            Screen.height + 100,
            24
        );
        mainCamera.targetTexture = renderTexture;
        Texture2D screenShot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // カメラの内容をレンダーテクスチャにレンダリング
        mainCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        screenShot.Apply();

        // カメラとレンダーテクスチャの設定を解除
        mainCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        
        // カメラの設定を元に戻す
        mainCamera.cullingMask = _originalCullingMask;
        mainCamera.clearFlags = _originalClearFlags;
        mainCamera.backgroundColor = _originalBackgroundColor;
        mainCamera.transform.position = _mainCameraPosition;
        mainCamera.orthographicSize = _orthographicSize;
        mainCamera.aspect = _originalAspect;

        // PNG形式のバイトデータとして保存
        #if UNITY_EDITOR
            string path = Directory.GetCurrentDirectory();
        #else
            string path = Application.persistentDataPath;
        #endif
            File.WriteAllBytes(path+"/testdata" + ".png", screenShot.EncodeToPNG());
        
        // オブジェクトのレイヤーを元に戻す
        foreach (GameObject obj in validTargetObjects)
        {
            obj.layer = LayerMask.NameToLayer("Default");
        }

        Debug.Log("Screenshot with transparency saved at: " + screenshotFilePath);
    }

    // y座標が最も高いオブジェクトのy座標を取得する関数
    private float GetHighestYPosition(GameObject[] objects)
    {
        float maxY = float.MinValue;
        foreach (GameObject obj in objects)
        {
            if (obj.transform.position.y > maxY)
            {
                maxY = obj.transform.position.y;
            }
        }
        return maxY;
    }
}