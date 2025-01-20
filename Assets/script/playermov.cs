using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    public float speed = 5f;
    public float jumpspeed = 8f;
    private float direction = 0f;
    private Rigidbody2D player;
    public Transform groundCheck;
    public float groundCheckRudius;
    public LayerMask groundLayer;
    private bool isTouchingGround;
    private Animator playerAnimation;
    private Vector3 respawnPoint;
    public GameObject fallDetector;
    private int Score = 0;
    public int minimumScoreToPass = 12;
    public TextMeshProUGUI ScoreText;
    public HealthBar healthBar;
    public TextMeshProUGUI messageText;
    private float messageDisplayTime = 2f;
    private Coroutine messageCoroutine;

    // Variables pour le contrôle tactile
    private float screenMiddle;
    private float lastTapTime;
    private float doubleTapTimeThreshold = 0.3f;

    void Start()
    {
        player = GetComponent<Rigidbody2D>();   
        playerAnimation = GetComponent<Animator>(); 
        respawnPoint = transform.position;
        UpdateScoreText();
        Health.totalHealth = 1.0f;
        healthBar.SetSize(1.0f);

        // Calculer le milieu de l'écran
        screenMiddle = Screen.width / 2;
        lastTapTime = 0f;
    }

    void Update()
    {   
        isTouchingGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRudius, groundLayer);
        
        // Gestion des touches
        HandleTouchInput();

        // Appliquer le mouvement
        if (direction != 0)
        {
            player.velocity = new Vector2(direction * speed, player.velocity.y);
            transform.localScale = new Vector2(direction > 0 ? 0.4662906f : -0.4662906f, 0.4534272f);
        }
        else
        {
            player.velocity = new Vector2(0, player.velocity.y);
        }

        playerAnimation.SetFloat("Speed", Mathf.Abs(player.velocity.x));
        playerAnimation.SetBool("OnGround", isTouchingGround);
        fallDetector.transform.position = new Vector2(transform.position.x, fallDetector.transform.position.y);
    }

    private void HandleTouchInput()
    {
        // Réinitialiser la direction
        direction = 0;

        // Vérifier chaque touche
        foreach (Touch touch in Input.touches)
        {
            // Pour les nouveaux touchés
            if (touch.phase == TouchPhase.Began)
            {
                float timeSinceLastTap = Time.time - lastTapTime;
                
                // Vérifier si c'est un double tap
                if (timeSinceLastTap <= doubleTapTimeThreshold && isTouchingGround)
                {
                    Jump();
                }
                
                lastTapTime = Time.time;
            }

            // Gérer le mouvement horizontal
            if (touch.position.x < screenMiddle)
            {
                direction = -1f;
            }
            else
            {
                direction = 1f;
            }
        }

#if UNITY_EDITOR
        // Support du clavier dans l'éditeur pour les tests
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            direction = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            direction = 1f;
        if (Input.GetKeyDown(KeyCode.Space) && isTouchingGround)
            Jump();
#endif
    }

    private void Jump()
    {
        if (isTouchingGround)
        {
            player.velocity = new Vector2(player.velocity.x, jumpspeed);
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play("jump");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "FallDetector")
        {
            transform.position = respawnPoint;
            healthBar.Damage(0.1f);
            CheckGameOver();
        }
        else if(collision.tag == "Checkpoint")
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
        else if(collision.tag == "Crystal")
        {
            Animator crystalAnimator = collision.gameObject.GetComponent<Animator>();
            if (crystalAnimator != null)
            {
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
        else if(collision.tag == "Box")
        {
            HandleBoxCollision(collision);
        }
    }

    private void HandleBoxCollision(Collider2D collision)
    {
        Animator boxAnimator = collision.gameObject.GetComponent<Animator>();
        if (boxAnimator != null)
        {
            float Duration = boxAnimator.GetCurrentAnimatorStateInfo(0).length;
            boxAnimator.SetBool("toucher", true);

            if (Random.value > 0.5f)
            {
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
                boxAnimator.SetBool("good", false);
                boxAnimator.SetBool("bad", true);
                healthBar.Damage(0.1f);
                CheckGameOver();
            }

            StartCoroutine(DisableBoxAfterAnimation(collision.gameObject, Duration));
        }
    }

    private void ShowScoreMessage()
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        string message = $"Il faut {minimumScoreToPass} cristaux pour passer ! (Actuel : {Score})";
        messageCoroutine = StartCoroutine(ShowMessage(message, messageDisplayTime));
    }

    private IEnumerator ShowMessage(string message, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            messageText.gameObject.SetActive(false);
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
        if(collision.tag == "spike")
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
        }
    }
}