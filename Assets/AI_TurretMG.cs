using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_TurretMG : MonoBehaviour
{

    Rigidbody targetRb;
    private float leadAngle = 0;


    private bool gunsOn = false;

    private float schutDistance = 1200f;

    private GameObject rootObj;

    public ParticleSystem gun;

    private float booleetSpeed;

    //private bool isJef = false;

    // Start is called before the first frame update
    void Start()
    {
        
        

        rootObj = transform.root.gameObject;
        booleetSpeed = gun.startSpeed;

        schutDistance = booleetSpeed * gun.startLifetime;

        //isJef = rootObj.name.Equals("JefTrok");

        //if (isJef)
        //{
        //    Debug.LogError("Jef found");
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if(targetRb == null)
        {
            GameObject player = GameManager.getGM().localPlayer;
            
            if(player != null)
            {
                CombatFlow playerFlow = player.GetComponent<CombatFlow>();
                if (playerFlow.team != rootObj.GetComponent<CombatFlow>().team)
                {

                    targetRb = player.GetComponent<Rigidbody>();
                }
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, targetRb.transform.position);
            bool gunSet = distance < schutDistance;

            setLead(distance);


            if (gunSet != gunsOn)
            {
                gunsOn = gunSet;
                if (gunSet)
                {
                    gun.Play();
                }
                else
                {
                    gun.Stop();
                }
            }
        }

       
    }

    private void setLead(float distance)
    {
        
        Vector3 targetBearingLine = targetRb.transform.position - transform.position;
        targetBearingLine = Vector3.Project(targetRb.velocity, targetBearingLine);
        float closingVel = targetBearingLine.magnitude;

        

        if (Vector3.Distance(transform.position, targetRb.transform.position + targetBearingLine) < distance)
        {
            closingVel *= -1f;
        }

        

        float timeToImpact = distance / (booleetSpeed - closingVel);

        //if (isJef)
        //{
        //    Debug.LogWarning("Closing Vel: " + closingVel + " target Vel: " + targetRb.velocity.magnitude);
        //}

        Vector3 targetPos = targetRb.transform.position + targetRb.velocity * timeToImpact;
        transform.rotation = Quaternion.LookRotation(targetPos - rootObj.transform.position, Vector3.up);
    }

    //private void setLeadAngle()
    //{
    //    Vector3 targetBearingLine = targetRb.position - rootObj.transform.position;
    //    Vector3 leadAxis = Vector3.Cross(targetBearingLine, targetRb.velocity).normalized;

    //    // Target tangential velocity --> missile will try to match its tangential velocity to this
    //    Vector3 targetTangentialVelocity = Vector3.Project(targetRb.velocity,
    //        Vector3.Cross(leadAxis, targetBearingLine));

    //    float leadAngleDegrees = Mathf.Rad2Deg * Mathf.Asin(targetTangentialVelocity.magnitude / targetRb.velocity.magnitude);


    //    //Vector3 leadVect = targetBearingLine * Quaternion.AngleAxis(leadAngleDegrees, leadAxis);
    //    transform.LookAt(targetRb.transform);
    //    Quaternion newRot = transform.rotation * Quaternion.AngleAxis(leadAngleDegrees, leadAxis);
    //    transform.rotation = newRot;
    //}


}
