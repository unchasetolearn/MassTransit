namespace MassTransit.Futures.Configurators
{
    using System.Threading.Tasks;
    using Courier;
    using Courier.Contracts;


    public class BuildRoutingSlipExecutor<TInput> :
        IRoutingSlipExecutor<TInput>
        where TInput : class
    {
        readonly BuildItineraryCallback<TInput> _buildItinerary;

        public BuildRoutingSlipExecutor(BuildItineraryCallback<TInput> buildItinerary)
        {
            _buildItinerary = buildItinerary;
        }

        public async Task Execute(FutureConsumeContext<TInput> context)
        {
            var trackingNumber = NewId.NextGuid();

            var builder = new RoutingSlipBuilder(trackingNumber);

            builder.AddVariable(nameof(FutureConsumeContext.FutureId), context.FutureId);

            builder.AddSubscription(context.ReceiveContext.InputAddress, RoutingSlipEvents.Completed | RoutingSlipEvents.Faulted);

            await _buildItinerary(context, builder).ConfigureAwait(false);

            var routingSlip = builder.Build();

            await context.Execute(routingSlip).ConfigureAwait(false);

            if (Pending)
                context.Instance.Pending.Add(trackingNumber);
        }

        public bool Pending { get; set; }
    }
}
