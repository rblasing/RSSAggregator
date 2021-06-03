﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace UnitTestCoverage
{
   public sealed class UnitTestCoverage
   {
      /// <summary>
      /// Ensure that every class in the assembly being tested has an
      /// associated test class, and that every method in the target class
      /// has an associated test method.  Classes and methods in the target
      /// assembly decorated with the <c>xcludeFromCodeCoverage</c> attribute
      /// will be ignored.
      /// </summary>
      public static IReadOnlyList<string> VerifyCoverage(Assembly targetAssembly, Assembly testAssembly)
      {
         List<string> errors = new List<string>();

         BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

         foreach (var targetClassType in targetAssembly.GetTypes())
         {
            // ignore classes auto-generated by the compiler
            if (targetClassType.CustomAttributes != null)
            {
               if (targetClassType.CustomAttributes.Where(
                  ca => ca.AttributeType == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)).Count() > 0)
                  continue;
            }

            // ignore abstract and auto-generated classes
            if (!targetClassType.IsAbstract  &&  !targetClassType.IsSpecialName  &&  
               targetClassType.Name.StartsWith("<")  &&  !targetClassType.AssemblyQualifiedName.StartsWith("<"))
            {
               // ignore classes marked as ignorable
               if (targetClassType.CustomAttributes != null)
               {
                  if (targetClassType.CustomAttributes.Where(
                     ca => ca.AttributeType == typeof(System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute)).Count() > 0)
                     continue;
               }

               var testClassTypes = testAssembly.GetTypes().Where(z => z.Name == targetClassType.Name + "Test");

               if (testClassTypes.Count() <= 0)
               {
                  errors.Add($"There is no test class for {targetClassType.Name}");

                  continue;
               }

               Type testClassType = testClassTypes.First();
               MethodInfo[] testClassMethods = testClassType.GetMethods(flags);
               MethodInfo[] targetClassMethods = targetClassType.GetMethods(flags);

               foreach (MethodInfo m in targetClassMethods)
               {
                  // ignore properties
                  if (m.IsSpecialName)
                     continue;

                  // ignore methods marked as ignorable
                  if (m.CustomAttributes != null)
                  {
                     if (m.CustomAttributes.Where(
                        ca => ca.AttributeType == typeof(System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute)).Count() > 0)
                        continue;
                  }

                  if (testClassMethods.Where(x => x.Name == m.Name + "Test").Count() <= 0)
                     errors.Add($"There is no test for method {targetClassType.Name}.{m.Name}");
               }
            }
         }

         return errors;
      }
   }
}