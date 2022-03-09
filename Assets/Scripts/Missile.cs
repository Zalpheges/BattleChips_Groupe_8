using System.Collections;
using UnityEngine;
using Cinemachine;
using System;

public class Missile : MonoBehaviour
{
    [SerializeField]
    private Vector3 offSetCameraPosition;

    [Space(5)]

    [SerializeField]
    private Vector3 offSetCameraRotation;

    [Space(5)]

    [SerializeField]
    private float offSetyBezierPoint;

    [Space(5)]

    [SerializeField]
    private float travelTime;

    [Space(5)]

    [SerializeField]
    private GameObject _explosionPrefab;

    [Space(5)]

    [SerializeField] private float explosionDelay = 0.5f;

    [System.NonSerialized] public Vector3 StartPosition;
    [System.NonSerialized] public Vector3 EndPosition;

    private new CinemachineVirtualCamera camera;

    private bool _explode;

    private Vector3 _from;
    private Vector3 _interpolator;
    private Vector3 _to;

    private float _time;

    private bool _moving = true;

    private Action _onTargetReached;
    private Action _onDestroy;

    private int idTarget = int.MaxValue;
    private Vector3 nextPosition;
    private Transform shipToDestroy;

    public void Shoot(Vector3 from, Vector3 to, bool explode)
    {
        _from = from;
        _to = to;

        _interpolator = (from + to) / 2f;
        _interpolator.y += 250f;

        _explode = explode;

        camera = CameraManager.CreateCamera(transform, offSetCameraPosition, offSetCameraRotation);
        camera.Follow = transform;

        CinemachineTransposer transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset = offSetCameraPosition;
    }

    public void SetCallbacks(Action onTargetReach, Action onDestroy)
    {
        _onTargetReached = onTargetReach;
        _onDestroy = onDestroy;
    }

    private void Update()
    {
        if (_moving)
        {
            _time += Time.deltaTime / travelTime;

            Vector3 position = Mathf.Pow(1f - _time, 2) * _from + 2f * (1f - _time) * _interpolator + Mathf.Pow(_time, 2) * _to;

            transform.LookAt(position);
            camera.transform.LookAt(position);

            transform.position = position;

            if (_time >= 1f)
                StartCoroutine(OnEndReached());
        }
    }

    private IEnumerator OnEndReached()
    {
        _moving = false;

        _onTargetReached?.Invoke();

        CameraManager.DestroyCamera(camera, idTarget);

        if (_explode)
        {
            GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(explosionDelay);

            Destroy(explosion);
        }

        yield return new WaitForSeconds(CameraManager.transitionDelay);

        Destroy(gameObject);

        _onDestroy?.Invoke();
    }
}
