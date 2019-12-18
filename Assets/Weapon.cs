using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{

    public GameObject myTarget;

    virtual public void launch()
    {
        Debug.Log("Parent Launch called");
    }

    virtual public void linkToOwner( GameObject owner)
    {
        Debug.Log("Parent LinkToOwner called");
    }

}
