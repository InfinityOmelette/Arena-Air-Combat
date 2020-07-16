using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareIconManager : MonoBehaviour
{

    public float ySpacing;

    public FlareIcon[] icons;

    public GameObject flareIconPrefab;


    void Awake()
    {
        Vector3 initPos = transform.localPosition;

        transform.localPosition = new Vector3(-Screen.width / 2f + initPos.x, -Screen.height / 2f + initPos.y, 0.0f);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        
    }


    public void createIcons(int numIcons)
    {
        icons = new FlareIcon[numIcons];

        for(int i = 0; i < icons.Length; i++)
        {
            GameObject newObj = GameObject.Instantiate(flareIconPrefab, transform);
            icons[i] = newObj.GetComponent<FlareIcon>();

            newObj.transform.localPosition = new Vector3(0.0f, ySpacing * i, 0.0f);

        }
    }

    public void destroyIcons()
    {
        for(int i = 0; i < icons.Length; i++)
        {
            GameObject.Destroy(icons[i].gameObject);
        }
        icons = null;
    }
}
