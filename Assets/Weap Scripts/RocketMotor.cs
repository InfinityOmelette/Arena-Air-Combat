using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketMotor : MonoBehaviour
{

    public float thrustForce;
    public float burnTime;

    private Rigidbody myRB;
    private Weapon myWeapon;


    

    public bool doBurn;

    private bool makeEffect = true;

    private void Awake()
    {
        doBurn = false;
        myRB = GetComponent<Rigidbody>();
        myWeapon = GetComponent<Weapon>();

        if (makeEffect)
        {
            myWeapon.effectsObj = Instantiate(myWeapon.effectsOriginalObj);
            myWeapon.effectsObj.transform.position = myWeapon.effectsCenter.position;

            myWeapon.effectsObj.GetComponent<Light>().enabled = false;
            myWeapon.effectsObj.GetComponent<TrailRenderer>().enabled = false;

            //myWeapon.effectsObj.GetComponent<AutoDelete>().setReference(gameObject);
        }
        Destroy(myWeapon.effectsOriginalObj);
    }

    public void cancelEffect()
    {
        makeEffect = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(myWeapon.launched && burnTime > 0)
        {
            burnTime -= Time.deltaTime;
            if (!doBurn) // activate effects on the very first frame that doBurn is enabled
            {
                myWeapon.effectsObj.GetComponent<Light>().enabled = true;
                myWeapon.effectsObj.GetComponent<TrailRenderer>().enabled = true;
            }
            myWeapon.effectsObj.transform.position = myWeapon.effectsCenter.transform.position;
            doBurn = true;
        }
        else
        {
            if (doBurn) // disable light on the very first frame that doBurn is disabled
            {
                myWeapon.effectsObj.GetComponent<Light>().enabled = false;
            }
            doBurn = false;
        }

    }

    private void FixedUpdate()
    {
        if (!makeEffect && myWeapon.effectsObj != null)
        {
            Destroy(myWeapon.effectsObj);
        }


        if (myWeapon != null)
        {
            if(doBurn)
            {
                

                if (myRB != null)
                    myRB.AddForce(transform.forward * thrustForce);



            }
        }
    }
}
