using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // XRBaseInteractor 사용을 위해 추가

public class VRWheelHandle : MonoBehaviour
{
    [Header("Linkages")]
    public VRCarController carController; // 메인 차량 제어 스크립트 연결
    public Transform handleVisual; // 핸들 3D 모델의 Transform (보통 this.transform)

    [Header("Rotation Limits")]
    public float maxHandleRotation = 180f; // 최대 조향 각도 (예: 180)
    
    private XRGrabInteractable grabInteractable;
    private Quaternion initialLocalRotation;
    private Quaternion initialControllerRotation; 
    private bool isGrabbed = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component is missing on the handle object.");
        }
        
        if (handleVisual == null)
        {
            handleVisual = this.transform;
        }
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        initialLocalRotation = handleVisual.localRotation; 
        
        // 최신 API 사용
        XRBaseInteractor interactor = args.interactorObject as XRBaseInteractor;
        if (interactor != null)
        {
            initialControllerRotation = interactor.transform.rotation;
        }
        else
        {
            initialControllerRotation = Quaternion.identity;
        }
        Debug.Log("[VRWheelHandle] Grabbed! Initial Rotations set.");
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        if (carController != null)
        {
        //     carController.SetSteerInput(0f); // 중앙 복귀
        }
        // 핸들 모델의 회전을 즉시 중앙으로 리셋 (진동 방지)
        handleVisual.localRotation = Quaternion.identity; 
        Debug.Log("[VRWheelHandle] Released! Steer Input Reset to 0.");
    }

    void Update()
    {
        if (isGrabbed && carController != null)
        {
            // 최신 API 사용
            XRBaseInteractor interactor = grabInteractable.GetOldestInteractorSelecting() as XRBaseInteractor;
            
            if (interactor != null)
            {
                Quaternion currentControllerRotation = interactor.transform.rotation;
                Quaternion deltaRotation = currentControllerRotation * Quaternion.Inverse(initialControllerRotation);

                // Y축 변화량만 추출 (스티어링 휠의 조향 축이라고 가정)
                float deltaAngleY = deltaRotation.eulerAngles.y;
                
                Quaternion targetLocalRotation = initialLocalRotation * Quaternion.Euler(0, deltaAngleY, 0);

                // 회전 제한 적용
                float finalY = targetLocalRotation.eulerAngles.y;
                
                if (finalY > 180f) {
                    if (finalY > (360f - maxHandleRotation)) {
                         finalY = 360f - maxHandleRotation; 
                    }
                } else {
                    if (finalY > maxHandleRotation) {
                        finalY = maxHandleRotation;
                    }
                }
                
                // F. 핸들 모델에 최종 회전 적용
                handleVisual.localRotation = Quaternion.Euler(handleVisual.localEulerAngles.x, finalY, handleVisual.localEulerAngles.z);

                // G. 최종 핸들 각도를 읽어 차량 제어 입력으로 변환
                float finalAngleForSteer = finalY;
                if (finalAngleForSteer > 180f) finalAngleForSteer -= 360f;
                
                float steerValue = finalAngleForSteer / maxHandleRotation;
                
               // carController.SetSteerInput(steerValue);
            }
            else
            {
                // 예외 처리: 잡고 있는데 Interactor가 사라진 경우 (비정상 상황)
                isGrabbed = false;
               // carController.SetSteerInput(0f);
            }
        }
    }
}