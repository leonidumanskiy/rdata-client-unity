using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Requests.System;
using RData.Exceptions;
using System.IO;
using System.Linq;
using System;

namespace RData.Data
{
    public class LocalDataRepository : ILocalDataRepository
    {   
        private const string RDataDir = "rdata";
        private const string DataDir = "data";
        private const string ChunksDir = "chunks";

        /// <summary>
        /// Stores the cached information about chunks in the memory
        /// Used to not query the filesystem every time we need to know about new chunks.
        /// Set to null in the beginning - that means the cache is not yet loaded
        /// userId -> (requestId -> data chunks)
        /// </summary>
        private Dictionary<string, Dictionary<string, LocalDataChunkInfo>> _dataChunksCache = new Dictionary<string, Dictionary<string, LocalDataChunkInfo>>();

        private string RDataDirectoryPath
        {
            get { return Path.Combine(Application.persistentDataPath, RDataDir); }
        }

        private string GetChunksDirectory(string userId)
        {
            return Path.Combine(GetUserDirectory(userId), ChunksDir);
        }

        private string GetDataDirectory(string userId)
        {
            return Path.Combine(GetUserDirectory(userId), DataDir);
        }

        private string GetUserDirectory(string userId)
        {
            return Path.Combine(RDataDirectoryPath, userId);
        }

        private string GetChunkFilePath(string userId, BulkRequest request)
        {
            return Path.Combine(GetChunksDirectory(userId), request.Id);
        }

        private string GetChunkFilePath(string userId, string requestId)
        {
            return Path.Combine(GetChunksDirectory(userId), requestId);
        }

        private string GetDataFilePath(string userId, string key)
        {
            return Path.Combine(GetDataDirectory(userId), key);
        }

        public void SaveDataChunk(string userId, BulkRequest dataChunk)
        {
            // Save on the disk
            EnsureChunksDirectoryExists(userId);
            var json = RData.LitJson.JsonMapper.ToJson(dataChunk);
            var path = GetChunkFilePath(userId, dataChunk);
            File.WriteAllText(path, json);

            // Save into the cache
            EnsureDataChunksCacheLoaded(userId);
            var chunkInfo = new LocalDataChunkInfo() { requestCreatedAt = dataChunk.CreatedAt, requestId = dataChunk.Id, requestJson = json };
            _dataChunksCache[userId].Add(dataChunk.Id, chunkInfo);
        }

        public IEnumerable<LocalDataChunkInfo> LoadDataChunksJson(string userId)
        {
            EnsureDataChunksCacheLoaded(userId);

            if (_dataChunksCache.Count == 0)
                return null;
            else
                return new List<LocalDataChunkInfo>(_dataChunksCache[userId].Values);
        }

        public void RemoveDataChunk(string userId, string requestId)
        {
            // Delete from disk
            var path = GetChunkFilePath(userId, requestId);
            if (File.Exists(path))
                File.Delete(path);

            // Delete from cache
            EnsureDataChunksCacheLoaded(userId);
            if(_dataChunksCache[userId].ContainsKey(requestId))
                _dataChunksCache[userId].Remove(requestId);            
        }

        private void EnsureDataDirectoryExists(string userId)
        {
            Directory.CreateDirectory(GetDataDirectory(userId));
        }

        private void EnsureChunksDirectoryExists(string userId)
        {
            Directory.CreateDirectory(GetChunksDirectory(userId));
        }
        
        private void EnsureDataChunksCacheLoaded(string userId)
        {
            if (_dataChunksCache.ContainsKey(userId) && _dataChunksCache[userId] != null)
                return;
            
            EnsureChunksDirectoryExists(userId);

            List<LocalDataChunkInfo> result = new List<LocalDataChunkInfo>();

            var info = new DirectoryInfo(GetChunksDirectory(userId));
            var filesInfo = info.GetFiles();
            foreach (var file in filesInfo)
            {
                var requestId = file.Name;
                var json = File.ReadAllText(file.FullName);
                var baseRequest = RData.LitJson.JsonMapper.ToObject<JsonRpc.JsonRpcBaseRequest>(json);

                var chunkInfo = new LocalDataChunkInfo() { requestCreatedAt = baseRequest.CreatedAt, requestId = requestId, requestJson = json };
                result.Add(chunkInfo);
            }

            _dataChunksCache[userId] = result
                .OrderBy(chunkInfo => chunkInfo.requestCreatedAt) // Order by creation time
                .ToDictionary(chunkInfo => chunkInfo.requestId, chunkInfo => chunkInfo); // Convert into the dictionary
        }

        public void SaveData<TData>(string userId, string key, TData data)
        {
            EnsureDataDirectoryExists(userId);
            var json = RData.LitJson.JsonMapper.ToJson(data);
            var path = GetDataFilePath(userId, key);
            File.WriteAllText(path, json);
        }

        public TData LoadData<TData>(string userId, string key)
            where TData : class
        {
            var path = GetDataFilePath(userId, key);
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            var data = RData.LitJson.JsonMapper.ToObject<TData>(json);
            return data;
        }

        public void RemoveData(string userId, string key)
        {
            var path = GetDataFilePath(userId, key);
            File.Delete(path);
        }
    }
}
