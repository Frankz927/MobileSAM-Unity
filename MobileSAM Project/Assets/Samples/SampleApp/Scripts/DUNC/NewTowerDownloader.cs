using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NewTowerDownloader : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] GameObject stage;
    private int _originalCullingMask;
    private CameraClearFlags _originalClearFlags;
    private Color _originalBackgroundColor;
    private float _orthographicSize;
    private float _originalAspect;
    private Vector3 _mainCameraPosition;
    
    public static NewTowerDownloader Instance { get; private set; }
    
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

    // 台上のTargetタグ付きオブジェクトをカメラの視界に入るまで順にスクリーンショットを撮影
    public void CaptureScreenshotsInSections()
    {
        GameObject[] allTargetObjects = GameObject.FindGameObjectsWithTag("Target");
        List<GameObject> validTargetObjects = new List<GameObject>();

        foreach (GameObject obj in allTargetObjects)
        {
            if (obj.transform.position.y >= stage.transform.position.y)
            {
                validTargetObjects.Add(obj);
            }
        }

        if (validTargetObjects.Count == 0)
        {
            return;
        }

        // オブジェクトをスクリーンショットレイヤーに設定
        int screenshotLayer = 8;
        foreach (GameObject obj in validTargetObjects)
        {
            obj.layer = screenshotLayer;
        }
        mainCamera.cullingMask = 1 << screenshotLayer;

        // カメラの初期位置と範囲をリセット
        mainCamera.transform.position = new Vector3(
            mainCamera.transform.position.x,
            stage.transform.position.y + _orthographicSize,
            mainCamera.transform.position.z
        );

        float stageYPosition = stage.transform.position.y;
        float maxYPosition = GetHighestYPosition(validTargetObjects.ToArray());
        float currentYPosition = stageYPosition;

        int screenshotIndex = 0;

        while (currentYPosition < maxYPosition)
        {
            // カメラのY位置を更新してスクリーンショットを撮影
            mainCamera.transform.position = new Vector3(
                mainCamera.transform.position.x,
                currentYPosition + _orthographicSize,
                mainCamera.transform.position.z
            );

            // スクリーンショットを取得 (Screen.heightとScreen.widthを使用)
            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            mainCamera.targetTexture = renderTexture;
            Texture2D screenShot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

            mainCamera.Render();
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenShot.Apply();

            // PNGファイルとして保存
            #if UNITY_EDITOR
                        string path = Directory.GetCurrentDirectory()  + "/screenshot_" + screenshotIndex + ".png";;
            #else
                    string path = Application.persistentDataPath + "/screenshot_" + screenshotIndex + ".png";;
            #endif
            File.WriteAllBytes(path, screenShot.EncodeToPNG());
            screenshotIndex++;
            Debug.Log("Screenshot saved to: " + path);

            // メモリの解放
            mainCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);

            // 次のY位置に進める
            currentYPosition += _orthographicSize * 2.0f; // カメラの視野分進める
        }

        // レイヤーを元に戻す
        foreach (GameObject obj in validTargetObjects)
        {
            obj.layer = LayerMask.NameToLayer("Default");
        }

        // カメラ設定を元に戻す
        ResetCameraSettings();
    }

    private void ResetCameraSettings()
    {
        mainCamera.cullingMask = _originalCullingMask;
        mainCamera.clearFlags = _originalClearFlags;
        mainCamera.backgroundColor = _originalBackgroundColor;
        mainCamera.transform.position = _mainCameraPosition;
        mainCamera.orthographicSize = _orthographicSize;
        mainCamera.aspect = _originalAspect;
    }

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
