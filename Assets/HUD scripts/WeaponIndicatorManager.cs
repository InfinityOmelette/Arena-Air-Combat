using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WeaponIndicatorManager : MonoBehaviour
{

    public GameObject indicatorPrefab;

    public HardpointController hardpointController;

    public Text changeText;
    public Image changeImage;

    public Text weaponTypesText;
    public Image weaponSelectBox;

    public GameObject statusIndicatorsCenter;


    public float offsetPerOrderPos;
    public float typeRectangleOffsetPerIndex;

    public float changeNotificationTimerMax;
    public float currentChangeNotificationTimer;

    private bool showControllerHud = false;


    private Vector3 initWeapSelectBoxLocalPos;

    public GameObject cornerObjectsContainer;

    public GameObject controllerHudObj;

    public Color dpadColorSelected;
    public Color dpadColorNotSelected;


    // these are the parent empty GameObjects that indicator icons of same type will be placed
    public List<GameObject> indicatorTypeContainers;  // assume initial active type is zero

    public Image[] dPadWeaponDirections;
    private byte dpadImagesSaved = 0;

    private void Awake()
    {
        initWeapSelectBoxLocalPos = weaponSelectBox.transform.localPosition;
        indicatorTypeContainers = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cornerObjectsContainer.transform.localPosition = 
            new Vector3(Mathf.Round(Screen.width / 2), Mathf.Round(- Screen.height / 2), 0f);// place on lower right corner
    }

    public void showActiveWeaponType(short weaponTypeIndex)
    {
        //  initiate weaponChangeNotification with new type's name and weapon image
        //  loop through all containers -- place inactive ones offscreen, place the active one onscreen
        for(short i = 0; i < indicatorTypeContainers.Count; i++)
        {
            if(i == weaponTypeIndex)  // SHOW THIS TYPE
            {
                //  move to proper location
                indicatorTypeContainers[i].transform.localPosition = new Vector3(0, 0, 0f); 
            }
            else  // DON'T SHOW THIS TYPE
            {
                // move offscreen
                indicatorTypeContainers[i].transform.localPosition = new Vector3(Screen.width * 2, Screen.width * 2, 0f); 
            }
        }

        beginWeaponChangeNotification(weaponTypeIndex);
        placeSelectionBox(weaponTypeIndex);


        for(int i = 0; i < dPadWeaponDirections.Length; i++)
        {
            if(i == weaponTypeIndex)
            {
                dPadWeaponDirections[i].color = dpadColorSelected;
            }
            else
            {
                dPadWeaponDirections[i].color = dpadColorNotSelected;
            }
        }

    }

    public void placeSelectionBox(short typeIndex)
    {
        // text list goes from top down

        // position from top, subtract down for each index

        float offset = typeRectangleOffsetPerIndex * indicatorTypeContainers.Count -
            typeIndex * typeRectangleOffsetPerIndex;

        weaponSelectBox.transform.localPosition = new Vector3(initWeapSelectBoxLocalPos.x,
            initWeapSelectBoxLocalPos.y + offset, initWeapSelectBoxLocalPos.z);
    }

    public void beginWeaponChangeNotification(short typeIndex)
    {
        // get child weaponIndicator object -- read its hardpoint's prefab's name
        WeaponIndicator activeWeapIndicator = indicatorTypeContainers[typeIndex].transform.GetChild(0).GetComponent<WeaponIndicator>();
        Sprite activeWeapIndicatorSprite = activeWeapIndicator.myHardpoint.weaponTypePrefab.GetComponent<Weapon>().iconImageSpriteFile;

        // enable these to begin editing them
        changeImage.enabled = true;
        changeText.enabled = true;

        changeImage.sprite = activeWeapIndicatorSprite; // set notification image and show
        changeText.text = activeWeapIndicator.myHardpoint.weaponTypePrefab.name; // set notification text and show

        currentChangeNotificationTimer = changeNotificationTimerMax; // begin timer
    }

    public GameObject spawnNewContainer(GameObject newPrefabType)
    {
        Debug.Log("spawn container called");

        if (dpadImagesSaved < dPadWeaponDirections.Length)
        {
            // also stores dpad image
            dPadWeaponDirections[dpadImagesSaved].sprite = newPrefabType.GetComponent<Weapon>().iconImageSpriteFile;
            dpadImagesSaved++;
        }

        GameObject newContainerObj = new GameObject(newPrefabType.name + " container"); // name will be useful for debugging
        newContainerObj.transform.SetParent(statusIndicatorsCenter.transform); // statusIndicatorsCenter is parent of all containers
        newContainerObj.transform.localPosition = new Vector3(0f, 0f, 0f);      // might be unnecessary

        

        indicatorTypeContainers.Add(newContainerObj);

        weaponTypesText.text += (indicatorTypeContainers.Count).ToString() + " --- " + newPrefabType.name + "\n";

        return newContainerObj;
    }


    public GameObject spawnNewIndicator(short indicatorType, short orderPosition, Hardpoint linkedHardpoint)
    {
        Debug.Log("spawn indicator called");

        // spawn indicator with proper container object as parent
        GameObject newIndicator = Instantiate(indicatorPrefab, indicatorTypeContainers[indicatorType].transform);

        // link to proper hardpoint -- indicator will independently control itself based on hardpoint's data
        newIndicator.GetComponent<WeaponIndicator>().myHardpoint = linkedHardpoint;

        // offset value to place each indicator side by side
        newIndicator.transform.localPosition = new Vector3(
            -(orderPosition * offsetPerOrderPos), 0.0f, 0.0f);

        // set weapon image
        newIndicator.GetComponent<WeaponIndicator>().weaponImage.sprite =
            linkedHardpoint.weaponTypePrefab.GetComponent<Weapon>().iconImageSpriteFile;

        return newIndicator;
    }

    public void deleteAll()
    {
        weaponTypesText.text = "";

        // delete all children of corner objects
        foreach (Transform child in statusIndicatorsCenter.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        indicatorTypeContainers.Clear();

    }

    public void displayControllerHud(bool showContHud)
    {
        if(showContHud != this.showControllerHud)
        {
            toggleControllerHud();
        }
    }

    public void toggleControllerHud()
    {
        showControllerHud = !showControllerHud;

        Debug.LogWarning("Toggling controller hud");

        // mouse hud objects:
        //  - weaponTypesText
        //  - selectedTypeBox


        if (showControllerHud)
        {
            // hide mouse hud
            weaponTypesText.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
            weaponSelectBox.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);

            // show controller hud
            controllerHudObj.transform.localScale = new Vector3(1, 1, 1);
        }
        else // show mouse hud
        {
            // hide controller hud
            controllerHudObj.transform.localScale = new Vector3(0, 0, 0);

            // show mouse hud
            weaponTypesText.transform.localScale = new Vector3(1, 1, 1);
            weaponSelectBox.transform.localScale = new Vector3(1, 1, 1);
        }

    }


    // Update is called once per frame
    void Update()
    {
        if(currentChangeNotificationTimer > 0) // only show until timer runs out
        {
            //Debug.Log("******************************** change indicator timer active");
            currentChangeNotificationTimer -= Time.deltaTime;
        }
        else
        {
            // hide notification image
            changeImage.enabled = false;

            // hide notification text
            changeText.enabled = false;
        }
    }
}
