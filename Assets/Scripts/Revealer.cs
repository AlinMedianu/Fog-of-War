using System;
using System.Linq;
using UnityEngine;

public class Revealer : MonoBehaviour
{
    private float previousViewingRange;
    [SerializeField][Range(0, 500)]
    private float viewingRange = 5;
    /// <summary>
    /// in texture coordinates
    /// </summary>
    private float viewingDiameter;
    private (Vector4, GameObject)[] collisions;
    private static FoggyArea[] foggyAreas;
    private PlayerMover mover;

    private void Awake()
    {
        mover = GetComponent<PlayerMover>();
    }

    private void Start()
    {
        collisions = new (Vector4, GameObject)[FogManager.Instance.CollisionsCount];
        if (foggyAreas == null)
            foggyAreas = FindObjectsOfType<FoggyArea>();
    }

    private void Update()
    {
        if (previousViewingRange != viewingRange)
        {
            viewingDiameter *= viewingRange / previousViewingRange;
            previousViewingRange = viewingRange;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (previousViewingRange != viewingRange)
        {
            viewingDiameter = 0.2f * viewingRange / other.transform.localScale.x;
            previousViewingRange = viewingRange;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        UpdateCollisions();
        foreach(FoggyArea foggyArea in foggyAreas)
            if(other.gameObject == foggyArea.gameObject)
                foggyArea.UpdateShaders(viewingDiameter, collisions.Select(collision => 
                    (collision.Item1, collision.Item2 == foggyArea.gameObject)).ToArray(), mover.Moving);
    }


    private void UpdateCollisions()
    {
        float angleIncrement = 360f / FogManager.Instance.CollisionsCount;
        float angleBetweenCollisions = 0;
        for(int i = 0; i < FogManager.Instance.CollisionsCount; ++i)
        {
            collisions[i] = GetCollision(Quaternion.AngleAxis
                (angleBetweenCollisions, transform.up) * transform.forward);
            angleBetweenCollisions += angleIncrement;
        }
    }

    private (Vector4, GameObject) GetCollision(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit collision, 
            viewingRange, ~(1 << 9), QueryTriggerInteraction.Ignore))
        {
            if (Physics.Raycast(collision.point, Vector3.down, out RaycastHit contact))
                return (contact.textureCoord, contact.collider.gameObject);
        }
        else if(Physics.Raycast(transform.position + direction.normalized * viewingRange, 
            Vector3.down, out RaycastHit contact))
            return (contact.textureCoord, contact.collider.gameObject);
        throw new Exception("No surface was detected");
    }
}
