﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using NHateoas.Configuration;
using NHateoas.Routes.RouteMetadataProviders.SimpleMetadataProvider;
using NHateoas.Routes.RouteValueSubstitution;

namespace NHateoas.Routes.RouteMetadataProviders.SirenMetadataProvider
{
    [SecuritySafeCritical]
    internal class SirenMetadataProvider : IMetadataProvider
    {
        private readonly IRouteNameBuilder _routeNameBuilder = new SirenRelNameBuilder();
        private readonly Dictionary<string, List<string>> _apiDescriptionToRouteNameDictionary = new Dictionary<string, List<string>>();
        private readonly IActionConfiguration _actionConfiguration;
        // Add this as class name or additional link rel for IEnumerable responses. This is a hint for AngularJS provider how to query resource
        public static string QueryClassName = "__query"; 

        public SirenMetadataProvider(IActionConfiguration actionConfiguration)
        {
            _actionConfiguration = actionConfiguration;

            GenerateLinkNames();
        }
        
        private void GenerateLinkNames()
        {
            var mappingRules = _actionConfiguration.MappingRules;

            foreach (var mappingRule in mappingRules)
            {
                var apiDescription = mappingRule.ApiDescriptions.OrderBy(d => d.RelativePath.Length).FirstOrDefault();

                if (apiDescription == null)
                    continue;

                var routeName = _routeNameBuilder.Build(mappingRule, apiDescription.HttpMethod.Method);

                _apiDescriptionToRouteNameDictionary.Add(apiDescription.ID, routeName);
            }
        }

        public object GetMetadataByType(Type metadataType, params object[] values)
        {
            if (typeof (MetadataPlainObjects.Links).IsAssignableFrom(metadataType))
                return LinksGenerator.Generate(_actionConfiguration, _apiDescriptionToRouteNameDictionary, values[0]);
            if (typeof(MetadataPlainObjects.Actions).IsAssignableFrom(metadataType))
                return ActionsGenerator.Generate(_actionConfiguration, _apiDescriptionToRouteNameDictionary, values[0]);
            if (typeof(MetadataPlainObjects.Entities).IsAssignableFrom(metadataType))
                return EntitiesGenerator.Generate(_actionConfiguration, _apiDescriptionToRouteNameDictionary, values[0]);
            
            throw new NotImplementedException();
        }

        public IList<Type> GetRegisteredMetadataTypes()
        {
            var result = new List<Type>
            {
                typeof (MetadataPlainObjects.Links),
                typeof (MetadataPlainObjects.Actions)
            };
            
            if (_actionConfiguration.EntityRules.Any())
                result.Add(typeof(MetadataPlainObjects.Entities));

            return result;
        }

        public string ContentType
        {
            get { return "application/vnd.siren+json"; }
        }
    }
}
