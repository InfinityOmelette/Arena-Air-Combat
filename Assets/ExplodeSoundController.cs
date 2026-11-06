using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeSoundController : MonoBehaviour
{
    public AudioSource audioSource;
    private AudioClip activeSound;

    public AudioClip smallExplodeSound;
    public AudioClip mediumExplodeSound;
    public AudioClip largeExplodeSound;


    public ExplosionSizer sizer;

    public static float MEDIUM_SIZE = 8f;
    public static float LARGE_SIZE = 30f;

    public static float RANDOM_PITCH_MIN = 0.75f; // was .35f
    public static float RANDOM_PITCH_MAX = 1.1f; // was 1.65f

    // Start is called before the first frame update
    void Start()
    {
        audioSource.pitch = randomizePitch();
        setExplodeSound();
        audioSource.Play();
        knock();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    public ExplosionSizer getSizer()
    {
        if(sizer == null)
        {
            sizer = GetComponent<ExplosionSizer>();
        }
        return sizer;
    }

    private float getRadius()
    {
        return getSizer().radius;
    }


    private void setExplodeSound()
    {
        if (getRadius() < MEDIUM_SIZE)
        {
            activeSound = smallExplodeSound;
        }
        else if (getRadius() < LARGE_SIZE)
        {
            activeSound = mediumExplodeSound;
        }
        else
        {
            activeSound = largeExplodeSound;
        }

        audioSource.clip = activeSound;
    }

    private float randomizePitch()
    {
        return Random.Range(RANDOM_PITCH_MIN, RANDOM_PITCH_MAX);
    }

    private void knock()
    {
        CamManipulation camManip = PerspectiveManager.getPManager().getPlayerCam();

        if (camManip != null)
        {
            camManip.knockExplode(getRadius(), transform.position);

        }
    }
}
