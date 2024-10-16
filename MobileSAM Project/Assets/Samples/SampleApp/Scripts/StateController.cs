using System;
using Sample;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;
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
        private float countdownTime = 5.0f;
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
            // セグメンテーションを初期化
            Segmentation.instance.ResetModel();
            
            Segmentation.instance.output_image.color = Color.white;
            Segmentation.instance.input_image.color = Color.white;
            
            // ここで少し遅延を持たせてカメラを起動
            Observable.Timer(TimeSpan.FromMilliseconds(100))
                .Subscribe(_ => Segmentation.instance.StartCam())
                .AddTo(StateController.Instance);
            
            // カウントダウンを開始し、3秒後にSubjectで完了を通知
            countdownSubscription = Observable
                .Timer(TimeSpan.FromSeconds(countdownTime))
                .Subscribe(_ =>
                {
                    Debug.Log("カウントダウン終了");
                    countdownCompleteSubject.OnNext(Unit.Default); // 完了を通知
                });
        }

        public void OnStateEnd()
        {
            Debug.Log("セグメンテーション終了");
            // 必要に応じて後処理を行う
            Segmentation.instance.StopCam();
            Segmentation.instance.CancelSegmentation();
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
                // Output用のRawImageからセグメンテーションされた画像を取得
                StateController.Instance.lastDroppedGameObject = BlockingPhotos.Create2DObjectFromSegmentation(Segmentation.instance.output_image.texture);
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
        private float moveSpeed = 5f;

        public StateType GetCurrentState { get; } = StateType.MOVE;

        public MoveState()
        {
            
        }

        public void OnStateBegin()
        {
            Debug.Log("オブジェクト移動開始");
            Segmentation.instance.output_image.color = Color.clear;
            Segmentation.instance.input_image.color = Color.clear;
        }

        public void OnStateEnd() {}

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
        }

        public void OnStateEnd() {}

        public void Update(float deltaTime)
        {
            float rotateAmount = 1.0f; // 回転量を調整

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                ObjectManager.Instance.RotateObject(StateController.Instance.lastDroppedGameObject,30f,true);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                ObjectManager.Instance.RotateObject(StateController.Instance.lastDroppedGameObject,30f,false);
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
            Judgment.Instance.StartMonitoring();
        }

        public void OnStateEnd()
        {
            // 最後においたオブジェクトのy座標とスポーン地点との比較 -> カメラの座標の変更
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