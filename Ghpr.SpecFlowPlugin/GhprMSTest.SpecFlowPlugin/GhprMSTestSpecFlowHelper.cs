﻿// ReSharper disable InconsistentNaming
using System;
using System.Collections;
using System.Collections.Generic;
using Ghpr.Core.Common;
using Ghpr.Core.Extensions;
using Ghpr.Core.Interfaces;
using GhprSpecFlow.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace GhprMSTest.SpecFlowPlugin
{
    public class GhprMSTestSpecFlowHelper : IGhprSpecFlowHelper
    {
        private readonly ILogger _logger;

        public GhprMSTestSpecFlowHelper(ILogger logger)
        {
            ScreenHelper = new GhprMSTestSpecFlowScreenHelper();
            TestDataHelper = new GhprMSTestSpecFlowTestDataHelper();
            UpdateTestDataProvider = true;
            _logger = logger;
        }

        public static string GetFullNameForGuid(TestContext tc, ScenarioContext sc, FeatureContext fc)
        {
            var parameters = new List<string>();
            if (tc?.Properties != null)
            {
                foreach (var entry in tc.Properties)
                {
                    if (entry is KeyValuePair<string, object> e)
                    {
                        if (e.Key.ToLower().Contains("parameter"))
                        {
                            parameters.Add($"{e.Key} = {e.Value}");
                        }
                    }
                }
            }

            return $"{tc?.FullyQualifiedTestClassName}.{string.Join(",", parameters)}.{fc.FeatureInfo.Title}" +
                   $"{fc.FeatureInfo.Description}{sc.ScenarioInfo.Title}";
        }

        public static string GetFullName(TestContext tc, ScenarioContext sc, FeatureContext fc)
        {
            return
                $"{sc.TestContext().FullyQualifiedTestClassName}.{fc.FeatureInfo.Title}.{sc.ScenarioInfo.Title}";
        }

        public bool UpdateTestDataProvider { get; }
        public IGhprSpecFlowScreenHelper ScreenHelper { get; private set; }
        public IGhprSpecFlowTestDataHelper TestDataHelper { get; private set; }

        public ITestDataProvider GetTestDataProvider(FeatureInfo fi, ScenarioInfo si, FeatureContext fc, ScenarioContext sc)
        {
            return new GhprMSTestSpecFlowTestDataProvider(sc.TestContext(), sc, fc);
        }

        public TestRunDto GetTestRunOnScenarioStart(ITestRunner runner, FeatureInfo fi, ScenarioInfo si, FeatureContext fc, ScenarioContext sc)
        {
            var tc = sc.TestContext();
            ScreenHelper = new GhprMSTestSpecFlowScreenHelper();
            TestDataHelper = new GhprMSTestSpecFlowTestDataHelper(tc, sc, fc);
            
            var fullName = $"{tc?.FullyQualifiedTestClassName}.{fi.Title}.{si.Title}";
            var name = si.Title;
            var nameForGuid = GetFullNameForGuid(tc, sc, fc);
            var guid = nameForGuid.ToMd5HashGuid().ToString();
            var testRun = new TestRunDto(guid, name, fullName)
            {
                Categories = si.Tags,
                TestInfo =
                {
                    Start = DateTime.Now
                }
            };
            _logger.Debug($"TestRunDto created in GetTestRunOnScenarioStart: FullName = {fullName}, FullNameForGuid = {nameForGuid}");
            return testRun;
        }

        public TestRunDto UpdateTestRunOnScenarioEnd(TestRunDto tr, Exception testError, TestOutputDto testOutputDto, FeatureContext fc, ScenarioContext sc)
        {
            var tc = sc.ScenarioContainer.Resolve<TestContext>();
            var nameForGuid = GetFullNameForGuid(tc, sc, fc);
            var guid = nameForGuid.ToMd5HashGuid().ToString();
            tr.TestInfo.Guid = Guid.Parse(guid);
            tr.FullName = GetFullName(tc, sc, fc);
            tr.Result = testError == null ? "Passed" : (testError is AssertFailedException ? "Failed" : "Error");
            tr.TestMessage = testError?.Message ?? "";
            tr.TestStackTrace = testError?.StackTrace ?? "";
            tr.TestData.AddRange(TestDataHelper.GetTestData());
            return tr;
        }

        public void OnGiven(ScenarioContext sc)
        {
            (TestDataHelper as GhprMSTestSpecFlowTestDataHelper)?.SetTestContext(sc.TestContext());
        }

        public void OnWhen(ScenarioContext sc)
        {
            (TestDataHelper as GhprMSTestSpecFlowTestDataHelper)?.SetTestContext(sc.TestContext());
        }

        public void OnAnd(ScenarioContext sc)
        {
            (TestDataHelper as GhprMSTestSpecFlowTestDataHelper)?.SetTestContext(sc.TestContext());
        }

        public void OnBut(ScenarioContext sc)
        {
            (TestDataHelper as GhprMSTestSpecFlowTestDataHelper)?.SetTestContext(sc.TestContext());
        }

        public void OnThen(ScenarioContext sc)
        {
            (TestDataHelper as GhprMSTestSpecFlowTestDataHelper)?.SetTestContext(sc.TestContext());
        }
    }
}