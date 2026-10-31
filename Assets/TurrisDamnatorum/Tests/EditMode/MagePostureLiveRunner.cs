using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Turris.Tests
{
    /// <summary>Testy w otwartym Unity 6.6; poprawny konstruktor ExecutionSettings w Test Framework 1.8.</summary>
    [InitializeOnLoad]
    public static class MagePostureLiveRunner
    {
        const string ActiveKey="Turris.MagePosture.Tests.Active";
        const string ResultKey="Turris.MagePosture.Tests.File";
        static readonly TestRunnerApi Api;
        static MagePostureLiveRunner() {Api=ScriptableObject.CreateInstance<TestRunnerApi>();Api.RegisterCallbacks(new Results());}

        [MenuItem("Turris/PoC/Mag anime/Testuj postawę w otwartym edytorze")]
        public static void Posture() {Run("^Turris\\.Tests\\.MagePosturePlayTests\\.","posture-live.xml");}
        [MenuItem("Turris/PoC/Mag anime/Testuj regresję postawy i bohaterów")]
        public static void Regression() {Run("^Turris\\.Tests\\.(MagePosturePlayTests|MageAnimePlayTests|BerserkerAnimePlayTests|HeroModelPlayTests)\\.","regression-live.xml");}
        static void Run(string pattern,string filename)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Testy uruchom z trybu edycji.");
            Directory.CreateDirectory("TestResults/mage-posture");
            SessionState.SetString(ResultKey,"TestResults/mage-posture/"+filename);
            SessionState.SetBool(ActiveKey,true);
            File.WriteAllText("TestResults/mage-posture/live-status.json","{\"running\":true}");
            Api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.PlayMode,groupNames=new[]{pattern}}));
        }
        class Results:ICallbacks
        {
            public void RunStarted(ITestAdaptor tests) {}
            public void TestStarted(ITestAdaptor test) {}
            public void TestFinished(ITestResultAdaptor result) {}
            public void RunFinished(ITestResultAdaptor result) {
                if(!SessionState.GetBool(ActiveKey,false))return;
                SessionState.SetBool(ActiveKey,false);
                TestRunnerApi.SaveResultToFile(result,SessionState.GetString(ResultKey,"TestResults/mage-posture/posture-live.xml"));
                File.WriteAllText("TestResults/mage-posture/live-status.json",JsonUtility.ToJson(new Status{passed=result.PassCount,failed=result.FailCount,skipped=result.SkipCount,result=result.ResultState},true));
                Debug.Log("MAGE_POSTURE_LIVE_RESULT "+result.ResultState+" passed="+result.PassCount+" failed="+result.FailCount+" skipped="+result.SkipCount);
            }
        }
        [Serializable]class Status {public bool running;public int passed,failed,skipped;public string result;}
    }
}
