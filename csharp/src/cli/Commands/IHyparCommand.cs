namespace Hypar.Commands
{    
    internal interface IHyparCommand : System.Windows.Input.ICommand
    {
        string[] Arguments{get;}
        string Description{get;}
        string Name{get;}
    }
}