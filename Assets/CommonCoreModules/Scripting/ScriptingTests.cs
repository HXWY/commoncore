﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Scripting
{
    internal class ScriptingTest
    {
        [CCScript(ClassName = "Test", Name = "HelloWorld")]
        public static void TestMethod(ScriptExecutionContext context)
        {
            Debug.Log("Hello world!");
        }

        [CCScript(ClassName = "Test", Name = "NoArgs")]
        private static void NoArgsTest()
        {
            Debug.Log("Hello world! (no args)");
        }

        [CCScript(ClassName = "Test", Name = "ContextArg")]
        public void ContextArgTest(ScriptExecutionContext context)
        {
            Debug.Log(context);
        }

        [CCScript(ClassName = "Test", Name = "SingleArg")]
        private void SingleArgTest(ScriptExecutionContext context, string arg0)
        {
            Debug.Log(arg0);
        }

        [CCScript(ClassName = "Test", Name = "ReturnValue")]
        private static string ReturnValueTest()
        {
            return "ReturnValueTestResult";
        }
    }
}