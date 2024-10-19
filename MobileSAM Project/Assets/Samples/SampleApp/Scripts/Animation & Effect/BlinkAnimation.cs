using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BlinkAnimation : MonoBehaviour
{
    public Text[] blinkTexts;  // 複数のテキストを保持
    public float dotweenInterval;

    private int currentTextIndex = 0;  // 現在点滅中のテキストのインデックス
    private Tween loopBlinkText;

    public static BlinkAnimation instance;  

    // プロパティで現在のテキストを公開する
    public Text CurrentBlinkingText => blinkTexts[currentTextIndex];

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        // 初回のテキスト点滅開始
        TextBlink();
    }

    private void Update()
    {
        // 左右キーの入力を直接監視
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeText(-1);  // 左に移動
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeText(1);  // 右に移動
        }
    }

    // テキストを点滅させる処理
    public void TextBlink()
    {
        if (blinkTexts.Length == 0) return;  // テキストが設定されていない場合は何もしない

        // 現在点滅中のテキストを取得
        Text blinkRunText = blinkTexts[currentTextIndex];

        // テキストの不透明度をリセットして即座に1.0にする
        blinkRunText.DOFade(1.0f, 0);

        // 点滅アニメーションを開始
        loopBlinkText = blinkRunText.DOFade(0.0f, dotweenInterval)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRecyclable(true)  // 再利用可能に設定
            .SetAutoKill(false);  // アニメーションをキルしないようにする
    }

    // 点滅アニメーションを停止する処理
    public void KillBlink()
    {
        if (loopBlinkText != null)
        {
            loopBlinkText.Kill();  // 現在の点滅Tweenを停止
            blinkTexts[currentTextIndex].DOFade(1, 0.5f);  // 不透明度を元に戻す
        }
    }

    // 左右キーでテキストを変更する処理
    private void ChangeText(int direction)
    {
        // 新しいインデックスを計算
        int newIndex = currentTextIndex + direction;

        // インデックスが範囲外の場合は何もしない
        if (newIndex < 0 || newIndex >= blinkTexts.Length)
        {
            return;
        }

        // 現在のテキストインデックスと同じ場合も何もしない
        if (newIndex == currentTextIndex)
        {
            return;
        }

        // 現在の点滅を停止
        KillBlink();

        // インデックスを更新
        currentTextIndex = newIndex;

        // 新しいテキストで点滅を開始
        TextBlink();
    }
}
