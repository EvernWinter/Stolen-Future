using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public float scrollSpeed = 2.0f;
    public List<Transform> backgroundSprites; // Assign the sprites in the inspector

    private Queue<Transform> backgroundQueue; 
    private float spriteHeight;

    void Start()
    {
        // Initialize the queue with the background sprites
        backgroundQueue = new Queue<Transform>(backgroundSprites);

        // Assuming each background sprite has the same height
        spriteHeight = backgroundSprites[0].GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        foreach (Transform background in backgroundQueue)
        {
            // Move each sprite downward in global space
            background.Translate(Vector3.up * -scrollSpeed * Time.deltaTime, Space.World);
        }

        // Check if the first background in the queue has moved off-screen in global Y
        if (backgroundQueue.Peek().position.y < -spriteHeight)
        {
            Transform offscreenSprite = backgroundQueue.Dequeue();

            // Reposition it at the top of the queue using global Y
            float highestYPosition = GetHighestSpriteYPosition();
            offscreenSprite.position = new Vector3(
                offscreenSprite.position.x,
                highestYPosition + spriteHeight,
                offscreenSprite.position.z
            );

            // Enqueue it back to keep the loop going
            backgroundQueue.Enqueue(offscreenSprite);
        }
    }

    // Helper function to find the highest sprite position in global Y
    float GetHighestSpriteYPosition()
    {
        float highestY = float.MinValue;
        foreach (Transform background in backgroundQueue)
        {
            if (background.position.y > highestY)
            {
                highestY = background.position.y;
            }
        }
        return highestY;
    }
}
