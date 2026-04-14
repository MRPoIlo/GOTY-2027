using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public Light LuzLinterna;

    private bool linternaBloqueada = true;

    void Start()
    {
        if (LuzLinterna != null)
            LuzLinterna.enabled = false;
    }

    void Update()
    {
        if (linternaBloqueada) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            LuzLinterna.enabled = !LuzLinterna.enabled;
        }
    }

    /// <summary>
    /// Llamar desde el ObjetoInteractuable del Flashlight (OnInteractuado).
    /// Desbloquea la linterna y destruye el objeto de la escena.
    /// </summary>
    public void RecogerLinterna(GameObject objetoFlashlight)
    {
        linternaBloqueada = false;

        if (LuzLinterna != null)
            LuzLinterna.enabled = true;

        if (objetoFlashlight != null)
            Destroy(objetoFlashlight);
    }
}