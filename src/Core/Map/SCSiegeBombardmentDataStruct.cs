using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace SeparatistCrisis.Map
{
    public struct SCSiegeBombardmentData
    {
        public Vec3 LaunchGlobalPosition;

        public Vec3 TargetPosition;

        public MatrixFrame ShooterGlobalFrame;

        public MatrixFrame TargetAlignedShooterGlobalFrame;

        public float MissileSpeed;

        public float Gravity;

        public float LaunchAngle;

        public float RotationDuration;

        public float ReloadDuration;

        public float AimingDuration;

        public float MissileLaunchDuration;

        public float FireDuration;

        public float FlightDuration;

        public float TotalDuration;
    }
}
