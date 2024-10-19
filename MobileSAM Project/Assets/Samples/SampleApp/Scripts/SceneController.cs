using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        //GameObjectが破棄された時にキャンセルを飛ばすトークンを作成
        var token = this.GetCancellationTokenOnDestroy();
        //UniTaskメソッドの引数にCancellationTokenを入れる
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Return), cancellationToken: token);
        if (SceneManager.GetActiveScene().name == "Start")
        {
            SceneManager.LoadScene("InGame");
        }

        if (SceneManager.GetActiveScene().name == "Result")
        {
            // Returnキーが押された時の処理
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // BlinkAnimationクラスから現在の点滅中のテキストを取得
                Text currentBlinkingText = BlinkAnimation.instance.CurrentBlinkingText;

                // 特定のテキスト名が一致したらシーン遷移を行う
                if (currentBlinkingText.text == "Home")  // 例えば「Start」というテキストの場合
                {
                    // シーン遷移を行う
                    SceneManager.LoadScene("Start");  // 適切なシーン名を指定
                }
                else if (currentBlinkingText.text == "Continue")  // 「Quit」の場合
                {
                    // ゲームを終了する
                    SceneManager.LoadScene("InGame");
                }
            }
        }

    }

}