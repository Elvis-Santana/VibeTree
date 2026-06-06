using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Domain.Entity;
using UserEntity = VibeTree.Domain.Entity.User;

namespace VibeTree.Infrastructure.AppDbContext;

public class ReadDbContext(DbContextOptions<ReadDbContext> options) : AbstractDbContext(options), IReadDbContext;

   

   

