using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;

public class BallManager : MonoBehaviour
{
    OVRInput.Controller Rcontroller = OVRInput.Controller.RTouch;
    [SerializeField, Range(0f, 1f)] private float b_powerAdjust = 0.6f;
    [SerializeField] private float _minReleasePower = 20f;
    [SerializeField] private float _maxReleasePower = 100f;

    public float mag_b;
    public Vector3 acc;

    public Camera subCamera;
    public Camera GameOverCamera;

    public Rigidbody rig;

    public Text resultText;
    public Text scoreText;
    public Text stateText;
    public Text outText;
    public Text strikeText;
    public Text ballText;
    public Text GameStateText;




    public GameObject originalBall;
    public GameObject BallPrefab;
    public GameObject holdingBall;

    public GameObject bat;


    OVRInput.Controller Lcontroller = OVRInput.Controller.LTouch;
    [SerializeField, Range(0f, 1f)] private float p_powerAdjust = 0.2f;
    public float mag_p;
    public Vector3 p_acc;
    public Vector3 force;

    [SerializeField] private float p_minReleasePower = 30f;

    private float moveX;
    private float moveY;


    public int[] cond = new int[] { 0, 0, 0, 0, 0 };   //state,Score, Ball, Strike, Out
    public int state;
    public int OutCount;
    public int ScoreCount;
    public int StrikeCount;
    public int BallCount;

    public GameObject FirstRunner;
    public GameObject SecondRunner;
    public GameObject ThirdRunner;


    public GameObject GameOverCanvas;
    public Text GameOverScore;

    public Animator animator;


    public GameObject oneBall,twoBall,threeBall, oneStrike,twoStrike, oneOut,twoOut;

    public Vector3 TargetPos = new Vector3(67f, 2.3f, 67f);


    //SE
    AudioSource audioSource;
    public AudioClip sound1;
    public AudioClip sound2;
    public AudioClip sound3;

    
    // Start is called before the first frame update
    void Start()
    {
        subCamera.depth = -1;
        GameOverCamera.depth = -2;
        rig = originalBall.GetComponent<Rigidbody>();
        rig.constraints = RigidbodyConstraints.FreezeRotation;                                 //回転固定←サブカメラに切り替えたときぐるぐるしないため

        GameState.canThrow = true;

        StartCoroutine(DisplayText(duration: 1f, ResultText: "PLAY!"));

        FirstRunner.SetActive(false);
        SecondRunner.SetActive(false);
        ThirdRunner.SetActive(false);

        OutCount = 0;
        ScoreCount = 0;
        state = 1;
        StrikeCount = 0;
        BallCount = 0;

        audioSource = GetComponent<AudioSource>();


        //animator.SetTrigger("StartThrow");
    }

    // Update is called once per frame
    //入力処理
    void Update()
    {

        //カウントボード
        CountBoard(BallCount, StrikeCount, OutCount);
        Runner(state);

        mag_b = OVRInput.GetLocalControllerAcceleration(Rcontroller).magnitude;

        if (mag_b < _minReleasePower)
        {
            mag_b = _minReleasePower;
        }

        if (mag_b > _maxReleasePower)
        {
            mag_b = _maxReleasePower;
        }

        acc = -10f * bat.transform.right * (mag_b * b_powerAdjust);    //バットの左側（ピッチャー側）への加速度

        if (GameState.BallFlying && OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch))          //
        {
            subCamera.depth = 0;
        }

        if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch))          //GameState.BallFlying && 
        {
            subCamera.depth = -1;
        }

        //左のアナログスティックの情報取得
        moveX = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x;
        moveY = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y;




        if (GameState.canThrow && OVRInput.Get(OVRInput.RawButton.LIndexTrigger))          //
        {
            mag_p = OVRInput.GetLocalControllerAcceleration(Lcontroller).magnitude;

            if (mag_p < p_minReleasePower)
            {
                mag_p = p_minReleasePower;
            }

            if (mag_p > _maxReleasePower)
            {
                mag_p = _maxReleasePower;
            }


            Vector3 ball_forward = new Vector3(400f, 0f, 400f);
            p_acc = ball_forward * (mag_p * p_powerAdjust);
            force = p_acc * 0.145f;

            rig.velocity = Vector3.zero;
            rig.angularVelocity = Vector3.zero;
            originalBall.transform.position = new Vector3(29f, 3.42f, 29f);
        }




        if (GameState.canThrow && OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))          // 
        {
            animator.SetTrigger("StartThrow");
            Runner(state);
        }




        /*
        stateText.text = "State : " + state.ToString();
        
        ballText.text = "Ball : " + BallCount.ToString();
        strikeText.text = "Strike : " + StrikeCount.ToString();
        outText.text = "Out : " + OutCount.ToString();
        */

        /*
        if (GameState.canThrow)
        {
            GameStateText.text = "GameState:  canThrow  ";
        }
        else if (GameState.Throwing)
        {
            GameStateText.text = "GameState:  Throwing  ";
        }
        else if (GameState.BallFlying)
        {
            GameStateText.text = "GameState:  BallFlying  ";
        }
        */
        if(GameState.canThrow)
            OVRInput.SetControllerVibration(0.1f, 0.1f, Lcontroller);
    }


    void FixedUpdate()
    {

        scoreText.text = "Score : " + ScoreCount.ToString();
        Runner(state);
        //ランナー表示
        
        
        if (GameState.canThrow)
        {
            originalBall.SetActive(false);
            holdingBall.SetActive(true);
        }
        else
        {
            originalBall.SetActive(true);
            holdingBall.SetActive(false);
        }
        


        if (GameState.BallFlying && rig.IsSleeping() && resultText.text == "")      //ボールが止まった時
        {
            GameState.BallFlying = false;
            GameState.canThrow = true;
            StartCoroutine(DisplayText(duration: 3f, ResultText: "アウト！"));
            StartCoroutine(Delay());
            OutCount += 1;
            StrikeCount = 0;
            BallCount = 0;
            CountReset(1);
        }

        if (GameState.GameOver && OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))                    //
        {
            GameState.Throwing = false;
            GameState.BallFlying = false;
            GameState.canThrow = true;
            GameState.GameOver = false;

            OutCount = 0;
            ScoreCount = 0;
            state = 1;
            StrikeCount = 0;
            BallCount = 0;
            GameOverCanvas.SetActive(false);
            GameOverCamera.depth = -2;

            CountReset(2);
        }



        //3アウトでGameOver
        if (OutCount > 2)
        {
            GameState.canThrow = false;
            GameState.Throwing = false;
            GameState.BallFlying = false;
            GameState.GameOver = true;
            StartCoroutine(DisplayText(duration: 5f, ResultText: "Game Over"));
            StartCoroutine(Delay());

            GameOverCanvas.SetActive(true);
            GameOverCamera.depth = 2;
            GameOverScore.text = "   Score: " + ScoreCount.ToString();


            OutCount = 0;
            ScoreCount = 0;
            state = 1;
            StrikeCount = 0;
            BallCount = 0;
        }


        /*
        var renderer = originalBall.GetComponent<Renderer>();
        var renderer1 = holdingBall.GetComponent<Renderer>();
        if (GameState.canThrow)
        {
            //renderer.enabled = false;
            //renderer1.enabled = true;
            
        }
        
        else
        {
            //renderer.enabled = true;
            //renderer1.enabled = false;
            originalBall.SetActive(true);
            holdingBall.SetActive(false);
        }
        
        if (GameState.Throwing)
        {
            originalBall.SetActive(true);
            holdingBall.SetActive(false);
        }

        else
        {
            originalBall.SetActive(false);
            holdingBall.SetActive(true);
        }
        */
    }

    //updateここまで
    /*
    public void Runner(int stat)
    {
        while(stat % 2 == 0)
        {
            FirstRunner.SetActive(true);
        }
        while(stat % 4 == 3 || stat % 4 == 0)
        {
            SecondRunner.SetActive(true);
        }
        while (stat > 4)
        {
            ThirdRunner.SetActive(true);
        }
    }
    */
    public void Runner(int stat)
    {
        /*
        var renderer1 = FirstRunner.GetComponent<Renderer>();
        var renderer2 = SecondRunner.GetComponent<Renderer>();
        var renderer3 = ThirdRunner.GetComponent<Renderer>();
        */
        FirstRunner.SetActive(false);
        SecondRunner.SetActive(false);
        ThirdRunner.SetActive(false);
        if (stat % 2 == 0)
        {
            FirstRunner.SetActive(true);
        }
        /*
        else
        {
            FirstRunner.SetActive(false);
        }
        */
        if (stat % 4 == 3 || stat % 4 == 0)
        {
            SecondRunner.SetActive(true);
        }
        /*
        else
        {
            SecondRunner.SetActive(false);
        }
        */
        if (stat > 4)
        {
            ThirdRunner.SetActive(true);
        }
        /*
        else
        {
            ThirdRunner.SetActive(false);
        }
        */
    }
    
    public void CountReset(int mode)
    {
        if(mode > 0)
        {
            BallCount = 0;
            StrikeCount = 0;
            oneBall.SetActive(true);
            twoBall.SetActive(true);
            threeBall.SetActive(true);
            oneStrike.SetActive(true);
            twoStrike.SetActive(true);
        }
        if(mode > 1)
        {
            OutCount = 0;
            oneOut.SetActive(true);
            twoOut.SetActive(true);
        }
    }


    public void CountBoard(float Ball, float Strike, float Out)
    {
        if (Ball > 0)
            oneBall.SetActive(false);
        /*
        else
            oneBall.SetActive(true);
        */
        if (Ball > 1)
            twoBall.SetActive(false);
        /*
        else
            twoBall.SetActive(true);
        */
        if (Ball > 2)
            threeBall.SetActive(false);
        /*
        else
            threeBall.SetActive(true);
        */
        if (Strike > 0)
            oneStrike.SetActive(false);
        /*
        else
            oneStrike.SetActive(true);
        */
        if (Strike > 1)
            twoStrike.SetActive(false);
        /*
        else
            twoStrike.SetActive(true);
        */
        if (Out > 0)
            oneOut.SetActive(false);
        /*
        else
            oneOut.SetActive(true);
        */
        if (Out > 1)
            twoOut.SetActive(false);
        /*
        else
            twoOut.SetActive(true);
        */
    }

    public void Throwing()
    {
        originalBall.transform.position = new Vector3(29f, 3.42f, 29f);
        StartThrow(originalBall, 6.0f, new Vector3(29f, 3.42f, 29f), new Vector3(65f, 1.8f, 65f), 175f - mag_p);     //175f - mag_p
        GameState.Throwing = true;
        GameState.canThrow = false;
        GameState.BallFlying = false;
    }
   
    public void StartThrow(GameObject target,float curve_rate, Vector3 start, Vector3 end, float duration)          
    {
        // 中点を求める
        Vector3 half = (end + start) * 0.50f;
        half += (target.transform.right * moveX + target.transform.up * moveY) * curve_rate - target.transform.forward;
        end -= (target.transform.right * moveX + target.transform.up * moveY) * 0.50f;

        StartCoroutine(LerpThrow(target, start, half, end, duration));
    }

    IEnumerator LerpThrow(GameObject target, Vector3 start, Vector3 half, Vector3 end, float duration)          
    {
        float startTime = Time.timeSinceLevelLoad;
        float rate = 0f;
        while (true)
        {
            float diff = Time.timeSinceLevelLoad - startTime;
            rate = diff / (duration / 60f);

            
            if (rate >= 0.7f)
            {
                Rigidbody rigidbody = target.GetComponent<Rigidbody>();
                rigidbody.AddForce((end - target.transform.position) * (mag_p + 30));     //(170- duration)
                //rigidbody.AddForce(target.transform.forward * 3000f);
                //rigidbody.AddForce(end);
                yield break;
            }
            
            
            /*
            if(rate >= 1.5f)
                yield break;
            */

            else
            {
                target.transform.position = CalcLerpPoint(start, half, end, rate);
            }

            yield return null;
        }
    }

    Vector3 CalcLerpPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var a = Vector3.Lerp(p0, p1, t);
        var b = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(a, b, t);
    }



    public static IEnumerator Vibrate(float duration = 0.1f, float frequency = 0.1f, float amplitude = 0.1f, OVRInput.Controller controller = OVRInput.Controller.Active)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }

    
    public IEnumerator DisplayText(string ResultText, float duration = 3f)       //textを数秒間表示する関数
    {
        resultText.text = ResultText;
        yield return new WaitForSeconds(duration);
        resultText.text = "";
        yield return new WaitForSeconds(duration);
    }
    

    public IEnumerator Delay(float duration = 3f)       
    {
        yield return new WaitForSeconds(duration);

        /*
        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;
        originalBall.transform.position = new Vector3(29f, 3.42f, 29f);
        */

        resultText.text = "";

        rig.useGravity = false;

        GameState.canThrow = true;
        GameState.Throwing = false;
        GameState.BallFlying = false;
        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;
        originalBall.transform.position = new Vector3(29f, 3.42f, 29f);

        originalBall.SetActive(false);
        Runner(state);

    }



    void OnCollisionEnter(Collision collision)
    {
        if (GameState.Throwing && collision.gameObject.tag == "Bat")                                //バットに当たった時
        {                                          
            Vector3 force = 0.145f * acc;                                                           // 力を設定。F=ma、ボールの質量約145g
            rig.AddForce(force, ForceMode.Impulse);                                                  // 加速度に応じた力を加える
            StartCoroutine(Vibrate(duration: 0.3f, controller: OVRInput.Controller.RTouch));　　　  //右コントローラーを0.3秒振動
            
            rig.useGravity = true;                                                                   //重力オン
            
            GameState.Throwing = false;
            GameState.BallFlying = true;

            audioSource.PlayOneShot(sound1);
        }

        

        if (GameState.BallFlying && collision.gameObject.tag == "1BH")
        {
            state = oneBH(state);
            CountReset(1);
            audioSource.PlayOneShot(sound2);
        }
      
        if (GameState.BallFlying && collision.gameObject.tag == "2BH")
        {
            state = twoBH(state);
            CountReset(1);
            audioSource.PlayOneShot(sound2);
        }

        if (GameState.BallFlying && collision.gameObject.tag == "3BH")
        {
            state = threeBH(state);
            CountReset(1);
            audioSource.PlayOneShot(sound2);
        }

        if (GameState.BallFlying && collision.gameObject.tag == "FinePlay")
        {
            StartCoroutine(DisplayText("Fine Play"));
            StartCoroutine(Delay());

            OutCount += 1;
            CountReset(1);
            audioSource.PlayOneShot(sound2);
        }

        if (GameState.BallFlying && collision.gameObject.tag == "DoublePlay")
        {
            if(state == 1)
            {
                StartCoroutine(DisplayText("アウト！"));
            }
            else
            {
                StartCoroutine(DisplayText("Double Play"));
            }
            
            StartCoroutine(Delay());

            state = DoublePlay(state);
            CountReset(1);
        }

        if (GameState.BallFlying && collision.gameObject.tag == "OUT")
        {
            StartCoroutine(DisplayText("アウト"));
            StartCoroutine(Delay());
            OutCount += 1;

            CountReset(1);
        }

    }


    void OnTriggerEnter(Collider collision)
    {
        if (GameState.Throwing && collision.gameObject.tag == "Strike")             //ストライクだった時
        {
            StartCoroutine(DisplayText("ストライク！"));
            StartCoroutine(Vibrate(duration: 3f, controller: OVRInput.Controller.LTouch));
            StartCoroutine(Delay(1f));

            if(StrikeCount < 2)
            {
                StrikeCount += 1;
            }
            else
            {
                OutCount += 1;
                StrikeCount = 0;
                BallCount = 0;
                StartCoroutine(DisplayText("三振"));
                CountReset(1);
            }
        }

        if (GameState.Throwing && collision.gameObject.tag == "catcher" && resultText.text == "")             //ボールだった時
        {
            StartCoroutine(DisplayText("ボール！"));
            StartCoroutine(Vibrate(duration: 3f,frequency: 0.5f, controller: OVRInput.Controller.LTouch));
            
            if(BallCount < 3)
            {
                BallCount += 1;
            }
            else
            {
                state = fourBall(state);
                CountReset(1);
            }
            StartCoroutine(Delay(1f));
        }

        if (GameState.BallFlying && collision.gameObject.tag == "Faul")
        {
            StartCoroutine(Delay(2f));
            StartCoroutine(DisplayText("ファール！"));

            if(StrikeCount < 2)
            {
                StrikeCount += 1;
            }
        }

        
        if (GameState.BallFlying && collision.gameObject.tag == "HomeRun")
        {
            //originalBall.SetActive(false);
            StartCoroutine(Delay(3f));
            StartCoroutine(DisplayText("Home Run!!!"));

            state = HomeRun(state);
            CountReset(1);
            audioSource.PlayOneShot(sound3);
        }
    }


    int fourBall(int state)
    {
        StartCoroutine(DisplayText("フォアボール"));
        StrikeCount = 0;
        BallCount = 0;

        if (state % 2 == 1)
        {
            state += 1;
        }
        else if(state == 2)
        {
            state = 4;
        }
        else if(state == 4 || state == 6)
        {
            state = 8;
        }
        else
        {
            state = 8;
            ScoreCount += 1;

            StartCoroutine(DisplayText("押し出しフォアボール"));
        }

        return state;
    }

    int oneBH(int state)
    {
        StrikeCount = 0;
        BallCount = 0;
        StartCoroutine(DisplayText("1BH"));
        StartCoroutine(Delay());

        if (state <= 4)
        {
            state = state * 2;
        }
        else
        {
            StartCoroutine(DisplayText("タイムリーヒット"));
            StartCoroutine(Delay());

            ScoreCount += 1;
            state = (state - 4) * 2;
        }

        return state;
    }

    int twoBH(int state)
    {
        StrikeCount = 0;
        BallCount = 0;
        StartCoroutine(DisplayText("2BH"));
        StartCoroutine(Delay());

        if(state >= 3 && state <= 6)
        {
            StartCoroutine(DisplayText("タイムリーヒット"));
            StartCoroutine(Delay());
            ScoreCount += 1;
        }

        if(state >= 7)
        {
            StartCoroutine(DisplayText("2点タイムリーヒット"));
            StartCoroutine(Delay());
            ScoreCount += 2;
        }
        
        if (state % 2 == 1)
        {
            state = 3;
        }
        if(state % 2 == 0)
        {
            state = 7;
        }

        return state;
    }

    int threeBH(int state)
    {
        StrikeCount = 0;
        BallCount = 0;
        StartCoroutine(DisplayText("3BH"));
        StartCoroutine(Delay());


        if (state == 2 && state == 3 && state == 5)
        {
            StartCoroutine(DisplayText("１点タイムリーヒット"));
            StartCoroutine(Delay());
            ScoreCount += 1;
        }
        if (state == 4 && state == 6 && state == 7)
        {
            StartCoroutine(DisplayText("2点タイムリーヒット"));
            StartCoroutine(Delay());
            ScoreCount += 2;
        }
        if (state == 8)
        {
            StartCoroutine(DisplayText("３点タイムリーヒット"));
            StartCoroutine(Delay());
            ScoreCount += 3;
        }
        //state = 5;
        return 5;
    }

    int DoublePlay(int state)
    {
        StrikeCount = 0;
        BallCount = 0;
        if (OutCount >= 1)
        {
            OutCount = 3;
        }

        else
        {
            OutCount += 1;
            if(state > 4)
            {
                OutCount += 1;
                state -= 4;
            }
            else if(state == 3 || state == 4)
            {
                OutCount += 1;
                state -= 2;
            }
            else if (state == 2)
            {
                OutCount += 1;
                state -= 1;
            }
        }

        return state;
    }

    int HomeRun(int state)
    {
        StrikeCount = 0;
        BallCount = 0;

        if (state == 1)
        {
            StartCoroutine(DisplayText("SOLO HOME RUN！"));
            StartCoroutine(Delay());
            ScoreCount += 1;
        }
        else if(state == 2 || state == 3 || state == 5)
        {
            StartCoroutine(DisplayText("2RUN HOME RUN！!"));
            StartCoroutine(Delay());
            ScoreCount += 2;
        }
        else if(state == 8)
        {
            StartCoroutine(DisplayText("GRAND SLAM!!!!"));
            StartCoroutine(Delay());
            ScoreCount += 4;
        }
        else
        {
            StartCoroutine(DisplayText("3RUN HOME RUN！"));
            StartCoroutine(Delay());
            ScoreCount += 3;
        }

        //state = 1;
        return 1;
    }
}
