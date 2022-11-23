using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack that spawns Enemies at set locations. Does no damage despite inheriting from BasicAttackCombo.
/// </summary>
public class SpawnEnemy : BasicAttackCombo
{
    [Header("Spawning")]
    [Tooltip("Spawned Enemy will be chosen randomly from this list.")]
    [SerializeField] protected List<GameObject> _enemiesToSpawn;
    [Tooltip("How many Enemies should be created per activation of this attack.")]
    [SerializeField] protected int _numberOfEnemiesSpawned = 1;
    [Tooltip("The places where spanwed Enemies could appear.")]
    [SerializeField] protected List<Transform> _spawnPositions;


    public override void Activate(GameObject player)
    {
        if (_enemiesToSpawn.Count <= 0 || _spawnPositions.Count <= 0)
        {
            Debug.LogError("No Enemies to spawn or no places to position them.");
            return;
        }

        for (int i = 0; i < _numberOfEnemiesSpawned; i++)
        {
            int spawnedEnemyIndex = Random.Range(0, _enemiesToSpawn.Count);
            int spawnPositionIndex = Random.Range(0, _spawnPositions.Count);

            Instantiate(_enemiesToSpawn[spawnedEnemyIndex], _spawnPositions[spawnPositionIndex].position, Quaternion.identity);
        }
    }
}
