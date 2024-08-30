using System;
using UnityEngine;

namespace Unbound.Core.Utils.UI {
    public class ActionOnBecameVisible:MonoBehaviour {
        public Action visibleAction = () => { };

        private void OnBecameVisible() {
            visibleAction.Invoke();
        }
    }
}