using UnityEngine;

namespace Elly.Runtime
{
    /// <summary>
    /// Vibration direction 
    /// </summary>
    public enum VibrationDirection
    {
        X, Y, Z, XY, XZ, YZ, XYZ
    }

    /// <summary>
    /// Vibration value pack
    /// </summary>
    [System.Serializable]
    public struct VibrationStat
    {
        #region Field
        /// <summary>
        /// Vibration direction <br />
        /// It will vibration by two point: [dir, -dir]
        /// </summary>
        [SerializeField] [Tooltip("Vibration direction")] public Vector3 Dir;
        /// <summary>
        /// The lower the value, the faster strength value decrease
        /// </summary>
        [SerializeField] [Tooltip("Slow down smoothTime")] public float SmoothTime;
        /// <summary>
        /// Strength is determine vibrate speed and if vibration is enable <br />
        /// During the update this value will decreasse over time smoothly <br />
        /// When it hit the tolerance value it will set to 0
        /// </summary>
        [SerializeField] [Tooltip("Strength is determine vibrate speed and if vibration is enable")] public float Strength;
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] [Tooltip("Frequency will effect vibrate speed")] public float Frequency;
        /// <summary>
        /// Smoothdamp velocity
        /// </summary>
        [HideInInspector] public float Velocity;
        /// <summary>
        /// When strength is lower than this value, just set to 0
        /// </summary>
        [SerializeField] [Tooltip("When strength is lower than this value, just set to 0")] public float Tolerance;
        /// <summary>
        /// Effect <seealso cref="SetVibrationDirection"/> vector length
        /// </summary>
        private const float DefaultLength = 0.01f;
        #endregion

        #region
        /// <summary>
        /// Current vibration additional vector <br />
        /// This is the result calculate vector <br />
        /// Apply this to camera position
        /// </summary>
        public Vector3 Vibration
        {
            get
            {
                if (Strength == 0) return Vector3.zero;
                float current = Mathf.Sin(Time.time * Frequency);
                return Dir * current * (1f / (1f / Strength));
            }
        }
        #endregion

        #region Setter
        /// <summary>
        /// Set vibrate direction
        /// </summary>
        /// <param name="dir">Direction</param>
        public void SetVibrationDirection(VibrationDirection dir)
        {
            switch (dir)
            {
                case VibrationDirection.X:
                    SetVibrationDirection(new Vector3(1, 0, 0).normalized * DefaultLength);
                    break;
                case VibrationDirection.Y:
                    SetVibrationDirection(new Vector3(0, 1, 0).normalized * DefaultLength);
                    break;
                case VibrationDirection.Z:
                    SetVibrationDirection(new Vector3(0, 0, 1).normalized * DefaultLength);
                    break;
                case VibrationDirection.XY:
                    SetVibrationDirection(new Vector3(1, 1, 0).normalized * DefaultLength);
                    break;
                case VibrationDirection.XZ:
                    SetVibrationDirection(new Vector3(1, 0, 1).normalized * DefaultLength);
                    break;
                case VibrationDirection.YZ:
                    SetVibrationDirection(new Vector3(0, 1, 1).normalized * DefaultLength);
                    break;
                case VibrationDirection.XYZ:
                    SetVibrationDirection(new Vector3(1, 1, 1).normalized * DefaultLength);
                    break;
            }
        }
        /// <summary>
        /// Set vibrate direction
        /// </summary>
        /// <param name="dir">Direction</param>
        public void SetVibrationDirection(Vector3 dir)
        {
            Dir = dir;
        }
        #endregion

        #region Method
        /// <summary>
        /// Strength decrease smoothly over time
        /// </summary>
        public void Update()
        {
            Strength = Mathf.SmoothDamp(Strength, 0f, ref Velocity, SmoothTime);
            if (Strength < Tolerance) Strength = 0;
        }
        #endregion

        #region Static Getter
        /// <summary>
        /// Default vibration values
        /// </summary>
        public static VibrationStat Default
        {
            get
            {
                return new VibrationStat()
                {
                    Dir = Vector3.up * 0.01f,
                    SmoothTime = 0.3f,
                    Strength = 0f,
                    Frequency = 100f,
                    Tolerance = 0.1f,
                };
            }
        }
        #endregion
    }
}
