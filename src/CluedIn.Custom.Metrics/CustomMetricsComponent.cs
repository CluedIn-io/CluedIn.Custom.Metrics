using Castle.MicroKernel.Registration;
using CluedIn.Core;
using CluedIn.Core.Server;
using ComponentHost;

namespace CluedIn.Custom.Metrics
{
    [Component("CustomMetrics", "Metrics", ComponentType.Service, ServerComponents.ProviderWebApi, Components.Server, Components.DataStores, Isolation = ComponentIsolation.NotIsolated)]
    public sealed class CustomMetricsComponent : ServiceApplicationComponent<IServer>
    {
        /**********************************************************************************************************
         * CONSTRUCTOR
         **********************************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMetricsComponent" /> class.
        /// </summary>
        /// <param name="componentInfo">The component information.</param>
        public CustomMetricsComponent(ComponentInfo componentInfo) : base(componentInfo)
        {
            // Dev. Note: Potential for compiler warning here ... CA2214: Do not call overridable methods in constructors
            //   this class has been sealed to prevent the CA2214 waring being raised by the compiler
            this.Container.Register(Component.For<CustomMetricsComponent>().Instance(this));
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        /// <summary>Starts this instance.</summary>
        public override void Start()
        {
            //this.Container.Register(CluedInTypes.FromCluedInAssembliesWithServiceFromInterface<IMetric>(d => d.WithServiceAllInterfaces().LifestyleSingleton()));

            this.State = ServiceState.Started;
        }

        /// <summary>Stops this instance.</summary>
        public override void Stop()
        {
            if (this.State == ServiceState.Stopped)
                return;

            this.State = ServiceState.Stopped;
        }
    }
}
