using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Application.Common;

public  record  Error(string Message)
{
    public static Error IdEmpty =
     new("id não pode ser vazio");

    public  static Error NameEmpty = 
        new ("o nome não pode ser vazio");

    public static Error NameMinimumLength = 
        new ("o nome deve conter no mínimo 3 caracteres");

    public static Error EmailEmpty = 
        new ("email não pode ser vazio");

    public static Error EmailAddress = 
        new ("Formato de email inválido.");

    public static Error PasswordEmpty = 
        new("senha não pode ser vazio");

    public static Error PasswordMinimumLength =
        new("senha deve conter no mínimo 6 caracteres");


    public static Error InvalidCredentials =
        new("Credenciais inválidas");

    public static Error ExisteUser = new("Não foi possível concluir o cadastro. Verifique os dados");

    public static Error CorEmpty = new("cor não pode ser vazio");

    public static Error SlugEmpty = new("Slug não pode ser vazio");

    public static Error IdUserEmpty = new("IdUser não pode ser vazio");

    public static Error NotFound = new("Not Found");

    public static Error UserNotFound = new("usuairo não encontrado");

    public static Error FalhaAoCadastrar = new("Cadastrar falhou");

    public static Error IdValid = new Error("formato do id inválido");


}
