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
    public float maxYPosition;
    
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
        maxYPosition = 0;
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
        
        // 固定のアスペクト比を設定 (3500ピクセル x 500ピクセル)
        float fixedWidth = 3500f;
        float baseHeight = 500f;  // オブジェクト1つのときの縦幅
        float referenceYPosition = 6f;  // y座標6で500ピクセルに対応


        // カメラの表示領域 (orthographicSize) を新しい高さに合わせて調整
        float stageYPosition = stage.transform.position.y;
        maxYPosition = GetHighestYPosition(validTargetObjects.ToArray());
        float newCameraHeight = (maxYPosition + 10 - stageYPosition);

        // カメラの中央を、ステージと最高ブロックの中間点に設定
        mainCamera.transform.position = new Vector3(
            mainCamera.transform.position.x,
            stageYPosition + newCameraHeight / 2.0f,
            mainCamera.transform.position.z
        );

        // カメラのorthographicSizeを固定サイズに基づいて設定 (高さを固定比率でスケーリング)
        mainCamera.orthographicSize = newCameraHeight / 2.0f;
        
        // 最上部オブジェクトの高さに基づいて縦幅を計算
        float adjustedHeight = baseHeight + (maxYPosition / referenceYPosition);
        Debug.Log("adjustedHeight" + adjustedHeight);

        // 横幅に合わせたアスペクト比を設定
        float aspectRatio = fixedWidth / baseHeight;
        mainCamera.aspect = aspectRatio;

        // RenderTexture を固定サイズ (3500x500) に設定
        RenderTexture renderTexture = new RenderTexture(
            Mathf.CeilToInt(fixedWidth),
            Mathf.CeilToInt(baseHeight),
            24
        );
        mainCamera.targetTexture = renderTexture;
        Texture2D screenShot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // カメラの内容をレンダーテクスチャにレンダリング
        mainCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        screenShot.Apply();

        // PNGとして保存
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory();
#else
        string path = Application.persistentDataPath;
#endif
        File.WriteAllBytes(path+"/testdata.png", screenShot.EncodeToPNG());
        
        
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
        
        // オブジェクトのレイヤーを元に戻す
        foreach (GameObject obj in validTargetObjects)
        {
            obj.layer = LayerMask.NameToLayer("Default");
        }

        Debug.Log("Screenshot with transparency saved at: " + screenshotFilePath);
    }

    // y座標が最も高いオブジェクトのy座標を取得する関数
    public float GetHighestYPosition(GameObject[] objects)
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