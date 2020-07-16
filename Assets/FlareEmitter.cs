using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FlareEmitter : MonoBehaviourPun
{
    public GameObject flarePrefab;

    public float flareReloadDelay;

    public int numFlareSlots;
    public float[] flareSlotReloads;

    public float rapidDelay;
    private float rapidTimer;

    public int rapidCountMax;
    private int rapidCount;

    public float deployCooldown;
    private float cooldownTimer;


    public float vertLaunchSpeed;
    public float vertRandRange;
    public float horizLaunchSpeedMax;
    public float horizRandomRange;

    public AudioSource flarePopSound;

    private Rigidbody myRb;

    

    public GameObject flareLaunchL;
    public GameObject flareLaunchR;

    private bool deploying = false;

    private CombatFlow myFlow;


    public float jammingTimeMax;
    private float jammingTimer;

    public bool flareButtonDown;

    private FlareIconManager flareIconManager;


    // Start is called before the first frame update
    void Awake()
    {
        myRb = GetComponent<Rigidbody>();

        myFlow = GetComponent<CombatFlow>();

        rapidTimer = rapidDelay;
        rapidCount = rapidCountMax;
        cooldownTimer = deployCooldown;

        flareSlotReloads = new float[numFlareSlots];
    }


    void Start()
    {
        flareIconManager = hudControl.mainHud.GetComponent<hudControl>().flareIconManager;

        flareIconManager.createIcons(flareSlotReloads.Length);
    }

    // Update is called once per frame
    void Update()
    {

        doRapidDeployTimer();

        // cooldown timer
        doCooldownTimer();

        doJammingTimer();

        doFlareSlotReloadTimer();

    }

    private int getAvailableFlareIndex()
    {
        int index = -1;

        for(int i = 0; i < flareSlotReloads.Length && index == -1; i++)
        {
            float flareSlotTime = flareSlotReloads[i];
            if(flareSlotTime < 0f)
            {
                index = i;
            }
        }

        return index;
    }

    private void doFlareSlotReloadTimer()
    {
        for(int i = 0; i < flareSlotReloads.Length; i++)
        {
            bool ready = flareSlotReloads[i] < 0f;

            if (!ready)
            {
                flareSlotReloads[i] -= Time.deltaTime;
            }

            flareIconManager.icons[i].setReloadStatus(ready, flareSlotReloads[i], flareReloadDelay);
        }
    }

    private void doJammingTimer()
    {
        myFlow.jamming = jammingTimer > 0f;

        if (myFlow.jamming)
        {
            jammingTimer -= Time.deltaTime;
        }
    }

    [PunRPC]
    private void activateFlares()
    {
        deploying = true;
        cooldownTimer = deployCooldown;
        jammingTimer = jammingTimeMax;
    }

    private void doCooldownTimer()
    {
        if (cooldownTimer < 0f)
        {
            if (myFlow.isLocalPlayer && flareButtonDown)
            {

                int flareSlot = getAvailableFlareIndex();

                if (flareSlot != -1)
                {
                    flareSlotReloads[flareSlot] = flareReloadDelay;
                    photonView.RPC("activateFlares", RpcTarget.All);
                }
            }

        }
        else
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void doRapidDeployTimer()
    {
        if (deploying)
        {
            if (rapidCount > 0f)
            {
                if (rapidTimer < 0f)
                {
                    flarePop();
                    rapidCount--;
                    rapidTimer = rapidDelay;
                }
                else
                {
                    rapidTimer -= Time.deltaTime;
                }
            }
            else
            {
                // putting this here allows deploying to be set
                //  in a situation where flares can't be immediately deployed
                //  ....this will allow flares to deploy once ready,
                //  instead of immediately disabling them (good for networking)
                deploying = false;
            }
        }
        else
        {
            rapidCount = rapidCountMax;
        }
    }

    private void flarePop()
    {
        emitFlare(flareLaunchL, -horizLaunchSpeedMax);
        emitFlare(flareLaunchR, horizLaunchSpeedMax);
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


    void OnDestroy()
    {
        if (myFlow.isLocalPlayer && flareIconManager != null)
        {
            flareIconManager.destroyIcons();
        }
    }
}
