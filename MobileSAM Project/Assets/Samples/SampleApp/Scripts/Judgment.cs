// Judgment.cs
using System.Collections.Generic;
using UniRx;
using Cysharp.Threading.Tasks;
using Dummy.StatePattern; // UniTask を使用するため
using UnityEngine;

public class Judgment : MonoBehaviour
{
    [HideInInspector] public static List<Rigidbody2D> trackedRigidbodies = new List<Rigidbody2D>();
    private bool isMonitoring = false;

    public static Judgment Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Application.quitting += SaveGameData;
    }

    public async void StartMonitoring()
    {
        Debug.Log("モニタリング開始");
        

        // 監視開始までの待機時間
        const int ignoreInitialDelayMilliseconds = 100;
        isMonitoring = true;

        // Rigidbody2Dの動きを監視
        foreach (var rb in trackedRigidbodies)
        {
            if (rb == null) continue;

            // Rigidbody2Dの初期の動きを無視するための待機
            await UniTask.Delay(ignoreInitialDelayMilliseconds);

            // Rigidbody2DのvelocityをReactivePropertyで監視
            var velocityProperty = new ReactiveProperty<Vector2>(rb.velocity);

            // Velocityの変更を監視
            velocityProperty
                .Subscribe(velocity =>
                {
                    // ゲームオーバー判定
                    if (rb.transform.position.y < -5)
                    {
                        ShowGameOverText();
                    }
                })
                .AddTo(this);

            // velocityを更新するための処理
            Observable.EveryFixedUpdate()
                .Subscribe(_ => velocityProperty.Value = rb.velocity)
                .AddTo(this);
        }
    }

    public void AddTrackedRigidbody(Rigidbody2D rb)
    {
        if (rb != null && !trackedRigidbodies.Contains(rb))
        {
            trackedRigidbodies.Add(rb);
        }
    }

    private void ShowGameOverText()
    {
        Debug.Log("ゲームオーバー！");
    }

    public bool IsStable()
    {
        if (isMonitoring)
        {
            foreach (var rb in trackedRigidbodies)
            {
                if (rb == null || rb.velocity.magnitude > 0)
                {
                    return false;
                }
            }

            Debug.Log("タワーは安定しています");
            isMonitoring = false;
            return true;
        }
        else{
            return false;
        }
    }
    private void SaveGameData()
    {
        trackedRigidbodies.Clear();
    }
}
