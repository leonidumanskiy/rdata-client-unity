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
        public Dictionary<string, string> _data = new Dictionary<string, string>();
        public Dictionary<string, Dictionary<string, LocalDataChunkInfo>> _dataChunks = new Dictionary<string, Dictionary<string, LocalDataChunkInfo>>();

        public TData LoadData<TData>(string userId, string key)
            where TData : class
        {
            var dictKey = userId + "." + key;
            if (!_data.ContainsKey(dictKey))
                return null;

            var json = _data[dictKey];
            TData data = RData.LitJson.JsonMapper.ToObject<TData>(json);
            return data;
        }

        public IEnumerable<LocalDataChunkInfo> LoadDataChunksJson(string userId)
        {
            if (!_dataChunks.ContainsKey(userId))
                return Enumerable.Empty<LocalDataChunkInfo>();

            return _dataChunks[userId].Values.OrderBy(v => v.requestCreatedAt);
        }

        public void RemoveData(string userId, string key)
        {
            _data.Remove(userId + "." + key);
        }

        public void RemoveDataChunk(string userId, string requestId)
        {
            if (!_dataChunks.ContainsKey(userId))
                throw new RDataException("User id not found in the data chunks list");

            if (!_dataChunks[userId].ContainsKey(requestId))
                throw new RDataException("requestId not found in the data chunks list");

            _dataChunks[userId].Remove(requestId);
        }

        public void SaveData<TData>(string userId, string key, TData data)
        {
            var json = RData.LitJson.JsonMapper.ToJson(data);
            _data[userId + "." + key] = json;
        }

        public void SaveDataChunk(string userId, BulkRequest dataChunk)
        {
            var requestJson = RData.LitJson.JsonMapper.ToJson(dataChunk);
            LocalDataChunkInfo chunkInfo = new LocalDataChunkInfo() { requestId = dataChunk.Id, requestJson = requestJson, requestCreatedAt = dataChunk.CreatedAt };
            if (!_dataChunks.ContainsKey(userId) || _dataChunks[userId] == null)
                _dataChunks[userId] = new Dictionary<string, LocalDataChunkInfo>();
            _dataChunks[userId].Add(dataChunk.Id, chunkInfo);
        }
    }
}