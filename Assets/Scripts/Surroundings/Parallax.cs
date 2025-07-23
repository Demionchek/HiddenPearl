using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxEffect;
    public float length, startPositionX, startPositionY;
    public GameObject cameraObj;
    private float lastCameraY;

    private void Start()
    {
        startPositionY = transform.position.y;
        startPositionX = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        lastCameraY = cameraObj.transform.position.y;
    }

    private void LateUpdate()
    {
        float temp = (cameraObj.transform.position.x * (1 - parallaxEffect));
        float dist = (cameraObj.transform.position.x * parallaxEffect);

        float deltaY = cameraObj.transform.position.y - lastCameraY;
        float newY = transform.position.y - deltaY;

        transform.position = new Vector3(startPositionX + dist, newY, transform.position.z);

        lastCameraY = cameraObj.transform.position.y;

        if (temp > startPositionX + length) startPositionX += length;
        else if (temp < startPositionX - length) startPositionX -= length;
    }
}
