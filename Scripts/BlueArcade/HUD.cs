using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using VContainer;

namespace BlueArcade
{
    [Binding]
    public class HUD : ViewModel
    {
        private StageManager _stageManager;
        private SpecialSkill _specialSkill;
        
        [Inject]
        private void Construct(StageManager stageManager)
        {
            _stageManager = stageManager;
            _specialSkill = _stageManager.GetComponentInChildren<SpecialSkill>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_stageManager.GameState != GameState.Playing)
            {
                return;
            }
            
            DashCoolTimeRatio = _stageManager.Player.DashCoolTimeRatio;
            SpecialSkillCoolTimeRatio = _specialSkill.CoolTimeRatio;
        }

        private float _dashCoolTimeRatio;

        [Binding]
        public float DashCoolTimeRatio
        {
            get => _dashCoolTimeRatio;
            set
            {
                _dashCoolTimeRatio = value;
                OnPropertyChanged(nameof(DashCoolTimeRatio));
            }
        }

        private float _specialSkillCoolTimeRatio;

        [Binding]
        public float SpecialSkillCoolTimeRatio
        {
            get => _specialSkillCoolTimeRatio;
            set
            {
                _specialSkillCoolTimeRatio = value;
                OnPropertyChanged(nameof(SpecialSkillCoolTimeRatio));
            }
        }

        
        
    }
}
