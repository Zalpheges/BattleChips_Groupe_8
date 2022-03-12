using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private static Map _instance;

    public GameObject _prefabCell;

    [SerializeField]
    private Transform _center;

    [SerializeField]
    private Player _playerPrefab;

    private void Awake()
    {
        _instance = this;
    }

    public static Player[] CreatePlayers(int myId, int nPlayers)
    {
        float radius = 10.3f * _instance._prefabCell.transform.localScale.x;

        Player[] players = new Player[nPlayers];
        Vector3[] positions = _instance.ComputePlayerPositions(nPlayers, radius);

        List<Transform> playerTransforms = new List<Transform>();

        for (int i = 0; i < nPlayers; ++i)
        {
            Player player = Instantiate(_instance._playerPrefab, positions[i], Quaternion.identity);
            player.transform.LookAt(_instance._center);

            player.you = i == myId;
            player.id = i;
            player.prefabCell = _instance._prefabCell;
            player.Initialize();

            playerTransforms.Add(player.transform);
        }

        CameraManager.InitCamera(playerTransforms);

        return players;
    }

    private Vector3[] ComputePlayerPositions(int nPlayers, float radius)
    {
        float deltaAngle = 360f / nPlayers;

        Vector3[] positions = new Vector3[nPlayers];
        positions[0] = _center.position + Vector3.back * radius;

        for (int i = 1; i < nPlayers; ++i)
            positions[i] = _center.position + ComputePlayerOffset(i * deltaAngle, radius);

        return positions;
    }

    // Warning : Code des Dieux - Ne pas essayer ne serait-ce que d'appréhender //
    private Vector3 ComputePlayerOffset(float angle, float radius)
    {
        Vector3 offset = Vector3.zero;

        float horizontalLength = Mathf.Sin(angle % 90f * Mathf.Deg2Rad);

        if (Mathf.FloorToInt(angle / 90f) % 2 == 1)
            horizontalLength += Mathf.PI / 2f;

        offset.x += horizontalLength * radius * (angle <= 180f ? 1f : -1f);

        float verticalLength = Mathf.Sin(angle % 90f * Mathf.Deg2Rad);

        if (Mathf.FloorToInt(angle / 90f) % 2 == 0)
            verticalLength += Mathf.PI / 2f;

        offset.z = verticalLength * radius * (angle >= 90f && angle <= 270f ? 1f : -1f);

        return offset;
    }
}
