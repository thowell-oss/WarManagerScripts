using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorSpawnTester : MonoBehaviour
{
    public GameObject CorridorSpawner;

    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            Instantiate(CorridorSpawner, new Vector3(i * 5, 0, 0), Quaternion.identity);
        }
    }
}
