using System.Collections;
using UnityEngine;

public class ShieldOrbSpawner : MonoBehaviour
{
    [Header("SPAWNING SETTINGS")]
    [Space(5)]
    public GameObject shieldOrbPrefab;
    public Transform[] spawnPoints; 
    public float spawnInterval = 15f; 
    public float spawnVariation = 5f;
    public int maxOrbs = 3; //in the scene
    
    [Header("AUTO SPAWN POINTS")]
    [Space(5)]
    public bool useAutoSpawnPoints = true;
    public float spawnHeight = 1.5f; 
    public LayerMask groundLayer = 1;
    
    private int currentOrbCount = 0;
    
    private void Start()
    {
        if (useAutoSpawnPoints)
        {
            GenerateSpawnPoints();
        }
        
    
        StartCoroutine(SpawnShieldOrbs());
    }
    
    private void GenerateSpawnPoints()
    {
       
        spawnPoints = new Transform[8]; 
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject spawnPoint = new GameObject("shieldSpawnPoint_" + i);
            spawnPoint.transform.parent = transform;
            
            
            Vector3 randomPos = new Vector3(
                Random.Range(-25f, 25f),
                spawnHeight,
                Random.Range(-25f, 25f)
            );
            
          
            if (Physics.Raycast(randomPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                randomPos.y = hit.point.y + spawnHeight;
            }
            
            spawnPoint.transform.position = randomPos;
            spawnPoints[i] = spawnPoint.transform;
        }
    }
    
    private IEnumerator SpawnShieldOrbs()
    {
       
        yield return new WaitForSeconds(spawnInterval * 0.5f);
        
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval + Random.Range(-spawnVariation, spawnVariation));
            
            if (currentOrbCount < maxOrbs)
            {
                SpawnShieldOrb();
            }
        }
    }
    
    private void SpawnShieldOrb()
    {
        if (spawnPoints.Length == 0) return;
        
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
       
        Collider[] nearbyColliders = Physics.OverlapSphere(spawnPoint.position, 2f);
        foreach (Collider col in nearbyColliders)
        {
            if (col.CompareTag("ShieldOrb") || col.CompareTag("HealthOrb"))
            {
               
                return;
            }
        }
        
      
        GameObject orb = Instantiate(shieldOrbPrefab, spawnPoint.position, Quaternion.identity);
        currentOrbCount++;
        
        
        ShieldOrb orbController = orb.GetComponent<ShieldOrb>();
        if (orbController == null)
        {
            orbController = orb.AddComponent<ShieldOrb>();
        }
        
        orbController.spawner = this;
        
        Debug.Log("Shield orb spawned at: " + spawnPoint.position);
    }
    
    public void OnOrbDestroyed()
    {
        currentOrbCount--;
        currentOrbCount = Mathf.Max(0, currentOrbCount);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        
        Gizmos.color = Color.cyan;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.7f);
                Gizmos.DrawLine(point.position, point.position + Vector3.up * 2f);
            }
        }
    }
}