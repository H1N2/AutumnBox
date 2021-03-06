﻿/*

* ==============================================================================
*
* Filename: IOpenFxManager
* Description: 
*
* Version: 1.0
* Created: 2020/3/3 16:07:58
* Compiler: Visual Studio 2019
*
* Author: zsh2401
*
* ==============================================================================
*/
using AutumnBox.OpenFramework.Wrapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutumnBox.OpenFramework.Open
{
    /// <summary>
    /// 开放框架管理器
    /// </summary>
    public interface IOpenFxManager
    {
        /// <summary>
        /// 获取所有拓展模块包装器
        /// </summary>
        IExtensionWrapper[] Warppers { get; }
        /// <summary>
        /// 获取SDK等级
        /// </summary>
        int SDKLevel { get; }
    }
}
