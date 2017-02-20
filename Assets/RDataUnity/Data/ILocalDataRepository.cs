using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Requests.System;

namespace RData.Data
{
    public interface ILocalDataRepository
    {
        void SaveData<TData>(string userId, string key, TData data);
        TData LoadData<TData>(string userId, string key)
            where TData : class;
        void RemoveData(string userId, string key);

        void SaveDataChunk(string userId, BulkRequest dataChunk);
        IEnumerable<LocalDataChunkInfo> LoadDataChunksJson(string userId);
        void RemoveDataChunk(string userId, string requestId);
    }
}