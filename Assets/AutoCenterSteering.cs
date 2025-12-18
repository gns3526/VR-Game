using System.Collections;
using System.Collections.Generic;
using Unity.VRTemplate;
using UnityEngine;

public class AutoCenterSteering : MonoBehaviour
{
    [Header("Settings")]
    public float returnSpeed = 2.0f; // 돌아오는 속도 (낮을수록 느림, 높을수록 빠름)
    
    // XR Knob 컴포넌트 참조
    private XRKnob knob;

    void Awake()
    {
        knob = GetComponent<XRKnob>();
    }

    void Update()
    {
        // 1. XR Knob이 없으면 아무것도 안 함
        if (knob == null) return;

        // 2. 사용자가 잡고 있는지 확인 (isSelected)
        // 잡고 있다면(True) 사용자가 돌리게 냅두고, 놓았으면(False) 중앙으로 복귀
        if (!knob.isSelected)
        {
            // 3. 현재 값에서 0.5(중앙)로 부드럽게 이동 (Lerp)
            // Time.deltaTime * returnSpeed 값을 조절하면 0.5의 느낌을 낼 수 있습니다.
            knob.value = Mathf.Lerp(knob.value, 0.5f, Time.deltaTime * returnSpeed);
        }
    }
}
