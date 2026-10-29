using System;
using HarmonyLib;
using UnityEngine;

namespace com.github.lhervier.ksp.fix.roboticsloadshift
{
    /// <summary>
    /// Installs the fix the settings turn on, once, when KSP starts: from then on, the moving part of a robotic part
    /// stays still while its vessel is packed, and moves again when the vessel is unpacked.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KSPFixRoboticsLoadShift : MonoBehaviour
    {
        private const string HarmonyId = "com.github.lhervier.ksp.fix.roboticsloadshift";

        private void Start()
        {
            Settings.Load();
            Log.Info($"Version {typeof(KSPFixRoboticsLoadShift).Assembly.GetName().Version}, " +
                $"servoStillWhilePacked {Settings.ServoStillWhilePacked}, log level {Log.Level}");
            if (!Settings.ServoStillWhilePacked)
            {
                return;
            }
            try
            {
                ServoStillWhilePacked.Install(new Harmony(HarmonyId));
                Log.Info("Servo still while packed fix installed");
            }
            catch (Exception e)
            {
                Log.Error("Could not install the servo still while packed fix, the parts a loaded servo carries can " +
                    $"still be shifted while its vessel loads, as without it: {e}");
            }
        }
    }
}
