using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlusCallBack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEndMove()
    {
        Debug.Log("--------------------------ItemPlusCallBack::OnEndMove--------------------------------------");

        GetComponent<Animator>().SetTrigger("PlusChange");
        transform.parent.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        GameplayMgr.Instance.OnAddNPoker_One();
    }

    public void OnPlusChangeEnd()
    {
        Debug.Log("--------------------------ItemPlusCallBack::OnPlusChangeEnd--------------------------------------");

        GetComponent<Animator>().SetTrigger("PlusChangeEnd");

        GamePoker pokerScript = transform.parent.GetComponent<GamePoker>();
        pokerScript.nAddNCount--;
        pokerScript.UpdateAddNEffectDisplay();

        Debug.Log("--------------------------ItemPlusCallBack::nAddNCount is: " + pokerScript.nAddNCount);

        /*GamePoker pokerScript = transform.parent.GetComponent<GamePoker>();
        pokerScript.nAddNCount--;

        if(pokerScript.nAddNCount > 0)*/

    }

    public void OnEmptyStateEnd()
    {
        Debug.Log("--------------------------ItemPlusCallBack::OnEmptyStateEnd--------------------------------------");

        /*GamePoker pokerScript = transform.parent.GetComponent<GamePoker>();
        pokerScript.nAddNCount--;
        pokerScript.UpdateAddNEffectDisplay();

        Debug.Log("--------------------------ItemPlusCallBack::nAddNCount is: " + pokerScript.nAddNCount);*/

        GamePoker pokerScript = transform.parent.GetComponent<GamePoker>();

        if (pokerScript.nAddNCount > 0)
        {
            GetComponent<Animator>().SetTrigger("PlusCircle");
            GameplayMgr.Instance.OnAddNPoker_One();
        }
        else
        {
            pokerScript.UpdateAddNEffectDisplay();
            GetComponent<Animator>().SetTrigger("PlusExit");

            //GameplayMgr.Instance.AdjustAllHandPokerPosition();
        }
    }
}
