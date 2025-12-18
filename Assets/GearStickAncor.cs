using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GearStickAnchor : MonoBehaviour
{
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        // 게임 시작 시, 차에 붙어있는 원래 위치(Local Position)를 기억합니다.
        initialLocalPos = transform.localPosition;
    }

    void OnEnable()
    {
        // 잡았을 때 부모가 해제되는 것을 방지하기 위해 이벤트를 등록할 수도 있지만,
        // LateUpdate에서 강제로 위치를 잡는 것이 더 확실합니다.
    }

    // XR Interaction Toolkit의 처리가 끝난 뒤(LateUpdate)에 위치를 덮어씌웁니다.
    void LateUpdate()
    {
        // 잡혀있든 아니든, 기어봉의 뿌리 위치는 항상 부모(차) 기준 제자리여야 합니다.
        // (회전은 손을 따라가야 하므로 건드리지 않고, 위치만 고정합니다)
        transform.localPosition = initialLocalPos;
    }
}