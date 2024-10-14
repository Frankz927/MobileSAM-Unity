using System;
using Sample;
using UnityEngine;
using UniRx;

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
        private float countdownTime = 3.0f; // 3秒のカウントダウン
        private IDisposable countdownSubscription;
        private Subject<Unit> countdownCompleteSubject = new Subject<Unit>();
        private IState m_nextState = null;

        public StateType GetCurrentState { get; } = StateType.SEGMENTATION;
        
        public SegmentationState()
        {
            countdownCompleteSubject
                .Subscribe(_ =>
                {
                    if (m_nextState != null)
                    {
                        StateController.Instance.SetState(m_nextState);
                    }
                })
                .AddTo(StateController.Instance);
        }
        
        public void OnStateBegin()
        {
            // ここで少し遅延を持たせてカメラを起動
            Observable.Timer(TimeSpan.FromMilliseconds(50))
                .Subscribe(_ => Segmentation.instance.StartCam())
                .AddTo(StateController.Instance);
            
            // カウントダウンを開始し、3秒後にSubjectで完了を通知
            countdownSubscription = Observable
                .Timer(TimeSpan.FromSeconds(countdownTime))
                .Subscribe(_ =>
                {
                    Debug.Log("カウントダウン終了");
                    Segmentation.instance.StopCam(); // カメラを停止
                    countdownCompleteSubject.OnNext(Unit.Default); // 完了を通知
                });
        }

        public void OnStateEnd()
        {
            Debug.Log("セグメンテーション終了");
            // 必要に応じて後処理を行う
            Segmentation.instance.StopCam(); // Webカメラを停止
        }

        public void Update(float deltaTime)
        {
            // ReturnキーでRETAKEまたは次のステートに移行
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 次のステートに移行
                if (m_nextState != null)
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

    // 再撮影状態
    public class RetakeState : IState
    {
        private IState m_nextState = null;

        public StateType GetCurrentState { get; } = StateType.RETAKE;

        public void OnStateBegin()
        {
            Debug.Log("再撮影状態開始");
            // 再撮影処理のための関数を呼び出す
        }

        public void OnStateEnd()
        {
            Debug.Log("再撮影状態終了");
        }

        public void Update(float deltaTime)
        {
            // Returnキーで次のステートに移行
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 次のステートに移行
                if (m_nextState != null)
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

    // 移動状態
    public class MoveState : IState
    {
        private Transform targetObject;
        private IState m_nextState = null;

        public StateType GetCurrentState { get; } = StateType.MOVE;

        public MoveState(Transform obj)
        {
            targetObject = obj;
        }

        public void OnStateBegin()
        {
            Debug.Log("オブジェクト移動開始");
        }

        public void OnStateEnd() {}

        public void Update(float deltaTime)
        {
            float moveAmount = 0.1f; // 移動量を調整

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                targetObject.position += new Vector3(-moveAmount, 0, 0);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                targetObject.position += new Vector3(moveAmount, 0, 0);
            }

            // Returnキーで次のステートに移行
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 次のステートに移行
                if (m_nextState != null)
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

        public RotateState(Transform obj)
        {
            targetObject = obj;
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
                targetObject.Rotate(0, -rotateAmount, 0);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                targetObject.Rotate(0, rotateAmount, 0);
            }

            // Returnキーで次のステートに移行
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 次のステートに移行
                if (m_nextState != null)
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

        public void OnStateBegin()
        {
            Debug.Log("オブジェクト落下開始");
            // Rigidbodyと当たり判定の設定
            // Segmentation.cs内の関数を呼び出す
        }

        public void OnStateEnd() {}

        public void Update(float deltaTime)
        {
            // ここでゲームオーバーの判定を行う
            // if (GameObjectのy座標 < -3) { GameOver処理を実行 }

            // すべてのオブジェクトが動かない場合、次のSEGMENTATIONに戻る
            // if (全てのオブジェクトが動かない) { StateController.Instance.SetState(new SegmentationState()); }
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
        public GameObject targetObject; // 移動・回転させるオブジェクト

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // 各状態を初期化
            var segmentationState = new SegmentationState();
            var retakeState = new RetakeState();
            var moveState = new MoveState(targetObject.transform);
            var rotateState = new RotateState(targetObject.transform);
            var fallState = new FallState();

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
                .Where(state => state != null)
                .Subscribe(state =>
                {
                    state.OnStateBegin();
                })
                .AddTo(this);
        }

        private void Update()
        {
            // 現在の状態のUpdateメソッドを実行
            if (currentState.Value != null)
            {
                currentState.Value.Update(Time.deltaTime);
            }
        }

        public void SetState(IState newState)
        {
            if (currentState.Value != null)
            {
                currentState.Value.OnStateEnd();
            }
            currentState.Value = newState;
        }
    }
}