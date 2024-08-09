using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;

namespace BlueArcade
{
    [Binding]
    public class MenuPopup : ViewModel
    {
        private UIManager _uiManager;
        public void Init(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        [Binding]
        public void Resume()
        {
            GetComponent<UIPopup>().Hide();
        }

        [Binding]
        public void Exit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        [Binding]
        public void Credit()
        {
            _uiManager.GetSimplePopup().Init("" +
                                             "프로그래머 : 장준혁\n" +
                                             "아티스트 : 권재혁\n" +
                                             "도움주신 분 : 김우찬");
        }

        [Binding]
        public void Key()
        {
            _uiManager.GetSimplePopup().Init("" +
                                             "이동 : 방향키\n" +
                                             "공격 : Z\n" +
                                             "대쉬 : X\n" +
                                             "점프 : C\n" +
                                             "스페셜 : V");
        }
    }
}
