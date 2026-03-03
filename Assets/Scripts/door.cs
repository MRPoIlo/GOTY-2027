using UnityEngine;

public class door : MonoBehaviour
{
    Animator anim;

        void Start()
    {
        anim = GetComponent<Animator>();
    }
    
    private void OnTriggerEnter(Collider col)
    {
        Debug.Log("Entra: " + col.name);
        anim.SetBool("Open", true);
    }
}
