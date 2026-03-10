using System;
using System.Collections.Generic;
using Core.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class RoomMessage : MonoBehaviour
    {
        public GameObject messageText;
        public Transform messageParent;
        public Scrollbar messageScrollbar;
        public TMP_InputField messageInput;
        
        

        public void AddMessage(string msg)
        {
            var obj = Instantiate(messageText);
            obj.GetComponent<TextMeshProUGUI>().text = msg;
            messageScrollbar.value = 0;
            obj.transform.SetParent(messageParent);
        }

        private void Start()
        {
            messageInput.onEndEdit.AddListener(OnMessageInputEndEdit);
        }

        private void OnMessageInputEndEdit(string arg0)
        {
            if (arg0 == "") return;
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGamePlayerSendMessage, new NetDictionary()
            {
                { "message", messageInput.text }
            }));
            messageInput.text = "";
        
        }
    }
}