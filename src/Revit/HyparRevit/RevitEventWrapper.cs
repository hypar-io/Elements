using Autodesk.Revit.UI;
using Serilog;

namespace Hypar.Revit
{
    //https://forums.autodesk.com/t5/revit-api-forum/externalevent-raise-should-accept-parameters/td-p/5508469
    abstract internal class RevitEventWrapper<T> : IExternalEventHandler 
    {
        private object @lock;
        private T savedArgs;
        private ExternalEvent revitEvent;
        protected ILogger logger;

        public RevitEventWrapper(ILogger logger) 
        {
            this.logger = logger;
            revitEvent = ExternalEvent.Create(this);
            @lock = new object();
        }
    
        public void Execute(UIApplication app) 
        {
            T args;
    
            lock (@lock)
            {
                args = savedArgs;
                savedArgs = default(T);
            }
    
            Execute(app, args);
        }
    
        public string GetName()
        {
            return GetType().Name;
        }
    
        public void Raise(T args)
        {
            lock (@lock)
            {
                savedArgs = args;
            }
    
            revitEvent.Raise();
        }
    
        abstract public void Execute(UIApplication app, T args);
    }
}