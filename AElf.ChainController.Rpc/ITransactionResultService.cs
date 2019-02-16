﻿using System.Threading.Tasks;
using AElf.Common;
using AElf.Kernel;

namespace AElf.ChainController.Rpc
{
    public interface ITransactionResultService
    {
        Task<TransactionResult> GetResultAsync(Hash txId);

        /// <summary>
        /// result added should be the final version 
        /// as it will be inserted to storage
        /// tx in pending will not be inserted to storage
        ///  </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        Task AddResultAsync(TransactionResult res);
    }
}