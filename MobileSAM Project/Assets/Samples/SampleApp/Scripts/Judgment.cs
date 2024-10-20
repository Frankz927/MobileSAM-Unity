// Judgment.cs
using System.Collections.Generic;
using UniRx;
using Cysharp.Threading.Tasks;
using Dummy.StatePattern; // UniTask を使用するため
using UnityEngine;
using UnityEngine.SceneManagement;

public class Judgment : MonoBehaviour
{
    [HideInInspector] public static List<Rigidbody2D> trackedRigidbodies = new List<Rigidbody2D>();
    private bool isMonitoring = false;
    private bool isGameOver;

    public static Judgment Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Application.quitting += SaveGameData;
        isGameOver = false;
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

            var velocityProperty = new ReactiveProperty<Vector2>(rb.velocity);
            // Rigidbody2DのvelocityをReactivePropertyで監視

            // Velocityの変更を監視
            velocityProperty
                .Subscribe(velocity =>
                {
                    // ゲームオーバー判定
                    if (!isGameOver && rb.transform.position.y < -5)
                    {
                        GameOver();
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

    private async void GameOver()
    {
        isGameOver = true;
        NewTowerDownloader.Instance.CaptureScreenshotsInSections();
        StateUIManager.instance.ShowUIForState(StateType.FALL,false, -1);
        
        // Rigidbody2Dの初期の動きを無視するための待機
        await UniTask.Delay(1000);
    
        // 画像パスを設定してから印刷
        NewWindowsNativePrinter.Instance.Init("testdata.png"); // ここで画像パスを設定
        //NewWindowsNativePrinter.Instance.PrintReceipt();

        await UniTask.Delay(2000);
        SceneManager.LoadScene("Result");
    }

    public bool IsStable()
    {
        if (isMonitoring)
        {
            foreach (var rb in trackedRigidbodies)
            {
                if (rb.velocity.magnitude > 0)
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
        isGameOver = false;
    }
}
