using System;
using System.Collections;
using System.Collections.Generic;
using Dummy.StatePattern;
using UnityEngine;

public class StateUIManager : MonoBehaviour
{
    // ステートとUIを管理するクラス
    [Serializable]
    public class UIStateMapping
    {
        public StateType stateType;  // ステートタイプ
        public GameObject associatedUI;  // 関連するUI（ない場合はnullを許容）
    }

    [Header("State UI Mappings")]
    public List<UIStateMapping> uiStateMappings = new List<UIStateMapping>();

    private Dictionary<StateType, GameObject> stateUIMap;
    private Dictionary<StateType, bool> hasUIBeenShownBefore;  // 各ステートの表示状況を追跡
    
    public static StateUIManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        // ステートとUIのマッピングを初期化
        InitializeStateUIMap();
    }

    private void InitializeStateUIMap()
    {
        // マッピングをディクショナリに変換
        stateUIMap = new Dictionary<StateType, GameObject>();
        hasUIBeenShownBefore = new Dictionary<StateType, bool>();

        foreach (var mapping in uiStateMappings)
        {
            stateUIMap[mapping.stateType] = mapping.associatedUI;  // nullでも許容
            hasUIBeenShownBefore[mapping.stateType] = false;  // 最初は全て未表示とする
        }
    }

    /// <summary>
    /// 指定されたステートのUIを表示する。
    /// </summary>
    /// <param name="stateType">表示するステートタイプ</param>
    /// <param name="forceShowEachTime">trueの場合、何回呼び出されても毎回UIを表示。falseの場合は最初の1回のみ表示。</param>
    /// <param name="displayDuration">UIの表示時間。-1なら永続的に表示。0以上なら指定秒数後に非表示。</param>
    public void ShowUIForState(StateType stateType, bool forceShowEachTime, float displayDuration)
    {
        // すべてのUIを非表示にする
        HideAllUI();

        // 選択されたステートにUIが割り当てられているか確認
        if (stateUIMap.ContainsKey(stateType) && stateUIMap[stateType] != null)
        {
            // forceShowEachTimeがtrueなら毎回表示する
            if (forceShowEachTime)
            {
                DisplayUI(stateType, displayDuration);
            }
            else
            {
                // 最初の1回だけ表示
                if (!hasUIBeenShownBefore[stateType])
                {
                    DisplayUI(stateType, displayDuration);
                    hasUIBeenShownBefore[stateType] = true;  // 表示されたことを記録
                }
            }
        }
        else
        {
            Debug.LogWarning($"State '{stateType}'に対応するUIがありません。");
        }
    }

    /// <summary>
    /// 指定されたUIを表示し、必要なら指定時間後に非表示にする。
    /// </summary>
    /// <param name="stateType">表示するステートタイプ</param>
    /// <param name="displayDuration">UIの表示時間。-1なら永続的に表示、0以上なら指定秒数後に非表示。</param>
    private void DisplayUI(StateType stateType, float displayDuration)
    {
        var uiElement = stateUIMap[stateType];
        if (uiElement != null)
        {
            uiElement.SetActive(true);

            // displayDurationが0以上なら指定秒数後にUIを非表示にする
            if (displayDuration >= 0)
            {
                StartCoroutine(HideUIAfterDelay(uiElement, displayDuration));
            }
        }
    }

    /// <summary>
    /// 指定時間後にUIを非表示にするコルーチン
    /// </summary>
    /// <param name="uiElement">非表示にするUI要素</param>
    /// <param name="delay">非表示までの遅延時間（秒）</param>
    private IEnumerator HideUIAfterDelay(GameObject uiElement, float delay)
    {
        yield return new WaitForSeconds(delay);
        uiElement.SetActive(false);
    }

    /// <summary>
    /// すべてのUIを非表示にする
    /// </summary>
    public void HideAllUI()
    {
        // すべてのUIを非表示にする
        foreach (var uiElement in stateUIMap.Values)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(false);
            }
        }
    }
}
