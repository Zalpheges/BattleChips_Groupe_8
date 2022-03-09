using System.Collections;
using UnityEngine;
using Cinemachine;
using System;

public class Missile : MonoBehaviour
{
    [Header("Camera Offset")]

    [SerializeField]
    private Vector3 _localCameraPosition;

    [SerializeField]
    private Vector3 _localCameraRotation;

    [Space(5)]
    [Header("Movement")]

    [SerializeField]
    private float _interpolatorHeight;

    [SerializeField]
    private float _travelTime;

    [Space(5)]
    [Header("Explosion")]

    [SerializeField]
    private GameObject _explosionPrefab;

    [SerializeField]
    private float _explosionDelay = 0.5f;

    private bool _explode;

    private Vector3 _from;
    private Vector3 _interpolator;
    private Vector3 _to;

    private float _time;

    private bool _moving = true;

    private Action _onTargetReached;
    private Action _onDestroy;

    private int _targetId = int.MaxValue;
    private CinemachineVirtualCamera _camera;

    public void Shoot(Vector3 from, Vector3 to, bool explode)
    {
        _from = from;
        _to = to;

        _interpolator = (from + to) / 2f;
        _interpolator.y += 250f;

        _explode = explode;

        _camera = CameraManager.CreateCamera(transform, _localCameraPosition, _localCameraRotation);
        _camera.Follow = transform;

        CinemachineTransposer transposer = _camera.GetCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset = _localCameraPosition;
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
            _time += Time.deltaTime / _travelTime;

            Vector3 position = Mathf.Pow(1f - _time, 2) * _from + 2f * (1f - _time) * _interpolator + Mathf.Pow(_time, 2) * _to;

            transform.LookAt(position);
            _camera.transform.LookAt(position);

            transform.position = position;

            if (_time >= 1f)
                StartCoroutine(OnEndReached());
        }
    }

    private IEnumerator OnEndReached()
    {
        _moving = false;

        _onTargetReached?.Invoke();

        CameraManager.DestroyCamera(_camera, _targetId);

        if (_explode)
        {
            GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(_explosionDelay);

            Destroy(explosion);
        }

        yield return new WaitForSeconds(CameraManager.transitionDelay);

        Destroy(gameObject);

        _onDestroy?.Invoke();
    }
}
