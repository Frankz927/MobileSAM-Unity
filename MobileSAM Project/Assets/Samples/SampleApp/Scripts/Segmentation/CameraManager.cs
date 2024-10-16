using System.Collections.Generic;
using Sample;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // カメラの名前リストを保持する変数
    public int selectedCameraIndex = 0;
    private WebCamTexture webCamTexture;
    private WebCamDevice[] availableCameras;

    void Start()
    {
        // 利用可能なカメラデバイスのリストを取得
        availableCameras = WebCamTexture.devices;
        SelectCamera(selectedCameraIndex);
    }

    // 選択されたカメラを使用する
    public void SelectCamera(int index)
    {
        if (availableCameras.Length > 0 && index < availableCameras.Length)
        {
            string cameraName = availableCameras[index].name;
            webCamTexture = new WebCamTexture(cameraName, 1920, 1080, 60);
            Segmentation.instance.SetWebCamTexture(webCamTexture);
            webCamTexture.Play();
        }
    }

}