using UnityEngine;

public class PlayerMovement : MonoBehaviour //makes this script a component
{
    public float moveSpeed = 5f; //public to show up in inspector

    private Rigidbody2D rb; // variable (rb) for storing physics body
    private float moveInput; //to store horizontal movement

    void Start() //runs once at the begining
    {
        rb = GetComponent<Rigidbody2D>(); 
    }

    void Update() //runs every frame
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate() //timed updates
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }   //current rb                                           //keep y untouched so we can use it for jumping/falling
        // mvment velocity
}

