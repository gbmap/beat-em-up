using Frictionless;
using UnityEngine.SceneManagement;

namespace Catacumba
{
    public class StateManager : SimpleSingleton<StateManager>
    {
        public static bool Retry
        {
            get { return CheckpointManager.Retry; }
        }

        public class MsgOnSceneChangeRequest
        {
            public Scene oldScene;
            public Scene newScene;
        }

        private void Awake()
        {
            ServiceFactory.Instance.RegisterSingleton<MessageRouter>();
        }

        public void ResetScene()
        {
            Scene s = SceneManager.GetActiveScene();
            ServiceFactory.Instance.Resolve<MessageRouter>().RaiseMessage(new MsgOnSceneChangeRequest { newScene = s, oldScene = s });
            SceneManager.LoadSceneAsync(s.buildIndex, LoadSceneMode.Single);
        }
    }
}