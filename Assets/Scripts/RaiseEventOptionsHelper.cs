using Photon.Realtime;

namespace LiftStudio
{
    public static class RaiseEventOptionsHelper
    {
        public static readonly RaiseEventOptions All = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        public static readonly RaiseEventOptions Others = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
        public static readonly RaiseEventOptions MasterClient = new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient};
    }
}