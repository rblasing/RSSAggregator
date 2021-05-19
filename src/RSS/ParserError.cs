﻿// Copyright 2020 Richard Blasingame. All rights reserved.

namespace RSS
{
   public class ParserError
   {
      public readonly string ElementName;
      public readonly string ElementValue;
      public readonly string Message;


      public ParserError(string name, string value, string msg)
      {
         ElementName = name;
         ElementValue = value;
         Message = msg;
      }
   }
}