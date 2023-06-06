using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomCustomPropertyGenerator : MonoBehaviour
{
    [SerializeField] private Text _text;
    
    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private void SetCustomNumber()
    {
        int result = Random.Range(0, 99);

        _text.text = result.ToString();
        _myCustomProperties["RandomNumber"] = result;
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
        // PhotonNetwork.LocalPlayer.CustomProperties = _myCustomProperties;
    }

    public void OnClickButton()
    {
        SetCustomNumber();
    }
    
}
