using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    /// <summary>
    /// stageをアタッチ
    /// </summary>
    [SerializeField] private Transform stageTransform;
    private float boundaryX;

    public void MoveObject(GameObject lastDroppedGameObject,　float moveSpeed)
    {
        float newX = Mathf.Clamp(
            lastDroppedGameObject.transform.position.x + moveSpeed * Time.deltaTime,
            -boundaryX,
            boundaryX
        );
        lastDroppedGameObject.transform.position = new Vector3(newX, lastDroppedGameObject.transform.position.y, lastDroppedGameObject.transform.position.z);
    }

    public void UpdateBoundary()
    {
        boundaryX = stageTransform.localScale.x / 2f;  // プラットフォームのスケールに基づいた移動範囲を更新
    }

    public void tester()
    {
        Debug.Log("そもそもこいつが呼び出せてない");
    }
}
