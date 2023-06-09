using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class shootball : MonoBehaviour
{
    public GameObject originalBall;
    public GameObject BallPrefab;


    public Animation anim;

    OVRInput.Controller controller = OVRInput.Controller.LTouch;
    [SerializeField, Range(0f, 1f)] private float _powerAdjust = 0.8f;
    public float mag;
    public Vector3 acc;
    public Vector3 force;

    [SerializeField] private float _minReleasePower = 30f;

    private float moveX;
    private float moveY;
   
    // Start is called before the first frame update
    void Start()
    {
        //GameState.canThrow = true;
        //originalBall.GetComponent<Rigidbody>().AddForce(new Vector3(400, 0, 400));
        //GameState.Throwing = true;
        //shootingball(new Vector3(400, 0, 400));

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameState.canThrow && OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch))             
        {
            mag = OVRInput.GetLocalControllerAcceleration(controller).magnitude;

            if (mag < _minReleasePower)
            {
                mag = _minReleasePower;
            }

            Vector3 forward = new Vector3 (400f, 0f, 400f);
            acc = forward * (mag * _powerAdjust);
            force = acc * 0.145f;
        }

        if (GameState.canThrow && OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            shootingball(force);
        }


        if (GameState.canThrow && OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            shootingball(new Vector3(400, 0, 400));
        }


        //左のアナログスティックの情報取得

        moveX = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x;
        moveY = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y;

        
        Vector3 ChangeForce = originalBall.transform.right * moveX + originalBall.transform.up * moveY;

        
        
        if (GameState.Throwing)                     
        {
            originalBall.GetComponent<Rigidbody>().AddForce(0.5f * ChangeForce);
        }
        
    }


   
    public void shootingball(Vector3 force)
    {
        //anim.Play();

        Destroy(originalBall);
        originalBall = Instantiate(BallPrefab, new Vector3(28f, 2.3f, 27f), Quaternion.identity);
        Rigidbody rigidbody = originalBall.GetComponent<Rigidbody>();
        
        rigidbody.AddForce(force);

        GameState.Throwing = true;
        GameState.canThrow = false;
        GameState.BallFlying = false;
        
    }

    
}
