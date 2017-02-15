using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Exceptions;
using RData.Data;
using RData.Requests.System;
using System;
using System.Linq;

namespace RData.Tests.Mock
{
    public class MockDataRepository : ILocalDataRepository
    {
        public Dictionary<string, Dictionary<string, LocalDataChunkInfo>> _dataChunks = new Dictionary<string, Dictionary<string, LocalDataChunkInfo>>();

        public IEnumerable<LocalDataChunkInfo> LoadDataChunksJson(string userId)
        {
            if (!_dataChunks.ContainsKey(userId))
                return Enumerable.Empty<LocalDataChunkInfo>();

            return _dataChunks[userId].Values.OrderBy(v => v.requestCreatedAt);
        }

        public void RemoveDataChunk(string userId, string requestId)
        {
            if (!_dataChunks.ContainsKey(userId))
                throw new RDataException("User id not found in the data chunks list");

            if (!_dataChunks[userId].ContainsKey(requestId))
                throw new RDataException("requestId not found in the data chunks list");

            _dataChunks[userId].Remove(requestId);
        }

        public void SaveDataChunk(string userId, BulkRequest dataChunk)
        {
            var requestJson = LitJson.JsonMapper.ToJson(dataChunk);
            LocalDataChunkInfo chunkInfo = new LocalDataChunkInfo() { requestId = dataChunk.Id, requestJson = requestJson, requestCreatedAt = dataChunk.CreatedAt };
        }
    }
}