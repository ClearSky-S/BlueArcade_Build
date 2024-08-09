using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;
using UnityEngine.Events;
using UnityWeld;

namespace BlueArcade
{
    public class UIManager : MonoBehaviour
    {
        public UnityAction onOpenMenu;
        public UnityAction onCloseMenu;
        private bool _isMenuOpen;
        private MenuPopup _menuPopup;
        public SimplePopup GetSimplePopup()
        {
            // Open popup
            var popup = UIPopup.Get("SimplePopup");
            popup.Show();
            return popup.GetComponent<SimplePopup>();
        }

        public void OpenMenu()
        {
            _isMenuOpen = true;
            onOpenMenu?.Invoke();
            var popup = UIPopup.Get("MenuPopup");
            popup.Show();
            _menuPopup = popup.GetComponent<MenuPopup>();
            _menuPopup.Init(this);
            popup.OnHideCallback.Event.AddListener(
                () =>
                {
                    onCloseMenu?.Invoke();
                    _isMenuOpen = false;
                });
        }

        public void OpenSpecialSkillPopup()
        {
            var popup = UIPopup.Get("SpecialSkillPopup");
            popup.Show();
        }

        private void Update()
        {
            // 메모 : StageManager 쪽으로 로직 옮겨가야 하지 않을까?
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!_isMenuOpen)
                {
                    if(UIPopup.popupsCanvas.transform.childCount == 0)
                    {
                        OpenMenu();
                    }
                }
                else
                {
                    // check is last child
                    if(UIPopup.popupsCanvas.transform.GetChild(UIPopup.popupsCanvas.transform.childCount - 1).gameObject == _menuPopup.gameObject)
                    {
                        _menuPopup.GetComponent<UIPopup>().Hide();
                    }
                }
            }
        }
    }
}
