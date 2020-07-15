using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareEmitter : MonoBehaviour
{
    public GameObject flarePrefab;


    public float vertLaunchSpeed;
    public float vertRandRange;
    public float horizLaunchSpeedMax;
    public float horizRandomRange;

    public AudioSource flarePopSound;

    private Rigidbody myRb;

    

    public GameObject flareLaunchL;
    public GameObject flareLaunchR;

    // Start is called before the first frame update
    void Awake()
    {
        myRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            emitFlare(flareLaunchL, -horizLaunchSpeedMax);
            emitFlare(flareLaunchR, horizLaunchSpeedMax);
        }
    }

    private void emitFlare(GameObject center, float horizSpeed)
    {
        flarePopSound.Play();


        GameObject newFlare = GameObject.Instantiate(flarePrefab);
        newFlare.transform.position = center.transform.position;

        Rigidbody flareRB = newFlare.GetComponent<Rigidbody>();


        horizSpeed += Random.Range(-horizRandomRange, horizRandomRange);

        float vertSpeed = vertLaunchSpeed + Random.Range(-vertRandRange, vertRandRange);

        flareRB.velocity = myRb.velocity + transform.up * vertSpeed + transform.right * horizSpeed;


    }
}
