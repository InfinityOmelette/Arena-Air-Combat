using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUnit : MonoBehaviour
{
    //public Vector3 relativePosition;
    //public float bearing;

    private MapManager mapManager;

    private Text text;

    private GameObject localPlayer;

    private CombatFlow linkedFlow;
    private TgtHudIcon linkedTgtIcon;
    private RectTransform rectTransform;

    private bool isLinked = false;

    private bool isShowing = true;


    private float blinkTimer;
    public float blinkTimeMax;

    void Awake()
    {
        mapManager = transform.parent.GetComponent<MapManager>();
        text = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //show(false);
    }

    public void linkToFlow(CombatFlow flow)
    {
        text = GetComponent<Text>();

        linkedFlow = flow;
        Debug.LogWarning(flow.name + " radar symbol " + flow.radarSymbol);
        text.text = flow.radarSymbol;
        gameObject.name = flow.gameObject.name + " map icon";
        linkedTgtIcon = flow.myHudIconRef;
        isLinked = true;

        show(flow.isActive);
    }
    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null)
        {
            localPlayer = GameManager.getGM().localPlayer;
            // set color. This object should only be created after to-be-linked CombatFlow already exists
        }
        else
        {
            if (linkedFlow == null)
            {
                if (isLinked)
                {
                    GameObject.Destroy(this.gameObject);
                }
            }
            else
            {
                // update position and rotation
                updatePosition();
                updateRotation();
                updateColor();
                if (linkedTgtIcon.targetedState == TgtHudIcon.TargetedState.TARGETED)
                {
                    doBlink();
                }
                else
                {
                    show(mapManager.withinBounds(transform.localPosition) && linkedTgtIcon.isDetected);
                }
                // Debug.LogError("Showing: " + showing);

            }

        }

    }

    private void doBlink()
    {
        blinkTimer -= Time.deltaTime;

        if(blinkTimer < 0f)
        {
            blinkTimer = blinkTimeMax;
            text.enabled = !text.enabled;
        }

    }

    private void updateColor()
    {
        text.color = linkedTgtIcon.activeColor;
    }

    private void updatePosition()
    {
        Vector3 relPos = linkedFlow.transform.position - localPlayer.transform.position;
        relPos = new Vector3(relPos.x, 0f, -relPos.z); // remove y component
        relPos *= mapManager.mapScaleFactor;
        
        transform.localPosition = new Vector3(relPos.z, relPos.x, 0f);
    }

    private void updateRotation()
    {
        transform.localEulerAngles = new Vector3(0f, 0f, -MapManager.getBearing(linkedFlow.transform));
    }

    private void show(bool doShow)
    {
        text.enabled = doShow;
    }


}
