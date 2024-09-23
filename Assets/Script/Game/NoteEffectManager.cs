﻿using MajdataPlay.Types;
using System;
using UnityEngine;

namespace MajdataPlay.Game
{
#nullable enable
    public class NoteEffectManager : MonoBehaviour
    {
        NoteEffectPool effectPool;
        GameObject fireworkEffect;
        Animator fireworkEffectAnimator;


        void Start()
        {
            fireworkEffect = GameObject.Find("FireworkEffect");
            fireworkEffectAnimator = fireworkEffect.GetComponent<Animator>();

            effectPool = GetComponent<NoteEffectPool>();
        }
        public void PlayFireworkEffect(in Vector3 position)
        {
            fireworkEffectAnimator.SetTrigger("Fire");
            fireworkEffect.transform.position = position;
        }
        /// <summary>
        /// Tap, Hold, Star
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isBreak"></param>
        /// <param name="judge"></param>
        public void PlayEffect(int position, in JudgeResult judgeResult)
        {
            effectPool.Play(judgeResult, position);
        }
        public void PlayTouchEffect(SensorType sensorPos, in JudgeResult judgeResult)
        {
            effectPool.Play(judgeResult, sensorPos);
        }
        public void PlayTouchHoldEffect(SensorType sensorPos, in JudgeResult judgeResult)
        {
            effectPool.PlayTouchHoldEffect(judgeResult, sensorPos);
        }
        public static bool CheckJudgeDisplaySetting(in JudgeDisplayType setting, in JudgeResult judgeResult)
        {
            var result = judgeResult.Result;
            var resultValue = (int)result;
            var absValue = Math.Abs(7 - resultValue);

            return setting switch
            {
                JudgeDisplayType.All => true,
                JudgeDisplayType.BelowCP => resultValue != 7,
                JudgeDisplayType.BelowP => absValue > 2,
                JudgeDisplayType.BelowGR => absValue > 5,
                JudgeDisplayType.All_BreakOnly => judgeResult.IsBreak,
                JudgeDisplayType.BelowCP_BreakOnly => absValue != 0 && judgeResult.IsBreak,
                JudgeDisplayType.BelowP_BreakOnly => absValue > 2 && judgeResult.IsBreak,
                JudgeDisplayType.BelowGR_BreakOnly => absValue > 5 && judgeResult.IsBreak,
                _ => false
            };
        }
        public void ResetEffect(int position)
        {
            effectPool.Reset(position);
        }
    }
}