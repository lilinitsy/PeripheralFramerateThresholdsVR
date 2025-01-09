using UnityEngine;

public class PeripheralUpdate : MonoBehaviour
{
    public uint framerate;
    public float speed;
    public Vector3 spawn_location;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.SetPositionAndRotation(spawn_location, transform.rotation);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
