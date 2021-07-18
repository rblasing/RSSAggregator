using System;


namespace RSS
{
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
