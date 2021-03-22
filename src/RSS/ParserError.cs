// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   public class ParserError
   {
      public readonly string elementName;
      public readonly string elementValue;
      public readonly string message;


      public ParserError(string name, string value, string msg)
      {
         elementName = name;
         elementValue = value;
         message = msg;
      }
   }
}
