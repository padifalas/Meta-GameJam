using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    [Header("round settings")]
    [Space(5)]
    public int collectiblesToWin = 3; // need 3 to progress
    public RoundType currentRound = RoundType.Hospital;
    
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
    
    [Header("win effects")]
    [Space(5)]
    public GameObject[] winningLights; // lights that activate for winner
    public GameObject winPortal; // portal effect for winner
    public float blackoutDuration = 2f; // how long loser screen stays black
    public float transitionDelay = 3f; // delay before scene transition
    
    [Header("ui references")]
    [Space(5)]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public GameObject player1BlackoutScreen; // black ui panel for player 1
    public GameObject player2BlackoutScreen; // black ui panel for player 2
    public Text winnerText; // text showing who won
    public Text roundText; // text showing current round
    
    private int player1Score = 0;
    private int player2Score = 0;
    private bool roundEnded = false;
    
    public enum RoundType
    {
        Hospital,
        Forest,
        ParkingLot
    }
    
    private void Start()
    {
        // ensure blackout screens start hidden
        if (player1BlackoutScreen != null)
            player1BlackoutScreen.SetActive(false);
        
        if (player2BlackoutScreen != null)
            player2BlackoutScreen.SetActive(false);
        
        // hide win effects
        if (winPortal != null)
            winPortal.SetActive(false);
        
        foreach (GameObject light in winningLights)
        {
            if (light != null)
                light.SetActive(false);
        }
        
        // hide winner text
        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
        
        // start in hospital round
        SetupRound(RoundType.Hospital);
        UpdateScoreUI();
    }
    
    private void SetupRound(RoundType roundType)
    {
        currentRound = roundType;
        Debug.Log($"setting up {roundType} round");
        
        // activate appropriate area spawner
        if (hospitalSpawner != null) hospitalSpawner.gameObject.SetActive(roundType == RoundType.Hospital);
        if (forestSpawner != null) forestSpawner.gameObject.SetActive(roundType == RoundType.Forest);
        if (parkingLotSpawner != null) parkingLotSpawner.gameObject.SetActive(roundType == RoundType.ParkingLot);
        
        // move players to appropriate spawn points
        MovePlayersToRoundStart(roundType);
        
        // clear any existing effects
        ClearPlayerEffects();
        
        // update round ui
        if (roundText != null)
            roundText.text = $"round: {roundType}";
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
        }
        
        // move player 2  
        if (player2 != null && playerSpawnPoints[spawnIndex + 1] != null)
        {
            player2.GetComponent<CharacterController>().enabled = false;
            player2.transform.position = playerSpawnPoints[spawnIndex + 1].position;
            player2.transform.rotation = playerSpawnPoints[spawnIndex + 1].rotation;
            player2.GetComponent<CharacterController>().enabled = true;
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
    
    public void OnCollectibleGathered(MonoBehaviour player)
    {
        if (roundEnded) return;
        
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
            OnPlayerWin(player1, "player 1");
        }
        else if (player2Score >= collectiblesToWin)
        {
            OnPlayerWin(player2, "player 2");
        }
    }
    
    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = $"player 1: {player1Score}/{collectiblesToWin}";
        
        if (player2ScoreText != null)
            player2ScoreText.text = $"player 2: {player2Score}/{collectiblesToWin}";
    }
    
    private void OnPlayerWin(MonoBehaviour winner, string winnerName)
    {
        if (roundEnded) return;
        roundEnded = true;
        
        Debug.Log($"{winnerName} wins the {currentRound} round!");
        
        // show winner text
        if (winnerText != null)
        {
            winnerText.text = $"{winnerName} wins {currentRound} round!";
            winnerText.gameObject.SetActive(true);
        }
        
        // activate winning effects
        StartCoroutine(HandleWinSequence(winner, winnerName));
    }
    
    private IEnumerator HandleWinSequence(MonoBehaviour winner, string winnerName)
    {
        // activate winning lights
        foreach (GameObject light in winningLights)
        {
            if (light != null)
                light.SetActive(true);
        }
        
        // show portal effect
        if (winPortal != null)
        {
            winPortal.SetActive(true);
        }
        
        // blackout the loser
        if (winner == player1 && player2BlackoutScreen != null)
        {
            player2BlackoutScreen.SetActive(true);
        }
        else if (winner == player2 && player1BlackoutScreen != null)
        {
            player1BlackoutScreen.SetActive(true);
        }
        
        // wait for dramatic effect
        yield return new WaitForSeconds(transitionDelay);
        
        // progress to next round
        ProgressToNextRound();
    }
    
    private void ProgressToNextRound()
    {
        RoundType nextRound = GetNextRound();
        
        if (nextRound != currentRound)
        {
            
            
            // reset scores
            player1Score = 0;
            player2Score = 0;
            roundEnded = false;
            
            // hide win effects
            if (winPortal != null) winPortal.SetActive(false);
            foreach (GameObject light in winningLights)
            {
                if (light != null) light.SetActive(false);
            }
            
           
            if (player1BlackoutScreen != null) player1BlackoutScreen.SetActive(false);
            if (player2BlackoutScreen != null) player2BlackoutScreen.SetActive(false);
            if (winnerText != null) winnerText.gameObject.SetActive(false);
            
           
            SetupRound(nextRound);
            UpdateScoreUI();
        }
        else
        {
           
            HandleGameCompletion();
        }
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
                return RoundType.ParkingLot; // game complete - stay on final round
            default:
                return RoundType.Hospital;
        }
    }
    
    private void HandleGameCompletion()
    {
        // show final completion message
        if (winnerText != null)
        {
            winnerText.text = "game done";
            winnerText.gameObject.SetActive(true);
        }
        
        Debug.Log("all rounds done");
        // add any final game completion logic here
    }
    
    // method to restart current round
    public void RestartRound()
    {
        player1Score = 0;
        player2Score = 0;
        roundEnded = false;
        
        // hide all win effects
        if (winPortal != null)
            winPortal.SetActive(false);
        
        foreach (GameObject light in winningLights)
        {
            if (light != null)
                light.SetActive(false);
        }
        
        // hide blackout screens
        if (player1BlackoutScreen != null)
            player1BlackoutScreen.SetActive(false);
        
        if (player2BlackoutScreen != null)
            player2BlackoutScreen.SetActive(false);
        
        // hide winner text
        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
        
        UpdateScoreUI();
        
        // respawn collectibles for current round only
        CollectibleSpawner currentSpawner = GetCurrentSpawner();
        if (currentSpawner != null)
            currentSpawner.RespawnCollectibles();
        
        Debug.Log("round restarted");
    }
    
    private CollectibleSpawner GetCurrentSpawner()
    {
        switch (currentRound)
        {
            case RoundType.Hospital:
                return hospitalSpawner;
            case RoundType.Forest:
                return forestSpawner;
            case RoundType.ParkingLot:
                return parkingLotSpawner;
            default:
                return hospitalSpawner;
        }
    }
    
    
    public void SetRound(RoundType roundType)
    {
        player1Score = 0;
        player2Score = 0;
        roundEnded = false;
        SetupRound(roundType);
        UpdateScoreUI();
    }
}