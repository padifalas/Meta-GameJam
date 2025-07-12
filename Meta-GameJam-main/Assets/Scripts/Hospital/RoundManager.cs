using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    [Header("round settings")]
    [Space(5)]
    public int collectiblesToWin = 3; // need 3 to progress
    [HideInInspector] public RoundType currentRound = RoundType.Hospital; // public for other scripts to check
    
    [Header("round areas")]
    [Space(5)]
    public Transform hospitalArea;
    public Transform forestArea;
    public Transform parkingLotArea;
    public Transform[] playerSpawnPoints; // spawn points for each round [hospital1, hospital2, forest1, forest2, parking1, parking2]
    
    [Header("area spawners")]
    [Space(5)]
    public CollectibleSpawner hospitalSpawner;
    public CollectibleSpawner forestSpawner;
    public CollectibleSpawner parkingLotSpawner;
    
    [Header("player references")]
    [Space(5)]
    public MonoBehaviour player1; // can be FirstPersonControls or FPC2
    public MonoBehaviour player2; // can be FirstPersonControls or FPC2
    
    [Header("player hat system")]
    [Space(5)]
    public GameObject player1GnomeHat; // hospital round hat
    public GameObject player1MushroomHat; // forest round hat
    public GameObject player1CowboyHat; // parking lot round hat
    public GameObject player2GnomeHat; // hospital round hat
    public GameObject player2MushroomHat; // forest round hat
    public GameObject player2CowboyHat; // parking lot round hat
    
    [Header("win effects")]
    [Space(5)]
    public GameObject[] winningLights; // lights that activate for winner
    public GameObject winPortal; // portal effect for winner
    public float winPanelDuration = 5f; // how long win panels show
    public float startImageDuration = 2f; // how long start image shows
    
    [Header("ui references")]
    [Space(5)]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public GameObject player1WinPanel; // player 1 win image/panel
    public GameObject player2WinPanel; // player 2 win image/panel
    public GameObject startImage; // "start" image for beginning of rounds
    public Text roundWinsText; // shows overall wins (e.g., "P1: 1  P2: 0")
    public Text finalWinnerText; // shows final game winner
    
    // round scoring
    private int player1Score = 0; // pills collected this round
    private int player2Score = 0; // pills collected this round
    private int player1RoundWins = 0; // rounds won
    private int player2RoundWins = 0; // rounds won
    private bool roundEnded = false;
    private bool gameEnded = false;
    private bool playersCanMove = true; // control player movement
    
    public enum RoundType
    {
        Hospital,
        Forest,
        ParkingLot
    }
    
    private void Start()
    {
        // hide all ui panels initially
        if (player1WinPanel != null) player1WinPanel.SetActive(false);
        if (player2WinPanel != null) player2WinPanel.SetActive(false);
        if (startImage != null) startImage.SetActive(false);
        if (finalWinnerText != null) finalWinnerText.gameObject.SetActive(false);
        
        // hide win effects
        if (winPortal != null) winPortal.SetActive(false);
        foreach (GameObject light in winningLights)
        {
            if (light != null) light.SetActive(false);
        }
        
        // start in hospital round
        SetupRound(RoundType.Hospital);
        UpdateScoreUI();
        UpdateRoundWinsUI();
        
        // ensure hospital hats are shown at game start
        UpdatePlayerHats(RoundType.Hospital);
        
        // show start image for hospital round
        StartCoroutine(ShowStartSequence());
    }
    
    private IEnumerator ShowStartSequence()
    {
        // disable player movement
        SetPlayersCanMove(false);
        
        // show start image
        if (startImage != null)
        {
            startImage.SetActive(true);
            Debug.Log("showing start image for " + startImageDuration + " seconds");
        }
        
        yield return new WaitForSeconds(startImageDuration);
        
        // hide start image and enable movement
        if (startImage != null) startImage.SetActive(false);
        SetPlayersCanMove(true);
        
        Debug.Log("round started! players can now move");
    }
    
    private void SetPlayersCanMove(bool canMove)
    {
        playersCanMove = canMove;
        
        // disable/enable player input based on canMove
        if (player1 != null)
        {
            MonoBehaviour[] scripts = player1.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script.GetType().Name == "FirstPersonControls" || script.GetType().Name == "FPC2")
                {
                    script.enabled = canMove;
                }
            }
        }
        
        if (player2 != null)
        {
            MonoBehaviour[] scripts = player2.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script.GetType().Name == "FirstPersonControls" || script.GetType().Name == "FPC2")
                {
                    script.enabled = canMove;
                }
            }
        }
        
        Debug.Log("players can move: " + canMove);
    }
    
    private void SetupRound(RoundType roundType)
    {
        currentRound = roundType;
        Debug.Log($"setting up {roundType} round");
        
        // activate appropriate area spawner
        if (hospitalSpawner != null) hospitalSpawner.gameObject.SetActive(roundType == RoundType.Hospital);
        if (forestSpawner != null) forestSpawner.gameObject.SetActive(roundType == RoundType.Forest);
        if (parkingLotSpawner != null) parkingLotSpawner.gameObject.SetActive(roundType == RoundType.ParkingLot);
        
        // handle chaser activation - only active in forest round
        ChaserManager chaserManager = FindObjectOfType<ChaserManager>();
        if (chaserManager != null)
        {
            if (roundType == RoundType.Forest)
            {
                chaserManager.gameObject.SetActive(true);
                Debug.Log("chasers activated for forest round");
            }
            else
            {
                chaserManager.SetChasersActive(false);
                Debug.Log($"chasers disabled for {roundType} round");
            }
        }
        
        // move players to appropriate spawn points
        MovePlayersToRoundStart(roundType);
        
        // clear any existing effects
        ClearPlayerEffects();
        
        // revive players if they died in previous round
        RevivePlayers();
        
        // update player hats for current round
        UpdatePlayerHats(roundType);
    }
    
    private void MovePlayersToRoundStart(RoundType roundType)
    {
        if (playerSpawnPoints == null || playerSpawnPoints.Length < 6) return;
        
        int spawnIndex = (int)roundType * 2; // hospital=0,1 forest=2,3 parking=4,5
        
        // move player 1
        if (player1 != null && playerSpawnPoints[spawnIndex] != null)
        {
            player1.GetComponent<CharacterController>().enabled = false;
            player1.transform.position = playerSpawnPoints[spawnIndex].position;
            player1.transform.rotation = playerSpawnPoints[spawnIndex].rotation;
            player1.GetComponent<CharacterController>().enabled = true;
            Debug.Log($"moved player 1 to {roundType} spawn point");
        }
        
        // move player 2  
        if (player2 != null && playerSpawnPoints[spawnIndex + 1] != null)
        {
            player2.GetComponent<CharacterController>().enabled = false;
            player2.transform.position = playerSpawnPoints[spawnIndex + 1].position;
            player2.transform.rotation = playerSpawnPoints[spawnIndex + 1].rotation;
            player2.GetComponent<CharacterController>().enabled = true;
            Debug.Log($"moved player 2 to {roundType} spawn point");
        }
    }
    
    private void ClearPlayerEffects()
    {
        // clear any active effects from previous round
        if (player1 != null)
        {
            EffectManager p1Effects = player1.GetComponent<EffectManager>();
            if (p1Effects != null) p1Effects.CureAllEffects();
        }
        
        if (player2 != null)
        {
            EffectManager p2Effects = player2.GetComponent<EffectManager>();
            if (p2Effects != null) p2Effects.CureAllEffects();
        }
    }
    
    private void RevivePlayers()
    {
        // revive both players at start of each round
        if (player1 != null)
        {
            HealthSystem p1Health = player1.GetComponent<HealthSystem>();
            if (p1Health != null) p1Health.Revive();
        }
        
        if (player2 != null)
        {
            HealthSystem p2Health = player2.GetComponent<HealthSystem>();
            if (p2Health != null) p2Health.Revive();
        }
    }
    
    // called when player collects pill (hospital round)
    public void OnCollectibleGathered(MonoBehaviour player)
    {
        if (roundEnded || !playersCanMove) return;
        
        if (player == player1)
        {
            player1Score++;
            Debug.Log($"player 1 collected pill! score: {player1Score}");
        }
        else if (player == player2)
        {
            player2Score++;
            Debug.Log($"player 2 collected pill! score: {player2Score}");
        }
        
        UpdateScoreUI();
        
        // check for win condition
        if (player1Score >= collectiblesToWin)
        {
            OnRoundWin(player1, "player 1");
        }
        else if (player2Score >= collectiblesToWin)
        {
            OnRoundWin(player2, "player 2");
        }
    }
    
    // called when player reaches finish line (forest round)
    public void OnFinishLineReached(MonoBehaviour player)
    {
        if (roundEnded || !playersCanMove || currentRound != RoundType.Forest) return;
        
        string winnerName = (player == player1) ? "player 1" : "player 2";
        Debug.Log($"{winnerName} reached the finish line first!");
        
        OnRoundWin(player, winnerName);
    }
    
    // called when player dies (forest or parking lot rounds)
    public void OnPlayerDeath(MonoBehaviour deadPlayer)
    {
        // allow death to end round in both forest (chaser kills) and parking lot (combat)
        if (roundEnded || !playersCanMove) return;
        
        MonoBehaviour winner = (deadPlayer == player1) ? player2 : player1;
        string winnerName = (winner == player1) ? "player 1" : "player 2";
        
        if (currentRound == RoundType.Forest)
        {
            Debug.Log($"{winnerName} wins forest round - {deadPlayer.name} killed by chaser!");
        }
        else if (currentRound == RoundType.ParkingLot)
        {
            Debug.Log($"{winnerName} wins parking lot round - {deadPlayer.name} eliminated in combat!");
        }
        
        OnRoundWin(winner, winnerName);
    }
    
    private void OnRoundWin(MonoBehaviour winner, string winnerName)
    {
        if (roundEnded) return;
        roundEnded = true;
        
        Debug.Log($"{winnerName} wins the {currentRound} round!");
        
        // update round wins
        if (winner == player1)
            player1RoundWins++;
        else
            player2RoundWins++;
        
        UpdateRoundWinsUI();
        
        // disable player movement
        SetPlayersCanMove(false);
        
        // show win panel and handle progression
        StartCoroutine(HandleRoundWinSequence(winner, winnerName));
    }
    
    private IEnumerator HandleRoundWinSequence(MonoBehaviour winner, string winnerName)
    {
        // activate winning effects
        foreach (GameObject light in winningLights)
        {
            if (light != null) light.SetActive(true);
        }
        if (winPortal != null) winPortal.SetActive(true);
        
        // show appropriate win panel
        if (winner == player1 && player1WinPanel != null)
        {
            player1WinPanel.SetActive(true);
            Debug.Log("showing player 1 win panel");
        }
        else if (winner == player2 && player2WinPanel != null)
        {
            player2WinPanel.SetActive(true);
            Debug.Log("showing player 2 win panel");
        }
        
        // wait for win panel duration
        yield return new WaitForSeconds(winPanelDuration);
        
        // hide win panel and effects
        if (player1WinPanel != null) player1WinPanel.SetActive(false);
        if (player2WinPanel != null) player2WinPanel.SetActive(false);
        if (winPortal != null) winPortal.SetActive(false);
        foreach (GameObject light in winningLights)
        {
            if (light != null) light.SetActive(false);
        }
        
        // check if all 3 rounds are complete
        if (player1RoundWins + player2RoundWins >= 3)
        {
            HandleGameCompletion();
        }
        else
        {
            // progress to next round
            ProgressToNextRound();
        }
    }
    
    private void ProgressToNextRound()
    {
        RoundType nextRound = GetNextRound();
        
        Debug.Log($"progressing from {currentRound} to {nextRound}");
        
        // reset round scores (not round wins)
        player1Score = 0;
        player2Score = 0;
        roundEnded = false;
        
        // setup next round
        SetupRound(nextRound);
        UpdateScoreUI();
        
        // show start sequence for new round
        StartCoroutine(ShowStartSequence());
    }
    
    private RoundType GetNextRound()
    {
        switch (currentRound)
        {
            case RoundType.Hospital:
                return RoundType.Forest;
            case RoundType.Forest:
                return RoundType.ParkingLot;
            case RoundType.ParkingLot:
                return RoundType.ParkingLot; // shouldn't happen
            default:
                return RoundType.Hospital;
        }
    }
    
    private void HandleGameCompletion()
    {
        gameEnded = true;
        
        // determine winner based on who won more rounds
        string finalWinner;
        int winnerRounds;
        
        if (player1RoundWins > player2RoundWins)
        {
            finalWinner = "player 1";
            winnerRounds = player1RoundWins;
        }
        else
        {
            finalWinner = "player 2"; 
            winnerRounds = player2RoundWins;
        }
        
        Debug.Log($"GAME OVER! {finalWinner} wins overall with {winnerRounds} round wins out of 3!");
        
        // show final winner message
        if (finalWinnerText != null)
        {
            finalWinnerText.text = $"{finalWinner} wins the game!\nFinal Score: {player1RoundWins} - {player2RoundWins}";
            finalWinnerText.gameObject.SetActive(true);
        }
        
        // keep players unable to move
        SetPlayersCanMove(false);
    }
    
    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = $"player 1: {player1Score}/{collectiblesToWin}";
        
        if (player2ScoreText != null)
            player2ScoreText.text = $"player 2: {player2Score}/{collectiblesToWin}";
    }
    
    private void UpdateRoundWinsUI()
    {
        if (roundWinsText != null)
            roundWinsText.text = $"round wins - p1: {player1RoundWins}  p2: {player2RoundWins}";
    }
    
    // public method to restart entire game
    public void RestartGame()
    {
        player1Score = 0;
        player2Score = 0;
        player1RoundWins = 0;
        player2RoundWins = 0;
        roundEnded = false;
        gameEnded = false;
        
        // hide all ui
        if (player1WinPanel != null) player1WinPanel.SetActive(false);
        if (player2WinPanel != null) player2WinPanel.SetActive(false);
        if (finalWinnerText != null) finalWinnerText.gameObject.SetActive(false);
        if (winPortal != null) winPortal.SetActive(false);
        foreach (GameObject light in winningLights)
        {
            if (light != null) light.SetActive(false);
        }
        
        // restart from hospital
        SetupRound(RoundType.Hospital);
        UpdateScoreUI();
        UpdateRoundWinsUI();
        
        // show start sequence
        StartCoroutine(ShowStartSequence());
        
        Debug.Log("game restarted");
    }
    
    // method for other scripts to check if players can move
    public bool CanPlayersMove()
    {
        return playersCanMove && !roundEnded && !gameEnded;
    }
    
    private void UpdatePlayerHats(RoundType roundType)
    {
        Debug.Log($"updating player hats for {roundType} round");
        
        // hide all hats first
        HideAllHats();
        
        // show appropriate hats based on round
        switch (roundType)
        {
            case RoundType.Hospital:
                // gnome hats for hospital round
                if (player1GnomeHat != null) 
                {
                    player1GnomeHat.SetActive(true);
                    Debug.Log("player 1 wearing gnome hat");
                }
                if (player2GnomeHat != null) 
                {
                    player2GnomeHat.SetActive(true);
                    Debug.Log("player 2 wearing gnome hat");
                }
                break;
                
            case RoundType.Forest:
                // mushroom hats for forest round
                if (player1MushroomHat != null) 
                {
                    player1MushroomHat.SetActive(true);
                    Debug.Log("player 1 wearing mushroom hat");
                }
                if (player2MushroomHat != null) 
                {
                    player2MushroomHat.SetActive(true);
                    Debug.Log("player 2 wearing mushroom hat");
                }
                break;
                
            case RoundType.ParkingLot:
                // cowboy hats for parking lot round
                if (player1CowboyHat != null) 
                {
                    player1CowboyHat.SetActive(true);
                    Debug.Log("player 1 wearing cowboy hat");
                }
                if (player2CowboyHat != null) 
                {
                    player2CowboyHat.SetActive(true);
                    Debug.Log("player 2 wearing cowboy hat");
                }
                break;
        }
    }
    
    private void HideAllHats()
    {
        // player 1 hats
        if (player1GnomeHat != null) player1GnomeHat.SetActive(false);
        if (player1MushroomHat != null) player1MushroomHat.SetActive(false);
        if (player1CowboyHat != null) player1CowboyHat.SetActive(false);
        
        // player 2 hats
        if (player2GnomeHat != null) player2GnomeHat.SetActive(false);
        if (player2MushroomHat != null) player2MushroomHat.SetActive(false);
        if (player2CowboyHat != null) player2CowboyHat.SetActive(false);
    }
}