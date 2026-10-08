using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingDoor : MonoBehaviour
{
    public Vector3 moveOffset = new Vector3(0, 3f, 0);
    public float speed = 2f;
    private Vector3 closedPosition;
    private Vector3 openedPosition;
    private Coroutine moveCoroutine;
    void Start()
    {
        closedPosition = transform.position;
        openedPosition = closedPosition + moveOffset;
    }

    public void OpenWall(){
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToPosition(openedPosition));
    }

    private IEnumerator MoveToPosition(Vector3 targetPos){
        while (Vector3.Distance(transform.position, targetPos) > 0.01f){
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }
}