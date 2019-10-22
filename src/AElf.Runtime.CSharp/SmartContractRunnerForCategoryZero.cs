using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using AElf.Kernel.SmartContract.Infrastructure;
using AElf.Types;
using Microsoft.Extensions.Logging;

namespace AElf.Runtime.CSharp
{
    public class SmartContractRunnerForCategoryZero : ISmartContractRunner
    {
        public ILogger<SmartContractRunnerForCategoryZero> Logger { get; set; }
        
        public int Category { get; protected set; }
        
        private readonly ISdkStreamManager _sdkStreamManager;

        private readonly string _sdkDir;
        private readonly ContractAuditor _contractAuditor;

        protected readonly IServiceContainer<IExecutivePlugin> _executivePlugins;
        
        public SmartContractRunnerForCategoryZero(
            string sdkDir,
            IServiceContainer<IExecutivePlugin> executivePlugins = null,
            IEnumerable<string> blackList = null,
            IEnumerable<string> whiteList = null)
        {
            _sdkDir = Path.GetFullPath(sdkDir);
            _sdkStreamManager = new SdkStreamManager(_sdkDir);
            _contractAuditor = new ContractAuditor(blackList, whiteList);
            _executivePlugins = executivePlugins ?? ServiceContainerFactory<IExecutivePlugin>.Empty;
        }

        /// <summary>
        /// Creates an isolated context for the smart contract residing with an Api singleton.
        /// </summary>
        /// <returns></returns>
        protected virtual AssemblyLoadContext GetLoadContext()
        {
            // To make sure each smart contract resides in an isolated context with an Api singleton
            return new ContractCodeLoadContext(_sdkStreamManager);
        }

        public virtual async Task<IExecutive> RunAsync(SmartContractRegistration reg)
        {
            var code = reg.Code.ToByteArray();
            var executive = new Executive(_executivePlugins, _sdkStreamManager) { ContractHash = reg.CodeHash };
            executive.Load(code);

            return await Task.FromResult(executive);
        }

        /// <summary>
        /// Performs code checks.
        /// </summary>
        /// <param name="code">The code to be checked.</param>
        /// <param name="isPrivileged">Is the contract deployed by system user.</param>
        /// <exception cref="InvalidCodeException">Thrown when issues are found in the code.</exception>
        public void CodeCheck(byte[] code, bool isPrivileged)
        {
#if !UNIT_TEST
            _contractAuditor.Audit(code, isPrivileged);
#endif
        }
    }
}