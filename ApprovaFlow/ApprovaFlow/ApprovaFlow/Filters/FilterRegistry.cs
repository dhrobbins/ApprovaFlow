using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ApprovaFlow.Utils;
using System.Linq.Expressions;
using System.Configuration;
using Newtonsoft.Json;

namespace ApprovaFlow.Filters
{
    public class FilterRegistry<T>
    {
        private List<FilterDefinition<T>> systemFilters;
        private List<FilterDefinition<T>> approvedFilters;

        public FilterRegistry()
        {
            this.systemFilters = new List<FilterDefinition<T>>();
            this.approvedFilters = new List<FilterDefinition<T>>();
            ReadManifest();
            RegisterDefaultFilters();
        }

        /// <summary>
        /// Add the four default filters to registry:  
        ///   FetchDataFilter
        ///   SaveDataFilter
        ///   TriggerStateFilter
        ///   ValidParticipantFilter
        /// </summary>
        private void RegisterDefaultFilters()
        {
            var filterDef = new FilterDefinition<T>();
            filterDef.Name = "FetchDataFilter";
            filterDef.FilterCategory = "Default";
            filterDef.FilterType = typeof(FetchDataFilter);
            filterDef.Filter = AddCreateFilter(filterDef);

            this.systemFilters.Add(filterDef);

            filterDef = new FilterDefinition<T>();
            filterDef.Name = "SaveDataFilter";
            filterDef.FilterCategory = "Default";
            filterDef.FilterType = typeof(SaveDataFilter);
            filterDef.Filter = AddCreateFilter(filterDef);

            this.systemFilters.Add(filterDef);

            filterDef = new FilterDefinition<T>();
            filterDef.Name = "TriggerStateFilter";
            filterDef.FilterCategory = "Default";
            filterDef.FilterType = typeof(TriggerStateFilter);
            filterDef.Filter = AddCreateFilter(filterDef);

            this.systemFilters.Add(filterDef);

            filterDef = new FilterDefinition<T>();
            filterDef.Name = "ValidParticipantFilter";
            filterDef.FilterCategory = "Default";
            filterDef.FilterType = typeof(ValidParticipantFilter);
            filterDef.Filter = AddCreateFilter(filterDef);

            this.systemFilters.Add(filterDef);
        }

        /// <summary>
        /// Create a lambda that will create the filter
        /// </summary>
        /// <param name="filterDef">Definition of Filter</param>
        /// <returns>Compiled Lambda</returns>
        private Func<FilterBase<T>> AddCreateFilter(FilterDefinition<T> filterDef)
        {
            var body = Expression.MemberInit(Expression.New(filterDef.FilterType));
            return Expression.Lambda<Func<FilterBase<T>>>(body, null).Compile();
        }

        /// <summary>
        /// Read the roster of assemblies that can be used by the workflows
        /// </summary>
        private void ReadManifest()
        {
            string manifestSource = ConfigurationManager.AppSettings["ManifestSource"].ToString();

            Enforce.That(string.IsNullOrEmpty(manifestSource) == false,
                                "FilterRegistry.ReadManifest - ManifestSource can not be null");

            var fileInfo = new FileInfo(manifestSource);

            if (fileInfo.Exists == false)
            {
                throw new ApplicationException("RequestPromotion.Configure - File not found");
            }

            StreamReader sr = fileInfo.OpenText();
            string json = sr.ReadToEnd();
            sr.Close();            

            this.approvedFilters = JsonConvert.DeserializeObject<List<FilterDefinition<T>>>(json); 
        }

        /// <summary>
        /// Given a list of filter names, fetch the corresponding filters from
        /// the registry
        /// </summary>
        /// <param name="filterNames">Filter Names as string</param>
        /// <returns>Matching filters as IEnumerable</returns>
        public IEnumerable<FilterBase<T>> GetFilters(string filterNames)
        {
            Enforce.That(string.IsNullOrEmpty(filterNames) == false, 
                            "FilterRegistry.GetFilters - filterNames can not be null");

            var returnFilters = new List<FilterBase<T>>();
            var names = filterNames.Split(';').ToList();

            names.ForEach(name =>
            {
                var filter = this.systemFilters.Where(x => x.Name == name)
                                           .SingleOrDefault();

                if (filter != null)
                {   
                    returnFilters.Add(filter.Filter.Invoke());
                }                         
            });

            return returnFilters;
        }

        /// <summary>
        /// Given the path to an assembly, load that assembly in AppDomain
        /// and add FilterBase objects to registry.
        /// </summary>
        /// <param name="source"></param>
        public void LoadPlugIn(string source)
        {
            Enforce.That(string.IsNullOrEmpty(source) == false,
                            "PlugInLoader.Load - source can not be null");

            AppDomain appDomain = AppDomain.CurrentDomain;
            var assembly = Assembly.LoadFrom(source);

            var types = assembly.GetTypes().ToList();

            types.ForEach(type =>
            {
                var registerFilterDef = new FilterDefinition<T>();

                //  Is type from assembly registered?
                registerFilterDef = this.approvedFilters.Where(app => app.TypeFullName == type.FullName)
                                                        .SingleOrDefault();

                if (registerFilterDef != null)
                {
                    object obj = Activator.CreateInstance(type);
                    var filterDef = new FilterDefinition<T>();
                    filterDef.Name = obj.ToString();
                    filterDef.FilterCategory = registerFilterDef.FilterCategory;
                    filterDef.FilterType = type;
                    filterDef.TypeFullName = type.FullName;
                    filterDef.Filter = AddCreateFilter(filterDef);

                    this.systemFilters.Add(filterDef);
                }
            });
        }

        /// <summary>
        /// Read all assemblies that are present in a folder.
        /// </summary>
        /// <param name="source">Path to share as string</param>
        public void LoadPlugInsFromShare(string source)
        {
            var dirInfo = new DirectoryInfo(source);
            var fileNames = new List<string>();

            if (dirInfo.Exists)
            {
                fileNames = dirInfo.EnumerateFiles()
                            .Select(x => source + @"\" + x.Name)
                            .ToList();
            }

            fileNames.ForEach(name => LoadPlugIn(name));
        }

        /// <summary>
        /// Fetch the names of the registered filters
        /// </summary>
        /// <returns>A ; delimited list of names as string</returns>
        public string GetFilterNames()
        {
            string results = string.Empty;

            this.systemFilters.ForEach(x => results += x.Name + ";");
            
            return results;
        }

        /// <summary>
        /// Fetch the count of registered filters
        /// </summary>
        /// <returns>The count as int</returns>
        public int GetFilterCount()
        {
            return this.systemFilters.Count;
        }

        /// <summary>
        /// Serialize the FilterDefinitions.
        /// </summary>
        /// <returns>All register filters as JSON</returns>
        public string SerializeFilterDefinitions()
        {
            return JsonConvert.SerializeObject(this.systemFilters);
        }
    }
}
