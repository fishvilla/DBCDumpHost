﻿using DBCD;
using DBCD.Providers;
using DBCDumpHost.Services;
using DBCDumpHost.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DBCDumpHost.Controllers
{
    [Route("api/header")]
    [ApiController]
    public class HeaderController : ControllerBase
    {
        public struct HeaderResult
        {
            public List<string> headers;
            public Dictionary<string, string> fks;
            public string error;
        }

        private readonly DBCManager dbcManager;
        private readonly DBDProvider dbdProvider;

        public HeaderController(IDBCManager dbcManager, IDBDProvider dbdProvider)
        {
            this.dbcManager = dbcManager as DBCManager;
            this.dbdProvider = dbdProvider as DBDProvider;
        }


        // GET: api/DBC
        [HttpGet]
        public string Get()
        {
            return "No DBC selected!";
        }

        // GET: api/DBC/name
        [HttpGet("{name}")]
        public HeaderResult Get(string name, string build)
        {
            Logger.WriteLine("Serving headers for " + name + " (" + build + ")");

            var result = new HeaderResult();
            try
            {
                var storage = dbcManager.GetOrLoad(name, build);

                if (!dbdProvider.TryGetDefinition(name, out var definition))
                {
                    throw new KeyNotFoundException("Definition for " + name);
                }

                result.headers = new List<string>();
                result.fks = new Dictionary<string, string>();

                if (!storage.Values.Any())
                {
                    for (var j = 0; j < storage.AvailableColumns.Length; ++j)
                    {
                        var fieldName = storage.AvailableColumns[j];
                        result.headers.Add(fieldName);

                        if (definition.columnDefinitions.TryGetValue(fieldName, out var columnDef))
                        {
                            if (columnDef.foreignTable != null)
                            {
                                result.fks.Add(fieldName, columnDef.foreignTable + "::" + columnDef.foreignColumn);
                            }
                        }
                    }
                }
                else
                {
                    foreach (DBCDRow item in storage.Values)
                    {
                        for (var j = 0; j < storage.AvailableColumns.Length; ++j)
                        {
                            string fieldName = storage.AvailableColumns[j];
                            var field = item[fieldName];

                            if (field is Array a)
                            {
                                for (var i = 0; i < a.Length; i++)
                                {
                                    result.headers.Add($"{fieldName}[{i}]");

                                    if (definition.columnDefinitions.TryGetValue(fieldName, out var columnDef))
                                    {
                                        if (columnDef.foreignTable != null)
                                        {
                                            result.fks.Add($"{fieldName}[{i}]", columnDef.foreignTable + "::" + columnDef.foreignColumn);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                result.headers.Add(fieldName);

                                if (definition.columnDefinitions.TryGetValue(fieldName, out var columnDef))
                                {
                                    if (columnDef.foreignTable != null)
                                    {
                                        result.fks.Add(fieldName, columnDef.foreignTable + "::" + columnDef.foreignColumn);
                                    }
                                }
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error occured during serving data: " + e.Message);
                result.error = e.Message.Replace(SettingManager.dbcDir, "");
            }
            return result;
        }
    }
}
