using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.InputSystem; 

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

    [Header("Inputs (키보드/VR컨트롤러)")]
    public InputActionProperty accelerateInput; // W 키
    public InputActionProperty reverseInput;    // S 키
    
    [Header("Steering Input")]
    public XRKnob steeringKnob;

    [Header("Driving Settings")]
    public float motorForce = 2000f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 5000f;
    public float accelerationRate = 2.0f;

    // === 내부 변수 ===
    private float currentMotorTorque = 0f;
    private Rigidbody rb;
    private bool isTilted = false;

    // ★ 이벤트 제어용 상태 변수 (스위치)
    private bool isEventAccelerating = false; // 외부에서 가속 요청이 있는가?
    private bool isEventReversing = false;    // 외부에서 후진 요청이 있는가?

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void OnEnable()
    {
        if (accelerateInput.action != null) accelerateInput.action.Enable();
        if (reverseInput.action != null) reverseInput.action.Enable();
    }
    
    void OnDisable()
    {
        if (accelerateInput.action != null) accelerateInput.action.Disable();
        if (reverseInput.action != null) reverseInput.action.Disable();
    }

    void FixedUpdate()
    {
        // 1. 키보드/VR컨트롤러 입력값 읽기 (0.0 ~ 1.0)
        float fwdRaw = accelerateInput.action.ReadValue<float>();
        float bwdRaw = reverseInput.action.ReadValue<float>();

        // 2. 이벤트 입력과 합치기 (키보드나 이벤트 둘 중 하나만 들어와도 작동)
        // 키보드를 안 눌러도 isEventAccelerating이 true면 1.0f가 됨
        float finalFwd = (isEventAccelerating || fwdRaw > 0.1f) ? 1.0f : 0f;
        float finalBwd = (isEventReversing || bwdRaw > 0.1f) ? 1.0f : 0f;

        // 아날로그 트리거(살살 누르기)를 지원하려면 아래처럼 Max값 사용
        if (fwdRaw > 0) finalFwd = Mathf.Max(fwdRaw, isEventAccelerating ? 1f : 0f);
        if (bwdRaw > 0) finalBwd = Mathf.Max(bwdRaw, isEventReversing ? 1f : 0f);

        // 디버깅: 입력 합산 결과 확인
        if (finalFwd > 0 || finalBwd > 0) 
            Debug.Log($"Driving.. Fwd: {finalFwd}, Bwd: {finalBwd}");

        CheckForFlipAndStop();

        if (!isTilted)
        {
            HandleMotor(finalFwd, finalBwd);
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
    // ★★★ 외부 이벤트 연결용 함수들 (Inspector에서 연결) ★★★
    // =========================================================
    
    // 버튼을 누를 때 호출 (Pointer Down)
    public void StartAccelerate() 
    { 
        isEventAccelerating = true; 
        Debug.Log("Event: Start Accel");
    }

    // 버튼을 뗄 때 호출 (Pointer Up)
    public void StopAccelerate() 
    { 
        isEventAccelerating = false; 
        Debug.Log("Event: Stop Accel");
    }

    // 후진 버튼 누를 때
    public void StartReverse() 
    { 
        isEventReversing = true; 
    }

    // 후진 버튼 뗄 때
    public void StopReverse() 
    { 
        isEventReversing = false; 
    }
    // =========================================================

    void HandleMotor(float forwardInput, float backwardInput)
    {
        float targetTorque = 0f;
        
        if (forwardInput > 0.1f) // 전진
        {
            targetTorque = forwardInput * motorForce;
            ApplyBrakeForce(0f);
        }
        else if (backwardInput > 0.1f) // 후진
        {
            targetTorque = -backwardInput * motorForce;
            ApplyBrakeForce(0f);
        }
        else // 정지
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