using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private Room currentRoom = null;
    [SerializeField] private Room nextRoom = null;
    [SerializeField] private PlayerMovement player = null;
    [SerializeField] Transform roomPool = null;
    [SerializeField] private Room[] rooms = null;

    private bool changingRoom = false;

    [Header("�� ����")]
    [SerializeField] private float roomSizeY = 22.5f;

    private void Awake()
    {
        rooms = roomPool.GetComponentsInChildren<Room>(true);
        SelectRandomRoom();
    }

    private void Update()
    {
        if (changingRoom) // �� ��ȯ
        {
            float dist = Mathf.Abs(nextRoom.transform.position.y);
            dist = Mathf.Max(dist * roomSizeY, 1f);

            // ���� �� �б�
            Vector3 newPos1 = Vector3.MoveTowards(currentRoom.transform.position,
                new Vector3(0, roomSizeY, 0f), dist * Time.deltaTime);
            currentRoom.transform.position = newPos1;

            // ���� �� ��ܿ���
            Vector3 newPos2 = Vector3.MoveTowards(nextRoom.transform.position,
                new Vector3(0, 0f, 0f), dist * Time.deltaTime);
            nextRoom.transform.position = newPos2;

            // �÷��̾� ��ġ ����
            Vector3 playerPos = player.transform.position;
            Vector3 newPos3 = Vector3.MoveTowards(new Vector3(playerPos.x, playerPos.y, 0f),
                new Vector3(playerPos.x, roomSizeY * 0.5f, 0f), dist * Time.deltaTime);
            player.transform.position = newPos3;
            player.ResetVelocity();

            if (nextRoom.transform.position == Vector3.zero)
            {
                // �÷��̾� �ӵ� �� ���� ����
                player.RestoreVelocity();
                player.LockBonusJump(false);

                // ���� �� ���� �� ���� �� �۵� ����
                changingRoom = false;
                currentRoom.ResetRoom();
                currentRoom = nextRoom;

                SelectRandomRoom();
            }
        }
    }

    public void NextRoom()
    {
        changingRoom = true;
    }

    private void SelectRandomRoom()
    {
        if (rooms.Length < 2) return;

        do
            nextRoom = rooms[Random.Range(0, rooms.Length)];
        while (nextRoom == currentRoom);
        nextRoom.transform.position = new Vector3(0, -roomSizeY, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NextRoom();
            nextRoom.ActiveRoom();

            player.LockBonusJump(true);
            player.SaveVelocity();
        }
    }
}
