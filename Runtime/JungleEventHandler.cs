namespace Jungle
{
    public static class JungleEventHandler
    {
        #region Variables
        
        /// <summary>
        /// 
        /// </summary>
        public static event JungleValidationCallback OnValidation;
        public delegate void JungleValidationCallback();
        
        /// <summary>
        /// 
        /// </summary>
        public static event JungleTreeValidationCallback OnTreeValidation;
        public delegate void JungleTreeValidationCallback(JungleTree tree);
        
        /// <summary>
        /// 
        /// </summary>
        public static event JungleNodeValidationCallback OnNodeValidation;
        public delegate void JungleNodeValidationCallback(JungleNode node);
        
        #endregion
    }
}