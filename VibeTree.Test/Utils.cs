using Bogus;
using Microsoft.Extensions.Hosting;
using VibeTree.Features.Link.CreateLink.Commands;
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

    public static CreateLinkCommand GenerateCreateLinkCommandInvalids(string? campo = default)
         => new Faker<CreateLinkCommand>("pt_BR")
         .CustomInstantiator(r =>
          campo switch
          {
              "LinkUrl" => new(string.Empty, r.Lorem.Letter(150), Guid.NewGuid().ToString(), 1, true),
              "Descricao" => new(r.Lorem.Letter(10), r.Lorem.Letter(266), Guid.NewGuid().ToString(), 1, true),
              "IdPerfil" => new(r.Lorem.Letter(10), r.Lorem.Letter(33), string.Empty, 1, true),
              _ => throw new Exception($"campo de {campo} não esta definodo em {nameof(GenerateCreateLinkCommandInvalids)}")

          }
        ).Generate();


    public static CreateLinkCommand GenerateCreateLinkCommandValid(string idPerfil)
     => new Faker<CreateLinkCommand>("pt_BR")
         .CustomInstantiator(r => new(r.Lorem.Letter(10), r.Lorem.Letter(33), idPerfil, 1, true));
     



}