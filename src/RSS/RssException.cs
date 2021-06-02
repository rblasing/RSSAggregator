// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   [Serializable]
   public class RssException : Exception
   {
      public ParserError Error;


      public RssException(ParserError e) : base(e.Message) => Error = e;
   }
}
