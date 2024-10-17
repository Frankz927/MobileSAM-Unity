using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance { get; private set; }
    /// <summary>
    /// stageをアタッチ
    /// </summary>
    [SerializeField] private Transform stageTransform;
    private float boundaryX;
    
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


}
