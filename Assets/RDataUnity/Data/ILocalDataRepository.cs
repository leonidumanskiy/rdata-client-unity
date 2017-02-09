using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Requests.System;

namespace RData.Data
{
    public interface ILocalDataRepository
    {
        void SaveDataChunk(string userId, BulkRequest dataChunk);
        IEnumerable<LocalDataChunkInfo> LoadDataChunksJson(string userId);
        void RemoveDataChunk(string userId, string requestId);
    }
}