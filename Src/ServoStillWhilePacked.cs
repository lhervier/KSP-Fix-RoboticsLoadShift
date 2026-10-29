using System;
using System.Reflection;
using Expansions.Serenity;
using HarmonyLib;
using UnityEngine;

namespace com.github.lhervier.ksp.fix.roboticsloadshift
{
    /// <summary>
    /// Keeps the moving part of a robotic part (piston, hinge, rotation servo, rotor) kinematic while its part is
    /// packed. When a vessel loads, BaseServo.OnStartBeforePartAttachJoint runs after Part.Pack and makes that moving
    /// part non-kinematic again, alone in a packed vessel: its motor then drives it towards its target position,
    /// without the load that held it elsewhere, and KSP Community Fixes RoboticsDrift turns that motion into the
    /// saved positions of the parts the servo carries.
    /// </summary>
    internal static class ServoStillWhilePacked
    {
        private static readonly MethodInfo OnStartBeforePartAttachJoint =
            AccessTools.DeclaredMethod(typeof(BaseServo), nameof(BaseServo.OnStartBeforePartAttachJoint));

        /// <summary>Patches the start of the robotic parts.</summary>
        /// <exception cref="MissingMemberException">BaseServo does not have the method patched.</exception>
        public static void Install(Harmony harmony)
        {
            if (OnStartBeforePartAttachJoint == null)
            {
                throw new MissingMemberException("BaseServo.OnStartBeforePartAttachJoint");
            }
            harmony.Patch(OnStartBeforePartAttachJoint,
                postfix: new HarmonyMethod(typeof(ServoStillWhilePacked), nameof(OnStartBeforePartAttachJointPostfix)));
        }

        /// <summary>
        /// Makes the moving part of the servo kinematic again when the servo starts while its part is packed.
        /// </summary>
        private static void OnStartBeforePartAttachJointPostfix(BaseServo __instance)
        {
            Part part = __instance.part;
            if (!HighLogic.LoadedSceneIsFlight || part == null || !part.packed)
            {
                return;
            }
            // part.servoRb is the moving part's Rigidbody (BaseServo.CreateServoRigidbody). Part.Unpack makes it
            // non-kinematic again with the rest of the part, so nothing has to be undone later.
            Rigidbody servoRb = part.servoRb;
            if (servoRb == null || servoRb.isKinematic)
            {
                return;
            }
            servoRb.isKinematic = true;
            if (Log.IsDebugEnabled)
            {
                Log.Debug($"Moving part of {part.partInfo?.name} ({part.persistentId}) of " +
                    $"{part.vessel?.vesselName} kept still while packed");
            }
        }
    }
}
