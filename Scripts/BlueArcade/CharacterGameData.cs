using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace BlueArcade
{
    [CreateAssetMenu(fileName = "CharacterGameData", menuName = "CharacterGameData")]
    public class CharacterGameData : ScriptableObject
    {
        
        public string characterName;
        public RuntimeAnimatorController animatorController;
        public Weapon weapon;
        public AudioClip[] battleInVoices; // 일단은 안 쓸 듯

        #region Editor_Utility
        #if UNITY_EDITOR
        // 나중에 에디터 스크립트 너무 커지면 분리 필요.
        private const string AnimationRootPath = "Assets/Animations/character";
        private const string AnimationControllerRootPath = "Assets/Animators/character";

        private const string DefaultCharacterName = "Shiroko";
        private static string[] AnimationNames = new string[]
        {
            "idle",
            "move",
            "jump",
        };
        
        [Button("Ping")]
        public void Ping()
        {
            EditorGUIUtility.PingObject(this);
        }
        
        [Button("일괄 생성")]
        public void CreateAll()
        {
            try
            {
                // 파일명 수정
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), $"CharacterGameData_{characterName}");
                CreateAnimationClip(false);
                CreateAnimatorController(false);
                SetWeapon();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.LogError($"{characterName} 일괄 생성 중 오류 발생");
                return;
            }
            Debug.Log($"{characterName} 일괄 생성 완료");
        }
        
        [Button("애니메이션 클립 생성")]
        public void CreateAnimationClip(bool ping = true)
        {
            if(characterName == DefaultCharacterName || characterName == "")
            {
                Debug.LogError("캐릭터 이름을 변경해주세요.");
                return;
            }

            foreach (var animationName in AnimationNames)
            {
                AnimationClip baseClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationRootPath}/{DefaultCharacterName}_{animationName}.anim");
                // duplicate
                AnimationClip newClip = new AnimationClip();
                EditorUtility.CopySerialized(baseClip, newClip);
                string newClipPath = $"{AnimationRootPath}/{characterName}_{animationName}.anim";
                AssetDatabase.CreateAsset(newClip, newClipPath);
                Debug.Log($"애니메이션 클립 생성 : {newClipPath}");
                
                // 애니메이션 클립에서 사용된 스프라이트 가져오기
                EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(newClip);
                foreach (var binding in bindings)
                {
                    if (binding.type == typeof(SpriteRenderer))
                    {
                        ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(newClip, binding);
                        for(int i = 0; i < keyframes.Length; i++)
                        {
                            Sprite sprite = keyframes[i].value as Sprite;
                            if (sprite != null)
                            {
                                // path of sprite
                                string spritePath = AssetDatabase.GetAssetPath(sprite);
                                string newSpritePath = spritePath.Replace(DefaultCharacterName, characterName);
                                // load sprite
                                Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(newSpritePath);
                                keyframes[i].value = newSprite;
                            }
                        }
                        AnimationUtility.SetObjectReferenceCurve(newClip, binding, keyframes);
                    }
                }
                EditorUtility.SetDirty(newClip);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                if (ping)
                {
                    EditorGUIUtility.PingObject(newClip);
                }
            }
        }
        [Button("애니메이션 컨트롤러 생성")]
        public void CreateAnimatorController(bool ping = true)
        {
            if(characterName == DefaultCharacterName || characterName == "")
            {
                Debug.LogError("캐릭터 이름을 변경해주세요.");
                return;
            }

            AnimatorOverrideController baseController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>($"{AnimationControllerRootPath}/{DefaultCharacterName}.overrideController");
            // duplicate
            AnimatorOverrideController newController = new AnimatorOverrideController();
            EditorUtility.CopySerialized(baseController, newController);
            string newControllerPath = $"{AnimationControllerRootPath}/{characterName}.overrideController";
            AssetDatabase.CreateAsset(newController, newControllerPath);
            Debug.Log($"애니메이션 컨트롤러 생성 : {newControllerPath}");
            
            foreach (var animationName in AnimationNames)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationRootPath}/{characterName}_{animationName}.anim");
                newController[$"{DefaultCharacterName}_{animationName}"] = clip;
            }
            animatorController = newController;
            if (ping)
            {
                EditorGUIUtility.PingObject(newController);
            }
            AssetDatabase.SaveAssets();
        }

        public void SetWeapon()
        {
            weapon = AssetDatabase.LoadAssetAtPath<Weapon>($"Assets/Prefabs/Weapon/{characterName}.prefab");
        }
        #endif
        #endregion
    }
}
