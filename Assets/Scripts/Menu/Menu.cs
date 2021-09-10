using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] TMP_Text userInfo;
    [SerializeField] int userAccount = 0;
    [SerializeField] int verificationCode = 0;
    private void Start()
    {
        userInfo.text = "User Account: " + userAccount;
        userInfo.text += "\n" + "Verification Code: " + verificationCode;
    }
}
