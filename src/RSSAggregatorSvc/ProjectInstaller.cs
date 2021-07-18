using System;
using System.ComponentModel;


namespace RssAggregatorSvc
{
   [RunInstaller(true)]
   public partial class ProjectInstaller : System.Configuration.Install.Installer
   {
      public ProjectInstaller() => InitializeComponent();
   }
}
