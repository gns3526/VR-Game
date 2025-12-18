using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // 네임스페이스 주의

public class SimpleVRGun : MonoBehaviour
{
    [Header("References")]
    [Tooltip("총알이 나갈 위치 (빈 오브젝트)")]
    public Transform firePoint;
    
    [Tooltip("발사할 총알 프리팹")]
    public GameObject bulletPrefab;

    [Tooltip("총 오브젝트에 붙어있는 XRGrabInteractable")]
    public XRGrabInteractable interactable;


    public void FireBullet(ActivateEventArgs args)
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // 총알 생성 및 발사 위치/회전 설정
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            
            // 여기에 발사 사운드나 반동(Haptic) 코드를 추가할 수 있습니다.
            // SendHapticFeedback(args.interactorObject);
        }
    }
    
    //햅틱(진동) 피드백 예시 (선택 사항)
    /*
    private void SendHapticFeedback(IXRInteractor interactor)
    {
        if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(0.5f, 0.1f);
        }
    }
    */
}