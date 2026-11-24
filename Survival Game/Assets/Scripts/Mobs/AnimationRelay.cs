using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    private MiniBossAI mainAI;

    void Start()
    {
        // Finds MiniBossAI on the parent object
        mainAI = GetComponentInParent<MiniBossAI>();
    }

    // Called by the Animation Event on the child object
    public void HitPlayer()
    {
        if (mainAI != null)
        {
            mainAI.HitPlayer();
        }
    }
}