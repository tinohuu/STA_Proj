using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] TMP_Text m_UserAccountText;
    [SerializeField] TMP_Text m_VerificationCodeText;
    private void Start()
    {
        //userInfo.text = "User Account: " + userAccount;
        //userInfo.text += "\n" + "Verification Code: " + verificationCode;
    }
}
