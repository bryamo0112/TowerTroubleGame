using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAnimation : MonoBehaviour
{
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            anim.SetTrigger("Shooting");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.SetTrigger("Jumping");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            anim.SetTrigger("Running");
        }
    }
}
