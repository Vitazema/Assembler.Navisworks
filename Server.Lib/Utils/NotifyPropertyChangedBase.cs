using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Server.Lib.Utils
{
  public class NotifyPropertyChangedBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
