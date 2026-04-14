using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Light LuzLinterna;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (LuzLinterna.enabled == true)
            {
                LuzLinterna.enabled = false;
            }
            else if (LuzLinterna.enabled == false)
            {
                LuzLinterna.enabled = true;
            }
        }
    }
}
