using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaBPMS.Domain.Enums
{

    public enum WorkFlowNodeType
    {
        Activity = 1,

        ParallelGateWay = 20,
        ExclusiveGateWay = 21,

        Start = 31, 
        Intermmediate = 32,
        End = 33

    }
    public enum TransitionCommandType
    {

        Send=0,
        Approve=1,
        Reject=2,
        DecisionByValue=3,
    }
    public enum TransitionCondition
    {
        None = 0,
        Equal,
        NotEqual,        
        GreaterThan,
        LeaThan
        
    }
}
