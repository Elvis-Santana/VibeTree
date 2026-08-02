using Bogus;
using Microsoft.Extensions.Hosting;
using VibeTree.Features.User.GetUser.GetAllUser;
using VibeTree.Shared.Entity;
using Wolverine;
using Wolverine.Tracking;

namespace VibeTree.Test;

public static class Utils
{
    public static async Task<HttpResponseMessage> Cast(IHost host,Task<HttpResponseMessage> task)
    {
        HttpResponseMessage response = default!;

        Func<IMessageContext, Task> action = async _ => response = await task;

        await host.TrackActivity().Timeout(TimeSpan.FromSeconds(10)).ExecuteAndWaitAsync(action);

        return response;
    }

    public static (User user, Perfil perfil) GetUserAndPerfil()
    {
        User user = new Faker<User>("pt_BR")
         .CustomInstantiator(f => new(Guid.NewGuid(),
             DateTime.Now,
             DateTime.Now,
             f.Internet.UserName(),
             f.Internet.Password(),
             f.Internet.Email())
         ).Generate();

        Perfil perfil = new Faker<Perfil>("pt_BR")
           .CustomInstantiator(f =>
               new(Guid.NewGuid(),
               DateTime.Now,
               DateTime.Now,
               f.Internet.Color(),
               f.Lorem.Text(),
               f.Image.LoremFlickrUrl(),
               f.Internet.UserName(),
               user.Id)
          ).Generate();

        return (user, perfil);

    }
}