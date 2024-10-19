using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BackgroundAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField, Range(1f, 10f)] private float fallSpeed = 5f;   // 降るスピード
    [SerializeField, Range(10f, 360f)] private float rotateSpeed = 180f;  // 回転スピード
    [SerializeField] private RectTransform canvasRectTransform;  // キャンバスのRectTransform

    [Header("Prefab Settings")]
    [SerializeField] private GameObject imagePrefab;  // 降らせるImageのプレハブ

    private float canvasHeight;

    private void Start()
    {
        // キャンバスの高さを取得
        canvasHeight = canvasRectTransform.rect.height;
        // 降らせる処理を繰り返す
        StartBackgroundAnimation();
    }

    private void StartBackgroundAnimation()
    {
        // 一定間隔で降らせる処理
        InvokeRepeating(nameof(SpawnAndAnimateImage), 0.5f, 1f);  // 0.5秒後に1秒ごとに実行
    }

    private void SpawnAndAnimateImage()
    {
        // キャンバスの幅内でランダムなX座標を設定
        float randomPosX = Random.Range(0f, canvasRectTransform.rect.width);

        // 新しいImageを生成し、スタートポジションに設定
        GameObject newImage = Instantiate(imagePrefab, canvasRectTransform);
        RectTransform imageRect = newImage.GetComponent<RectTransform>();
        imageRect.anchoredPosition = new Vector2(randomPosX, canvasHeight / 2);  // 上からスタート

        // 降らせるアニメーション
        AnimateImageFall(newImage, imageRect);
    }

    private void AnimateImageFall(GameObject image, RectTransform imageRect)
    {
        // Y座標を負の方向に移動させる
        float targetYPos = -canvasHeight / 2;

        // 画像を降らせる
        imageRect.DOAnchorPosY(targetYPos, fallSpeed).SetEase(Ease.Linear).OnComplete(() =>
        {
            // カメラ外に出た後は削除してメモリを解放
            Destroy(image);
        });

        // 画像を回転させる
        imageRect.DORotate(new Vector3(0f, 0f, 360f), rotateSpeed, RotateMode.FastBeyond360)
                 .SetLoops(-1, LoopType.Restart)  // 無限ループで回転
                 .SetEase(Ease.Linear);  // 一定速度で回転
    }

    // インスペクターで設定を即座に反映するために、設定変更時にアニメーションを更新
    private void OnValidate()
    {
        // 新しいスライダー設定値でアニメーションを反映（DOTweenに即反映）
        DOTween.KillAll();  // 現在のアニメーションをリセット
        StartBackgroundAnimation();
    }
}
