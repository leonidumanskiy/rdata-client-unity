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
        private const string ChunksDir = "chunks";
        private const char ChunkNameDelimeter = '_';

        private string RDataDirectoryPath
        {
            get { return Path.Combine(Application.persistentDataPath, RDataDir); }
        }

        private string GetChunksDirectory(string userId)
        {
            return Path.Combine(GetUserDataDirectory(userId), ChunksDir);
        }

        private string GetUserDataDirectory(string userId)
        {
            return Path.Combine(RDataDirectoryPath, userId);
        }

        private string GetChunkFilePath(string userId, BulkRequest request)
        {
            return Path.Combine(GetChunksDirectory(userId), string.Format("{0}{1}{2}", request.CreatedAt, ChunkNameDelimeter, request.Id));
        }

        private string GetChunkFilePath(string userId, string requestId)
        {
            return Path.Combine(GetChunksDirectory(userId), requestId);
        }

        public void SaveDataChunk(string userId, BulkRequest dataChunk)
        {
            var json = LitJson.JsonMapper.ToJson(dataChunk);
            var path = GetChunkFilePath(userId, dataChunk);
            File.WriteAllText(path, json);
        }

        public IEnumerable<LocalDataChunkInfo> LoadDataChunksJson(string userId)
        {
            List<LocalDataChunkInfo> result = new List<LocalDataChunkInfo>();
            
            var info = new DirectoryInfo(ChunksDir);
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
    }
}