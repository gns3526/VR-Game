using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // ★ TextMeshPro 사용을 위해 필수

public class VRCarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    [Header("Wheel Visuals")]
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    [Header("Inputs")]
    public InputActionProperty throttleInput; // 액셀 페달 (W키)
    
    [Header("Steering & Gear UI")]
    public XRKnob steeringKnob;
    public TextMeshPro gearText; // ★ UI가 아닌 3D Object의 TextMeshPro 컴포넌트

    [Header("Driving Settings")]
    public float motorForce = 2000f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 5000f;
    public float accelerationRate = 2.0f;

    // === 상태 변수 ===
    private float currentMotorTorque = 0f;
    private Rigidbody rb;
    private bool isTilted = false;

    // 기어 상태 (False: 전진 D / True: 후진 R)
    private bool isReverseGear = false; 

    // 액셀 페달 상태
    private bool isThrottlePressed = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        // 시작할 때 기어 표시 초기화 (D)
        UpdateGearDisplay();
    }

    void OnEnable() { throttleInput.action?.Enable(); }
    void OnDisable() { throttleInput.action?.Disable(); }

    void FixedUpdate()
    {
        float inputVal = throttleInput.action.ReadValue<float>();
        
        // 외부 이벤트(isThrottlePressed) 또는 키보드 입력 체크
        float finalThrottle = (isThrottlePressed || inputVal > 0.1f) ? 1.0f : 0f;

        // 아날로그 입력 대응
        if (inputVal > 0) finalThrottle = Mathf.Max(inputVal, isThrottlePressed ? 1f : 0f);

        CheckForFlipAndStop();

        if (!isTilted)
        {
            HandleMotor(finalThrottle);
            HandleSteering(); 
        }
        else
        {
            ApplyMotorTorque(0f);
            ApplyBrakeForce(0f);
        }
        UpdateWheels();
    }

    // =========================================================
    // ★★★ 액셀 페달 이벤트 (누르면 가속, 떼면 멈춤) ★★★
    // =========================================================
    public void StartAccelerate() 
    { 
        isThrottlePressed = true; 
    }

    public void StopAccelerate() 
    { 
        isThrottlePressed = false; 
    }

    // =========================================================
    // ★★★ 기어 변속 (이벤트 연결용) ★★★
    // =========================================================
    
    // ★ 이 함수를 기어 버튼(Button/Lever)의 이벤트에 연결하세요.
    // 누를 때마다 D -> R -> D -> R 순서로 바뀝니다.
    public void ToggleGear()
    {
        isReverseGear = !isReverseGear; // 상태 반전 (True <-> False)
        UpdateGearDisplay();            // 텍스트 변경
        
        Debug.Log($"Gear Changed: {(isReverseGear ? "R (Reverse)" : "D (Drive)")}");
    }

    // 내부적으로 텍스트를 업데이트하는 함수
    public void UpdateGearDisplay()
    {
        if (gearText != null)
        {
            if (isReverseGear)
            {
                gearText.text = "R";
                gearText.color = Color.red; // 후진은 빨간색 (원하면 변경 가능)
            }
            else
            {
                gearText.text = "D";
                gearText.color = Color.green; // 전진은 초록색
            }
        }
    }

    // =========================================================

    void HandleMotor(float throttleInput)
    {
        float targetTorque = 0f;
        
        if (throttleInput > 0.1f) // 액셀 밟음
        {
            if (isReverseGear)
            {
                // 후진 기어 상태면 뒤로 힘을 줌
                targetTorque = -throttleInput * motorForce;
            }
            else
            {
                // 전진 기어 상태면 앞으로 힘을 줌
                targetTorque = throttleInput * motorForce;
            }
            ApplyBrakeForce(0f);
        }
        else // 액셀 뗌
        {
            targetTorque = 0f;
            ApplyBrakeForce(brakeForce * 0.5f);
        }

        currentMotorTorque = Mathf.Lerp(currentMotorTorque, targetTorque, Time.fixedDeltaTime * accelerationRate * 5f);
        ApplyMotorTorque(currentMotorTorque);
    }

    void HandleSteering()
    {
        float currentSteerAngle = 0f;
        if (steeringKnob != null)
        {
            float knobValue = steeringKnob.value; 
            float normalizedKnobValue = (knobValue * 2.0f) - 1.0f;
            currentSteerAngle = Mathf.Clamp(normalizedKnobValue, -1f, 1f) * maxSteerAngle;
        }
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    void ApplyMotorTorque(float torque)
    {
        frontLeftWheelCollider.motorTorque = torque;
        frontRightWheelCollider.motorTorque = torque;
    }

    void ApplyBrakeForce(float force)
    {
        frontLeftWheelCollider.brakeTorque = force;
        frontRightWheelCollider.brakeTorque = force;
        rearLeftWheelCollider.brakeTorque = force;
        rearRightWheelCollider.brakeTorque = force;
    }

    void CheckForFlipAndStop()
    {
        if (Vector3.Dot(transform.up, Vector3.down) > 0) isTilted = true;
        else isTilted = false;
    }

    void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    void UpdateSingleWheel(WheelCollider collider, Transform visualWheel)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        visualWheel.position = pos;
        visualWheel.rotation = rot; 
    }
}