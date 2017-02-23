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
            EnsureChunksDirectoryExists(userId);

            var json = LitJson.JsonMapper.ToJson(dataChunk);
            var path = GetChunkFilePath(userId, dataChunk);
            File.WriteAllText(path, json);
        }

        public IEnumerable<LocalDataChunkInfo> LoadDataChunksJson(string userId)
        {
            EnsureChunksDirectoryExists(userId);

            List<LocalDataChunkInfo> result = new List<LocalDataChunkInfo>();
            
            var info = new DirectoryInfo(GetChunksDirectory(userId));
            var filesInfo = info.GetFiles();
            foreach (var file in filesInfo)
            {
                var requestId = file.Name;
                var json = File.ReadAllText(file.FullName);
                var baseRequest = LitJson.JsonMapper.ToObject<JsonRpc.JsonRpcBaseRequest>(json);
                
                var chunkInfo = new LocalDataChunkInfo() { requestCreatedAt = baseRequest.CreatedAt, requestId = requestId, requestJson = json };
                result.Add(chunkInfo);
            }

            return result.OrderBy(chunkInfo => chunkInfo.requestCreatedAt);
        }

        public void RemoveDataChunk(string userId, string requestId)
        {
            File.Delete(GetChunkFilePath(userId, requestId));
        }

        private void EnsureDataDirectoryExists(string userId)
        {
            Directory.CreateDirectory(GetDataDirectory(userId));
        }

        private void EnsureChunksDirectoryExists(string userId)
        {
            Directory.CreateDirectory(GetChunksDirectory(userId));
        }

        public void SaveData<TData>(string userId, string key, TData data)
        {
            EnsureDataDirectoryExists(userId);
            var json = LitJson.JsonMapper.ToJson(data);
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
            var data = LitJson.JsonMapper.ToObject<TData>(json);
            return data;
        }

        public void RemoveData(string userId, string key)
        {
            var path = GetDataFilePath(userId, key);
            File.Delete(path);
        }
    }
}
