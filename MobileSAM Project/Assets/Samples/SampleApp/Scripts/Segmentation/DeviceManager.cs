using System.Collections.Generic;
using Sample;
using UnityEngine;
using Cysharp.Threading;
using Cysharp.Threading.Tasks;

public class DeviceManager : MonoBehaviour
{
    // カメラの名前リストを保持する変数
    public int selectedCameraIndex = 0;
    private WebCamTexture webCamTexture;
    private WebCamDevice[] availableCameras;

    async void Start()
    {
        var token = this.GetCancellationTokenOnDestroy();
        // 利用可能なカメラデバイスのリストを取得
        availableCameras = WebCamTexture.devices;
        SelectCamera(selectedCameraIndex);
        
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Q), cancellationToken: token);
        SelectCamera(0);
    }

    // 選択されたカメラを使用する
    public void SelectCamera(int index)
    {
        if (availableCameras.Length > 0 && index < availableCameras.Length)
        {
            string cameraName = availableCameras[index].name;
            Debug.Log(cameraName);
            webCamTexture = new WebCamTexture(cameraName, 1920, 1080, 60);
            Segmentation.instance.SetWebCamTexture(webCamTexture);
            webCamTexture.Play();
        }
    }

}