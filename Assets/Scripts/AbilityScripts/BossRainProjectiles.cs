using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRainProjectiles : BasicAttackCombo
{
    [SerializeField]
    private GameObject _projectilePrefab;

    [SerializeField]
    private float _spawnPositionOffset = 25f;

    [SerializeField]
    private float _spawnPositionSpread = 3.5f;

    [SerializeField]
    private float _spawnPositionVerticalSpread = 10f;

    [SerializeField]
    private float _spawnPositionSpreadVariance = 1f;

    [SerializeField]
    private int _projectileCount = 10;

    private List<RainProjectile> _spawnedProjectiles = new List<RainProjectile>();

    public override void Activate(GameObject player)
    {
        List<RainProjectile> RPs = new List<RainProjectile>();
        for(int i = 0; i < _projectileCount; i++)
        {
            RPs.Add(GetProjectile());
        }

        Rain(RPs, GetRandomBool(), GetRandomBool());
    }

    public override void Deactivate(GameObject player)
    {
        // StartCoroutine(ActivateCooldown());
    }
    
    private void Rain(List<RainProjectile> RPs, bool isDown, bool isRight)
    {
        for (int i = 0; i < RPs.Count; i++)
        {
            Vector3 origin = transform.position;
            
            if (isDown)
            {
                RPs[i].transform.position = new Vector3(
                    origin.x + ((i % 2 == 0 ? -1 : 1) * _spawnPositionSpread * i) + GetVariance(_spawnPositionSpreadVariance),
                    origin.y + _spawnPositionOffset + GetVariance(_spawnPositionSpreadVariance),
                    origin.z);

                RPs[i].SetDirection(new Vector3(0, -1, 0));
            }
            else
            {
                RPs[i].transform.position = new Vector3(
                    origin.x + 2 * _spawnPositionOffset * (isRight ? 1 : -1) + GetVariance(_spawnPositionSpreadVariance),
                    origin.y + ((i % 2 == 0 ? -1 : 1) * _spawnPositionVerticalSpread * i) + GetVariance(_spawnPositionSpreadVariance),
                    origin.z);

                RPs[i].SetDirection(new Vector3(-1 * (isRight ? 1 : -1), 0, 0));
            }
        }

        Deactivate(gameObject);
    }

    private RainProjectile GetProjectile()
    {
        foreach (RainProjectile p in _spawnedProjectiles)
        {
            if (!p.gameObject.activeSelf)
            {
                p.gameObject.SetActive(true);
                return p;
            }
        }

        GameObject newProjectile = Instantiate(_projectilePrefab);

        RainProjectile RP = newProjectile.GetComponent<RainProjectile>();
        _spawnedProjectiles.Add(RP);
        return RP;
    }

    private bool GetRandomBool()
    {
        return Random.Range(0, 2) == 1;
    }

    private float GetVariance(float variance)
    {
        return Random.Range(-variance, variance);
    }
}
