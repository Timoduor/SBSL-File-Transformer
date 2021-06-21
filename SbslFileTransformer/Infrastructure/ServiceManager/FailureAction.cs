namespace SbslFileTransformer.Infrastructure.ServiceManager
{
    public class FailureAction
    {
        // Default constructor
        public FailureAction()
        {
        }

        // Constructor
        public FailureAction(ServiceRecoveryOptionHelper.RecoverAction actionType, int actionDelay)
        {
            Type = actionType;
            Delay = actionDelay;
        }

        // Property to set recover action type
        public ServiceRecoveryOptionHelper.RecoverAction Type { get; set; } =
            ServiceRecoveryOptionHelper.RecoverAction.None;

        // Property to set recover action delay
        public int Delay { get; set; }
    }
}