using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DeviceManager))]
public class DeviceManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // CameraManagerのターゲットを取得
        DeviceManager cameraManager = (DeviceManager)target;

        // 利用可能なカメラデバイスを取得
        WebCamDevice[] availableCameras = WebCamTexture.devices;

        if (availableCameras.Length > 0)
        {
            // カメラデバイス名のリストを作成
            string[] cameraNames = new string[availableCameras.Length];
            for (int i = 0; i < availableCameras.Length; i++)
            {
                cameraNames[i] = availableCameras[i].name;
            }

            // ドロップダウンリストを表示して選択肢を作成
            cameraManager.selectedCameraIndex = EditorGUILayout.Popup("devices", cameraManager.selectedCameraIndex, cameraNames);

            // カメラが変更されたら選択を更新
            if (GUI.changed)
            {
                cameraManager.SelectCamera(cameraManager.selectedCameraIndex);
            }
        }
        else
        {
            // カメラが見つからない場合のメッセージ
            EditorGUILayout.HelpBox("No camera devices found.", MessageType.Warning);
        }

        // デフォルトのInspectorも描画
        DrawDefaultInspector();
    }
}