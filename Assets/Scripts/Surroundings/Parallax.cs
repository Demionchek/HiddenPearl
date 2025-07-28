using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxEffect;
    public float length, startPositionX, startPositionY;
    public GameObject cameraObj;
    public bool isFollowY = false;
    public bool isFollowX = true;
    public float yFollowStrength = 1.0f;
    public float maxYOffset = 5.0f;
    public float minYOffset = -5.0f;

    private float lastCameraY;
    private float initialYPosition;

    private void Start()
    {
        startPositionY = transform.position.y;
        startPositionX = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        lastCameraY = cameraObj.transform.position.y;
        initialYPosition = transform.position.y;
    }

    private void LateUpdate()
    {
        float temp = (cameraObj.transform.position.x * (1 - parallaxEffect));
        float dist = (cameraObj.transform.position.x * parallaxEffect);

        float newY = transform.position.y;
        float deltaY = cameraObj.transform.position.y - lastCameraY;

        if (isFollowY)
        {
            float targetY = initialYPosition + deltaY * yFollowStrength;

            targetY = Mathf.Clamp(
                targetY,
                initialYPosition + minYOffset,
                initialYPosition + maxYOffset
            );

            newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 5f);
        }
        else
        {
            newY = transform.position.y - deltaY;
        }

        if (isFollowX)
        {
            transform.position = new Vector3(startPositionX + dist, newY, transform.position.z);
            lastCameraY = cameraObj.transform.position.y;
        }

        if (temp > startPositionX + length) startPositionX += length;
        else if (temp < startPositionX - length) startPositionX -= length;
    }
}
