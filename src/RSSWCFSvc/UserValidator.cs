using System;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace RSSWCFSvc
{
   /// <summary>
   /// In web.config, add wsHttpBinding with mode attr of security element = Message, Transport, or TransportWithMessageCredential.
   /// Set clientCredentialType attr of message or transport element to Basic or UserName.
   /// 
   /// Ex:
   /// <system.serviceModel>
   ///    <bindings>
   ///       <wsHttpBinding>
   ///          <binding name="messageBinding">
   ///             <security mode="Message">
   ///                <message clientCredentialType="UserName" />
   ///             </security>
   ///          </binding>
   ///       </wsHttpBinding>
   ///    </bindings>
   /// </system.serviceModel>
   /// <services>
   ///    <service behaviorConfiguration="yourConfig" name="RSSWCFSvc.RssService">
   ///       <endpoint address="" binding="wsHttpBinding" bindingConfiguration="messageBinding" name="UserValidatedService" contract="RSSWCFSvc.IRSS">
   ///       </endpoint>
   ///    </service>
   /// </services>
   /// 
   /// Add markup to use this validator class:
   /// <behaviors>
   ///    <serviceBehaviors>
   ///       <behavior name="yourConfig">
   ///          <serviceCredentials>
   ///             <userNameAuthentication userNamePasswordValidationMode="Custom" customUserNamePasswordValidatorType="RSSWCFSvc.UserValidator, RSSWCFSvc" />
   ///          </serviceCredentials>
   ///       </behavior>
   ///    </serviceBehaviors>
   /// </behaviors>
   /// </summary>
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   public class UserValidator : UserNamePasswordValidator
   {
      public override void Validate(string userName, string password)
      {
         if (string.IsNullOrWhiteSpace(userName)  ||  string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException("User name and password are required");

         if (userName != "JennyJenny"  ||  password != "8675309")
            throw new FaultException("Unknown user name and/or password");
      }
   }
}