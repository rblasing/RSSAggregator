// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   [Serializable]
   public class RssException : Exception
   {
      public ParserError Error;


      public RssException(ParserError e) : base(e.Message) => Error = e;
   }
}
