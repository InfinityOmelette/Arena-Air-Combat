using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamKnocker : MonoBehaviour
{
    public class OneKnock
    {
        public float timeRemain;
        public float timeRemainMax;
        public Vector3 knockEuler;

# region knockMethods
        public float knockTimePercent()
        {
            return Mathf.Clamp(timeRemain / timeRemainMax, 0.0f, 1.0f);
        }

        public bool knockComplete()
        {
            return timeRemain < 0.0f;
        }
#endregion
    }

    //Quaternion originalLocalRot;
    Vector3 originalLocalEuler;
    public Vector3 returnLocalEulerOffset;

    public HashSet<OneKnock> allKnocks;

    public float rotReturnLerpRate;

    public float knockPowerCoeff;

    public float minKnockEulerRadius = 1.0f;
    public float maxKnockEulerRadius = 60f;
    public float rollCoeff = 5.0f;

    public Vector3 debugKnockEuler;
    public float debugKnockTime;
    public Transform root;

    public static float minDist = 4f; // avoid excessively massive knocks from small impacts

    private void Awake()
    {
        //originalLocalRot = transform.localRotation;
        originalLocalEuler = transform.localEulerAngles;
        //transform.local

        allKnocks = new HashSet<OneKnock>();

        root = transform.root;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Quaternion quatEulerKnocked = getKnockedEuler(Time.deltaTime);

        transform.localRotation = decayKnock(quatEulerKnocked, Time.deltaTime);
            
        


        if (Input.GetKeyDown(KeyCode.F5))
        {
            addKnock(debugKnockEuler, debugKnockTime);
        }
    }

    public Quaternion decayKnock(Quaternion knockedRot, float deltaTime)
    {
        Vector3 totalEulerReturn = originalLocalEuler + returnLocalEulerOffset;
        Quaternion returnLocalRot = 
            Quaternion.Euler(totalEulerReturn.x, totalEulerReturn.y, totalEulerReturn.z);

        return Quaternion.Slerp(knockedRot, returnLocalRot, rotReturnLerpRate * deltaTime);
    }

    public Quaternion getKnockedEuler(float deltaTime)
    {
        Vector3 totalKnockEuler = allKnocksEuler(deltaTime);
        Vector3 currentEuler = transform.localEulerAngles + totalKnockEuler;
        return Quaternion.Euler(currentEuler.x, currentEuler.y, currentEuler.z);
    }

    public Vector3 allKnocksEuler(float deltaTime)
    {
        Vector3 totalKnockEuler = new Vector3();

        // process knocks --> "torque" applied to cam
        // Euler to push rotation off center
        foreach (OneKnock knock in allKnocks)
        {
            totalKnockEuler += knock.knockEuler * knock.knockTimePercent() * deltaTime;
            knock.timeRemain -= deltaTime;


        }



        if (allKnocks.Count > 0)
        {
            allKnocks.RemoveWhere(knockComplete);
        }




        return totalKnockEuler;
    }

    public bool knockComplete(OneKnock knock)
    {
        return knock.knockComplete();
    }

    public void addKnock(OneKnock knock)
    {
        allKnocks.Add(knock);
    }

    public void addKnock(Vector3 knockEuler, float timeRemain)
    {
        OneKnock knock = new OneKnock();
        knock.knockEuler = knockEuler;
        knock.timeRemain = timeRemain;
        knock.timeRemainMax = timeRemain;
        addKnock(knock);
    }

    public void knockExplode(float radius, Vector3 explodePos)
    {
        Vector3 explodeToCam = root.position - explodePos;
        float dist = Mathf.Max( explodeToCam.magnitude, minDist);

        float knockEulerRadius = (radius * radius * knockPowerCoeff) / (dist * dist);
        knockEulerRadius = Mathf.Min(knockEulerRadius, maxKnockEulerRadius);

        if (knockEulerRadius > minKnockEulerRadius)
        {

            Vector3 knockLineLocal = -transform.InverseTransformDirection(explodeToCam);
            knockLineLocal.z = 0;
            //knockLineLocal.x = -1f;
            knockLineLocal = swapXY(knockLineLocal);

            knockLineLocal = knockLineLocal.normalized * knockEulerRadius;

            knockLineLocal.z = Random.Range(-knockLineLocal.y * rollCoeff, knockLineLocal.y * rollCoeff);

            float timeRemain = .5f;

            addKnock(knockLineLocal, timeRemain);

        }


    }

    Vector3 swapXY(Vector3 vect)
    {
        float tempX = vect.x;
        vect.x = vect.y;
        vect.y = tempX;
        return vect;
    }

    public void zoomPitchCompensation(float startFOV, float newFOV, float restOrbitPitch)
    {
        float origFovRemain = (.5f * startFOV) - Mathf.Abs(restOrbitPitch);

        float origRatio = restOrbitPitch / origFovRemain;

        float newPitch = (origRatio * newFOV) / 3f; // aimpoint should be this pitch below horizon

        float newPitchEffective = newPitch - restOrbitPitch;

        returnLocalEulerOffset.x = newPitchEffective;


    }

}
