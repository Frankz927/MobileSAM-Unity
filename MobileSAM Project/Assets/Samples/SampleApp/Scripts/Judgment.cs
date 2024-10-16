using System;
using System.Collections.Generic;
using Dummy.StatePattern;
using UniRx;
using UnityEngine;

public class Judgment : MonoBehaviour
{
    private static List<Rigidbody2D> trackedRigidbodies = new List<Rigidbody2D>();
    private bool isMonitoring = false;
    
    public static Judgment Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void StartMonitoring()
    {
        Debug.Log("モニタリング開始");
        isMonitoring = true;

        // Rigidbody2Dの動きを監視
        foreach (var rb in trackedRigidbodies)
        {
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

                    // ステージの安定性判定
                    if (IsStable())
                    {
                        
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
        foreach (var rb in trackedRigidbodies)
        {
            if (rb.velocity.magnitude > 0)
            {
                return false;
            }
        }
        return true;
    }
}