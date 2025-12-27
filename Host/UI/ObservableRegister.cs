using Chameleon.Emulator.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Host.UI
{
    class ObservableRegister : INotifyPropertyChanged
    {
        public ObservableRegister(IRegister register)
        {
            Register = register;
            LastData = register.GetData();
        }

        public void NotifyIfChanged()
        {
            if (LastData != Register.GetData())
            {
                LastData = Register.GetData();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Data"));
            }
        }

        public string Name => Register.Name;
        public string DisplayValue => Register.DisplayValue;

        public event PropertyChangedEventHandler PropertyChanged;
        private UInt32 LastData;
        private IRegister Register;


    }
}
