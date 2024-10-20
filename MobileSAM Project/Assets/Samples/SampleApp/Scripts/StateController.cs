using System;
using System.Collections.Generic;
using Sample;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;
using UnityEngine.UI;
using Unit = UniRx.Unit;

namespace Dummy.StatePattern
{
    public enum StateType
    {
        SEGMENTATION,
        RETAKE,
        MOVE,
        ROTATE,
        FALL,
    }

    public interface IState
    {
        StateType GetCurrentState { get; }
        void OnStateBegin();
        void OnStateEnd();
        void Update(float deltaTime);
        void SetNextState(IState nextState);
    }
    
    // カウントダウン状態
    
    // セグメンテーション状態
    public class SegmentationState : IState
    {
        private float countdownTime = 3.0f;
        private float displayDuration = 0.0f;
        private IDisposable countdownSubscription;
        private Subject<Unit> countdownCompleteSubject = new Subject<Unit>();
        private IState m_nextState = null;

        public StateType GetCurrentState { get; } = StateType.SEGMENTATION;
        
        public SegmentationState()
        {
            // m_nextState = retakeState;
            countdownCompleteSubject
                .Subscribe(_ =>
                {
                    if (!System.Object.ReferenceEquals(m_nextState, null))
                    {
                        StateController.Instance.SetState(m_nextState);
                    }
                })
                .AddTo(StateController.Instance);
        }
        
        public void OnStateBegin()
        {
            // SegmentationStateのUIを8秒間表示
            StateUIManager.instance.ShowUIForState(GetCurrentState, false, displayDuration);
    
            // セグメンテーションを初期化
            Segmentation.instance.ResetModel();
            Segmentation.instance.output_image.color = Color.white;
            Segmentation.instance.input_image.color = Color.white;
    
            // カメラを起動する処理（100ミリ秒遅延）
            Observable.Timer(TimeSpan.FromMilliseconds(50))
                .Subscribe(_ => Segmentation.instance.StartCam())
                .AddTo(StateController.Instance);

            // 8秒後にカウントダウンを開始
            Observable.Timer(TimeSpan.FromSeconds(displayDuration))
                .Subscribe(_ =>
                {
                    Debug.Log("UI表示が終了し、カウントダウンを開始");

                    // カウントダウンの残り秒数を表示する処理
                    float remainingTime = countdownTime;
                    StateController.Instance.countdownText.text = remainingTime.ToString();

                    // カウントダウンを進行させ、テキストに反映
                    countdownSubscription = Observable
                        .Interval(TimeSpan.FromSeconds(1))  // 1秒ごとにカウントダウンを進行
                        .TakeWhile(_ => remainingTime > 0)  // 残り時間が0になるまでループ
                        .Subscribe(_ =>
                        {
                            remainingTime--;
                            StateController.Instance.countdownText.text = remainingTime.ToString();
                    
                            // 残り時間が0になったら完了を通知
                            if (remainingTime <= 0)
                            {
                                StateController.Instance.countdownText.text = null;
                                Debug.Log("カウントダウン終了");
                                countdownCompleteSubject.OnNext(Unit.Default);  // 完了を通知
                            }
                        });
                })
                .AddTo(StateController.Instance);
        }



        public void OnStateEnd()
        {
            Debug.Log("セグメンテーション終了");
            // 必要に応じて後処理を行う
            Segmentation.instance.StopCam();
            Segmentation.instance.CancelSegmentation();
            displayDuration = 0;
        }

        public void Update(float deltaTime)
        {
           
        }

        public void SetNextState(IState nextState)
        {   
            m_nextState = nextState;
        }
    }

    // 再撮影状態
    public class RetakeState : IState
    {
        private IState m_nextState = null;

        public StateType GetCurrentState { get; } = StateType.RETAKE;

        public void OnStateBegin()
        {
            // 再撮影処理のための関数を呼び出す
            StateUIManager.instance.ShowUIForState(GetCurrentState,true,-1);
        }

        public void OnStateEnd()
        {
        }

        public void Update(float deltaTime)
        {
            // ESCキーでSEGMENTATIONに戻る
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StateController.Instance.SetState(StateController.Instance.segmentationState);
            }

            // ReturnキーでOutput用のRawImageの内容を2Dオブジェクト化する
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("returnキーが呼ばれたよ");
                // Output用のRawImageからセグメンテーションされた画像を取得
                StateController.Instance.lastDroppedGameObject = ObjectManager.Instance.Create2DObjectFromSegmentation(Segmentation.instance.output_image.texture);
                StateController.Instance.SetState(m_nextState);
            }
        }

        public void SetNextState(IState nextState)
        {
            m_nextState = nextState;
        }
    }

    // 移動状態
    public class MoveState : IState
    {
        private Transform targetObject;
        private IState m_nextState = null;
        private float moveSpeed = 10f;

        public StateType GetCurrentState { get; } = StateType.MOVE;

        public MoveState()
        {
            
        }

        public void OnStateBegin()
        {
            Debug.Log("オブジェクト移動開始");
            Segmentation.instance.output_image.color = Color.clear;
            Segmentation.instance.input_image.color = Color.clear;
            
            StateUIManager.instance.ShowUIForState(GetCurrentState,true,-1);
        }

        public void OnStateEnd()
        {
            
        }

        public void Update(float deltaTime)
        {
            float moveInput = Input.GetAxis("Horizontal");
            ObjectManager.Instance.MoveObject(StateController.Instance.lastDroppedGameObject,moveInput * moveSpeed);

            // Returnキーで次のステートに移行
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 次のステートに移行
                if (!System.Object.ReferenceEquals(m_nextState, null))
                {
                    StateController.Instance.SetState(m_nextState);
                }
            }
        }

        public void SetNextState(IState nextState)
        {
            m_nextState = nextState;
        }
    }

    // 回転状態
    public class RotateState : IState
    {
        private Transform targetObject;
        private IState m_nextState = null;
        

        public StateType GetCurrentState { get; } = StateType.ROTATE;

        public RotateState()
        {
            
        }

        public void OnStateBegin()
        {
            Debug.Log("オブジェクト回転開始");
            StateUIManager.instance.ShowUIForState(GetCurrentState,true,-1);
        }

        public void OnStateEnd()
        {
            StateUIManager.instance.HideAllUI();
        }

        public void Update(float deltaTime)
        {
            float rotateAmount = 1.0f; // 回転量を調整

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                ObjectManager.Instance.RotateObject(StateController.Instance.lastDroppedGameObject,50f,true);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                ObjectManager.Instance.RotateObject(StateController.Instance.lastDroppedGameObject,50f,false);
            }

            // Returnキーで次のステートに移行
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 次のステートに移行
                if (!System.Object.ReferenceEquals(m_nextState, null))
                {
                    StateController.Instance.SetState(m_nextState);
                }
            }
        }

        public void SetNextState(IState nextState)
        {
            m_nextState = nextState;
        }
    }

    // 落下状態
    public class FallState : IState
    {
        private IState m_nextState = null;
        public StateType GetCurrentState { get; } = StateType.FALL;
        public bool isStable;

        public void OnStateBegin()
        {
            Debug.Log("オブジェクト落下開始");
            ObjectManager.Instance.ApplyGravity(StateController.Instance.lastDroppedGameObject);
        }

        public void OnStateEnd()
        {
            // 最後においたオブジェクトのy座標とスポーン地点との比較 -> カメラの座標の変更
            ObjectManager.Instance.CameraTransformUpdate();
        }

        public void Update(float deltaTime)
        {
            if (Judgment.Instance.IsStable())
            {
                StateController.Instance.SetState(m_nextState);
            }
        }

        public void SetNextState(IState nextState)
        {
            m_nextState = nextState;
        }
    }

    public class StateController : MonoBehaviour
    {
        public static StateController Instance { get; private set; }
        public ReactiveProperty<IState> currentState = new ReactiveProperty<IState>();
        [HideInInspector] public GameObject lastDroppedGameObject;
        
        public SegmentationState segmentationState;
        private RetakeState retakeState;
        private MoveState moveState;
        private RotateState rotateState;
        private FallState fallState;
        
        // UIマネージャーをインスペクターからアタッチ
        [SerializeField]
        private StateUIManager stateUIManager;

        [SerializeField] public Text countdownText;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // 各状態を初期化
            segmentationState = new SegmentationState();
            retakeState = new RetakeState();
            moveState = new MoveState();
            rotateState = new RotateState();
            fallState = new FallState();

            // 状態遷移を設定
            segmentationState.SetNextState(retakeState);
            retakeState.SetNextState(moveState);
            moveState.SetNextState(rotateState);
            rotateState.SetNextState(fallState);
            fallState.SetNextState(segmentationState); // ループ状態

            // 初期状態に設定
            SetState(segmentationState);

            // 状態が変わったときにOnStateBeginを呼び出す
            currentState
                .Where(state => !System.Object.ReferenceEquals(state, null))
                .Subscribe(state =>
                {
                    state.OnStateBegin();
                })
                .AddTo(this);
        }

        private void Update()
        {
            // 現在の状態のUpdateメソッドを実行
            if (!System.Object.ReferenceEquals(currentState.Value, null))
            {
                currentState.Value.Update(Time.deltaTime);
            }
        }

        public void SetState(IState newState)
        {
            if (!System.Object.ReferenceEquals(currentState.Value, null))
            {
                currentState.Value.OnStateEnd();
            }
            currentState.Value = newState;
        }
    }
}