using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BloodClotID
{
    public class SwitchEventArgs : EventArgs
    {
        private Stage stage;

        public SwitchEventArgs(Stage dstStage)
        {
            stage = dstStage;
        }
        public Stage Stage
        {
            get { return stage; }
        }
    }


    public class BaseUserControl:UserControl
    {
        protected bool bInitialized = false;
        public event EventHandler<SwitchEventArgs> stageSwitched;
        protected virtual void OnSwitch(Stage stage)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<SwitchEventArgs> handler = stageSwitched;
            if (handler != null)
            {
                handler(this, new SwitchEventArgs(stage));
            }
        }
        public virtual void Initialize()
        {
            bInitialized = true;
            //InitializeImpl();
        }

        
    }

    public enum Stage
    {
        Preapare,
        Analysis,
        Report
    }
}
