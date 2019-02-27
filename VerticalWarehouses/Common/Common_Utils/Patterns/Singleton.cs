namespace Ferretto.Common.Common_Utils
{
    //Usage: var instance = Singleton<typeof(instance)>.UniqueInstance;
    public class Singleton<T> where T : class, new()
    {
        #region Constructors

        private Singleton()
        {
        }

        #endregion

        #region Properties

        public static T UniqueInstance => SingletonCreator.instance;

        #endregion

        #region Classes

        private class SingletonCreator
        {
            #region Fields

            internal static readonly T instance = new T();

            #endregion

            #region Constructors

            static SingletonCreator()
            {
            }

            #endregion
        }

        #endregion
    }
}
