using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    public void Recoger()
    {
        Flashlight.DesbloquearTodas();

        Destroy(gameObject);
    }
}
