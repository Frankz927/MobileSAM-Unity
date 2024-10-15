using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingPhotos : MonoBehaviour
{
    public static GameObject Create2DObjectFromSegmentation(Texture texture)
    {
        // テクスチャから2Dオブジェクトを生成するロジック
        GameObject obj = new GameObject("SegmentedObject");
        
        if (texture is Texture2D texture2D)
        {
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            
            // テクスチャからスプライトを作成
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            renderer.sprite = sprite;

            // BoxColliderを追加
            BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(texture2D.width / 100f, texture2D.height / 100f, 1); // サイズを調整（スプライトのピクセルサイズをUnityの単位に変換）
            
            // オブジェクトの座標を設定（必要に応じて調整）
            obj.transform.position = new Vector3(0, 10, 0); // 指定した座標に変更
        }

        return obj;
    }
}