using System.Diagnostics;


namespace Util
{
   /// <summary>
   /// Add the following to web.config:
   /// 
   /// <system.diagnostics>
   ///    <sources>
   ///       <source name="System.ServiceModel.MessageLogging" switchValue="All">
   ///          <listeners>
   ///             <add name="messages" />
   ///          </listeners>
   ///       </source>
   ///    </sources>
   ///    <sharedListeners>
   ///       <add name="messages" type="Util.ReqRspTraceListener, Util" initializeData="yourFileName.txt" />
   ///    </sharedListeners>
   /// </system.diagnostics>
   ///
   /// and,
   /// 
   /// <system.serviceModel>
   ///    <diagnostics>
   ///       <messageLogging
   ///          logEntireMessage="true"
   ///          logMalformedMessages="true"
   ///          logMessagesAtServiceLevel="true"
   ///          logMessagesAtTransportLevel="true"
   ///          maxMessagesToLog="3000000"
   ///          maxSizeOfMessageToLog="20000000" />
   ///    </diagnostics>
   /// </system.serviceModel>
   ///
   /// </summary>
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   public class ReqRspTraceListener : TextWriterTraceListener
   {
      public ReqRspTraceListener() :base()
      {
      }


      public ReqRspTraceListener(string filename) : base(filename)
      {
      }
      
      
      public override void Write(string message)
      {
         // only log the edge IO messages, and skip all the .NET event internals
         if ((message.Contains("TransportSend")  ||  message.Contains("TransportReceive"))  &&
            !message.Contains("System.ServiceModel.Channels.NullMessage")  &&
            !message.Contains("ServiceMetadataExtension"))
         {
            base.Write(message.FormattedXml());
         }
      }


      public override void WriteLine(string message)
      {
         if ((message.Contains("TransportSend")  ||  message.Contains("TransportReceive"))  &&
            !message.Contains("System.ServiceModel.Channels.NullMessage")  &&
            !message.Contains("ServiceMetadataExtension"))
         {
            base.WriteLine(message.FormattedXml());
         }
      }
   }
}