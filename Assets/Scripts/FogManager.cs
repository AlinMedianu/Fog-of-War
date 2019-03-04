using System;
using UnityEngine;

public class FogManager : MonoBehaviour
{
    [SerializeField][Range(3, 32)]
    private int collisionsCount = 32;

    public int CollisionsCount => collisionsCount;
    public int RevealersCount { get; private set; }
    public static FogManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            RevealersCount = FindObjectsOfType<Revealer>().Length;
        }
        else
            throw new Exception("There can only be one FogManager");
    }
}
