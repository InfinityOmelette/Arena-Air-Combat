using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertnessIndicator : MonoBehaviour
{

    public enum State
    {
        ALERT,
        WAKING,
        SLEEP
    }

    public State activeState;


    public Text alertnessText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setAlertnessDisplay(State state)
    {
        if(state != activeState)
        {
            activeState = state;

            switch (state)
            {
                case State.ALERT:
                    alertnessText.text = "!";
                    break;
                case State.WAKING:
                    alertnessText.text = "?";
                    break;
                case State.SLEEP:
                    alertnessText.text = "";
                    break;
            }
        }
    }
}
