using Doozy.Runtime.UIManager.Containers;
using UnityEngine.Events;
using UnityWeld;
using UnityWeld.Binding;

namespace BlueArcade
{
    [Binding]
    public class SimplePopup : ViewModel
    {
        public void Init(string desc, UnityAction onConfirm = null, UnityAction onCancel = null)
        {
            Desc = desc;
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
            ShowCancalButton = onCancel != null;
        }
        private string _desc;

        [Binding]
        public string Desc
        {
            get => _desc;
            set
            {
                _desc = value;
                OnPropertyChanged(nameof(Desc));
            }
        }

        public UnityAction onConfirm;
        [Binding]
        public void OnConfirm()
        {
            GetComponent<UIPopup>().Hide();
            onConfirm?.Invoke();
        }

        private bool _showCancalButton;

        [Binding]
        public bool ShowCancalButton
        {
            get => _showCancalButton;
            set
            {
                _showCancalButton = value;
                OnPropertyChanged(nameof(ShowCancalButton));
            }
        }

        
        public UnityAction onCancel;
        [Binding]
        public void OnCancel()
        {
            GetComponent<UIPopup>().Hide();
            onCancel?.Invoke();
        }
    }
}