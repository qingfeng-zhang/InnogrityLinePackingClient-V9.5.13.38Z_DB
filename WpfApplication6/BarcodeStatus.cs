using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InnogrityLinePackingClient
{
    public enum BarcodeStatus:int
    {
        Null,
        Equal,
        NotEqual,
        NotScanned,
        NotLoggedIn,
        LoggedIn,
        Waiting,
        Timeout,
        AttemptOut,
        ExceptionError,
        Arriving,
        NoTrackingLabel,
        Speck,
        Rejected,
        Duplicated
    }
    public enum AreaStatus : int
    {
        Null,
        Block,
        MemNoClear,
        EarlyTake
    }
}
