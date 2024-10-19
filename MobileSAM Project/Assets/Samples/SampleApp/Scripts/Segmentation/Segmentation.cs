using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Unity.Sentis;
using HoloLab.DNN.Segmentation;
using Cysharp.Threading.Tasks;
using UnityEngine.Profiling;


namespace Sample
{
    public class Segmentation : MonoBehaviour
    {
        [SerializeField] public RawImage input_image;  
        [SerializeField] public RawImage output_image; 
        [SerializeField] private ModelAsset encoder_asset;  
        [SerializeField] private ModelAsset decoder_asset;  
        [SerializeField, Range(0.0f, 1.0f)] private float alpha = 0.5f;  
        [SerializeField, Range(0.05f, 1.0f)] private float segmentationInterval = 0.1f;  

        private SegmentationModel_MobileSAM model = null;
        private Selector selector = null;
        private List<Color> colors;
        private Coroutine segmentationCoroutine;
        
        public WebCamTexture _webcamTexture;
        public static Segmentation instance;
        private CancellationTokenSource cancellationTokenSource;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            
            if (model != null)
            {
                model.Dispose();
                model = null;
            }
        }

        private void Start()
        {
            model = new SegmentationModel_MobileSAM(encoder_asset, decoder_asset, BackendType.GPUCompute);
            colors = new List<Color>() { Color.clear, new Color(1.0f, 0.0f, 0.0f, alpha) };
            Application.quitting += ResetModel;
        }
        
        public void SetWebCamTexture(WebCamTexture webcamTexture)
        {
            if (_webcamTexture != null && _webcamTexture.isPlaying)
            {
                _webcamTexture.Stop();
            }

            _webcamTexture = webcamTexture;
            input_image.texture = _webcamTexture;
            input_image.enabled = true;
        }

        public void StartCam()
        {
            _webcamTexture.Play();
            StartCoroutine(SegmentAtCenterWithInterval());
        }

        public void StopCam()
        {
            _webcamTexture.Pause(); // カメラを停止
            if (segmentationCoroutine != null)
            {
                StopCoroutine(segmentationCoroutine); // コルーチンを停止
            }
        }
        
        public void ResetModel()
        {
            model?.Dispose(); // モデルを一度破棄してリセット
            model = new SegmentationModel_MobileSAM(encoder_asset, decoder_asset, BackendType.GPUCompute);
        }
        
        public void CancelSegmentation()
        {
            cancellationTokenSource?.Cancel();
        }

        public IEnumerator SegmentAtCenterWithInterval()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
        
            while (!token.IsCancellationRequested)
            {
                yield return new WaitForSeconds(segmentationInterval);
            
                if (input_image.texture != null)
                {
                    StartCoroutine(SegmentAtCenter());
                }
            }
        }

        private IEnumerator SegmentAtCenter()
        {
            Texture tex = this.input_image.texture;
            int w = tex.width;
            int h = tex.height;

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture rt = new RenderTexture(w, h, 32);

            Graphics.Blit(tex, rt);
            RenderTexture.active = rt;

            Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            result.Apply();
            RenderTexture.active = currentRT;
            
            var inputTexture = result;
            if (inputTexture == null)
            {
                yield break;
            }

            Vector2 centerPoint = new Vector2(inputTexture.width / 2, inputTexture.height / 2);
            Texture2D indices_texture = null;
            
            yield return StartCoroutine(model.Segment(inputTexture, centerPoint, (output) => indices_texture = output));
            
            Destroy(result);

            if (indices_texture != null)
            {
                DisplaySegmentationResult(indices_texture);
            }
        }

        public void OnPointSelect(object sender, PointEventArgs e)
        {
            StartCoroutine(Segment(e));
        }

        private IEnumerator Segment(PointEventArgs e)
        {
            var inputTexture = input_image.texture as Texture2D;
            if (inputTexture == null)
            {
                yield break;
            }

            Texture2D indices_texture = null;
            yield return StartCoroutine(model.Segment(inputTexture, e.point, (output) => indices_texture = output));

            if (indices_texture != null)
            {
                DisplaySegmentationResult(indices_texture);
            }
        }

        private void DisplaySegmentationResult(Texture2D indices_texture)
        {
            
            var colorized_texture = Visualizer.ColorizeArea(indices_texture, colors);
            
            // RenderTextureを使用して出力サイズを一貫させる
            RenderTexture rt = new RenderTexture(indices_texture.width, indices_texture.height, 32);
            Graphics.Blit(colorized_texture, rt);

            // RenderTextureからTexture2Dに変換して出力に使用
            Texture2D outputTex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            outputTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            outputTex.Apply();

            output_image.texture = outputTex;
            
            // Cleanup
            RenderTexture.active = null;
            rt.Release();
            Destroy(colorized_texture);
            Destroy(indices_texture);
        }

        private void OnDestroy()
        {
            model?.Dispose();
            model = null;

            selector?.Dispose();
            selector = null;
            
        }

        public void initializeCamera()
        {
            
        }
    }
}