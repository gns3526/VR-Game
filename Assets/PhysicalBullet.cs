using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicalBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("총알이 날아가는 속도")]
    public float speed = 20f;
    
    [Tooltip("충돌 시 물체를 밀어내는 힘")]
    public float pushForce = 5f;
    
    [Tooltip("총알의 수명 (초)")]
    public float lifeTime = 3f;

    private Rigidbody rb;
    public GameObject particleObject;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 총알은 중력의 영향을 덜 받거나 안 받게 설정하는 것이 보통 좋습니다.
        rb.useGravity = false; 
    }

    void Start()
    {
        // 시작하자마자 앞쪽으로 날아갑니다.
        // Unity 6 (2023.3+) 부터는 linearVelocity, 이전 버전은 velocity 사용
#if UNITY_2023_3_OR_NEWER
        rb.linearVelocity = transform.forward * speed;
#else
        rb.velocity = transform.forward * speed;
#endif
        // 일정 시간이 지나면 자동 삭제 (성능 최적화)
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 부딪힌 물체의 Rigidbody 가져오기
        Rigidbody targetRb = collision.rigidbody;

        if (targetRb != null)
        {
            // 충돌 지점과 방향 계산
            // ForceMode.Impulse는 순간적인 타격감을 줍니다.
            Vector3 forceDirection = transform.forward; // 총알의 진행 방향으로 밀기
            targetRb.AddForceAtPosition(forceDirection * pushForce, collision.contacts[0].point, ForceMode.Impulse);
        }

        // 총알은 충돌 후 삭제 (이펙트가 있다면 여기서 Instantiate)
        Instantiate(particleObject, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}