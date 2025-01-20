using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpspeed = 8f;
    private float direction = 0f;
    private Rigidbody2D player;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRudius;
    public LayerMask groundLayer;
    private bool isTouchingGround;

    [Header("Components")]
    private Animator playerAnimation;
    private Vector3 respawnPoint;
    public GameObject fallDetector;

    [Header("UI & Score")]
    private int Score = 0;
    public int minimumScoreToPass = 3;
    public TextMeshProUGUI ScoreText;
    public HealthBar healthBar;

    [Header("Touch Controls")]
    private Vector2 touchStartPos;
    private bool isTouching = false;
    private float swipeThreshold = 50f;
    private float touchStartTime;
    private float tapTimeThreshold = 0.2f;

    void Start()
    {
        // Initialisation des composants
        player = GetComponent<Rigidbody2D>();
        playerAnimation = GetComponent<Animator>();
        respawnPoint = transform.position;
        UpdateScoreText();

        // Configuration Android
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        isTouchingGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRudius, groundLayer);
        HandleMobileInput();
        UpdatePlayerMovement();
        UpdateAnimations();
    }

    private void HandleMobileInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    touchStartTime = Time.time;
                    isTouching = true;
                    break;

                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        float deltaX = touch.position.x - touchStartPos.x;
                        if (Mathf.Abs(deltaX) > swipeThreshold)
                        {
                            direction = Mathf.Sign(deltaX);
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    float touchDuration = Time.time - touchStartTime;
                    float touchDistance = Vector2.Distance(touch.position, touchStartPos);
                    
                    // Vérifier si c'est un tap rapide pour sauter
                    if (touchDuration <= tapTimeThreshold && touchDistance < swipeThreshold && isTouchingGround)
                    {
                        player.velocity = new Vector2(player.velocity.x, jumpspeed);
                        if (AudioManager.instance != null)
                        {
                            AudioManager.instance.Play("jump");
                        }
                    }
                    
                    isTouching = false;
                    direction = 0f;
                    break;
            }
        }
        else
        {
            direction = 0f;
        }

        // Support du clavier dans l'éditeur
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            direction = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            direction = 1f;
        if (Input.GetKeyDown(KeyCode.Space) && isTouchingGround)
        {
            player.velocity = new Vector2(player.velocity.x, jumpspeed);
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play("jump");
            }
        }
#endif
    }

    private void UpdatePlayerMovement()
    {
        if (direction != 0)
        {
            // Mouvement avec accélération progressive
            float targetVelocityX = direction * speed;
            float smoothTime = 0.05f;
            player.velocity = new Vector2(
                Mathf.Lerp(player.velocity.x, targetVelocityX, smoothTime / Time.deltaTime),
                player.velocity.y
            );
            transform.localScale = new Vector2(direction > 0 ? 0.4662906f : -0.4662906f, 0.4534272f);
        }
        else
        {
            // Décélération progressive
            float currentVelocityX = player.velocity.x;
            player.velocity = new Vector2(
                Mathf.MoveTowards(currentVelocityX, 0, speed * Time.deltaTime * 2),
                player.velocity.y
            );
        }
    }

    private void UpdateAnimations()
    {
        playerAnimation.SetFloat("Speed", Mathf.Abs(player.velocity.x));
        playerAnimation.SetBool("OnGround", isTouchingGround);
        fallDetector.transform.position = new Vector2(transform.position.x, fallDetector.transform.position.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "FallDetector")
        {
            transform.position = respawnPoint;
            healthBar.Damage(0.1f);
            CheckGameOver();
        }
        else if (collision.tag == "Checkpoint")
        {
            respawnPoint = transform.position;
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play("checkpoint");
            }
        }
        else if (collision.tag == "NextLevel")
        {
            if (Score >= minimumScoreToPass)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play("level");
                }
            }
            else
            {
                PlayerManger.error = true;
            }
        }
        else if (collision.tag == "PreviousLevel")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            respawnPoint = transform.position;
        }
        else if (collision.tag == "Crystal")
        {
            Animator crystalAnimator = collision.gameObject.GetComponent<Animator>();
            if (crystalAnimator != null)
            {
                Debug.Log("Animator found. Triggering isCollected.");
                crystalAnimator.SetBool("isCollected", true);
                float animationDuration = crystalAnimator.GetCurrentAnimatorStateInfo(0).length;
                StartCoroutine(DisableAfterAnimation(collision.gameObject, animationDuration));
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play("Crystal");
                }
            }
            Score += 1;
            UpdateScoreText();
            StartCoroutine(AnimateScoreText());
            StartCoroutine(HighlightText());
        }
        else if (collision.tag == "Box")
        {
            HandleBoxCollision(collision);
        }
    }

    private void HandleBoxCollision(Collider2D collision)
    {
        Animator boxAnimator = collision.gameObject.GetComponent<Animator>();
        if (boxAnimator != null)
        {
            Debug.Log("Animator found. Box.");
            float Duration = boxAnimator.GetCurrentAnimatorStateInfo(0).length;
            boxAnimator.SetBool("toucher", true);

            if (Random.value > 0.5f)
            {
                Debug.Log("Box will turn into a Crystal.");
                boxAnimator.SetBool("good", true);
                boxAnimator.SetBool("bad", false);
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play("Crystal");
                }
                Score += 5;
                UpdateScoreText();
                StartCoroutine(AnimateScoreText());
                StartCoroutine(HighlightText());
            }
            else
            {
                Debug.Log("Box will turn into a Spike.");
                boxAnimator.SetBool("good", false);
                boxAnimator.SetBool("bad", true);
                healthBar.Damage(0.1f);
                CheckGameOver();
            }

            StartCoroutine(DisableBoxAfterAnimation(collision.gameObject, Duration));
        }
        else
        {
            Debug.LogWarning("No Animator found on the Box!");
        }
    }

    private void UpdateScoreText()
    {
        ScoreText.text = $" {Score}";
    }

    private IEnumerator AnimateScoreText()
    {
        Vector3 originalScale = ScoreText.transform.localScale;
        ScoreText.transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        ScoreText.transform.localScale = originalScale;
    }

    private IEnumerator HighlightText()
    {
        Color originalColor = ScoreText.color;
        ScoreText.color = Color.yellow;
        yield return new WaitForSeconds(0.2f);
        ScoreText.color = originalColor;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "spike")
        {
            healthBar.Damage(0.002f);
            CheckGameOver();
        }
    }

    private IEnumerator DisableAfterAnimation(GameObject crystal, float delay)
    {
        yield return new WaitForSeconds(delay + 0.3f);
        crystal.SetActive(false);
    }

    private IEnumerator DisableBoxAfterAnimation(GameObject box, float delay)
    {
        yield return new WaitForSeconds(delay + 1f);
        Debug.Log($"Disabling the box: {box.name}");
        box.SetActive(false);
    }

    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    private void CheckGameOver()
    {
        if (Health.totalHealth <= 0)
        {
            PlayerManger.isGameOver = true;
            gameObject.SetActive(false);
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play("GameOver");
            }
            Debug.Log("Game Over triggered!");
        }
    }
}