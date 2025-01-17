using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidBody;
    Animator animator;
    public float speed = 5.0f;
    public float jumpForce = 9.0f;
    public float airControlForce = 10.0f;
    public float airControlMax = 1.5f;
    Vector2 boxExtents;

    public Vector3 movement;

    public AudioSource coinSound;
    public TextMeshProUGUI uiText;
    public TextMeshProUGUI levelCompleteText; //level end text
    int totalCoins;
    int coinsCollected;

    string curLevel;
    string nextLevel;

    // Start is called before the first frame update
    void Start()
    {
        // get the extent of the collision box
        boxExtents = GetComponent<BoxCollider2D>().bounds.extents;
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
       
        // find out how many coins in the level
        coinsCollected = 0;
        totalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;

        curLevel = SceneManager.GetActiveScene().name;
        if (curLevel == "Level1")
            nextLevel = "Level2";
        else if (curLevel == "Level2")
            nextLevel = "Finished";

    }

    // Update is called once per frame
    void Update()
    {
        string uiString = "x " + coinsCollected + "/" + totalCoins;
        uiText.text = uiString;

        float xSpeed = Mathf.Abs(rigidBody.velocity.x);
        animator.SetFloat("xSpeed", xSpeed); //for running animation

        float ySpeed = rigidBody.velocity.y;
        animator.SetFloat("ySpeed", ySpeed); //for jumping animation

        float blinkVal = Random.Range(0.0f, 200.0f);
        if (blinkVal < 1.0f)
            animator.SetTrigger("blinkTrigger"); //for blinking animation

    }
    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        // check if we are on the ground
        Vector2 bottom =
        new Vector2(transform.position.x, transform.position.y - boxExtents.y);

        Vector2 hitBoxSize = new Vector2(boxExtents.x * 2.0f, 0.05f);

        RaycastHit2D result = Physics2D.BoxCast(bottom, hitBoxSize, 0.0f,
        new Vector3(0.0f, -1.0f), 0.0f, 1 << LayerMask.NameToLayer("Ground"));

        bool grounded = result.collider != null && result.normal.y > 0.9f;
        if (grounded)
        {

            // Horizontal movement when grounded, multiplied by the speed modifier
            Vector2 movement = new Vector2(h * speed, rigidBody.velocity.y);
            rigidBody.velocity = movement;

            if (Input.GetAxis("Jump") > 0.0f)
                rigidBody.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
        }
        else
        {
            // allow a small amount of movement in the air
            float vx = rigidBody.velocity.x;
            if (h * vx < airControlMax)
            {
                rigidBody.AddForce(new Vector2(h * airControlForce, 0));
            }
        }

        if (rigidBody.velocity.x * transform.localScale.x < 0.0f) //check for sprite flipping
        {
            //flip sprite based on direction
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Coin")
        {
            Destroy(coll.gameObject);
            coinSound.Play();
            coinsCollected++;
        }
        if (coll.gameObject.tag == "Level End")
        {
            // hide the level end object
            coll.gameObject.SetActive(false);
            StartCoroutine(LoadNextLevel());
        }

    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Death")
        {
            StartCoroutine(DoDeath());
        }
    }

    IEnumerator DoDeath()
    {
        // freeze the rigidbody
        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        // hide the player
        GetComponent<Renderer>().enabled = false;
        // reload the level in 2 seconds
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(curLevel);
    }
    
    IEnumerator LoadNextLevel()
    {
        if (nextLevel != "Finished")
        {
            // Show "Level Complete" text
            levelCompleteText.gameObject.SetActive(true);

            // hide the player
            GetComponent<Renderer>().enabled = false;

            yield return new WaitForSeconds(2);
            SceneManager.LoadScene(nextLevel);
        }
        else
        {
            //Game is finished 
            StartCoroutine(EndGame()); 
        }
    }

    IEnumerator EndGame()
    {
        // Optionally hide the player or display a "Game Over" screen
        GetComponent<Renderer>().enabled = false;

        // Display "Game Finished" message or credits
        levelCompleteText.text = "Game Complete!";
        levelCompleteText.gameObject.SetActive(true);
        
        // Wait for a few seconds before going back to main menu or quitting
        yield return new WaitForSeconds(2);

        // Go back to the main menu or quit the game
        SceneManager.LoadScene("Level1"); //resets game to level 1 temporarily

        // Application.Quit();  //to quit application but will only work on built game
    }



}