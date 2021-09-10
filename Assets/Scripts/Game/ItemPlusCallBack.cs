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
        //Debug.Log("--------------------------ItemPlusCallBack::OnEndMove--------------------------------------\n switch to next state that is plus change.");

        GamePoker pokerScript = transform.parent.GetComponent<GamePoker>();
        if(pokerScript.bAccelAddingNPoker)
        {
            GetComponent<Animator>().SetTrigger("PlusChangeQuick");
            //Debug.Log("--------------------------ItemPlusCallBack::PlusChangeQuick----------------------------000000000000000");
        }
        else
        {
            GetComponent<Animator>().ResetTrigger("PlusChange");
            GetComponent<Animator>().SetTrigger("PlusChange");
        }
        
        //Debug.Log("--------------------------ItemPlusCallBack::OnEndMove--------------------------------------\n --------------------------------this is the first time to add one add n poker-----------------------.");
        transform.parent.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        GameplayMgr.Instance.OnAddNPoker_One(transform.parent.GetComponent<GamePoker>());
    }

    public void OnPlusChangeEnd()
    {
        //Debug.Log("--------------------------ItemPlusCallBack::OnPlusChangeEnd--------------------------------------");

        GamePoker pokerScript = transform.parent.GetComponent<GamePoker>();
        if (pokerScript.nAddNCount == 0)
            return;
        
        pokerScript.nAddNCount--;
        //Debug.Log("--------------------------ItemPlusCallBack::nAddNCount is: " + pokerScript.nAddNCount);

        if (pokerScript.nAddNCount > 0)
        {
            if (pokerScript.bAccelAddingNPoker)
            {
                GetComponent<Animator>().SetTrigger("PlusChangeQuick");
                //Debug.Log("--------------------------ItemPlusCallBack::OnPlusChangeQuickEnd----------------------------111111111");
            }
            GameplayMgr.Instance.OnAddNPoker_One(transform.parent.GetComponent<GamePoker>());
        }
        else
        {
            //pokerScript.UpdateAddNEffectDisplay();
            if(pokerScript.bAccelAddingNPoker)
            {
                GetComponent<Animator>().SetTrigger("PlusExitQuick");
                GetComponent<Animator>().SetTrigger("PlusExit");
                //Debug.Log("--------------------------ItemPlusCallBack::OnPlusChangeQuickEnd----------------------------quick exit");
            }
            else
            {
                //GetComponent<Animator>().ResetTrigger("PlusChange");
                //GetComponent<Animator>().ResetTrigger("PlusChange");
                GetComponent<Animator>().SetTrigger("PlusExit");
                //Debug.Log("--------------------------ItemPlusCallBack::OnPlusChangeEnd----------------------------normal exit....");
            }
            
            GameplayMgr.Instance.OnAddNPokerExit(transform.parent.GetComponent<GamePoker>());
        }
        
        pokerScript.UpdateAddNEffectDisplay();

    }

    public void OnEmptyStateEnd()
    {
        //Debug.Log("--------------------------ItemPlusCallBack::OnEmptyStateEnd--------------------------------------");
        return;
        GamePoker pokerScript = transform.parent.GetComponent<GamePoker>();

        if (pokerScript.nAddNCount > 0)
        {
            GetComponent<Animator>().SetTrigger("PlusCircle");
            GameplayMgr.Instance.OnAddNPoker_One(transform.parent.GetComponent<GamePoker>());
            Debug.Log("--------------------------ItemPlusCallBack::OnEmptyStateEnd is: " + pokerScript.nAddNCount);
        }
        else
        {
            pokerScript.UpdateAddNEffectDisplay();
            GetComponent<Animator>().SetTrigger("PlusExit");
            GameplayMgr.Instance.OnAddNPokerExit(transform.parent.GetComponent<GamePoker>());
        }
    }
}
