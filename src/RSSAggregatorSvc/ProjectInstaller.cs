// Copyright 2020 Richard Blasingame. All rights reserved.

using System.ComponentModel;


namespace RssAggregatorSvc
{
   [RunInstaller(true)]
   public partial class ProjectInstaller : System.Configuration.Install.Installer
   {
      public ProjectInstaller() => InitializeComponent();
   }
}
