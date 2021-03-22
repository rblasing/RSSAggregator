// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.ComponentModel;


namespace RSSAggregatorSvc
{
   [RunInstaller(true)]
   public partial class ProjectInstaller : System.Configuration.Install.Installer
   {
      public ProjectInstaller()
      {
         InitializeComponent();
      }
   }
}
