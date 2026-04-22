using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    public void Recoger()
    {
        Flashlight playerFlashlight = FindFirstObjectByType<Flashlight>();
        if (playerFlashlight != null)
            playerFlashlight.DesbloquearLinterna();

        Destroy(gameObject);
    }
}
