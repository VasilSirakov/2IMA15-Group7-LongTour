namespace LongTour
{
    using System.Collections.Generic;
    using UnityEngine;

    ///<summary>
    /// Data container for long tour level, containing the point set
    /// </summary>
    [CreateAssetMenu(fileName = "tourLevelNew", menuName = "Levels/Long Tour Level")]
    public class TourLevel : ScriptableObject
    {
        [Header("Tour Points")]
        public List<Vector2> Points = new List<Vector2>();
    }
}

