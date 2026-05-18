using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Application.Result;

public  record  Error(string Message)
{
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



}
