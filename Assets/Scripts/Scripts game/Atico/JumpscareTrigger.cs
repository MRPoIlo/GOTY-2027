using UnityEngine;

public class JumpscareTrigger : MonoBehaviour
{
    [Header("CanvasGroup del Jumpscare")]
    [SerializeField] private CanvasGroup jumpscareCanvas;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[JumpscareTrigger] Algo entró al trigger: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("[JumpscareTrigger] El objeto tiene tag Player, activando jumpscare...");

            if (jumpscareCanvas != null)
            {
                jumpscareCanvas.alpha = 1f;
                jumpscareCanvas.interactable = true;
                jumpscareCanvas.blocksRaycasts = true;

                Debug.Log("[JumpscareTrigger] CanvasGroup actualizado: alpha=1, interactable=true, blocksRaycasts=true");
            }
            else
            {
                Debug.LogWarning("[JumpscareTrigger] No se asignó ningún CanvasGroup en el inspector.");
            }
        }
        else
        {
            Debug.Log("[JumpscareTrigger] El objeto no tiene tag Player, ignorando.");
        }
    }
}
