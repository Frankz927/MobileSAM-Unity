// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Serialization;
// using UnityEngine.UI;
//
// public class ImageSource : MonoBehaviour
// {
//     [SerializeField] private int m_width　= 1920;
//     [SerializeField] private int m_height　= 1080;
//     [SerializeField] private RawImage m_displayUI = null;
//     private Texture2D camTexture2D;
//     [HideInInspector] public WebCamTexture　_webCamTexture;
//     Color32[] _colors;
//
//     public static ImageSource instance;
//
//     private void Awake()
//     {
//         if (instance == null)
//         {
//             instance = this;
//         }
//     }
//
//     IEnumerator Init()
//     {
//         while (true)
//         {
//             if (_webCamTexture.width > 16 && _webCamTexture.height > 16)
//             {
//                 _colors = new Color32[_webCamTexture.width * _webCamTexture.height];
//                 camTexture2D = new Texture2D (_webCamTexture.width, _webCamTexture.height, TextureFormat.RGBA32, false);
//                 m_displayUI.GetComponent<Renderer>().material.mainTexture = camTexture2D;
//                 break;
//             }
//             yield return null;
//         }
//     }
//     
//     // Start is called before the first frame update
//     private IEnumerator　Start()
//     {
//         if( WebCamTexture.devices.Length == 0 )
//         {
//             Debug.LogFormat( "カメラのデバイスが無い様だ。撮影は諦めよう。" );
//             yield break;
//         }
//
//         yield return Application.RequestUserAuthorization( UserAuthorization.WebCam );
//         if( !Application.HasUserAuthorization( UserAuthorization.WebCam ) )
//         {
//             Debug.LogFormat( "カメラを使うことが許可されていないようだ。市役所に届けでてくれ！" );
//             yield break;
//         }
//
//         // とりあえず最初に取得されたデバイスを使ってテクスチャを作りますよ。
//         WebCamDevice userCameraDevice = WebCamTexture.devices[ 0 ];
//         _webCamTexture = new WebCamTexture( userCameraDevice.name, m_width, m_height);
//
//         m_displayUI.texture = _webCamTexture;
//
//         // さあ、撮影開始だ！
//         _webCamTexture.Play();
//         
//         while (_webCamTexture.width < 100) {
//             yield return null;
//         }
//         
//         StartCoroutine(Init());
//         
//     }
//     
//     private void OnApplicationQuit() 
//     {
//         _webCamTexture.Stop();
//         Debug.Log("Camera is stop...");
//     }
//     
//     public Texture2D GetTexture2dFromWebcam(WebCamTexture webCamTexture)
//     {
//         camTexture2D = new Texture2D(webCamTexture.width, webCamTexture.height);
//         camTexture2D.SetPixels32(webCamTexture.GetPixels32());
//         camTexture2D.Apply();
//
//         Resources.UnloadUnusedAssets();
//         
//         return camTexture2D;
//     }
//     
// }
