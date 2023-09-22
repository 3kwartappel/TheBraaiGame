using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

[SuppressMessage("ReSharper", "CheckNamespace")]
public class BraaiGame : MonoBehaviour
{
    public GameObject meat;
    public GameObject smoke;
    public GameObject fireBurn;
    public Text turnTimerText;
    public Text scoreText;
    public Text endGameTimerText;
    public Text endGameText;
    public Text startGameText;
    public Image gameTitle;
    public Image howToImage1;
    //public Image howToImage2;
    public Button startButton;
    public Button toggleMusic;
    public Button restartButton;
    public Button marinaButton;
    public InputField playerNameInput;
    public Button submitButton;
    public Button meatTurnButton;
    public AudioSource musicSource;
    public AudioSource scoreSound;
    public AudioSource shakeSound;
    public AudioSource audioLightFire;
    public AudioSource audioBurnMeat;
    public AudioSource audioFireCrackle;

    private float _countdownTime = 5f;
    private float _score;
    private float _gameTime = 60f; //60f
    private readonly float flipSpeed = 80f;
    private readonly float resetTime = 5f;
    private float _lastClickTime;
    public float vibrationDuration = 0.1f; // Adjust the duration of the vibration

    private bool _isGameEnded;
    private bool isMusicOn;

    private HighScoreService _highScoreManager;

    private bool _canFlip = true;
    private float _flipCooldown = 1.0f;

    private Quaternion initialRotation;

    public Secrets secrets;

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            meatTurnButton.onClick.AddListener(AddMarinaPoints);
            meatTurnButton.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);

        }
        else
        {
            initialRotation = transform.rotation;
            //disable auto rotation
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }

        submitButton.onClick.AddListener(SubmitHighScore);
        restartButton.onClick.AddListener(Restart);
        marinaButton.onClick.AddListener(AddMarinaPoints);
        toggleMusic.onClick.AddListener(ToggleMusicPoints);

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            meatTurnButton.onClick.AddListener(TurnMeat);
        }

        Input.gyro.enabled = true;

        //pause game
        Time.timeScale = 0;

        startButton.onClick.AddListener(StartGame);
        startButton.gameObject.SetActive(true);
        startGameText.gameObject.SetActive(true);
        startGameText.text = "Welcome to the Braai Game! \n\n" +
                             "The aim of the game is to braai. \n\n" +
                             "Flip the phone upside-down precisely every 5 seconds. \n\n" +
                             "Good luck!";

        toggleMusic.gameObject.SetActive(false);
        meat.gameObject.SetActive(false);
        endGameText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);
        playerNameInput.gameObject.SetActive(false);
        turnTimerText.gameObject.SetActive(false);
        marinaButton.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        endGameTimerText.gameObject.SetActive(false);
    }

    private void StartGame()
    {
        Time.timeScale = 1;

        howToImage1.gameObject.SetActive(false);
        //howToImage2.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        startGameText.gameObject.SetActive(false);
        gameTitle.gameObject.SetActive(false);

        audioLightFire.Play();
        audioFireCrackle.Play();

        Restart();
    }

    private void Restart()
    {
        meat.transform.rotation = Quaternion.Euler(0, 90, -45);
        fireBurn.SetActive(false);
        audioBurnMeat.Stop();


        endGameText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);
        playerNameInput.gameObject.SetActive(false);

        smoke.SetActive(true);
        meat.gameObject.SetActive(true);
        endGameTimerText.gameObject.SetActive(true);
        turnTimerText.gameObject.SetActive(true);
        marinaButton.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        toggleMusic.gameObject.SetActive(true);

        _isGameEnded = false;
        _gameTime = 60f; // Reset game time

        //if the music is playing, then the user should start with 2 points, othwerwise 0
        _score = isMusicOn ? 2 : 0;

        _countdownTime = 5f; // Reset countdown time
        _lastClickTime = 0f; // Reset last click time
        _canFlip = true; // Reset can flip
        _flipCooldown = 1.0f; // Reset flip cooldown

        playerNameInput.characterLimit = 20;
        scoreText.text = "Score: 0";

        Screen.sleepTimeout = SleepTimeout.NeverSleep;


        _highScoreManager = new HighScoreService();
    }


    //// Check if the phone has rotated 180 degrees (or more) to the side or
    /// and return true if it has
    /// it should feel like the user is flipping the phone to the side
    /// the user should not be able to flip the phone 1sec after they have flipped it
    private bool HasFlippedPhone()
    {
        if (_canFlip)
        {
            if (Input.deviceOrientation == DeviceOrientation.FaceDown)
            {
                _canFlip = false;
                return true;
            }
        }
        else
        {
            _flipCooldown -= Time.deltaTime;
            if (_flipCooldown <= 0.0f)
            {
                _canFlip = true;
                _flipCooldown = 1.0f;
            }
        }

        return false;
    }

    // ReSharper disable once UnusedMember.Local
    private async void Update()
    {
        // Countdown timer and scoring
        _countdownTime -= Time.deltaTime;


        endGameTimerText.text = "Eat in: " + _gameTime.ToString("F0") + "s";

        _gameTime -= Time.deltaTime;

        if (_gameTime <= 0f && !_isGameEnded)
        {
            _gameTime = 0f;
            _isGameEnded = true;
            await EndgameRoutine();
        }
        else if (HasFlippedPhone())
        {
            audioBurnMeat.Stop();
            if (_countdownTime > 0.5f)
            {
                turnTimerText.text = "Turned too early";
            }
            TurnMeat();
        }
        else if (_countdownTime <= 0f)
        {
            turnTimerText.text = "Turned too late";
        }
        else if (_countdownTime <= 0.5f && _countdownTime > 0f)
        {
            audioBurnMeat.Play();
            fireBurn.SetActive(true);
            turnTimerText.color = Color.red;
            turnTimerText.text = "Turn now!";
        }
        else
        {
            turnTimerText.text = "Turn in " + _countdownTime.ToString("F0") + "s";
        }


    }

    private IEnumerator Flip(GameObject target)
    {
        Quaternion startRotation = target.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 180);
        float elapsedTime = 0f;

        while (elapsedTime < flipSpeed / 1000f)
        {
            target.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / (flipSpeed / 1000f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.transform.rotation = endRotation;
    }


    public void TurnMeat()
    {
        if (_countdownTime <= 0.5 && _countdownTime > 0)
        {
            _score++;
            scoreSound.Play();

            scoreText.text = "Score: " + _score.ToString("F2");

        }

        _countdownTime = 5f;
        _lastClickTime = Time.time; // update last click time

        fireBurn.SetActive(false);
        audioBurnMeat.Stop();
        turnTimerText.color = Color.white;

        // check if enough time has passed since last click
        if (Time.time - _lastClickTime >= resetTime)
        {
            _countdownTime = 5f;
            _lastClickTime = Time.time;
        }

        StartCoroutine(Flip(meat));
    }


    private async Task EndgameRoutine()
    {
        // Hide other game objects and UI elements
        turnTimerText.gameObject.SetActive(false);
        fireBurn.gameObject.SetActive(false);
        smoke.gameObject.SetActive(false);
        endGameTimerText.gameObject.SetActive(false);
        marinaButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(true);


        audioBurnMeat.Stop();
        audioFireCrackle.Stop();
        audioLightFire.Stop();

        //show the high score list
        List<HighScore> highScores = await _highScoreManager.FetchHighScores(secrets);

        endGameText.gameObject.SetActive(true);
        endGameText.text = "Loading High scores..";

        string highScoreString = BuildHighScoreEndText(highScores);


        //determine if the user has a high score and show input
        float userScore = _score;
        var isHighScore = _highScoreManager.IsHighScore(userScore, highScores);
        submitButton.gameObject.SetActive(isHighScore);
        playerNameInput.gameObject.SetActive(isHighScore);
        if (isHighScore)
        {
            endGameText.text = "Well done - New Score!" + "\n" + highScoreString;
        }
        else
        {
            endGameText.text = "Top 10 " + "\n" + highScoreString;
        }

        await Task.Delay(10000);
    }

    private string BuildHighScoreEndText(List<HighScore> highScores)
    {
        string highScoreString = "";
        int position = 1;
        foreach (HighScore highScore in highScores)
        {
            highScoreString += position + ". " + highScore.name + " - " + highScore.score + "\n";
            position++;
        }

        return highScoreString;
    }

    private async void SubmitHighScore()
    {
        //get the name from the input field
        string playerName = playerNameInput.text;

        //get the score from the score text
        double userScore = _score;

        //get the high scores from the high score manager
        List<HighScore> highScores = await _highScoreManager.FetchHighScores(secrets);

        //add the new high score to the list
        highScores = _highScoreManager.AddHighScore(highScores, playerName, userScore);

        //update the high scores on the server
        await _highScoreManager.UpdateHighScores(highScores, secrets);

        // update game score text
        string highScoreString = BuildHighScoreEndText(highScores);
        endGameText.text = "Top 10 " + "\n" + highScoreString;

        //hide the submit button
        submitButton.gameObject.SetActive(false);
        playerNameInput.gameObject.SetActive(false);
    }

    private void AddMarinaPoints()
    {
        if (!shakeSound.isPlaying)
        {
            shakeSound.Play();
        }
        _score += 0.01f;

        scoreText.text = "Score: " + _score.ToString("F2");
        StartCoroutine(EnlargeMarina());
    }

    private IEnumerator EnlargeMarina()
    {
        marinaButton.transform.localScale = new Vector3(2.2f, 12f, 1f);
        yield return new WaitForSeconds(0.05f);
        marinaButton.transform.localScale = new Vector3(2f, 10f, 1f);
    }

    private void ToggleMusicPoints()
    {
        if (isMusicOn)
        {
            // turn off music
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }
            isMusicOn = false;
            _score -= 2.00f;
        }
        else
        {
            // turn off on
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }

            isMusicOn = true;
            _score += 2.00f;
        }

        scoreText.text = "Score: " + _score.ToString("F2");
    }
}
