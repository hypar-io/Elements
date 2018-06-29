namespace Hypar.Commands
{    
    internal interface IHyparCommand : System.Windows.Input.ICommand
    {
        void Help();
    }
}