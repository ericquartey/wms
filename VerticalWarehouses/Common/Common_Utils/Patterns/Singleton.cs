namespace Ferretto.Common.Common_Utils
{
    //Usage: var instance = Singleton<typeof(instance)>.UniqueInstance;
    public class Singleton<T> where T : class, new()
    {
        #region Constructors

        private Singleton()
        { }

        #endregion Constructors

        #region Properties

        public static T UniqueInstance => SingletonCreator.instance;

        #endregion Properties

        #region Classes

        private class SingletonCreator
        {
            #region Fields

            internal static readonly T instance = new T();

            #endregion Fields

            #region Constructors

            static SingletonCreator()
            {
            }

            #endregion Constructors
        }

        #endregion Classes
    }
}
