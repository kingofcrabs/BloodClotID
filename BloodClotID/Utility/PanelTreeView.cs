using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class PanelViewModel : INotifyPropertyChanged
    {
        #region Data

        bool? _isChecked = false;
        PanelViewModel _parent;
        bool bFreeezeEvent = false;
        #endregion // Data

        #region CreateFoos
        public static PanelViewModel CreateViewModel(List<AssayGroup> assayGroups)
        {
            PanelViewModel root = new PanelViewModel("所有试验");
            foreach (var group in assayGroups)
            {
                PanelViewModel firstLevel = new PanelViewModel(group.Name);
                foreach (var assay in group.Assays)
                {
                    PanelViewModel secondLevel = new PanelViewModel(assay);
                    firstLevel.Children.Add(secondLevel);
                }
                firstLevel.Initialize();
                root.Children.Add(firstLevel);
                root.Initialize();
            }
            return root;
        }
     
        public void UpdateState(ObservableCollection<string> assays)
        {
            bFreeezeEvent = true;
            foreach (var firstLevel in this.Children)
            {
                foreach (var secondLevel in firstLevel.Children)
                {
                    secondLevel.IsChecked = assays.Contains(secondLevel.Name);
                }
            }
            bFreeezeEvent = false;
        }



        PanelViewModel(string name)
        {
            this.Name = name;
            this.Children = new List<PanelViewModel>();
        }

        void Initialize()
        {
            foreach (PanelViewModel child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        #endregion // CreateFoos

        #region Properties

        public List<PanelViewModel> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }

        public string Name { get; private set; }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            if (!bFreeezeEvent)
            {
                OnCheckStateChanged();

            }
            this.OnPropertyChanged("IsChecked");
        }

        private void OnCheckStateChanged()
        {
            var tmp = this;
            while (true)
            {
                if (tmp._parent != null)
                    tmp = tmp._parent;
                else
                    break;
            }
            tmp.OnPropertyChanged("SonChanged");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        public ObservableCollection<string> GetAssays()
        {
            ObservableCollection<string> assays = new ObservableCollection<string>();
            foreach (var firstLevel in this.Children)
            {
                foreach (var secondLevel in firstLevel.Children)
                {
                    if ((bool)secondLevel.IsChecked)
                    {
                        assays.Add(secondLevel.Name);
                    }
                }
            }
            return assays;
        }
    }

    public class AssayGroup
    {
        string sName = "";
        List<string> assays;
        public AssayGroup(string s, List<string> assays)
        {
            sName = s;
            this.assays = assays;
        }

        public List<string> Assays
        {
            get
            {
                return assays;
            }
        }
        public string Name
        {
            get
            {
                return sName;
            }
        }
    }
}
