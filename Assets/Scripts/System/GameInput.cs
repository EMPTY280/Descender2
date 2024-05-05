using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputData
{
    public bool isMouse;
    public int inputCode;

    public InputData(bool m, int i)
    {
        isMouse = m;
        inputCode = i;
    }

    public InputData(bool m, KeyCode c)
    {
        isMouse = m;
        inputCode = (int)c;
    }
}

public class GameInput : MonoBehaviour
{
    private PlayerMovement pm = null;
    private PlayerGun pg = null;

    private InputData moveLeft;
    private InputData moveRIght;
    private InputData jump;
    private InputData descend;

    private InputData primary;
    private InputData secondary;
    private InputData reload;

    // 초기화, 플레이어의 컴포넌트들을 받아오고, 키 입력 정보 업데이트.
    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        pm = player.GetComponent<PlayerMovement>();
        pg = player.GetComponent<PlayerGun>();

        // 각 키의 입력 코드 지정 (기본값)
        moveLeft = new InputData(false, KeyCode.A);
        moveRIght = new InputData(false, KeyCode.D);
        jump = new InputData(false, KeyCode.Space);
        descend = new InputData(false, KeyCode.S);

        primary = new InputData(true, 0);
        reload = new InputData(false, KeyCode.R);
    }

    private void Update()
    {
        pm.SetMove(false, GetInput(moveLeft));
        pm.SetMove(true, GetInput(moveRIght));
        pm.Jump(GetInput(jump));
        if (GetInput(descend)) pm.DescendPlatform();

        if (GetInput(primary)) pg.Fire();
        if (GetInputDown(reload)) pg.Reload();
    }
    private bool GetInputDown(InputData d)
    {
        if (d.isMouse)
            return Input.GetMouseButtonDown(d.inputCode);
        return Input.GetKeyDown((KeyCode)d.inputCode);
    }

    private bool GetInputUp(InputData d)
    {
        if (d.isMouse)
            return Input.GetMouseButtonUp(d.inputCode);
        return Input.GetKeyUp((KeyCode)d.inputCode);
    }

    private bool GetInput(InputData d)
    {
        if (d.isMouse)
            return Input.GetMouseButton(d.inputCode);
        return Input.GetKey((KeyCode)d.inputCode);
    }
}
