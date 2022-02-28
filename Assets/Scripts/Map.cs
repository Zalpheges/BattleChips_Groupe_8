using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] private Transform center;
    [SerializeField] private float _radius;
    [SerializeField] private int _nPlayers;
    [SerializeField] private GameObject _playerPrefab;

    private void Start()
    {
        List<Transform> playersTransform = new List<Transform>();

        List<Vector3> players = CalculatePlayersCoords();

        bool a = false;
        foreach (Vector3 playerPos in players)
        {
            GameObject go = Instantiate(_playerPrefab, playerPos, Quaternion.identity);
            go.transform.LookAt(center);
            go.GetComponent<Player>().Initialize();
            if (!a)
                go.GetComponent<Renderer>().material.color = Color.red;
            a = true;
            playersTransform.Add(go.transform);
        }
        CameraManager.InitCamera(playersTransform);
    }

    private List<Vector3> CalculatePlayersCoords()
    {
        float angleDelta = 360f / _nPlayers;
        List<Vector3> coords = new List<Vector3>();
        coords.Add(center.position + Vector3.back * _radius);
        for (int i = 1; i < _nPlayers; ++i)
        {
            Vector3 pos = center.position;
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
}
