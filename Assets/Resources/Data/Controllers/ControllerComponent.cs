using Catacumba.Data.Controllers;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Entity
{
    public class ControllerComponent : CharacterComponentBase
    {
        public ControllerBase Controller;
        public CharacterData Data { get { return data; } }

        public void SetController(ControllerBase newController)
        {
            if (!newController)
                throw new System.Exception("controller passed == null");

            if (Controller)
                Controller.Destroy(this);

            Controller = Instantiate<ControllerBase>(newController);
            Controller.Setup(this);
        }
        
        ////////////////////////////// 
        //      MONOBEHAVIOUR
#region MONOBEHAVIOUR

        protected override void Start()
        {
            base.Start();
            if (Controller)
                SetController(Controller);   
            else
                Debug.LogError("ControllerComponent with no Controller set.");
        }

        void Update()
        {
            Controller?.OnUpdate(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Controller?.Destroy(this);
        }

#endregion

    }
}