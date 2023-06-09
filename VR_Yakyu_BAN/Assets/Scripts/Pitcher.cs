using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{
    public GameObject ball;
    public GameObject pitcher;

    public float ballDistance = 2f;
    public float ballThrowingForce = 5f;


    //public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        //ball.GetComponent<Rigidbody>().useGravity = false;
        //ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch) )             //&& GameState.canThrow
        {
            ball.transform.position = pitcher.transform.position + pitcher.transform.forward * 2;

            
            //animator.SetTrigger("pitch");
            //holdingBall = false;
            //ball.GetComponent<Rigidbody>().useGravity = false;
            //ball.GetComponent<Rigidbody>().AddForce(pitcher.transform.forward * ballThrowingForce);

            

        }
        
    }


    /*
    void throwBall()
    {
        Rigidbody rigidbody = ball.GetComponent<Rigidbody>();
        rigidbody.AddForce(-200, 0, -200);
    }
    */

}
