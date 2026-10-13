using UnityEngine;

public class CompassNeedle : MonoBehaviour
{
    [SerializeField] private Transform player;

    void Update()
    {
        Transform t = player != null ? player : Camera.main.transform;
        transform.localEulerAngles = new Vector3(0f, 0f, t.eulerAngles.y);
    }
}