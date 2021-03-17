// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   public class RSSException : Exception
   {
      public readonly ParserError error;


      public RSSException(ParserError e) : base(e.message)
      {
         error = e;
      }
   }
}
