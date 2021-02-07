namespace MassTransit.MongoDbIntegration.Configurators
{
    using System;
    using GreenPipes.Internals.Extensions;
    using MassTransit.Saga;
    using Registration;
    using Registration.Sagas;


    public class MongoDbSagaRepositoryRegistrationProvider :
        ISagaRepositoryRegistrationProvider
    {
        readonly Action<IMongoDbSagaRepositoryConfigurator> _configure;

        public MongoDbSagaRepositoryRegistrationProvider(Action<IMongoDbSagaRepositoryConfigurator> configure)
        {
            _configure = configure;
        }

        void ISagaRepositoryRegistrationProvider.Configure<TSaga>(ISagaRegistrationConfigurator<TSaga> configurator)
            where TSaga : class
        {
            if (typeof(TSaga).HasInterface<ISagaVersion>())
            {
                var proxy = (IProxy)Activator.CreateInstance(typeof(Proxy<>).MakeGenericType(typeof(TSaga)));

                proxy.Configure(this);
            }
        }

        protected virtual void Configure<TSaga>(ISagaRegistrationConfigurator<TSaga> configurator)
            where TSaga : class, ISagaVersion
        {
            configurator.MongoDbRepository(r => _configure?.Invoke(r));
        }


        interface IProxy
        {
            public void Configure<T>(T provider)
                where T : MongoDbSagaRepositoryRegistrationProvider;
        }


        class Proxy<TSaga> :
            IProxy
            where TSaga : class, ISagaVersion
        {
            readonly ISagaRegistrationConfigurator<TSaga> _configurator;

            public Proxy(ISagaRegistrationConfigurator<TSaga> configurator)
            {
                _configurator = configurator;
            }

            public void Configure<T>(T provider)
                where T : MongoDbSagaRepositoryRegistrationProvider
            {
                provider.Configure(_configurator);
            }
        }
    }
}
