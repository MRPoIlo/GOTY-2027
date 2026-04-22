using UnityEngine;

public class ScrewdriverPickup : MonoBehaviour
{
    public void Recoger()
    {
        // Activa el flag en GameManager2
        GameManager2.Instance.tieneDestornillador = true;

        // Destruye el objeto recogido
        Destroy(gameObject);
    }
}
