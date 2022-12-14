using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
public class playerMovement : MonoBehaviour
{

    //Player Config
    public float hSpeed;
    public float vSpeed;
    public float jumpForce;
    public Animator animator;
    private float runMultiplier;
    private float H;
    private float V;
    private Rigidbody2D rb;
    public static Vector2 playerPos;
    public static Vector2 playerVel;
    private bool canJump;
    private bool hasJumped;
    private bool grounded = false;
    private Vector2 startPos;
    private bool canScroll = true;
    public TMP_Text selectedName;
    public float deathZone = -20;


    //Throw Stuff
    public float throwConstant = 1;
    private float throwForce;
    private float throwTimer = 0;
    public float throwTime;
    private bool holdingThrow = false;
    private int selectExplosive = 0;
    private List<GameObject> stickies = new List<GameObject>();
<<<<<<< HEAD
    private string direction = "";
=======
    private int airbornCounter = 0;
>>>>>>> jaime-

    //World 
    private Vector2 normalGrav;

    //Object Stuff
    private GameObject lastTouched;
    public GameObject Grenade;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        normalGrav = Physics2D.gravity;
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Speed", 0);
        Move();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        if (Input.GetMouseButtonDown(0))
        {
            throwTimer = 0;
            holdingThrow = true;
        }
        if (holdingThrow == true)
        {
            ThrowProjectile();
        }

        if (Input.inputString != "")
        {
            int number;
            bool is_a_number = Int32.TryParse(Input.inputString, out number);
            if (is_a_number && number >= 1 && number < 10)
            {
                selectExplosive = number - 1;
                selectedName.alpha = 1;
            }

        }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0 && canScroll == true)
            {
                if(scroll > 0 && selectExplosive < 2) { selectExplosive += 1; }
                else if (scroll > 0) { selectExplosive = 0; }
                if(scroll < 0 && selectExplosive > 0) { selectExplosive -= 1; }
                else if(scroll < 0) { selectExplosive = 2; }
                selectedName.alpha = 1;
                canScroll = false;
            }
            else { canScroll = true; }

        switch(selectExplosive)
        {
            case 0:
                selectedName.text = "Rat";
                break;
            case 1:
                selectedName.text = "Cat";
                break;
            case 2:
                selectedName.text = "Crocodile";
                break;
        }
        
        if(selectedName.alpha >= 0) { selectedName.alpha -= Time.deltaTime; }
        

        if(transform.position.y < deathZone) { rb.velocity = Vector2.zero; transform.position = startPos; }
    }

    private void Move()
    {
        H = Input.GetAxisRaw("Horizontal");
        V = Input.GetAxisRaw("Vertical");
        if (H != 0)
        {
            if (Mathf.Abs(rb.velocity.x) < hSpeed || Mathf.Sign(H) != Mathf.Sign(rb.velocity.x))
            {
                rb.velocity = new Vector2(H * hSpeed, rb.velocity.y);
            }
        }
        else
        {
            if(grounded == true) { rb.velocity = new Vector2(rb.velocity.x * 0.5f, rb.velocity.y); }
        }
        if (V < 0) { Physics2D.gravity = normalGrav * 1.5f; }
        else { Physics2D.gravity = normalGrav; }

        if (rb.velocity.y > vSpeed) { rb.velocity = new Vector2(rb.velocity.x , rb.velocity.y - 0.5f); }

        if(rb.velocity.x < 0)
        {
            transform.GetComponent<SpriteRenderer>().flipX = true;
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            direction = "left";
        }
        else if(rb.velocity.x > 0)
        {
            transform.GetComponent<SpriteRenderer>().flipX = false;
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            direction = "right";
        }

            //if velocity.x > 0 then transform.GetComponent<SpriteRenderer>().flipX = true
    }

    private void Jump()
    {
        if(canJump == true)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            canJump = false;
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ground" )
        {
            lastTouched = null;
            canJump = true;
            grounded = true;
            airbornCounter = 0;
        }
        if (collision.transform.tag == "Wall")
        {
 
            if(collision.gameObject != lastTouched)
            {
                lastTouched = collision.transform.gameObject;
                canJump = true;
            }

            airbornCounter = 0;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.transform.tag == "Ground") { lastTouched = null; }
        grounded = false;
        canJump = false;
    }
    private void ThrowProjectile()
    {
        if (throwTimer <= throwTime)
        {
            throwTimer += Time.deltaTime;
            throwForce = throwTimer / throwTime;
            //Debug.Log(throwForce);
        }
        if(Input.GetMouseButtonUp(0))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (selectExplosive == 1 && stickies.Count >= 3) { Destroy(stickies[0].gameObject); stickies.RemoveAt(0); }
            var newGrenade = Instantiate(Grenade, transform.position, Quaternion.identity);
            var grenadeScript = newGrenade.GetComponent<Projectile>();
            grenadeScript.type = (Projectile.ProjectileType)selectExplosive;
            Vector3 difference = new Vector2(mousePos.x - newGrenade.transform.position.x, mousePos.y - newGrenade.transform.position.y);

            newGrenade.transform.position = newGrenade.transform.position + difference.normalized;
            if(grenadeScript.type != Projectile.ProjectileType.Rocket) { newGrenade.GetComponent<Rigidbody2D>().velocity = difference.normalized * throwConstant * throwForce; }
            else { newGrenade.GetComponent<Rigidbody2D>().velocity = difference.normalized * throwConstant; }
            
            Physics2D.IgnoreCollision(newGrenade.transform.GetComponent<Collider2D>(), GetComponent<Collider2D>());

            if(grenadeScript.type == Projectile.ProjectileType.Sticky) { stickies.Add(newGrenade.gameObject); }
            holdingThrow = false;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Explosion")
        {
            rb.AddForce((new Vector2(transform.position.x - collision.transform.position.x , transform.position.y - collision.transform.position.y).normalized + new Vector2(0, 0.1f)) * collision.transform.GetComponent<Explosion>().explosiveForce);
            
        }
    }
}
