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

    private InputData moveLeft;
    private InputData moveRIght;
    private InputData jump;

    // 초기화, 플레이어의 컴포넌트들을 받아오고, 키 입력 정보 업데이트.
    private void Awake()
    {
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();

        // 각 키의 입력 코드 지정
        moveLeft = new InputData(false, KeyCode.A);
        moveRIght = new InputData(false, KeyCode.D);
        jump = new InputData(false, KeyCode.Space);
    }

    private void Update()
    {
        pm.SetMove(false, GetInput(moveLeft));
        pm.SetMove(true, GetInput(moveRIght));
        pm.Jump(GetInput(jump));
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
