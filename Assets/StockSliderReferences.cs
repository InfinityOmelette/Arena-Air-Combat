using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockSliderReferences : MonoBehaviour
{

    public Text weaponNameText;
    public Text quantityReadoutText;
    public Slider stockSlider;

    //public ref LoadoutStorage.LoadoutPreset linkedLoadout;

    // how to handle the fucking default, being one place in memory,
    // that potentially both teams could modify
    // here's the plan, i'm gonna just not fucking worry about it right now, and just
    // work on custom loadouts

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void initializeSlider(Weapon weap, int qty = -1)
    {
        weaponNameText.text = weap.name + ", wt: " + weap.stockWeight.ToString();

        // if no value was saved, default to 0
        if(qty == -1)
        {
            stockSlider.value = 0;
            quantityReadoutText.text = qty.ToString();
        }
        else
        {
            stockSlider.value = qty;
            quantityReadoutText.text = qty.ToString();
        }

        // any other action required to refresh slider w changes?
    }
}
