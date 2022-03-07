using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private static Map _instance;
    public GameObject _prefabCell;
    public static int nPlayers;
    public static int myId;
    public static List<Transform> playersTransform = new List<Transform>();
    [SerializeField] private Transform _center;
    [SerializeField] private GameObject _playerPrefab;
    private float _radius;


    private void Awake()
    {
        _instance = this;
    }

    public static void Init()
    {
        _instance._radius = 1.5f * 10f / 2f * _instance._prefabCell.transform.localScale.x / Mathf.Tan(36f * Mathf.Deg2Rad);

        List<Vector3> players = _instance.CalculatePlayersCoords();

        for (int i = 0; i < players.Count; i++)
        {
            GameObject go = Instantiate(_instance._playerPrefab, players[i], Quaternion.identity);
            go.transform.LookAt(_instance._center);
            Player player = go.GetComponent<Player>();
            player.you = i == 0;
            player.id = (myId + i) % nPlayers;
            player.prefabCell = _instance._prefabCell;
            player.Initialize();
            playersTransform.Add(go.transform);
        }
        CameraManager.InitCamera(playersTransform);
    }

    private List<Vector3> CalculatePlayersCoords()
    {
        float angleDelta = 360f / nPlayers;
        List<Vector3> coords = new List<Vector3>();
        coords.Add(_center.position + Vector3.back * _radius);
        for (int i = 1; i < nPlayers; ++i)
        {
            Vector3 pos = _center.position;
            float currentAngle = i * angleDelta;
            pos += Mathf.Sin(currentAngle % 90 * Mathf.Deg2Rad + ((int)(currentAngle / 90) % 2 == 0 ? 0 : Mathf.PI / 2)/*turn sin into cos if needed*/)
                * _radius *
                (currentAngle <= 180 ? Vector3.right : Vector3.left) //x-positive or x-negative
                +
                Mathf.Sin(currentAngle % 90 * Mathf.Deg2Rad + ((int)(currentAngle / 90) % 2 == 1 ? 0 : Mathf.PI / 2)/*turn sin into cos if needed*/)
                * _radius *
                (currentAngle >= 90 && currentAngle <= 270 ? Vector3.forward : Vector3.back); //z-positive or z-negative

            coords.Add(pos);
        }
        return coords;
    }

    public static Player GetPlayerById(int id)
    {
        foreach (Transform playerT in playersTransform)
        {
            Player player = playerT.GetComponent<Player>();
            if (player.id == id)
                return player;
        }
        return null;
    }
}
