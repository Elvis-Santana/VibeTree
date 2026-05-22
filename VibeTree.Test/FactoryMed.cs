using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Mediator;
using VibeTree.Application.Perfil;
using VibeTree.Application.Perfil.Create;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.Test;

public class FactoryMed 
{
    public static IMediator CreateMediatorWithHandler<TRequest, TResponse>(IHandler<TRequest, TResponse> handler) where TRequest :IRequest<TResponse>
    {
        IServiceProvider provider = Substitute.For<IServiceProvider>();

        provider.GetService(Arg.Any<Type>()).Returns(handler);

        return new Mediator(provider);

    }
}
