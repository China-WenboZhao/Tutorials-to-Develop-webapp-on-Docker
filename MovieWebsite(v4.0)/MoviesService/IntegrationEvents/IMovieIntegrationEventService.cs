﻿using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MoviesService.IntegrationEvents
{
    public interface IMovieIntegrationEventService
    {
       
            Task SaveEventAndMovieContextChangesAsync(IntegrationEvent evt);
            Task PublishThroughEventBusAsync(IntegrationEvent evt);
        
    }
}
