using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingPhotos : MonoBehaviour
{
    public static void Create2DObjectFromSegmentation(Texture texture)
    {
        if (texture is Texture2D texture2D)
        {
            // テクスチャから2Dオブジェクトを生成するロジック
            GameObject obj = new GameObject("SegmentedObject");
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            
            // テクスチャからスプライトを作成
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            renderer.sprite = sprite;

            // BoxColliderを追加
            BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(texture2D.width / 100f, texture2D.height / 100f, 1); // サイズを調整（スプライトのピクセルサイズをUnityの単位に変換）

            // Rigidbodyを追加して重力を有効にする
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = true; // 重力を使用する

            // オブジェクトの座標を設定（必要に応じて調整）
            obj.transform.position = new Vector3(-100, 30, 0); // 指定した座標に変更

            // オブジェクトの親を指定する（必要に応じて）
            // obj.transform.SetParent(parentTransform);
        }
    }
}