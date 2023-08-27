using UnityEngine;

namespace Elly.Runtime
{
    /// <summary>
    /// Controller value pack
    /// </summary>
    [System.Serializable]
    public struct ControllerStat
    {
        #region Field
        /// <summary>
        /// Player look control y will multiply by -1 <br />
        /// If false then it will multiply by 1
        /// </summary>
        [SerializeField] [Tooltip("Inverse mouse y")] public bool InverseY;
        /// <summary>
        /// The trigger tell if player looking control is enable
        /// <code>
        /// if (_CanSee){
        ///     // Insert seeing related code here
        /// }
        /// </code>
        /// </summary>
        [SerializeField] [Tooltip("Rotate control enable")] public bool CanSee;
        /// <summary>
        /// The trigger tell if player moving control is enable
        /// <code>
        /// if (_CanMove){
        ///     // Insert moving related code here
        /// }
        /// </code>
        /// </summary>
        [SerializeField] [Tooltip("Moving control enable")] public bool CanMove;
        /// <summary>
        /// Camera rigging transform smoothing moving speed <br />
        /// The small the number, the faster it move <br />
        /// If it's 0 then it won't called smoothdamp function, it will just apply the <seealso cref="_CamRiggingMovingDest"/>
        /// </summary>
        [SerializeField] [Tooltip("The lower the value, the faster it move")] [Range(0f, 2f)] public float CamRiggingMoveSmoothSpeed;
        /// <summary>
        /// Camera rigging transform smoothing rotating speed <br />
        /// The small the number, the faster it rotate <br />
        /// If it's 0 then it won't called smoothdamp function, it will just apply the <seealso cref="_CamRiggingRotationDest"/>
        /// </summary>
        [SerializeField] [Tooltip("The lower the value, the faster it rotate")] [Range(0f, 2f)] public float CamRiggingRotateSmoothSpeed;
        /// <summary>
        /// Camera transform smoothing moving speed <br />
        /// The small the number, the faster it move <br />
        /// If it's 0 then it won't called smoothdamp function, it will just apply the <seealso cref="_CamMovingDest"/>
        /// </summary>
        [SerializeField] [Tooltip("The lower the value, the faster it move")] [Range(0f, 2f)] public float CamMoveSmoothSpeed;
        /// <summary>
        /// Camera transform smoothing rotating speed <br />
        /// The small the number, the faster it rotate <br />
        /// If it's 0 then it won't called smoothdamp function, it will just apply the <seealso cref="_CamRotationDest"/>
        /// </summary>
        [SerializeField] [Tooltip("The lower the value, the faster it rotate")] [Range(0f, 2f)] public float CamRotateSmoothSpeed;
        #endregion

        #region Static Getter
        /// <summary>
        /// Default controller values
        /// </summary>
        public static ControllerStat Default
        {
            get
            {
                return new ControllerStat()
                {
                    InverseY = true,
                    CanMove = false,
                    CanSee = false,
                    CamRiggingMoveSmoothSpeed = 0.03f,
                    CamRiggingRotateSmoothSpeed = 0.03f,
                    CamMoveSmoothSpeed = 0.03f,
                    CamRotateSmoothSpeed = 0.03f
                };
            }
        }
        #endregion
    }
}
