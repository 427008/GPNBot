using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;

using Dapper;
using Serilog;

using GPNBot.API.Extensions;
using GPNBot.API.Services;
using GPNBot.API.Models;
using GPNBot.API.Attributes;
using GPNBot.API.Tools;


namespace GPNBot.API.Repositories
{
    public class Repository<T> where T : new()
    {
        protected readonly IConnectionService _connectionService;
        protected readonly string repository = "Repository";
        protected Stopwatch StopWatch;
        protected static string LogDBname = "apilog";

        private readonly int _longQueryLimit = 50;

        public long Count;
        public Type Model { get; }

        public Repository(IConnectionService connectionService)
        {
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            Model = typeof(T);
        }

        public async Task<(bool, string, long, List<T>)> GetAsync()
        {
            return await GetAsync(new Dictionary<string, Condition[]>() { }).ConfigureAwait(false);
        }

        public virtual async Task<(bool, string, long, List<T>)> GetAsync(
            Dictionary<string, Condition[]> dictWhere, 
            CSSet setConditionOr = null, 
            Dictionary<string, string> dictOrder = null, 
            long limit = 0,
            bool onlyCount = false)
        {
            using var db = _connectionService.DataBase();
            var query = "";
            var parameters = "";
            try
            {
                // WHERE
                var (strWhere, objParams, strInfo) = Where(Model, dictWhere, setConditionOr);
                parameters = JsonSerializer.Serialize(objParams, GPNJsonSerializer.Option());

                // SELECT COUNT(*)
                var (success, sql) = GetQuery(Model, "SelectCount", db.Database);
                if (!success)
                    return (false, "'SelectCount' command not found", 0, null);

                query = $"{sql}{strWhere}";
                StopWatch = Stopwatch.StartNew();
                Count = await db.ExecuteScalarAsync<long>(query, objParams).ConfigureAwait(false);
                LogTime(query, strInfo);
                if (Count == 0)
                    return (true, null, 0, new List<T>());

                if(onlyCount)
                    return (true, null, Count, new List<T>());

                // SELECT 
                (success, sql) = GetQuery(Model, "Select", db.Database);
                if (!success) return (false, "'Select' command not found", 0, null);

                // ORDER
                var strOrder = Order(Model, dictOrder);

                query = $"{sql}{strWhere}{strOrder}{(limit == 0 ? "" : $" LIMIT {limit}")}";
                StopWatch.Restart();
                var results = (await db.QueryAsync<T>(query, objParams).ConfigureAwait(false)).ToList();
                LogTime(query, strInfo);
                db.Close();

                return (true, null, Count, results);
            }
            catch (Exception ex)
            {
                if(!LogDBname.Equals(db.Database)) 
                    Log.Error(ex, "{source}: '{query}'. Params: '{parameters}'", repository, query, parameters);
                return (false, ex.Message, 0, null);
            }
        }


        //public virtual async Task<(bool, string, long, List<T>)> GetCustomAsync(
        //    string customSelect,
        //    Dictionary<string, Condition[]> dictWhere, 
        //    Dictionary<string, string> dictOrder = null, 
        //    long limit = 0,
        //    bool onlyCount = false)
        //{
        //    using var db = _connectionService.DataBase();
        //    var query = "";
        //    var parameters = "";
        //    try
        //    {
        //        // WHERE
        //        var (strWhere, objParams, strInfo) = Where(Model, dictWhere);
        //        parameters = JsonSerializer.Serialize(objParams, GPNJsonSerializer.Option());

        //        // SELECT COUNT(*)
        //        var (success, sql) = GetQuery(Model, "SelectCount", db.Database);
        //        if (!success)
        //            return (false, "'SelectCount' command not found", 0, null);

        //        query = $"{sql}{strWhere}";
        //        StopWatch = Stopwatch.StartNew();
        //        Count = await db.ExecuteScalarAsync<long>(query, objParams).ConfigureAwait(false);
        //        LogTime(query, strInfo);
        //        if (Count == 0)
        //            return (true, null, 0, new List<T>());

        //        if(onlyCount)
        //            return (true, null, Count, new List<T>());

        //        // SELECT 
        //        (success, sql) = GetQuery(Model, customSelect, db.Database);
        //        if (!success) return (false, $"'{customSelect}' command not found", 0, null);

        //        // ORDER
        //        var strOrder = Order(Model, dictOrder);

        //        query = $"{sql}{strWhere}{strOrder}{(limit == 0 ? "" : $" LIMIT {limit}")}";
        //        StopWatch.Restart();
        //        var results = (await db.QueryAsync<T>(query, objParams).ConfigureAwait(false)).ToList();
        //        LogTime(query, strInfo);
        //        db.Close();

        //        return (true, null, Count, results);
        //    }
        //    catch (Exception ex)
        //    {
        //        if(!LogDBname.Equals(db.Database)) 
        //            Log.Error(ex, "{source}: '{query}'. Params: '{parameters}'", repository, query, parameters);
        //        return (false, ex.Message, 0, null);
        //    }
        //}

        public virtual async Task<(bool, string, T)>  GetAsync(ulong ID, bool errorIfNotExists = true)
        { 
            using var db = _connectionService.DataBase();
            var query = "";
            var parameters = "";
            try
            {
                var keys = Model.GetKeyList();
                if (keys.Count != 1) return (false, "pripary key must be one field", default);
                var dictWhere = Condition.Create(keys.First().Name, ID);

                // WHERE
                var (strWhere, objParams, strInfo) = Where(Model, dictWhere);
                parameters = JsonSerializer.Serialize(objParams, GPNJsonSerializer.Option());

                // SELECT 
                var (success, sql) = GetQuery(Model, "Select", db.Database);
                if (!success) return (false, "'Select' command not found", default);

                query = $"{sql}{strWhere} LIMIT 1";
                StopWatch = Stopwatch.StartNew();
                var result = await db.QueryFirstOrDefaultAsync<T>(query, objParams).ConfigureAwait(false);
                LogTime(query, strInfo);
                db.Close();

                if(errorIfNotExists && result == null)
                    return (false, "Not Found", result);

                return (true, null, result);
            }
            catch (Exception ex)
            {
                if(!LogDBname.Equals(db.Database)) 
                    Log.Error(ex, "{source}: '{query}'. Params: '{parameters}'", repository, query, parameters);
                return (false, ex.Message, default);
            }
        }

        public virtual async Task<(bool, string, T)> PostAsync(T data)
        {
            using var db = _connectionService.DataBase();
            var query = "";

            try
            {
                var (success, sql) = GetQuery(Model, "Insert", db.Database);
                if (!success)
                    return (false, "'Insert' command not found", default);

                var (successOnValues, message, valuesStr, param, strInfo) = InsertValues(Model, data);
                if (!successOnValues)
                    return (false, message, default);

                string postSelect = "";
                (success, postSelect) = GetQuery(Model, "PostSelect", db.Database);

                StopWatch = Stopwatch.StartNew();
                T newData = default;
                
                if(string.IsNullOrEmpty(postSelect))
                {
                    query = $"{sql} {valuesStr}";
                    Count = await db.ExecuteAsync(query, param).ConfigureAwait(false);
                }
                else
                { 
                    query = $"{sql} {valuesStr}; {postSelect};";
                    newData = await db.QuerySingleAsync<T>(query, param).ConfigureAwait(false);
                }

                LogTime(query, strInfo);
                return (true, null, newData);
            }
            catch (Exception ex)
            {
                var parameters = JsonSerializer.Serialize(data, GPNJsonSerializer.Option());
                if(!LogDBname.Equals(db.Database)) 
                    Log.Error(ex, "{source}: '{query}'. Params: '{parameters}'", repository, query, parameters);
                return (false, ex.Message, default);
            }
        }

        public virtual async Task<(bool, string, T)> PutAsync(T data, Dictionary<string, object> dictNewValues, bool mustGetBuckData = true)
        {
            using var db = _connectionService.DataBase();
            var query = "";

            try
            {
                var (success, sql) = GetQuery(Model, "Update", db.Database);
                if (!success)
                    return (false, "'Update' command not found", default);

                string postSelect = "";
                (success, postSelect) = GetQuery(Model, "PostSelect", db.Database);
                if(mustGetBuckData && string.IsNullOrEmpty(postSelect))
                    return (false, "No PostSelect specified", default);

                //var (successOnValues, message, valuesStr, param, strInfo) = InsertValues(Model, data);
                //if (!successOnValues)
                //    return (false, message, default);

                var (composeSuccess, error, strSetAndWhere, param, strInfo) = SetValues(data, dictNewValues);
                if (!composeSuccess)
                    return (false, error, default);

                StopWatch = Stopwatch.StartNew();
                T updatedData = default;
                if(mustGetBuckData)
                {
                    query = $"{sql}{strSetAndWhere}; {postSelect};";
                    updatedData = await db.QuerySingleAsync<T>(query, param).ConfigureAwait(false);
                    if(updatedData != null) Count = 1;
                }
                else
                { 
                    query = $"{sql}{strSetAndWhere};";
                    Count = await db.ExecuteAsync(query, param).ConfigureAwait(false);
                }
                LogTime(query, strInfo);
                if(Count == 0) 
                    return (false, "Record not found", default);

                return (true, null, updatedData);
            }
            catch (Exception ex)
            {
                var parameters = JsonSerializer.Serialize(data, GPNJsonSerializer.Option());
                if(!LogDBname.Equals(db.Database)) 
                    Log.Error(ex, "{source}: '{query}'. Params: '{parameters}'", repository, query, parameters);
                return (false, ex.Message, default);
            }
        }

        protected static (bool, string) GetQuery(Type model, string queryType, string database)
        {
            var propertyInfo = model.GetProperty(queryType);

            if (propertyInfo != null &&
                propertyInfo.PropertyType.IsEquivalentTo(typeof(string)) &&
                propertyInfo.GetAccessors().FirstOrDefault(m => m.IsStatic) != null)
            {
                var result = (string)propertyInfo.GetValue(null);

                if (!string.IsNullOrEmpty(result))
                    return (true, result.Replace("?", database));
            }

            return (false, null);
        }

        protected static (string, object, string) Where(Type model, Dictionary<string, Condition[]> dictWhere, CSSet setConditionOr = null)
        {
            if(dictWhere.Count == 0) 
                return ("", null, "");
            
            var whereListVerified = new List<string>();
            var whereDictVerified = new Dictionary<string, object>();
            var whereListInfo = new List<string>();

            model
                .GetProperties()
                .Where(p => Attribute.GetCustomAttribute(p, typeof(PersistAttribute)) != null)
                .ToList()
                .ForEach(p =>
                {
                    if (dictWhere.TryGetValueIgnoreCase(p.Name, out var conditions))
                    {
                        var isOne = conditions.Length == 1;
                        var i = 0;
                        var whereListTmp = new List<string>();

                        foreach(var condition in conditions)
                        {
                            i++;
                            var name = isOne ? p.Name : $"{p.Name}{i}";
                            whereDictVerified[name] = condition.Value;
                            whereListInfo.Add($"@{p.Name}={condition.Value}");
                            whereListTmp.Add($"{p.Name}{condition.Operator}@{name}");
                        }

                        if(whereListTmp.Count > 1 && (setConditionOr?.HasKey(p.Name) ?? false))
                            whereListVerified.Add($"({string.Join(" OR ", whereListTmp)})");
                        else
                            whereListVerified.AddRange(whereListTmp);
                    }
                });

            var strWhere = whereListVerified.Count == 0 ? "" : $" WHERE {string.Join(" AND ", whereListVerified)}";
            var objParams = whereDictVerified.Count == 0 ? null : whereDictVerified.ToObject();
            var strInfo = whereListVerified.Count == 0 ? "" : string.Join(", ", whereListInfo);

            return (strWhere, objParams, strInfo);
        }

        protected static string Order(Type model, Dictionary<string, string> dictOrder)
        {
            if(dictOrder is null) return "";
            
            var listOrderVerified = new List<string>();

            model
                .GetProperties()
                .Where(p => Attribute.GetCustomAttribute(p, typeof(PersistAttribute)) != null)
                .ToList()
                .ForEach(p =>
                {
                    if (dictOrder.TryGetValueIgnoreCase(p.Name, out var strDirection))
                    {
                        if(string.IsNullOrEmpty(strDirection) || 
                           strDirection.EqIgnoreCase("asc"))
                           listOrderVerified.Add($"{p.Name} ASC");
                        else if(strDirection.EqIgnoreCase("desc"))
                           listOrderVerified.Add($"{p.Name} DESC");
                    }
                });

            return listOrderVerified.Count == 0 ? "" : $" ORDER BY {string.Join(" ,", listOrderVerified)}";
        }

        protected static (bool, string, string, object, string) InsertValues(Type model, T data)
        { 
            var insertDict = new Dictionary<string, object>();
            var insertFields = new List<string>();
            var insertParams = new List<string>();
            var insertListInfo = new List<string>();
            
            model.GetProperties()
                .Where(p => Attribute.GetCustomAttribute(p, typeof(PersistAttribute)) != null &&
                            Attribute.GetCustomAttribute(p, typeof(ReadOnlyAttribute)) is null &&
                            p.GetAccessors().FirstOrDefault(m =>m.IsStatic) is null)
                .ToList()
                .ForEach(p =>
                {
                    var value = p.GetValue(data);
                    insertDict[p.Name] = value;
                    insertFields.Add(p.Name);
                    insertParams.Add($"@{p.Name}");
                    insertListInfo.Add($"@{p.Name}={value}");
                });

            if (insertFields.Count == 0 || insertDict.Count == 0 || insertDict.Count != insertFields.Count)
                return (false, "Data to insert not found or not correct", null, null, null);

            var valuesStr = $"({string.Join(", ", insertFields)}) VALUES ({string.Join(", ", insertParams)})";
            var strInfo = insertListInfo.Count == 0 ? "" : string.Join(", ", insertListInfo);

            return (true, null, valuesStr, insertDict.ToObject(), strInfo);
        }

        protected static (bool, string, string, object, string) SetValues(T data, Dictionary<string, object> setDict)
        {
            var model = data.GetType();
            var keys = model.GetKeyList();
            if (keys.Count == 0)
                return (false, "'Update' command can`t run without key fields", null, null, null);

            var listModelKeys = new List<string>();
            var dictModelKeys = new Dictionary<string, object>();
            var setListInfoLog = new List<string>();

            foreach(var key in keys)
            { 
                var keyValue = key.GetValue(data);
                if(keyValue is null)
                    return (false, "'Update' command can`t run without key field value", null, null, null);

                var paramName = $"{key.Name}_paramValue";
                listModelKeys.Add($"{key.Name}=@{paramName}");
                dictModelKeys[paramName] = keyValue;
                setListInfoLog.Add($"@{paramName}={keyValue}");
            }

            var listVerifiedToSet = new List<string>();
            var dictVerifiedToSet = new Dictionary<string, object>();

            model.GetProperties()
                .ToList()
                .ForEach(p =>
                {
                    if (setDict.TryGetValueIgnoreCase(p.Name, out var obj))
                    {
                        dictVerifiedToSet[p.Name] = obj;
                        listVerifiedToSet.Add($"{p.Name}=@{p.Name}");
                        setListInfoLog.Add($"@{p.Name}={obj}");
                    }
                });

            if (listVerifiedToSet.Count == 0)
                return (false, "'Update' command can`t run without data to set", null, null, null);

            var strSetAndWhere = $" {string.Join(", ", listVerifiedToSet)}";
            strSetAndWhere += $" WHERE {string.Join(", ", listModelKeys)}";
            var strInfoLog = setListInfoLog.Count == 0 ? "" : string.Join(", ", setListInfoLog);
            
            var dictParams = dictVerifiedToSet.Concat(dictModelKeys).ToDictionary(s => s.Key, s => s.Value);

            return (true, null, strSetAndWhere, dictParams.ToObject(), strInfoLog);
        }



        protected virtual void LogTime(string query, string parameters = null)
        {
            var watch = StopWatch.ElapsedMilliseconds;
            StopWatch.Stop();

            if (watch < _longQueryLimit) return;
            if (string.IsNullOrEmpty(query)) return;

            Log.Information("{source}: {watch} ms for '{query}'. {parameters}",
                repository,
                watch, 
                query, 
                string.IsNullOrEmpty(parameters) ? "''" : $"'{parameters}'");
        }
    }
}
