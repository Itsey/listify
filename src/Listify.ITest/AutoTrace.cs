[assembly: Xunit.TestFramework("Listify.ITest.XunitAutoTraceFixture", "Listify.ITest")]

namespace Listify.ITest;

using global::Plisky.Diagnostics;
using global::Plisky.Diagnostics.Listeners;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitAutoTraceFixture : XunitTestFramework {

    public XunitAutoTraceFixture(IMessageSink messageSink)
        : base(messageSink) {
        bool trace = true;
        if (trace) {
            Bilge.AddHandler(new TCPHandler(new TCPHandlerOptions("127.0.0.1", 9060, true)), HandlerAddOptions.SingleType);
            Bilge.SetConfigurationResolver((a, b) => System.Diagnostics.SourceLevels.Verbose);
            Bilge.Alert.Online("testing-online");
            Bilge.Default.Info.Log("Diagnostic fixture activating trace");
        }
    }

    public new void Dispose() {
        base.Dispose();
    }
}