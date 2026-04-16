using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    public void Recoger()
    {
        Flashlight playerFlashlight = FindObjectOfType<Flashlight>();
        if (playerFlashlight != null)
            playerFlashlight.DesbloquearLinterna();

        Destroy(gameObject);
    }
}
