using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //begin from zero
    public int nIndex {get; set; }
    public int nGroupID { get; set; }

    bool bIsDragging = false;
    bool bIsRotating = false;

    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    
    public RectTransform rectTransform;
    private InputField rotateAngle;
    private string strAngle;

    private Text layerIndex;

    private float fAngle { get; set; }
    private float x = 0.0f;
    private float y = 0.0f;

    private float time = 0.0f;

    Sprite selectSprite = null;//store file "PokerBack_Select.png"
    Sprite normalSprite = null;//store file ""

    Image imgComponent;

    //2021.7.21 added, to update the display value
    RightMenuUI rightMenuUI;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        Button btn = GetComponent<Button>(); 
        btn.onClick.AddListener(delegate () { this.OnClickCard(); });

        fAngle = 0.0f;
        rotateAngle = GetComponentInChildren<InputField>();

        strAngle = string.Format("{0:F1}", rectTransform.rotation.z);
        //Debug.Log("dragbutton start, the rotation is: " + strAngle);
        //rotateAngle.text = strAngle;
        rotateAngle.onEndEdit.AddListener(EndValue);

        //SpriteRenderer sprRenderer = GetComponent<SpriteRenderer>();
        imgComponent = GetComponent<Image>();
        selectSprite = Resources.Load<Sprite>("LevelEditor/PokerBack_Select");
        normalSprite = Resources.Load<Sprite>("LevelEditor/PokerBack");
        imgComponent.sprite = normalSprite;

        //this code is for test adjusting the layer.
        Transform _layerTrans = rectTransform.transform.Find("layerIndex");
        layerIndex = _layerTrans.GetComponent<Text>();
        string strLayer = string.Format("{0}", rectTransform.GetSiblingIndex());
        layerIndex.text = strLayer;

        rightMenuUI = Object.FindObjectOfType<RightMenuUI>();
        if (rightMenuUI == null)
            Debug.Log("DragButton::Start... can not init rightMenuUI menu. check your code.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClickCard()
    {
        if (Time.time - time <= 0.4f)
        {
            //Debug.Log("-----------------DragButton::OnClickCard  double click the card... ... " + gameObject.name);
            //todo we reset the rotation z to zero.
            rectTransform.rotation = Quaternion.AngleAxis(0.0f, transform.forward);
            rotateAngle.text = string.Format("{0:F1}", 0.0);
            fAngle = 0.0f;

            //todo: rightMenuUI.ro
            rightMenuUI.setRotationDisplay(fAngle);

            return;
        }

        //Debug.Log("DragButton::OnClickCard This button is clicked! name is: " + nIndex);
        PokerAreaMgr.Instance.setCurrentSelectPoker(this);

        //更新当前选择的扑克的信息，不应该写在这，应该和上一个语句组合在一起，放在PokerAreaMgr中。为了方便起见，先写在这里
        JsonReadWriteTest.LevelData data = EditorScriptMgr.Instance.chapterInfo.levelDataList[PokerAreaMgr.Instance.nCurrentLevel - 1];
        
        rightMenuUI.setPosXDisplay(data.pokerInfo[nIndex].fPosX);
        rightMenuUI.setPosYDisplay(data.pokerInfo[nIndex].fPosY);
        rightMenuUI.setRotationDisplay(data.pokerInfo[nIndex].fRotation);
        rightMenuUI.setGroupIDDisplay(data.pokerInfo[nIndex].nGroupID);
        //Debug.Log("DragButton clicked! the group id is: " + data.pokerInfo[nIndex].nGroupID);

        if (PokerAreaMgr.Instance.bLeftCtrlDown)
        {
            //Debug.Log("DragButton::OnClickCard...   here we capture the left control combo key!");
            //todo: here we can add the group selection code.
            imgComponent.sprite = selectSprite;
            if(!PokerAreaMgr.Instance.AddToSelectPokerList(this))
            {
                imgComponent.sprite = normalSprite;
            }
        }

        time = Time.time;
    }

    private void EndValue(string strValue)
    {
        //Debug.Log("-----------------DragButton::EndValue  double click the card... ... " + strValue);
        //here we rotate the card with the input angle.
        rotateAngle.text = strValue;
        fAngle = float.Parse(strValue);
        rectTransform.rotation = Quaternion.AngleAxis(fAngle, transform.forward);
    }

    private float QuaternionToRotation(Quaternion quat)
    {
        float fAngle;
        Vector3 vecDir;
        quat.ToAngleAxis(out fAngle, out vecDir);

        float fRotation = fAngle * vecDir.z;

        return fRotation;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            bIsDragging = true;
        }

        if(eventData.button == PointerEventData.InputButton.Right)
        {
            bIsRotating = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(bIsDragging)
        {
            Vector3 position = new Vector3(eventData.position.x, eventData.position.y, rectTransform.position.z);

            if (PokerAreaMgr.Instance.InSelectPokerList(this))
            {
                Vector3 deltaPos = position - rectTransform.position;
                PokerAreaMgr.Instance.UpdateAllSelectPokerDisplayPosition(deltaPos);

                rightMenuUI.setPosDisplay(position, PokerAreaMgr.Instance.rectTrans.rect);
                float fRotation = QuaternionToRotation(transform.rotation);
                rightMenuUI.setRotationDisplay(fRotation);
            }
            else
            {
                rectTransform.position = position;

                Vector2 displayPos = PokerAreaMgr.Instance.ToRelativePos(position, PokerAreaMgr.Instance.rectTrans.rect);
                string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", displayPos.x, displayPos.y);
                //string strDisplay = string.Format("X:{0:F1}, Y:{1:F1}", position.x, position.y);
                Text text = GetComponentInChildren<Text>();
                text.text = strDisplay;

                rightMenuUI.setPosDisplay(position, PokerAreaMgr.Instance.rectTrans.rect);

                float fRotation = QuaternionToRotation(transform.rotation);
                rightMenuUI.setRotationDisplay(fRotation);
                
            }
            //Debug.Log("eventData.x is: " + eventData.position.x + "eventData.y is: " + eventData.position.y);
            //Debug.Log("position.x is: " + position.x + "position.y is: " + position.y);
        }

        if(bIsRotating)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            rectTransform.rotation = Quaternion.AngleAxis(x, transform.forward);
            fAngle = x;
            rotateAngle.text = string.Format("{0}",x);
            rightMenuUI.setRotationDisplay(x);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("DragButton end drag...");

        bIsDragging = false;
        bIsRotating = false;

        if (PokerAreaMgr.Instance.InSelectPokerList(this))
        {
            //info.fPosX = transform.position.x - PokerAreaMgr.Instance.trans.position.x;
            //info.fPosY = transform.position.y - PokerAreaMgr.Instance.trans.position.y;
            Vector3 position = transform.position - PokerAreaMgr.Instance.trans.position;
            Vector3 deltaPos = position - rectTransform.position;

            float fRotation = QuaternionToRotation(transform.rotation);
            PokerAreaMgr.Instance.UpdateAllSelectPokerStoreInfo(deltaPos, fRotation);
        }
        else
        {
            JsonReadWriteTest.PokerInfo info = new JsonReadWriteTest.PokerInfo();
            info.fPosX = transform.position.x - PokerAreaMgr.Instance.trans.position.x;
            info.fPosY = transform.position.y - PokerAreaMgr.Instance.trans.position.y;

            //Debug.Log("---DragButton::OnEndDrag the poker game object's rotation is: " + info.fRotation + " vecdir is: " + vecDir);
            float fRotation = QuaternionToRotation(transform.rotation);
            info.nGroupID = nGroupID;
            info.fRotation = fRotation;
            //Debug.Log("---DragButton::OnEndDrag the poker game object's rotation is: " + fRotation);
            PokerAreaMgr.Instance.UpdateOnePokerInfo(nIndex, info);
        }
    }

    public void OnLayerIndexChanged(int nNewIndex)
    {
        //string strLayer = string.Format("{0}", nNewIndex);
        string strLayer = string.Format("{0}", rectTransform.GetSiblingIndex());
        layerIndex.text = strLayer;
    }

    /*void OnGUI()
    {
        string strDisplay = string.Format("X: {0}, Y: {1}", pos.x, pos.y);
        GUI.Label(new Rect(pos.x, pos.y, 100.0f, 30.0f), strDisplay);
    }*/

}
