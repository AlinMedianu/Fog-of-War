using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [Header("Controls")]
    [SerializeField]
    private KeyCode up = KeyCode.W;
    [SerializeField]
    private KeyCode left = KeyCode.A;
    [SerializeField]
    private KeyCode down = KeyCode.S;
    [SerializeField]
    private KeyCode right = KeyCode.D;
    private Rigidbody physicsInteraction;

    public bool Moving { get; private set; } = false;

    private void Awake()
    {
        physicsInteraction = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        physicsInteraction.velocity = Vector3.zero;
        if (Input.GetKey(up))
        {
            physicsInteraction.velocity = (physicsInteraction.velocity + Vector3.forward).normalized;
            Moving = true;
        }
        if (Input.GetKey(left))
        {
            physicsInteraction.velocity = (physicsInteraction.velocity - Vector3.right).normalized;
            Moving = true;
        }
        if (Input.GetKey(down))
        {
            physicsInteraction.velocity = (physicsInteraction.velocity - Vector3.forward).normalized;
            Moving = true;
        }
        if (Input.GetKey(right))
        {
            physicsInteraction.velocity = (physicsInteraction.velocity + Vector3.right).normalized;
            Moving = true;
        }
        if(!Input.GetKey(up) && !Input.GetKey(left) && !Input.GetKey(down) && !Input.GetKey(right))
            Moving = false;
        physicsInteraction.velocity *= speed;
    }
}
