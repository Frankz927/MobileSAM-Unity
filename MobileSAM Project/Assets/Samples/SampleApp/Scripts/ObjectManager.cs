using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance { get; private set; }
    /// <summary>
    /// stageをアタッチ
    /// </summary>
    [SerializeField] private Transform stageTransform;
    [SerializeField] private Transform cameraTransform;
    private Transform originalCameraTransform;
    private float boundaryX;
    private Vector3 respawnPosition;
    [SerializeField] private float interval = 16f;
    
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
        UpdateBoundary();
        respawnPosition = new Vector3(0,20,0);
        originalCameraTransform = cameraTransform;
    }
    
    public GameObject Create2DObjectFromSegmentation(Texture texture)
    {
        // テクスチャから2Dオブジェクトを生成するロジック
        GameObject obj = new GameObject("SegmentedObject");
        obj.tag = "Target";
        
        if (texture is Texture2D texture2D)
        {
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            
            // テクスチャからスプライトを作成
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            renderer.sprite = sprite;

            // BoxColliderを追加
            PolygonCollider2D boxCollider = obj.AddComponent<PolygonCollider2D>();
            //boxCollider.size = new Vector3(texture2D.width / 100f, texture2D.height / 100f, 1); // サイズを調整（スプライトのピクセルサイズをUnityの単位に変換）
            
            // オブジェクトの座標を設定（必要に応じて調整）
            obj.transform.position = respawnPosition;
        }

        return obj;
    }

    public void MoveObject(GameObject lastDroppedGameObject,　float moveSpeed)
    {
        float newX = Mathf.Clamp(
            lastDroppedGameObject.transform.position.x + moveSpeed * Time.deltaTime,
            -boundaryX,
            boundaryX
        );
        lastDroppedGameObject.transform.position = new Vector3(newX, lastDroppedGameObject.transform.position.y, lastDroppedGameObject.transform.position.z);
    }
    
    // オブジェクトを回転させるメソッド
    public void RotateObject(GameObject lastDroppedGameObject, float rotationSpeed, bool rotateClockwise)
    {
        // 回転方向に応じて回転角度を設定
        float direction = rotateClockwise ? -1f : 1f;
        float rotationAmount = rotationSpeed * direction * Time.deltaTime;

        // オブジェクトを中心軸に回転させる
        lastDroppedGameObject.transform.Rotate(Vector3.forward, rotationAmount);
    }

    public void UpdateBoundary()
    {
        boundaryX = stageTransform.localScale.x / 2f;  // プラットフォームのスケールに基づいた移動範囲を更新
    }
    
    public void ApplyGravity(GameObject target)
    {
        Rigidbody2D rb = target.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1.0f;
        
        // ApplyGravity が終わった後、モニタリングを遅延付きで開始する
        StartMonitoringAfterDelay(rb);
    }

    private async void StartMonitoringAfterDelay(Rigidbody2D rb)
    {
        // 0.2秒の待機を入れてからモニタリングを開始
        await UniTask.Delay(400);
        Judgment.Instance.AddTrackedRigidbody(rb);
        Judgment.Instance.StartMonitoring();
    }

    public void CameraTransformUpdate()
    {
        Debug.Log("カメラとリスポーン地点をあげたよ");
    
        // タワーの一番上のオブジェクトを取得
        var topmostTransform = Judgment.trackedRigidbodies
            .Where(rb => rb != null) // nullチェック
            .Select(rb => rb.transform) // Rigidbody2DからTransformを取得
            .OrderByDescending(t => t.position.y) // Y座標で降順ソート
            .FirstOrDefault(); // 最初の要素を取得（最も高いY座標のTransform）
        
        Debug.Log("この中で一番高いブロックのy座標は、" + topmostTransform.position.y);

        // タワーの一番上のオブジェクトのY座標を取得
        float lastObjY = topmostTransform.transform.position.y;
        float averageScale = (topmostTransform.transform.localScale.x + topmostTransform.transform.localScale.y) / 2;
        var intervalValue = Mathf.Abs(lastObjY - respawnPosition.y+ averageScale);
        Debug.Log("今回の変化量は、" + intervalValue);

        // 新しいリスポーン位置を計算
        if (topmostTransform.transform.position.y >= 7)
        {
            respawnPosition.y += topmostTransform.transform.localScale.y + intervalValue;
            Vector3 cameraPos = cameraTransform.position;
            cameraPos.y = respawnPosition.y/1.5f;     
            cameraTransform.position = cameraPos;
        }
        else
        {
            // デフォルトのリスポーン位置を設定
            respawnPosition.y = 20f; // デフォルト値に設定
            cameraTransform.position = originalCameraTransform.position;
        }


        Debug.Log($"新しいリスポーン位置: {respawnPosition.y}, カメラ位置: {cameraTransform.position.y}");
    }

}
