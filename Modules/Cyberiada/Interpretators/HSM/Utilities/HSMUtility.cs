namespace Talent.Logic.HSM.Utilities
{
    /// <summary>
    /// Класс, содержащий полезные методы для работы с иерархической машиной состояний (ИМС)
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Определяет, является ли данное родительское состояние прямым или косвенным родителем следующего состояния.
        /// </summary>
        /// <param name="parent">Родительское состояние</param>
        /// <param name="next">Идентификатор следующего состояния</param>
        /// <returns>true, родительское состояние родитель следующего состояния, иначе false</returns>
        public static bool IsParentOfNextState(State parent, string next)
        {
            if (parent.ChildStates == null)
            {
                return false;
            }

            foreach (State state in parent.ChildStates)
            {
                if (state.ID == next || IsParentOfNextState(state, next))
                {
                    return true;
                }
            }

            return false;
        }
    }
}