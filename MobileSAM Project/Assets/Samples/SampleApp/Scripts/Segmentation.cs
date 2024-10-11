using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Sentis;
using HoloLab.DNN.Segmentation;

namespace Sample
{
    public class Segmentation : MonoBehaviour
    {
        [SerializeField] private RawImage input_image;  // Webカメラの入力を表示するRawImage
        [SerializeField] private RawImage output_image; // セグメンテーション結果を表示するRawImage
        [SerializeField] private ModelAsset encoder_asset;  // MobileSAMのエンコーダーモデル
        [SerializeField] private ModelAsset decoder_asset;  // MobileSAMのデコーダーモデル
        [SerializeField, Range(0.0f, 1.0f)] private float alpha = 0.5f;  // セグメンテーションの透過度
        [SerializeField, Range(0.05f, 1.0f)] private float segmentationInterval = 0.1f;  // セグメンテーション実行間隔

        private SegmentationModel_MobileSAM model = null;
        private Selector selector = null;
        private List<Color> colors;
        private Coroutine segmentationCoroutine;
        
        WebCamTexture _webcamTexture;

        public bool isWebCamOn;

        private void Start()
        {
            // MobileSAMモデルの初期化
            model = new SegmentationModel_MobileSAM(encoder_asset, decoder_asset, BackendType.GPUCompute);

            // セグメンテーション結果の描画に使用する色の設定
            colors = new List<Color>() { Color.clear, new Color(1.0f, 0.0f, 0.0f, alpha) };

            if (isWebCamOn)
            {
                WebCamDevice[] devices = WebCamTexture.devices;
                this.input_image = input_image.GetComponent<RawImage>();
                _webcamTexture = new WebCamTexture(devices[0].name,1920, 1080, 30);
                this.input_image.texture = _webcamTexture;
                this.input_image.enabled = true;
                _webcamTexture.Play();
            }

            // 選択ツール (Selector) の初期化
            var rect_transform = input_image.GetComponent<RectTransform>();
            

            var width = input_image.texture.width;
            var height = input_image.texture.height;
            selector = new Selector(rect_transform, width, height);

            // ポイントおよび矩形選択時のイベントハンドラを割り当てる
            selector.OnPointSelected += OnPointSelect;

            // 定期的にセグメンテーションを実行するコルーチンを開始
            segmentationCoroutine = StartCoroutine(SegmentAtCenterWithInterval());
        }

        private void Update()
        {
            // 選択ツールの更新
            selector?.Update();
        }

        /// <summary>
        /// セグメンテーション関数を実行間隔を指定するコルーチン
        /// </summary>
        private IEnumerator SegmentAtCenterWithInterval()
        {
            while (true)
            {
                yield return new WaitForSeconds(segmentationInterval);  // セグメンテーションの実行間隔を待機

                if (input_image.texture != null)
                {
                    StartCoroutine(SegmentAtCenter());  // カメラ中心に対するセグメンテーションを実行
                }
                Debug.Log("間隔を空けて呼び出しているよ");
            }
        }

        /// <summary>
        /// カメラの中心点を指定してセグメンテーション処理の呼び出し
        /// </summary>
        private IEnumerator SegmentAtCenter()
        {
            var inputTexture = input_image.texture as Texture2D;
            if (inputTexture == null)
            {
                yield break;
            }

            // カメラの中心点を指定
            Vector2 centerPoint = new Vector2(inputTexture.width / 2, inputTexture.height / 2);

            // セグメンテーション処理の実行
            Texture2D indices_texture = null;
            yield return StartCoroutine(model.Segment(inputTexture, centerPoint, (output) => indices_texture = output));

            if (indices_texture != null)
            {
                // セグメンテーション結果を表示する
                DisplaySegmentationResult(indices_texture);
            }
            Debug.Log("カメラの真ん中を指定してセグメンテーション関数を呼び出しているよ-SegmentAtCenter()");
        }

        /// <summary>
        /// ポイント選択時に呼び出されるイベント
        /// </summary>
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
            Debug.Log("セグメンテーション関数が呼び出されているよ-Segment(PointEventArgs e))");
        }

        /// <summary>
        /// セグメンテーション結果を表示する
        /// </summary>
        private void DisplaySegmentationResult(Texture2D indices_texture)
        {
            var colorized_texture = Visualizer.ColorizeArea(indices_texture, colors);
            if (output_image.texture == null)
            {
                output_image.color = Color.white;
                output_image.texture = new Texture2D(indices_texture.width, indices_texture.height, TextureFormat.RGBA32, false);
            }
            Graphics.CopyTexture(colorized_texture, output_image.texture);

            // 不要なテクスチャを破棄
            Destroy(colorized_texture);
            Destroy(indices_texture);
            // Debug.Log("セグメンテーション関数が呼び出されてOutput用のRawImageに反映されているよ-DisplaySegmentationResult(Texture2D indices_texture)");
        }

        private void OnDestroy()
        {
            // リソースの解放
            model?.Dispose();
            model = null;

            selector?.Dispose();
            selector = null;

            // コルーチンの停止
            if (segmentationCoroutine != null)
            {
                StopCoroutine(segmentationCoroutine);
            }
        }
    }
}