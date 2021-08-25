using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightMenuUI : MonoBehaviour
{
    Transform trans;

    InputField inputPosX;
    InputField inputPosY;
    InputField inputRotation;
    InputField inputGroupID;

    JsonReadWriteTest.PokerInfo info = new JsonReadWriteTest.PokerInfo();

    float fPosX = 0.0f;
    float fPosY = 0.0f;
    float fRotation = 0.0f;
    int nGroupID = 1;

    // Start is called before the first frame update
    void Start()
    {
        //trans = GetComponent<Transform>();
        inputPosX = transform.Find("PosX").GetComponent<InputField>();
        inputPosX.onEndEdit.AddListener(delegate { this.OnInputPosXEnd(); });

        inputPosY = transform.Find("PosY").GetComponent<InputField>();
        inputPosY.onEndEdit.AddListener(delegate { this.OnInputPosYEnd(); });

        inputRotation = transform.Find("Rotation").GetComponent<InputField>();
        inputRotation.onEndEdit.AddListener(delegate { this.OnInputRotationEnd(); });

        inputGroupID = transform.Find("GroupID").GetComponent<InputField>();
        inputGroupID.onEndEdit.AddListener(delegate { this.OnInputGroupIDEnd(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setPosDisplay(Vector3 pos, Rect rect)
    {
        Vector2 relativePos = PokerAreaMgr.Instance.ToRelativePos(pos, rect);

        setPosXDisplay(relativePos.x);
        setPosYDisplay(relativePos.y);
    }
    
    public void setPosXDisplay(float X)
    {
        fPosX = X;

        string strDisplay = string.Format("{0:F1}",fPosX);
        inputPosX.text = strDisplay;
    }

    public void setPosYDisplay(float Y)
    {
        fPosY = Y;

        string strDisplay = string.Format("{0:F1}", fPosY);
        inputPosY.text = strDisplay;
    }

    public void setRotationDisplay(float R)
    {
        fRotation = R;

        string strDisplay = string.Format("{0:F1}", fRotation);
        inputRotation.text = strDisplay;
    }

    public void setGroupIDDisplay(int G)
    {
        nGroupID = G;

        string strDisplay = string.Format("{0}", nGroupID);
        //Debug.Log("RightMenuUI::setGroupIDDisplay ... the Group ID is: " + nGroupID);
        inputGroupID.text = strDisplay;
    }

    private void OnInputPosXEnd()
    {
        float fPosX = float.Parse(inputPosX.text);

        PokerAreaMgr.Instance.setSelectPokerPosX(fPosX);

        //Debug.Log("RightMenuUI::OnInputPosXEnd ... the input PosX is: " + fPosX);
    }

    private void OnInputPosYEnd()
    {
        float fPosY = float.Parse(inputPosY.text);

        PokerAreaMgr.Instance.setSelectPokerPosY(fPosY);

        //Debug.Log("RightMenuUI::OnInputPosYEnd ... the input PosY is: " + fPosY);
    }

    private void OnInputRotationEnd()
    {
        float fRotation = float.Parse(inputRotation.text);

        PokerAreaMgr.Instance.setSelectPokerRotation(fRotation);

        //Debug.Log("RightMenuUI::OnInputRotationEnd ... the input Rotation is: " + fRotation);
    }

    private void OnInputGroupIDEnd()
    {
        int nGroupID = int.Parse(inputGroupID.text);

        //todo: here we store the group id
        PokerAreaMgr.Instance.setSelectPokerGroupID(nGroupID);
    }
}
