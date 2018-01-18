﻿/* =============================================================================*\
*
* Filename: FunctionFlow.Private
* Description: 
*
* Version: 1.2
* Created: 2017/11/23 14:52:17 (UTC+8:00)
* Compiler: Visual Studio 2017
* 
* Author: zsh2401
* Company: I am free man
*
\* =============================================================================*/
using AutumnBox.Basic.Executer;
using AutumnBox.Support.CstmDebug;
using System;

namespace AutumnBox.Basic.FlowFramework
{
    partial class FunctionFlow<TArgs, TResult>
    {
        private CommandExecuter _executer;
        private int? _pid;
        private TArgs _args;
        private bool _isInited = false;
        private TResult _resultTmp;
        private bool isForceStoped = false;
        private void MainFlow()
        {
            IsClosed = true;
            try
            {
                _MainFlow();
            }
            catch (Exception e)
            {
                Logger.T(TAG, "a exception happend ->" + e);
                try
                {
                    OnFinished(new FinishedEventArgs<TResult>(new TResult() { ResultType = ResultType.Exception, Exception = e }));
                }
                catch (Exception e_)
                {
                    Logger.T(TAG, "Exception happend on OnFinished() when already happend a expcetion and reporting.....fuck!", e_);
                }
            }
        }
        private void _MainFlow()
        {
            /*Checking*/
            Logger.T(TAG, "step 1: checking...");
            TResult result = new TResult
            {
                CheckResult = Check(_args)
            };
            Logger.T(TAG, "step 1 finished: check result ->" + result.CheckResult);
            if (result.CheckResult != CheckResult.OK)
            {
                result.ResultType = ResultType.Unsuccessful;
                OnFinished(new FinishedEventArgs<TResult>(result));
                return;
            }
            Logger.T(TAG, "step 2: call OnStartup() Startup...");
            OnStartup(new StartupEventArgs());
            Logger.T(TAG, "step 3: call MainMethod(),this FunctionFlow is Running...");
            result.OutputData = MainMethod(new ToolKit<TArgs>(_args, _executer));
            Logger.T(TAG, "step 4: MainMethod() finished,call AnalyzeResult()");
            AnalyzeResult(result);
            Logger.T(TAG, "step 5: all finished,this FunctionFlow is all finished,trigger Finished event");
            OnFinished(new FinishedEventArgs<TResult>(result));
        }
    }
}
