using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float jumpForce = 700f;                          // Amount of force added when the player jumps.
    [Range(0, .3f)] [SerializeField] private float movementSmoothing = 0.05f; // How much to smooth out the movement
    [SerializeField] private bool isAirControlActive;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask whatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform groundCheck;                           // A position marking where to check if the player is grounded.

    [SerializeField] private Animator animator;

    private const float groundCheckRadius = 0.3f; // Radius of the overlap circle to determine if grounded
    private const float doubleJumpReset = 0.5f;
    private float originalGravity;

    private bool isGrounded;            // Whether or not the player is grounded
    private bool isFacingRight = true;  // For determining which way the player is currently facing.
    private bool jumped, doubleJumped;
    private int lastSound = 1;

    private Rigidbody2D m_Rigidbody2D;
    private DistanceJoint2D tongueJoint;
    private Vector2 velocity = Vector2.zero;
    private PlayerMovement pm;
    private CapsuleCollider2D bodyCollider;

    public AudioManager audioManager;

    private void Awake()
    {
        bodyCollider = GetComponent<CapsuleCollider2D>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        tongueJoint = GetComponent<DistanceJoint2D>();
        pm = GetComponent<PlayerMovement>();
        originalGravity = m_Rigidbody2D.gravityScale;
    }

    private void Update()
    {
        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, whatIsGround);

        if(colliders.Length == 0)
            isGrounded = false;
        else if(!jumped || m_Rigidbody2D.velocity.y == 0)
        {
            jumped = false;
            isGrounded = true;
            doubleJumped = false;
        }
    }

    public void Move(float horizontal, bool jump, bool onLadder)
    {
        //only control the player if grounded or airControl is turned on
        if((isGrounded || isAirControlActive) && !onLadder)
        {
            // Move the character by finding the target velocity
            Vector2 targetVelocity = Vector2.zero;
            if(tongueJoint.enabled)
            {
                //jumped = false;
                Invoke("EnableDoubleJump", doubleJumpReset);

                if(tongueJoint.connectedBody.position.y > transform.position.y)
                    targetVelocity = new Vector2(horizontal * 15f, m_Rigidbody2D.velocity.y);
                else
                    return;
                //targetVelocity = Physics2D.gravity;
            }
            else
            {
                targetVelocity = new Vector2(horizontal * 10f, m_Rigidbody2D.velocity.y);
                CancelInvoke();
            }

            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector2.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, movementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if(horizontal > 0 && !isFacingRight)
                Flip();
            // Otherwise if the input is moving the player left and the player is facing right...
            else if(horizontal < 0 && isFacingRight)
                Flip();
        }

        if(onLadder)
            HandleLadder();
        else
        {
            m_Rigidbody2D.gravityScale = originalGravity;
            //Can jump only if not on a ladder
            animator.SetBool("Climbing", false);
            if(jump)
                CheckAndJump();

        }
        HandleAnimation(horizontal, onLadder);
    }

    /// <summary>
    /// Called with an invoke, prevents the abuse of attatching with the toungue and jumping
    /// </summary>
    private void EnableDoubleJump()
    {
        doubleJumped = false;
    }

    private void HandleAnimation(float horizontal, bool onLadder)
    {
        if(onLadder)
        {
            animator.SetBool("Jumping", false);
            animator.SetBool("Moving", false);
        }
        else if(isGrounded)
        {
            if(horizontal != 0f)
            {
                animator.SetBool("Jumping", false);
                animator.SetBool("DoubleJump", false);
                animator.SetBool("Moving", true);
            }
            else
            {

                animator.SetBool("Jumping", false);
                animator.SetBool("DoubleJump", false);
                animator.SetBool("Moving", false);
            }
        }
        else if(!isGrounded && jumped)
        {
            if(!animator.GetBool("Jumping"))
                animator.SetBool("Jumping", true);

            else if(doubleJumped)
            {
                animator.SetBool("DoubleJump", true);
            }

            animator.SetBool("Moving", false);

        }

        else
        {
            animator.SetBool("Moving", false);
        }
    }

    private void HandleLadder()
    {
        jumped = false;
        animator.SetBool("DoubleJump", false);
        doubleJumped = false;
        animator.SetBool("Jumping", false);
        animator.SetBool("Climbing", true);
        float vertical = Input.GetAxis("Vertical");

        if(isGrounded && vertical < 0)
        {
            vertical = 0;
            pm.SetIsOnLadder(false);
            animator.SetBool("Climbing", false);
        }

        float speed = 10;
        m_Rigidbody2D.gravityScale = 0;
        m_Rigidbody2D.velocity = Vector2.zero;

        Vector2 direction = new Vector2(Input.GetAxis("Horizontal"), vertical);

        transform.Translate(direction * (speed * Time.deltaTime));
    }

    /// <summary>
    /// Checks if the character can jump and executes the movement.
    /// </summary>
    private void CheckAndJump()
    {
        if(tongueJoint.enabled)
        {
            //If anchor is below Bob prevent from jumping
            if(tongueJoint.connectedAnchor.y < transform.position.y)
                return;
            else
                EventManager.TriggerEvent(Names.Events.TongueIn);
        }

        if(isGrounded || !jumped)
        {
            m_Rigidbody2D.velocity = Vector3.zero;
            m_Rigidbody2D.angularVelocity = 0;

            m_Rigidbody2D.AddForce(new Vector2(0f, jumpForce * 1f));

            //audioManager.Play("jump1");
            jumped = true;
            isGrounded = false;
        }
        else if(!doubleJumped)
        {
            m_Rigidbody2D.velocity = Vector3.zero;
            m_Rigidbody2D.angularVelocity = 0;
            doubleJumped = true;
            m_Rigidbody2D.AddForce(new Vector2(0f, jumpForce * 1));
            animator.SetBool("DoubleJump", true);
            //audioManager.Play("jump2");

        }
    }


    public void Flip()
    {
        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        if(isGrounded)
            EventManager.TriggerEvent(Names.Events.TongueIn);

        // Multiply the player's x local scale by -1.
        Vector3 theScale = bodyCollider.transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public bool getFacingRight()
    {
        return isFacingRight;
    }

    public bool getGrounded()
    {
        return isGrounded;
    }

    public void playFootstep()
    {


        if(lastSound == 1)
        {
            audioManager.Play("walk2");
            lastSound = 2;
        }

        else if(lastSound == 2)
        {
            audioManager.Play("walk3");
            lastSound = 3;
        }

        else
        {
            audioManager.Play("walk3");
            lastSound = 1;
        }
    }

    public void playJump()
    {
        audioManager.Play("jump1");
    }

    public void playDoubleJump()
    {
        audioManager.Play("jump2");
    }
}