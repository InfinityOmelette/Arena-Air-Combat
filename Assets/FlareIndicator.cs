using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlareIndicator : MonoBehaviour
{

    public Text flareText;

    int numSet = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setFlareText(int numFlares)
    {
        if(numSet != numFlares)
        {
            string perFlare = "* ";

            string strOut = "";
            for (int i = 0; i < numFlares; i++)
            {
                strOut += perFlare;
            }
            flareText.text = strOut;

            numSet = numFlares;
        }
        
    }
}
