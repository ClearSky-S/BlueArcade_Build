using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SROptions
{
    public void ReloadScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void SetRandomCharacter(){
        var stageManager = Object.FindObjectOfType<BlueArcade.StageManager>();
        stageManager.SetRandomCharacter();
    }
    public void AddScore(){
        var stageManager = Object.FindObjectOfType<BlueArcade.StageManager>();
        stageManager.AddScore();
    }
    public void SpawnWeaponBox(){
        var stageManager = Object.FindObjectOfType<BlueArcade.StageManager>();
        stageManager.SpawnWeaponBox();
    }
}
