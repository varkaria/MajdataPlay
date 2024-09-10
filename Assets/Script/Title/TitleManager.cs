using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public TextMeshProUGUI echoText;
    public Animator fadeInAnim;

    // Start is called before the first frame update
    void Start()
    {
        DelayPlayVoice().Forget();
        WaitForScanningTask().Forget();
        LightManager.Instance.SetAllLight(Color.white);
    }
    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (!e.IsClick)
            return;
        if(e.IsButton)
            NextScene();
        else
        {
            switch(e.Type)
            {
                case SensorType.A8:
                    AudioManager.Instance.OpenAsioPannel();
                    break;
                case SensorType.E5:
                    NextScene();
                    break;
            }
        }
    }
    async UniTaskVoid WaitForScanningTask()
    {
        float animTimer = 0;
        while(animTimer < 2f)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (SongStorage.State == ComponentState.Finished)
            {
                echoText.text = "Press Any Key"; 
                InputManager.Instance.BindAnyArea(OnAreaDown);
                return;
            }
        }
        fadeInAnim.enabled = false;
        int a = -1;
        float timer = 2f;
        while(true)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (timer >= 2f)
                a = -1;
            else if(timer == 0)
                a = 1;
            timer += Time.deltaTime * a;
            timer = timer.Clamp(2f, 0);
            var newColor = Color.white;

            if (SongStorage.State == ComponentState.Finished)
            {
                echoText.text = "Press Any Key";
                if (timer >= 1.8f)
                {
                    echoText.color = newColor;
                    break;
                }
            }
            newColor.a = timer / 2f;
            echoText.color = newColor;
            print(newColor.a);
        }
        InputManager.Instance.BindAnyArea(OnAreaDown);
    }
    async UniTaskVoid DelayPlayVoice()
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        AudioManager.Instance.PlaySFX("MajdataPlay.wav");
        AudioManager.Instance.PlaySFX("titlebgm.mp3");
    }
    void NextScene()
    {
        SceneManager.LoadSceneAsync(1);
    }
    private void OnDestroy()
    {
        InputManager.Instance.UnbindAnyArea(OnAreaDown);
        AudioManager.Instance.StopSFX("titlebgm.mp3");
        AudioManager.Instance.StopSFX("MajdataPlay.wav");
    }
}
