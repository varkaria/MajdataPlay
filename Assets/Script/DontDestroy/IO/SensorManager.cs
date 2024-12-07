﻿using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class InputManager : MonoBehaviour
    {
        readonly Sensor[] _sensors = new Sensor[33];
        readonly Dictionary<SensorType, DateTime> _sensorLastTriggerTimes = new();
        void UpdateSensorState()
        {
            foreach(var (index, on) in _COMReport.WithIndex())
            {
                if (index > _sensors.Length)
                    break;
                var sensor = index switch
                {
                    <= (int)SensorType.C => _sensors[index],
                     > 17 => _sensors[index - 1],
                     _    => _sensors[16],
                };
                if (sensor == null)
                {
                    Debug.LogError($"{index}# Sensor instance is null");
                    continue;
                }
                var oState = sensor.Status;
                var nState = on ? SensorStatus.On : SensorStatus.Off;
                if (sensor.Type == SensorType.C)
                    nState = _COMReport[16] || _COMReport[17] ? SensorStatus.On : SensorStatus.Off;
                if(oState != nState)
                {
                    var now = DateTime.Now;
                    if (JitterDetect(sensor.Type, now))
                        continue;
                    _sensorLastTriggerTimes[sensor.Type] = now;
                    Debug.Log($"Sensor \"{sensor.Type}\": {nState}");
                    sensor.Status = nState;
                    var msg = new InputEventArgs()
                    {
                        Type = sensor.Type,
                        OldStatus = oState,
                        Status = nState,
                        IsButton = false
                    };
                    sensor.PushEvent(msg);
                    PushEvent(msg);
                    SetIdle(msg);
                }
            }
        }
        void SetSensorState(SensorType type,SensorStatus nState)
        {
            var sensor = _sensors[(int)type];
            if (sensor == null)
                throw new Exception($"{type} Sensor not found.");
            var oState = sensor.Status;
            sensor.Status = nState;

            if (oState != nState)
            {
                Debug.Log($"Sensor \"{sensor.Type}\": {nState}");
                sensor.Status = nState;
                var msg = new InputEventArgs()
                {
                    Type = sensor.Type,
                    OldStatus = oState,
                    Status = nState,
                    IsButton = false
                };
                sensor.PushEvent(msg);
                PushEvent(msg);
                SetIdle(msg);
            }
        }
        public void BindSensor(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = _sensors.Find(x => x?.Type == sType);
            if (sensor == null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.AddSubscriber(checker);
        }
        public void UnbindSensor(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = _sensors.Find(x => x?.Type == sType);
            if (sensor == null)
                throw new Exception($"{sType} Sensor not found.");
            sensor.RemoveSubscriber(checker);
        }
    }
}
