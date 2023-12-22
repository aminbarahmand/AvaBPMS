namespace AvaBPMS.Domain.Events;

public class WorkFlowStartedEvent : BaseEvent
{
    public WorkFlowStartedEvent(WorkFlowStep item)
    {
        Item = item;
    }

    public WorkFlowStep Item { get; }
}
