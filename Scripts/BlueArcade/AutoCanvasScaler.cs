using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace BlueArcade
{
    public class AutoCanvasScaler : MonoBehaviour
    {
        private Vector2 _resolution; // 해상도 저장할 변수
        private CanvasScaler _canvasScaler; // CanvasScaler 컴포넌트
        private void Start()
        {
            _canvasScaler = GetComponent<CanvasScaler>(); // CanvasScaler 컴포넌트 가져오기
            SetResolution(); // 초기에 게임 해상도 고정
        }

        private void Update()
        {
            if(_resolution.x != Screen.width || _resolution.y != Screen.height) // 해상도가 변경되었을 때
            {
                SetResolution(); // 해상도 변경 함수 호출
            }
        }

        /* 해상도 설정하는 함수 */
        [Button]
        public void SetResolution()
        {
            int setWidth = 1920; // 사용자 설정 너비
            int setHeight = 1080; // 사용자 설정 높이

            int deviceWidth = Screen.width; // 기기 너비 저장
            int deviceHeight = Screen.height; // 기기 높이 저장
            _resolution = new Vector2(deviceWidth, deviceHeight); // 해상도 저장
            
            if(_canvasScaler == null)
            {
                _canvasScaler = GetComponent<CanvasScaler>(); // CanvasScaler 컴포넌트 가져오기
            }

            _canvasScaler.matchWidthOrHeight = (float)setWidth / setHeight < (float)deviceWidth / deviceHeight ? 1 : 0; // 기기의 해상도 비가 더 큰 경우
        }
    }
}
