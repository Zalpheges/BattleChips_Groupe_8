using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] private Vector3 offSetCameraPosition;

    [Space(5)]

    [SerializeField] private Vector3 offSetCameraRotation;

    [Space(5)]

    [SerializeField] private float offSetyBezierPoint;

    [Space(5)]

    [SerializeField] private float travelTime;

    [Space(5)]

    [SerializeField] private GameObject explosionPrefab;

    [Space(5)]

    [SerializeField] private float explosionDelay = 0.5f;



    [System.NonSerialized] public GameObject TextGameObject;
    [System.NonSerialized] public GameObject GameUiPrefab;
    [System.NonSerialized] public Vector3 StartPosition;
    [System.NonSerialized] public Vector3 EndPosition;

    private Vector3 interpolatePoint;

    private new CinemachineVirtualCamera camera;

    private bool launchMissile = false;
    private bool shipHit = false;
    private float bezierProgression = 0f;

    private int idTarget = int.MaxValue;
    private Vector3 nextPosition;
    private Transform shipToDestroy;

    private TextMeshProUGUI text;

    public void Init(Vector3 startPosition, Vector3 endPosition, bool shipHit, int targetID)
    {
        text = TextGameObject.GetComponentInChildren<TextMeshProUGUI>(true);
        GameUiPrefab.SetActive(false);
        idTarget = targetID;
        StartPosition = startPosition;
        EndPosition = endPosition;

        transform.position = StartPosition;
        camera = CameraManager.CreateCamera(transform, offSetCameraPosition, offSetCameraRotation);
        camera.Follow = transform;

        CinemachineTransposer transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset = offSetCameraPosition;

        StartCoroutine(CameraMove());
        StartCoroutine(WaitTransition());

        interpolatePoint = (EndPosition - StartPosition) / 2f;
        interpolatePoint.y += offSetyBezierPoint;

        this.shipHit = shipHit;
        
    }

    public void SetDestroyedShip(Transform ship)
    {
        shipToDestroy = ship;
    }

    public void SetText(string message)
    {
        text.text = message;
        TextGameObject.SetActive(true);
    }

    private void Update()
    {
        if (launchMissile && bezierProgression < 1)
        {
            transform.position = nextPosition;

            float coeff = 1 - bezierProgression;
            nextPosition = Mathf.Pow(coeff, 2) * StartPosition + 2 * bezierProgression * coeff * interpolatePoint + Mathf.Pow(bezierProgression, 2) * EndPosition;
            bezierProgression += Time.deltaTime / travelTime;
            transform.LookAt(nextPosition);
            camera.transform.LookAt(nextPosition);

            if (bezierProgression >= 1)
            {
                EndReached();
                if(shipHit)
                    Explosion();
            }           

            if (CameraManager.transitionDelay / 10 + bezierProgression >= 1f && camera != null)
                CameraManager.DestroyCamera(camera, idTarget);
        }
    }

    private void Explosion()
    {
        Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), explosionDelay);
    }

    private void EndReached()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        if (shipToDestroy)
        {
            Vector3 vect = shipToDestroy.localRotation.eulerAngles + Vector3.right * 1000f;
            shipToDestroy.localRotation = Quaternion.Euler(vect);
        }
        StartCoroutine(SetUi());
        Destroy(gameObject, explosionDelay);
    }

    private IEnumerator SetUi()
    {
        yield return new WaitForSeconds(explosionDelay - 0.1f);
        TextGameObject.SetActive(false);
        GameUiPrefab.SetActive(true);
        ClientManager.wait = false;
    }

    private IEnumerator CameraMove()
    {
        yield return new WaitForFixedUpdate();
        camera.Priority = int.MaxValue;
    }

    private IEnumerator WaitTransition()
    {
        yield return new WaitForSeconds(CameraManager.transitionDelay);
        launchMissile = true;
        float coeff = 1 - bezierProgression;
        nextPosition = Mathf.Pow(coeff, 2) * StartPosition + 2 * bezierProgression * coeff * interpolatePoint + Mathf.Pow(bezierProgression, 2) * EndPosition;
        bezierProgression += Time.deltaTime / travelTime;
    }
}
