using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private Vector2 roomSize = Vector2.one;
    private Vector2 originPos = Vector2.zero;
    [SerializeField] private bool isStartRoom = false;

    private IResettable[] resettables;

#if UNITY_EDITOR
    [SerializeField] readonly private Color BORDER_COLOR = Color.cyan;

    private void OnDrawGizmos()
    {
        Gizmos.color = BORDER_COLOR;
        Gizmos.DrawWireCube(transform.position, roomSize);
    }
#endif

    private void Awake()
    {
        originPos = transform.position;
        resettables = GetComponentsInChildren<IResettable>();
        if (isStartRoom) originPos.x -= 40f;
        else gameObject.SetActive(false);
    }

    public void ActiveRoom()
    {
        gameObject.SetActive(true);
    }

    public void ResetRoom()
    {
        transform.position = originPos;
        foreach (IResettable i in resettables) { i.ResetCondition(); }
        gameObject.SetActive(false);
    }
}
