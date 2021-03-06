﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using NHateoas.Configuration;
using NHateoas.Dynamic.Interfaces;
using NHateoas.Routes;

namespace NHateoas.Dynamic.Strategies
{
    internal class StrategyBuilder : AbstractStrategy
    {
        private readonly List<ITypeBuilderStrategy> _compositeStrategiesList = new List<ITypeBuilderStrategy>();
        private readonly HashSet<Type> _complexTypes = new HashSet<Type>();
        private Type _objectType;

        #region AbstractStrategy
        public override string ClassKey(Type originalType)
        {
            return _compositeStrategiesList.Aggregate("_"  + originalType.FullName + "_", (s, strategy) => string.Format("{0}{1}_", s, strategy.ClassKey(originalType)));
        }

        public override void Configure(ITypeBuilderContainer container)
        {
            _compositeStrategiesList.ForEach(strategy => strategy.Configure(container));
        }
        #endregion

        #region IInstanceActivator
        public override void ActivateInstance(object proxyInstance, object originalInstance, IActionConfiguration actionConfiguration)
        {
            _compositeStrategiesList.ForEach(a => a.ActivateInstance(proxyInstance, originalInstance, actionConfiguration));
        }
        #endregion

        #region Fluent
        public StrategyBuilder For(Type objectType)
        {
            _objectType = objectType;
            return this;
        }

        public StrategyBuilder WithSimpleProperties()
        {
            _compositeStrategiesList.Add(
                new SimplePropertiesAggregatedStrategy(_objectType, _complexTypes)
                );
            return this;
        }

        public StrategyBuilder WithPlainRouteInformation(IList<string> routeInformation)
        {
            _compositeStrategiesList.Add(
                new SimpleRoutesStrategy(routeInformation)
                );
            return this;
        }

        public StrategyBuilder WithTypedMetadataProperty(Type metadataType, string propertyName)
        {
            _compositeStrategiesList.Add(
                new TypedMetadataPropertyStrategy(metadataType, propertyName)
                );
            return this;
        }
        
        public StrategyBuilder WithPayloadPropertyStrategy(Type metadataType, string propertyName)
        {
            _compositeStrategiesList.Add(
                new PayloadPropertyStrategy(metadataType, propertyName)
                );
            return this;
        }

        public StrategyBuilder WithSimpleAttributedPropertyStrategy(Type propType, string propName,
            Func<CustomAttributeBuilder>[] attributeFactories)
        {
            _compositeStrategiesList.Add(
                new SimplePropertyStrategy(propType, propName, attributeFactories));
            return this;
        }

        public StrategyBuilder WithCustomActivationStrategy(Action<object, object, IActionConfiguration> customActivator)
        {
            _compositeStrategiesList.Add(new CustomActivatorStrategy(customActivator));
            return this;
        }
        public ITypeBuilderStrategy Build()
        {
            return this;
        }
        #endregion
    }
}
