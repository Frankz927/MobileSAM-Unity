using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

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
        
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
}
